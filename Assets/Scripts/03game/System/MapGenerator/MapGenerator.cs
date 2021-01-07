using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    private enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap }

    [Header("Map parameters")]
    public Vector2 realMapSize;
    [SerializeField] [Range(0, 6)] private int editorPreviewLOD;
    [SerializeField] private bool useFlatShading;
    [SerializeField] private TerrainType[] regions;

    [Header("Generation parameters")]
    [SerializeField] private DrawMode drawMode;
    [SerializeField] private Noise.NormalizeMode normalizeMode;
    public int seed;
    [SerializeField] private float meshHeightMultiplier;
    [SerializeField] private AnimationCurve meshHeightCurve;
    [SerializeField] private float noiseScale;
    [SerializeField] private int octaves;
    [SerializeField] [Range(0, 1)] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private Vector2 offset;
    [SerializeField] private bool useFalloff;

    [Header("Others")]
    public bool autoUpdate;
    public MeshData meshData;

    static MapGenerator instance;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private float[,] falloffMap;
    private EndlessTerrain endlessTerrain;

    private void Awake()
    {
        if (useFalloff) falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        endlessTerrain = GetComponent<EndlessTerrain>();
    }

    public int chunkSize()
    {
        return mapChunkSize;
    }

    public static int mapChunkSize
    {
        get
        {
            return 95;
        }
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        try
        {
            if (drawMode == DrawMode.NoiseMap)
            { 
                display.DrawTexture(TextureGenerator.TextureFromNoiseMap(mapData.heightMap));
            } 
            else if (drawMode == DrawMode.ColorMap)
            { 
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD, useFlatShading);
                display.DrawMesh(meshData, TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            }
            else if(drawMode == DrawMode.FalloffMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromNoiseMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            }
        }
        catch (Exception e)
        {
            Debug.Log("[INFO:MapGenerator] Can't generate terrain! " + e);
        }
    }

    public void PregenerateMap(int seed, Action callback)
    {
        this.seed = seed;
        endlessTerrain.PregenerateMap(callback, realMapSize);
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    public void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback, int lod)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callback, lod);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback, int lod)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod, useFlatShading);

        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                try { threadInfo.callback(threadInfo.parameter); } catch { }
            }
        }
    }

    private MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]) ;
                }
                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    struct MapThreadInfo
    {
        public readonly Action callback;

        public MapThreadInfo(Action callback)
        {
            this.callback = callback;
        }
    }

    /*public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(), new Vector3(realMapSize.x * 2, 0, realMapSize.y * 2));
    }*/
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}