using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public WorldSettings settings;
    public Camera cam;

    private WorldGenerator generator;
    private readonly Dictionary<Vector2Int, Chunk> loaded = new();

    void Awake()
    {
        generator = new WorldGenerator(settings);

        if (cam == null)
            cam = Camera.main; // или FindObjectOfType<Camera>()
    }

    public void LateUpdate()
    {
        if (!cam) return;

        if (cam == null)
        {
            Debug.LogWarning("ChunkManager: cam is NULL");
            return;
        }

        Debug.Log($"ChunkManager tick. Loaded: {loaded.Count}");

        Vector2Int camChunk = WorldToChunkCoord(cam.transform.position);

        // 1) грузим вокруг камеры
        int r = settings.loadRadiusChunks;
        for (int cy = camChunk.y - r; cy <= camChunk.y + r; cy++)
        {
            for (int cx = camChunk.x - r; cx <= camChunk.x + r; cx++)
            {
                var c = new Vector2Int(cx, cy);
                EnsureChunkLoaded(c);
            }
        }

        // 2) выгружаем дальние
        int ur = settings.unloadRadiusChunks;
        var toRemove = new List<Vector2Int>();
        foreach (var kv in loaded)
        {
            var c = kv.Key;
            int dist = Mathf.Max(Mathf.Abs(c.x - camChunk.x), Mathf.Abs(c.y - camChunk.y)); // Chebyshev
            if (dist > ur)
                toRemove.Add(c);
        }

        foreach (var c in toRemove)
        {
            Destroy(loaded[c].gameObject);
            loaded.Remove(c);
        }
    }

    private void EnsureChunkLoaded(Vector2Int coord)
    {
        if (loaded.ContainsKey(coord)) return;

        // создаём чанк
        var go = new GameObject();
        go.transform.SetParent(transform, false);

        var chunk = go.AddComponent<Chunk>();
        chunk.Init(settings, coord);

        // генерим данные и рисуем
        var data = generator.GenerateChunkData(coord);
        chunk.Render(data);

        loaded.Add(coord, chunk);
    }

    private Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        int s = settings.chunkSize;
        int cx = Mathf.FloorToInt(worldPos.x / s);
        int cy = Mathf.FloorToInt(worldPos.y / s);
        return new Vector2Int(cx, cy);
    }
}
