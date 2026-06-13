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
    /// Возвращает true, если клетка (x, y) является водой.
    /// Клетки за границей чанка считаются землёй (консервативный подход).
    /// </summary>
    private static bool IsWater(TileType[,] data, int x, int y)
    {
        if (x < 0 || x >= data.GetLength(0) || y < 0 || y >= data.GetLength(1))
            return false;
        return data[x, y] == TileType.Water;
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
                    // Вычисляем водных соседей по всем 8 направлениям
                    bool wTop         = IsWater(data, x,     y + 1);
                    bool wBottom      = IsWater(data, x,     y - 1);
                    bool wLeft        = IsWater(data, x - 1, y    );
                    bool wRight       = IsWater(data, x + 1, y    );
                    bool wTopLeft     = IsWater(data, x - 1, y + 1);
                    bool wTopRight    = IsWater(data, x + 1, y + 1);
                    bool wBottomLeft  = IsWater(data, x - 1, y - 1);
                    bool wBottomRight = IsWater(data, x + 1, y - 1);

                    bool isShore = wTop || wBottom || wLeft || wRight
                                || wTopLeft || wTopRight || wBottomLeft || wBottomRight;

                    if (isShore)
                    {
                        // Береговая клетка: под берегом кладём анимированную воду,
                        // чтобы она просвечивала через полупрозрачные края берегового тайла.
                        // На слое земли — направленный береговой тайл.
                        TileBase shoreResult = settings.shoreTiles.Resolve(
                            wTop,    wBottom,
                            wLeft,   wRight,
                            wTopLeft,    wTopRight,
                            wBottomLeft, wBottomRight);

                        if (shoreResult == null)
                            shoreResult = groundTile;

                        waterMap.SetTile(pos, waterTile);
                        groundMap.SetTile(pos, shoreResult);
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
