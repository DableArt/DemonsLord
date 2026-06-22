using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldGenerate
{
    public class Chunk : MonoBehaviour
    {
        public Vector2Int Coord { get; private set; }

        private WorldSettings settings;
        private DualGridTileHelper tileHelper;

        //Карта коллизий воды
        private Tilemap waterCollisionMap;
        
        // Sorting order 0 — вода (рисуется ниже всего)
        private Tilemap waterMap;
        // Sorting order 1 — берег (рисуется поверх воды)
        private Tilemap shoreMap;
        // Sorting order 2 — земля (рисуется поверх воды и берега)
        private Tilemap groundMap;

        private readonly System.Collections.Generic.List<GameObject> spawnedTrees =
            new System.Collections.Generic.List<GameObject>();

        public void Init(WorldSettings settings, DualGridTileHelper tileHelper, Vector2Int coord)
        {
            this.settings = settings;
            this.Coord = coord;
            this.tileHelper = tileHelper;

            name = $"Chunk_{coord.x}_{coord.y}";
            CreateTilemaps();
            PositionChunk();
        }

        private void CreateTilemaps()
        {
            // Grid обязателен для корректного расчёта позиций тайлов
            gameObject.AddComponent<UnityEngine.Grid>();

            // Сначала создаём слой воды (sortingOrder = 0) — он под землёй
            waterMap = CreateTilemapChild("Water", sortingOrder: 0, addCollider: false);

            // Береговая линия (sortingOrder = 1) над водой, но под землёй
            shoreMap = CreateTilemapChild("Shore", sortingOrder: 1, addCollider: false); 

            // Затем слой земли (sortingOrder = 2) — поверх воды и береговой линии
            groundMap = CreateTilemapChild("Ground", sortingOrder: 2, addCollider: false);

            waterCollisionMap = CreateCollisionTilemap();
        }

        private Tilemap CreateTilemapChild(string n, int sortingOrder, bool addCollider)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;

            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            if (addCollider)
            {
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;

                var col = go.AddComponent<TilemapCollider2D>();
                col.compositeOperation = Collider2D.CompositeOperation.Merge;

                var comp = go.AddComponent<CompositeCollider2D>();
                comp.geometryType = CompositeCollider2D.GeometryType.Outlines;
            }

            return tilemap;
        }
        
        private Tilemap CreateCollisionTilemap()
        {
            var go = new GameObject("WaterCollision");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;

            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.enabled = false;                              // скрыт

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var col = go.AddComponent<TilemapCollider2D>();
            col.compositeOperation = Collider2D.CompositeOperation.Merge;

            var comp = go.AddComponent<CompositeCollider2D>();
            comp.geometryType = CompositeCollider2D.GeometryType.Outlines;

            return tilemap;
        }

        private void PositionChunk()
        {
            int s = settings.chunkSize;
            transform.position = new Vector3(Coord.x * s, Coord.y * s, 0f);
        }

        public void Render(TerrainType[,] data, WorldGenerator generator)
        {
            ClearAll();

            int s = settings.chunkSize;
            Vector3 chunkWorldPos = transform.position;

            for (int ry = 0; ry < s; ry++)
            {
                for (int rx = 0; rx < s; rx++)
                {
                    var bl = data[rx,   ry];   // bottom-left
                    var br = GetCorner(data, rx + 1, ry, generator);   // bottom-right
                    var tl = GetCorner(data, rx, ry + 1, generator); // top-left
                    var tr = GetCorner(data, rx + 1, ry + 1, generator); // top-right

                    var entries = tileHelper.GetEntries(bl, br, tl, tr);
                    var pos = new Vector3Int(rx, ry, 0);

                    if (entries.TryGetValue(TerrainType.Water, out var wTile))
                        waterMap.SetTile(pos, wTile);

                    if (entries.TryGetValue(TerrainType.Shore, out var shTile))
                        shoreMap.SetTile(pos, shTile);

                    if (entries.TryGetValue(TerrainType.Ground, out var gTile))
                    {
                        groundMap.SetTile(pos, gTile);
                        TrySpawnTree(data, chunkWorldPos, rx, ry, generator);
                    }

                    if (bl == TerrainType.Water &&  br == TerrainType.Water && tl ==TerrainType.Water && tr == TerrainType.Water)
                        waterCollisionMap.SetTile(pos, tileHelper.WaterCollisionTile);
                }
            }
        }
        
        private TerrainType GetCorner(TerrainType[,] data, int x, int y, WorldGenerator generator)
        {
            int s = settings.chunkSize;
            if (x < s && y < s)
                return data[x, y];

            float wx = Coord.x * s + x;
            float wy = Coord.y * s + y;
            return generator.GetTerrainType(new Vector3(wx, wy, 0));
        }

        private void TrySpawnTree(TerrainType[,] data, Vector3 chunkWorldPos, int x, int y, WorldGenerator generator)
        {
            if (!IsAllGroundAround(data, x, y, generator))
                return;
            
            if (settings.treePrefab != null && Random.value < settings.treeSpawnChance)
            {
                float wx = Coord.x * settings.chunkSize + x + 0.5f;
                float wy = Coord.y * settings.chunkSize + y + 0.5f;
                var treeObj = Instantiate(
                    settings.treePrefab,
                    new Vector3(wx, wy, 0f),
                    Quaternion.identity,
                    transform);
                spawnedTrees.Add(treeObj);
            }
        }
        
        private bool IsAllGroundAround(TerrainType[,] data, int x, int y, WorldGenerator generator)
        {
            return IsGroundAt(data, x, y + 1, generator)
                   && IsGroundAt(data, x, y - 1, generator)
                   && IsGroundAt(data, x - 1, y, generator)
                   && IsGroundAt(data, x + 1, y, generator);
        }
        
        private bool IsGroundAt(TerrainType[,] data, int x, int y, WorldGenerator generator)
        {
            int s = settings.chunkSize;
            if (x >= 0 && x < s && y >= 0 && y < s)
                return data[x, y] == TerrainType.Ground;

            float wx = Coord.x * s + x;
            float wy = Coord.y * s + y;
            return generator.GetTerrainType(new Vector3(wx, wy, 0)) == TerrainType.Ground;
        }

        private void ClearAll()
        {
            // Удаляем деревья предыдущего рендера
            foreach (var tree in spawnedTrees)
                if (tree != null) Destroy(tree);
            spawnedTrees.Clear();

            groundMap.ClearAllTiles();
            shoreMap.ClearAllTiles();
            waterCollisionMap.ClearAllTiles();
            waterMap.ClearAllTiles();
        }
    }
}
