using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerate
{
    public class ChunkManager : MonoBehaviour
    {
        public WorldSettings settings;
        public Camera cam;

        private WorldGenerator generator;
        private DualGridTileHelper tileHelper;
        private readonly Dictionary<Vector2Int, Chunk> loaded = new();

        void Awake()
        {
            generator = new WorldGenerator(settings);
            tileHelper = new DualGridTileHelper();
            tileHelper.Init(settings.dualGridTileSets);

            if (cam == null)
                cam = Camera.main; // ��� FindObjectOfType<Camera>()
        }

        public void LateUpdate()
        {
            if (!cam) return;

            if (cam == null)
            {
                Debug.LogWarning("ChunkManager: cam is NULL");
                return;
            }

            Vector2Int camChunk = WorldToChunkCoord(cam.transform.position);
            bool chunksChanged = false;

            // 1) ������ ������ ������
            int r = settings.loadRadiusChunks;
            for (int cy = camChunk.y - r; cy <= camChunk.y + r; cy++)
            {
                for (int cx = camChunk.x - r; cx <= camChunk.x + r; cx++)
                {
                    var c = new Vector2Int(cx, cy);
                    if (EnsureChunkLoaded(c))
                    {
                        chunksChanged = true;
                    }
                }
            }

            // 2) ��������� �������
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
                chunksChanged = true;
            }

            if (chunksChanged)
            {
                UpdateMinimapBounds();
            }
        }

        private bool EnsureChunkLoaded(Vector2Int coord)
        {
            if (loaded.ContainsKey(coord)) return false;

            // ������ ����
            var go = new GameObject();
            go.transform.SetParent(transform, false);

            var chunk = go.AddComponent<Chunk>();
            chunk.Init(settings, tileHelper, coord);

            // ������� ������ � ������
            var data = generator.GenerateChunkData(coord);
            chunk.Render(data, generator);

            if (MinimapController.Instance != null)
                MinimapController.Instance.RegisterChunk(coord, settings.chunkSize, data);

            loaded.Add(coord, chunk);
            return true;
        }

        private void UpdateMinimapBounds()
        {
            if (MinimapController.Instance == null || loaded.Count == 0)
            {
                return;
            }

            bool initialized = false;
            Vector2Int min = Vector2Int.zero;
            Vector2Int max = Vector2Int.zero;

            foreach (var coord in loaded.Keys)
            {
                if (!initialized)
                {
                    min = coord;
                    max = coord;
                    initialized = true;
                    continue;
                }

                min.x = Mathf.Min(min.x, coord.x);
                min.y = Mathf.Min(min.y, coord.y);
                max.x = Mathf.Max(max.x, coord.x);
                max.y = Mathf.Max(max.y, coord.y);
            }

            int chunkSize = settings.chunkSize;
            Vector2 worldMin = new Vector2(min.x * chunkSize, min.y * chunkSize);
            Vector2 worldMax = new Vector2((max.x + 1) * chunkSize, (max.y + 1) * chunkSize);
            MinimapController.Instance.SetWorldBounds(worldMin, worldMax);
        }

        private Vector2Int WorldToChunkCoord(Vector3 worldPos)
        {
            int s = settings.chunkSize;
            int cx = Mathf.FloorToInt(worldPos.x / s);
            int cy = Mathf.FloorToInt(worldPos.y / s);
            return new Vector2Int(cx, cy);
        }
    }
}
