using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EndlessTerrain : MonoBehaviour
{
    [Header("Map properties")]
    [SerializeField] private Vector2 chunkCount;
    [SerializeField] private Material mapMaterial;
    [SerializeField] private LODInfo[] detailLevels;

    [Header("Others")]
    [SerializeField] private bool hideChunkOnStartup;

    public static float maxViewDst;
    const float scale = 1f;

    private Transform parent;
    public static Transform player;

    public static Vector2 viewerPosition;
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
        player = GameObject.Find("Player").transform;
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
            tc.SetVisible(true, true);
        }

        terrainChunkVisibleLastUpdate.Clear();

        for (int y = (int)(-chunkCount.y / 2); y <= (int)(chunkCount.y / 2); y++)
        {
            for (int x = (int)(-chunkCount.x / 2); x <= (int)(chunkCount.x / 2); x++)
            {
                Vector2 viewedChunkCoord = new Vector2(x, y);

                if (terrainChunkDictonary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictonary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictonary.Add(
                        viewedChunkCoord,
                        new TerrainChunk(viewedChunkCoord, chunkSize, realMapSize, detailLevels, parent, mapMaterial, hideChunkOnStartup));
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

            meshObject = new GameObject(coord.ToString());
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

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(player.position));
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
                        Destroy(meshCollider);
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
            SetVisible(true, false);
        }

        public void SetVisible(bool visible, bool hideChunk, bool disableChunk = false)
        {
            if (disableChunk)
            {
                meshRenderer.enabled = visible;
            }
            else if(hideChunk)
            {
                try
                {
                    meshObject.SetActive(visible);
                }
                catch {}
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