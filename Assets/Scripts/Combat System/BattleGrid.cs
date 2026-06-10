using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleGrid
{
    public int width = 8;
    public int height = 12;
    public BiomeType biome = BiomeType.Plains;
    public Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();

    public BattleGrid() { }

    public BattleGrid(int width, int height, BiomeType biome, IEnumerable<KeyValuePair<Vector2Int, GridCell>> cellData)
    {
        this.width = width;
        this.height = height;
        this.biome = biome;
        cells = new Dictionary<Vector2Int, GridCell>();
        foreach (var kv in cellData)
        {
            cells[kv.Key] = kv.Value;
        }
    }

    public GridCell GetCell(Vector2Int point)
    {
        if (!IsWithinBounds(point))
            throw new ArgumentOutOfRangeException(nameof(point), $"Point {point} is out of grid bounds ({width}x{height}).");
        if (cells.TryGetValue(point, out var cell))
            return cell;
        var newCell = new GridCell();
        cells[point] = newCell;
        return newCell;
    }

    public bool IsWithinBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < width
            && point.y >= 0 && point.y < height;
    }

    public bool IsPassable(Vector2Int point)
    {
        if (!IsWithinBounds(point)) return false;
        return GetCell(point).IsPassable;
    }

    public bool IsOccupied(Vector2Int point)
    {
        if (!IsWithinBounds(point)) return true;
        return GetCell(point).occupied;
    }

    public void SetOccupied(Vector2Int point, bool value)
    {
        if (!IsWithinBounds(point)) return;
        GetCell(point).occupied = value;
    }

    public int GetMovementCost(Vector2Int point)
    {
        if (!IsWithinBounds(point)) return int.MaxValue;
        return GetCell(point).MovementCost;
    }

    public int GetHeight(Vector2Int point)
    {
        if (!IsWithinBounds(point)) return 0;
        return GetCell(point).height;
    }

    public List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var result = new List<Vector2Int>();
        var dirs = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        foreach (var d in dirs)
        {
            var n = pos + d;
            if (IsWithinBounds(n))
                result.Add(n);
        }
        return result;
    }

    public void GenerateDefaultLayout()
    {
        cells.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pt = new Vector2Int(x, y);
                cells[pt] = new GridCell();
            }
        }
    }
}
