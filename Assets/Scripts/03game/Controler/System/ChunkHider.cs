using System.Collections.Generic;
using UnityEngine;

public class ChunkHider : MonoBehaviour
{
    [SerializeField] private Vector2 range;
    [SerializeField] private int border;

    private EndlessTerrain endlessTerrain;
    private MapGenerator mapGenerator;

    private Transform player;
    private Camera camera;

    private Vector3 previousPlayerPosition;
    private Quaternion previousPlayerRotation;

    private List<Vector2> visibleChunks;

    private void Start()
    {
        endlessTerrain = GetComponent<EndlessTerrain>();
        mapGenerator = GetComponent<MapGenerator>();

        player = GameObject.Find("Player").transform;
        camera = player.Find("Main Camera").GetComponent<Camera>();

        visibleChunks = new List<Vector2>();
        Invoke("Initialize", .1f);
    }

    private void Initialize()
    {
        ResolvingDeltas(GetVisibleChunk());
    }

    private void Update()
    {
        if (previousPlayerPosition == player.position && previousPlayerRotation == player.rotation)
            return;

        previousPlayerPosition = player.position;
        previousPlayerRotation = player.rotation;

        ResolvingDeltas(GetVisibleChunk());
    }

    private List<Vector2> GetVisibleChunk()
    {
        //Vector2 topLeftPosition = GetTopLeftPosition();
        //Vector2 topChunkPosition = (topLeftPosition / mapGenerator.chunkSize());

        Vector3 playerPositionInChunk = player.position / mapGenerator.chunkSize();

        Vector2 from = new Vector2();
        from.x = playerPositionInChunk.x - range.x;
        from.y = playerPositionInChunk.z + range.y;

        Vector2 to = new Vector2();
        to.x = playerPositionInChunk.x + range.x;
        to.y = playerPositionInChunk.z - 1;

        from = new Vector2((int)from.x - border, (int)from.y + border);
        to = new Vector2((int)to.x + border, (int)to.y - border);

        //Debug.Log("From: " + from + ", To: " + to);

        List<Vector2> visibleChunks = new List<Vector2>();

        for (int y = (int)from.y; y > to.y; y--)
        {
            for (int x = (int)from.x; x < to.x; x++)
            {
                visibleChunks.Add(new Vector2(x, y));
                //Debug.Log("X: " + x + ", Y: " + y);
            }
        }

        return visibleChunks;
    }

    private Vector2 GetTopLeftPosition()
    {
        float a = player.position.y;
        float b = Mathf.Abs(a / Mathf.Cos((camera.transform.rotation.x - camera.fieldOfView / 2) * Mathf.PI / 180));
        float c = Mathf.Sqrt(Mathf.Pow(b, 2) - Mathf.Pow(a, 2));
        float d = c / Mathf.Cos(45 * Mathf.PI / 180);

        Vector2 position = new Vector2();
        position.x = (int)(player.position.x + d);
        position.y = (int)(player.position.z + c);

        return position;
    }

    private void ResolvingDeltas(List<Vector2> chunkPositions)
    {
        List<Vector2> currentlyVisibleChunks = new List<Vector2>();

        foreach(Vector2 v in chunkPositions)
        {
            if (visibleChunks.Contains(v))
            {
                currentlyVisibleChunks.Add(v);
            }
            else
            {
                try { endlessTerrain.terrainChunkDictonary[v].SetVisible(true, false, true); }
                catch { }
                currentlyVisibleChunks.Add(v);
            }
        }

        foreach(Vector2 v in visibleChunks)
        {
            if (!chunkPositions.Contains(v))
            {
                try { endlessTerrain.terrainChunkDictonary[v].SetVisible(false, false, true); }
                catch { }
            }
        }

        visibleChunks = currentlyVisibleChunks;
    }
}
