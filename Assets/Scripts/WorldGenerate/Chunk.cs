using UnityEngine;
using UnityEngine.Tilemaps;
public class Chunk : MonoBehaviour
{
        public Vector2Int Coord { get; private set; }

    private WorldSettings settings;

    private Tilemap groundMap;
    private Tilemap waterMap;

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
        groundMap.ClearAllTiles();
        waterMap.ClearAllTiles();

        int s = settings.chunkSize;

        // ��������� ��������� ���������� ������ (0..s-1)
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                var pos = new Vector3Int(x, y, 0);

                if (data[x, y] == TileType.Water)
                    waterMap.SetTile(pos, settings.waterTile);
                else
                    groundMap.SetTile(pos, settings.groundTile);
            }
        }
    }
}
