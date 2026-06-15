using UnityEngine;
using UnityEngine.Tilemaps;
public class Chunk : MonoBehaviour
{
        public Vector2Int Coord { get; private set; }

    private WorldSettings settings;

    private Tilemap groundMap;
    private Tilemap waterMap;

    private readonly System.Collections.Generic.List<GameObject> spawnedTrees =
        new System.Collections.Generic.List<GameObject>();

    public void Init(WorldSettings settings, Vector2Int coord)
    {
        this.settings = settings;
        this.Coord = coord;

        name = $"Chunk_{coord.x}_{coord.y}";
        CreateTilemaps();
        PositionChunk();
    }

    private void CreateTilemaps()
    {
        // Grid is required so Tilemap children can calculate tile positions and render correctly
        gameObject.AddComponent<UnityEngine.Grid>();

        // Ground tilemap
        groundMap = CreateTilemapChild("Ground", addCollider: false);

        // Water tilemap + collider (water tile Collider Type must be Sprite or Grid in the tile asset)
        waterMap = CreateTilemapChild("Water", addCollider: true);
    }

    private Tilemap CreateTilemapChild(string n, bool addCollider)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);

        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = (n == "Water") ? 1 : 0;

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

    private void PositionChunk()
    {
        int s = settings.chunkSize;
        transform.position = new Vector3(Coord.x * s, Coord.y * s, 0f);
    }

    public void Render(TileType[,] data)
    {
        // Destroy previously spawned trees for this chunk
        foreach (var tree in spawnedTrees)
            if (tree != null) Destroy(tree);
        spawnedTrees.Clear();

        groundMap.ClearAllTiles();
        waterMap.ClearAllTiles();

        int s = settings.chunkSize;
        Vector3 chunkWorldPos = transform.position;

        // ��������� ��������� ���������� ������ (0..s-1)
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                var pos = new Vector3Int(x, y, 0);

                if (data[x, y] == TileType.Water)
                {
                    waterMap.SetTile(pos, settings.waterTile);
                }
                else
                {
                    groundMap.SetTile(pos, settings.groundTile);

                    // Randomly spawn a tree on this Ground tile
                    if (settings.treePrefab != null && Random.value < settings.treeSpawnChance)
                    {
                        float wx = chunkWorldPos.x + x + 0.5f;
                        float wy = chunkWorldPos.y + y + 0.5f;
                        var treeObj = Instantiate(
                            settings.treePrefab,
                            new Vector3(wx, wy, 0f),
                            Quaternion.identity,
                            transform);
                        spawnedTrees.Add(treeObj);
                    }
                }
            }
        }
    }
}
