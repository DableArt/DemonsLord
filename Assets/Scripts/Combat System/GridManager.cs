using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 12;
    public BiomeType biome = BiomeType.Plains;
    public GridCellData[] cellData;

    public UnityEvent<UnitMoveContext> OnUnitMove;

    public BattleGrid grid { get; private set; }

    [Serializable]
    public class GridCellData
    {
        public Vector2Int point;
        public TerrainType terrain = TerrainType.Normal;
        public int height;
        public bool occupied;
    }

    private void Start()
    {
        InitializeGrid();
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
        unit.SyncWorldPosition();
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
        unit.SyncWorldPosition();

        OnUnitMove?.Invoke(new UnitMoveContext(unit, from, to));
    }

    public void ResetGrid()
    {
        InitializeGrid();
    }
}
