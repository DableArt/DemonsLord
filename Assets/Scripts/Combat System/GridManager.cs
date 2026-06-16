using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    public int width = 8;
    public int height = 12;
    public BiomeType biome = BiomeType.Plains;
    public GridCellData[] cellData;

    public UnityEvent<UnitMoveContext> OnUnitMove;

    public BattleGrid grid { get; private set; }
    public Vector3 GridOrigin => cellsParent != null ? cellsParent.position : Vector3.zero;

    public Color highlightColor = Color.yellow;
    private HashSet<Vector2Int> highlightedCells = new HashSet<Vector2Int>();

    public void HighlightCells(IEnumerable<Vector2Int> cells)
    {
        ClearHighlights();
        foreach (var pt in cells)
        {
            if (cellObjects.TryGetValue(pt, out var go))
            {
                highlightedCells.Add(pt);
                go.GetComponent<SpriteRenderer>().color = highlightColor;
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var pt in highlightedCells)
        {
            if (cellObjects.TryGetValue(pt, out var go))
            {
                var cell = grid.GetCell(pt);
                go.GetComponent<SpriteRenderer>().color = GetTerrainColor(cell.terrain);
            }
        }
        highlightedCells.Clear();
    }

    public List<Vector2Int> GetReachableCells(Vector2Int from, int maxDist, Unit unit)
    {
        var result = new List<Vector2Int>();
        if (grid == null || unit == null) return result;

        var visited = new HashSet<Vector2Int> { from };
        var queue = new Queue<(Vector2Int pos, int dist)>();
        queue.Enqueue((from, 0));

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();
            if (dist >= maxDist) continue;

            for (int i = 0; i < 4; i++)
            {
                var next = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
                if (visited.Contains(next)) continue;
                if (!grid.IsWithinBounds(next)) continue;
                if (!grid.IsPassable(next)) continue;
                if (grid.IsOccupied(next)) continue;

                visited.Add(next);
                result.Add(next);
                queue.Enqueue((next, dist + 1));
            }
        }
        return result;
    }

    [Serializable]
    public class GridCellData
    {
        public Vector2Int point;
        public TerrainType terrain = TerrainType.Normal;
        public int height;
        public bool occupied;
    }

    [Header("Visual")]
    public Transform cellsParent;
    public float cellSize = 1f;
    public Color cellNormalColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
    public Color cellForestColor = new Color(0.2f, 0.7f, 0.2f, 0.5f);
    public Color cellWaterColor = new Color(0.2f, 0.4f, 1f, 0.6f);
    public Color cellMountainColor = new Color(0.5f, 0.35f, 0.2f, 0.6f);
    public Color cellSwampColor = new Color(0.3f, 0.5f, 0.2f, 0.6f);
    public Color cellLavaColor = new Color(1f, 0.3f, 0f, 0.7f);
    public Color cellIceColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    public Color cellSandColor = new Color(1f, 0.9f, 0.5f, 0.5f);
    public Color cellRubbleColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public Color cellMagicFieldColor = new Color(0.6f, 0.2f, 1f, 0.5f);
    public Color cellTrapColor = new Color(1f, 0f, 0f, 0.4f);
    public Color cellObstacleColor = Color.black;

    private GameObject spawnedParent;
    private Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    private Sprite cellSprite;

    private void Start()
    {
        cellSprite = CreateCellSprite();
        InitializeGrid();
    }

    Sprite CreateCellSprite()
    {
        int texSize = 64;
        var tex = new Texture2D(texSize, texSize);
        var colors = new Color[texSize * texSize];
        int border = 3;
        for (int y = 0; y < texSize; y++)
            for (int x = 0; x < texSize; x++)
            {
                bool edge = x < border || x >= texSize - border || y < border || y >= texSize - border;
                colors[y * texSize + x] = edge ? new Color(0, 0, 0, 0.5f) : Color.white;
            }
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), Vector2.zero, texSize);
    }

    private void InitializeGrid()
    {
        var data = cellData?.Select(d =>
            new KeyValuePair<Vector2Int, GridCell>(
                d.point,
                new GridCell(d.terrain, d.height, d.occupied)
            )
        ) ?? Enumerable.Empty<KeyValuePair<Vector2Int, GridCell>>();

        grid = new BattleGrid(width, height, biome, data);
        BuildVisualGrid();
    }

    void BuildVisualGrid()
    {
        ClearVisualGrid();

        Transform parent;
        if (cellsParent != null)
        {
            parent = cellsParent;
        }
        else
        {
            spawnedParent = new GameObject("GridVisual");
            spawnedParent.transform.SetParent(transform, false);
            parent = spawnedParent.transform;
        }

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                var pt = new Vector2Int(x, y);
                var cell = grid.GetCell(pt);
                var go = new GameObject($"Cell_{x}_{y}");
                go.transform.SetParent(parent, false);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = cellSprite;
                sr.color = GetTerrainColor(cell.terrain);
                sr.sortingOrder = -10;

                go.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0);
                go.transform.localScale = Vector3.one * cellSize;

                cellObjects[pt] = go;
            }
        }
    }

    void ClearVisualGrid()
    {
        foreach (var kv in cellObjects)
            if (kv.Value != null) Destroy(kv.Value);
        cellObjects.Clear();
        if (spawnedParent != null)
        {
            Destroy(spawnedParent);
            spawnedParent = null;
        }
    }

    Color GetTerrainColor(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Normal: return cellNormalColor;
            case TerrainType.Forest: return cellForestColor;
            case TerrainType.Water: return cellWaterColor;
            case TerrainType.Mountain: return cellMountainColor;
            case TerrainType.Swamp: return cellSwampColor;
            case TerrainType.Lava: return cellLavaColor;
            case TerrainType.Ice: return cellIceColor;
            case TerrainType.Sand: return cellSandColor;
            case TerrainType.Rubble: return cellRubbleColor;
            case TerrainType.MagicField: return cellMagicFieldColor;
            case TerrainType.Trap: return cellTrapColor;
            case TerrainType.Obstacle: return cellObstacleColor;
            default: return cellNormalColor;
        }
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (grid == null) return true;
        return grid.IsOccupied(cell);
    }

    public bool IsCellPassable(Vector2Int cell)
    {
        if (grid == null) return false;
        return grid.IsPassable(cell);
    }

    public Vector2Int[] GetOccupiedCells(Unit unit)
    {
        if (unit.size == UnitSize.Large)
        {
            return new Vector2Int[]
            {
                unit.gridPosition,
                unit.gridPosition + Vector2Int.right
            };
        }
        return new Vector2Int[] { unit.gridPosition };
    }

    public bool CanOccupy(Unit unit, Vector2Int pos)
    {
        if (unit.size == UnitSize.Large)
        {
            Vector2Int second = pos + Vector2Int.right;
            return grid.IsWithinBounds(pos) && grid.IsWithinBounds(second)
                && !grid.IsOccupied(pos) && !grid.IsOccupied(second);
        }
        return grid.IsWithinBounds(pos) && !grid.IsOccupied(pos);
    }

    public void SetUnitOccupancy(Unit unit, Vector2Int pos, bool occupied)
    {
        Vector2Int[] cells = unit.size == UnitSize.Large
            ? new Vector2Int[] { pos, pos + Vector2Int.right }
            : new Vector2Int[] { pos };

        foreach (var cell in cells)
        {
            if (grid.IsWithinBounds(cell))
            {
                grid.SetOccupied(cell, occupied);
                grid.GetCell(cell).unit = occupied ? unit : null;
            }
        }
    }

    public void PlaceUnit(Unit unit, Vector2Int pos)
    {
        if (grid == null || unit == null) return;
        if (!grid.IsWithinBounds(pos)) return;

        SetUnitOccupancy(unit, pos, true);
        unit.gridPosition = pos;
        unit.transform.position = GridOrigin + new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
    }

    public void RemoveUnitFromGrid(Unit unit)
    {
        if (grid == null || unit == null) return;
        SetUnitOccupancy(unit, unit.gridPosition, false);
    }

    public void MoveUnit(Unit unit, Vector2Int to)
    {
        if (grid == null || unit == null) return;

        var from = unit.gridPosition;
        if (from == to) return;

        if (!CanOccupy(unit, to))
        {
            Debug.LogWarning($"Cannot move unit {unit.unitName} to {to}: occupied or out of bounds.");
            return;
        }

        SetUnitOccupancy(unit, from, false);
        SetUnitOccupancy(unit, to, true);
        unit.gridPosition = to;
        unit.transform.position = GridOrigin + new Vector3(to.x + 0.5f, to.y + 0.5f, 0);

        OnUnitMove?.Invoke(new UnitMoveContext(unit, from, to));
    }

    public void ResetGrid()
    {
        InitializeGrid();
    }

    void OnDestroy()
    {
        ClearVisualGrid();
    }
}
