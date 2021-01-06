using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EndlessTerrain : MonoBehaviour
{
    [SerializeField] private bool hideChunkOnStartup;
    [SerializeField] private Material mapMaterial;
    [SerializeField] private LODInfo[] detailLevels;

    public static float maxViewDst;
    const float viewerMoveThresholdForChunkUpdate = 15f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float scale = 1f;

    [SerializeField] private Transform[] viewers;
    private Transform parent;

    public static Vector2 viewerPosition;
    public static Vector2 viewerPositionOld;
    private Vector2 realMapSize;

    private int chunkSize;
    [HideInInspector] public int chunkVisibleInViewDst;

    public Dictionary<Vector2, TerrainChunk> terrainChunkDictonary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

    private static MapGenerator mapGenerator;
    private static SectorManager sectorManager;

    private void Awake()
    {
        parent = GameObject.Find("MapChunk").transform;
        mapGenerator = FindObjectOfType<MapGenerator>();
        sectorManager = GetComponent<SectorManager>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        sectorManager.GenerateSectors();
    }

    private void UpdateVisibleChunk()
    {
        foreach (TerrainChunk tc in terrainChunkVisibleLastUpdate)
        {
            tc.SetVisible(false, false);
        }

        terrainChunkVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictonary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictonary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictonary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, realMapSize, detailLevels, parent, mapMaterial, hideChunkOnStartup));
                    //sectorManager.AddSector(viewedChunkCoord);
                }
            }
        }
    }

    public void PregenerateMap(Action callback, Vector2 realMapSize)
    {
        viewerPosition = new Vector2(0, 0);
        this.realMapSize = realMapSize;
        UpdateVisibleChunk();
        callback();
    }

    public class TerrainChunk
    {
        Vector2 position;
        public GameObject meshObject;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailsLevels;
        public LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        int mapSize;
        Vector2 realMapSize;

        public TerrainChunk(Vector2 coord, int size, Vector2 realSize, LODInfo[] detailsLevels, Transform parent, Material material, bool hide)
        {
            this.detailsLevels = detailsLevels;
            mapSize = size;
            realMapSize = realSize;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, -9.6f, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshObject.layer = 9;

            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.SetParent(parent);
            meshObject.transform.localScale = Vector3.one * scale;

            SetVisible(!hide, false, true);

            lodMeshes = new LODMesh[detailsLevels.Length];

            for (int i = 0; i < detailsLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailsLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived) return;

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailsLevels.Length - 1; i++)
                {
                    if(viewerDistanceFromNearestEdge > detailsLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if(lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];

                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;

                        if((position.x + mapSize > realMapSize.x + mapSize || position.x - mapSize < -realMapSize.x - mapSize) || (position.y + mapSize > realMapSize.y + mapSize || position.y - mapSize < -realMapSize.y - mapSize))
                        {
                            //Debug.Log("  [INFO:EndlessTerrain] Chunk's " + position + " physics disabled.");
                        }
                        else
                        {
                            meshCollider.sharedMesh = lodMesh.mesh;
                        }
                    }
                    else if(!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                terrainChunkVisibleLastUpdate.Add(this);
            }

            SetVisible(visible, false);
        }

        public void SetVisible(bool visible, bool hideChunk, bool disableChunk = false)
        {
            if (disableChunk)
            {
                meshRenderer.enabled = visible;
            }
            else if(hideChunk)
            {
                meshObject.SetActive(visible);
            }
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    [System.Serializable]
    public class LODMesh
    {
        public Mesh mesh;
        public MeshData meshData;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void OnMeshDataReceived(MeshData meshData)
        {
            this.meshData = meshData;
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived, lod);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}

[System.Serializable]
public class HeightPoint
{
    public Vector3 m_position;
    
    public HeightPoint(Vector3 position)
    {
        m_position = position;
    }

    public float GetHeight()
    {
        return m_position.y;
    }

    public bool CheckPosition(Vector2 postion)
    {
        return postion.x == m_position.x && postion.y == m_position.z;
    }
}