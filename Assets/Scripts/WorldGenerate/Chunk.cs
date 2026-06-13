using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public Vector2Int Coord { get; private set; }

    private WorldSettings settings;

    // Sorting order 0 — вода (рисуется ниже всего)
    private Tilemap waterMap;
    // Sorting order 1 — земля/берег (рисуется поверх воды)
    private Tilemap groundMap;

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
        // Grid обязателен для корректного расчёта позиций тайлов
        gameObject.AddComponent<UnityEngine.Grid>();

        // Сначала создаём слой воды (sortingOrder = 0) — он под землёй
        waterMap = CreateTilemapChild("Water", sortingOrder: 0, addCollider: true);

        // Затем слой земли (sortingOrder = 1) — поверх воды
        groundMap = CreateTilemapChild("Ground", sortingOrder: 1, addCollider: false);
    }

    private Tilemap CreateTilemapChild(string n, int sortingOrder, bool addCollider)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);

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

    private void PositionChunk()
    {
        int s = settings.chunkSize;
        transform.position = new Vector3(Coord.x * s, Coord.y * s, 0f);
    }

    /// <summary>
    /// Проверяет, есть ли среди 8-связных соседей клетки (x, y) хоть одна вода.
    /// Клетки за границей чанка считаются землёй (консервативный подход).
    /// </summary>
    private static bool HasWaterNeighbor(TileType[,] data, int x, int y)
    {
        int w = data.GetLength(0);
        int h = data.GetLength(1);
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                if (data[nx, ny] == TileType.Water) return true;
            }
        }
        return false;
    }

    public void Render(TileType[,] data)
    {
        // Удаляем деревья предыдущего рендера
        foreach (var tree in spawnedTrees)
            if (tree != null) Destroy(tree);
        spawnedTrees.Clear();

        groundMap.ClearAllTiles();
        waterMap.ClearAllTiles();

        int s = settings.chunkSize;
        Vector3 chunkWorldPos = transform.position;

        TileBase waterTile  = settings.GetWaterTile();
        TileBase groundTile = settings.GetGroundTile();
        TileBase shoreTile  = settings.GetShoreTile();

        // Обходим локальные координаты чанка (0..s-1)
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                var pos = new Vector3Int(x, y, 0);

                if (data[x, y] == TileType.Water)
                {
                    // Водная клетка — только на слое воды
                    waterMap.SetTile(pos, waterTile);
                }
                else
                {
                    // Земляная клетка — проверяем, граничит ли с водой
                    bool isShore = HasWaterNeighbor(data, x, y);

                    if (isShore)
                    {
                        // Береговая клетка: на слое воды кладём анимированную воду,
                        // чтобы она просвечивала через полупрозрачные края берегового тайла.
                        // На слое земли — береговой/переходный тайл (трава→камни).
                        waterMap.SetTile(pos, waterTile);
                        groundMap.SetTile(pos, shoreTile);
                    }
                    else
                    {
                        // Обычная земля вдали от воды
                        groundMap.SetTile(pos, groundTile);

                        // Деревья спавним только на обычной земле (не на берегу)
                        if (settings.treePrefab != null
                            && Random.value < settings.treeSpawnChance)
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
}
