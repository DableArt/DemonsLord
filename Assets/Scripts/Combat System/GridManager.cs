using System;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public Grid grid;
    public UnityEvent<UnitMoveContext> OnUnitMove;

    public void InitGrid(GridSO gridSO)
    {
        grid = new Grid(gridSO.width, gridSO.height, gridSO.Cells);
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= grid.width || cell.y < 0 || cell.y >= grid.height)
            return true;

        int index = cell.y * grid.width + cell.x;
        return grid.Cells[index].Occupied;
    }

    public void SetCellOccupied(Vector2Int cell, bool value)
    {
        if (cell.x < 0 || cell.x >= grid.width || cell.y < 0 || cell.y >= grid.height)
            return;

        int index = cell.y * grid.width + cell.x;
        grid.Cells[index].Occupied = value;
    }

    public bool[,] GetOccupiedGrid()
    {
        bool[,] occupied = new bool[grid.width, grid.height];
        for (int y = 0; y < grid.height; y++)
        {
            for (int x = 0; x < grid.width; x++)
            {
                int idx = y * grid.width + x;
                occupied[x, y] = grid.Cells[idx].Occupied;
            }
        }
        return occupied;
    }

    public void MoveUnit(Unit unit, Vector2Int to)
    {
        var from = unit.gridPosition;
        if (from == to || IsCellOccupied(to)) return;
        SetCellOccupied(from, false);
        SetCellOccupied(to, true);
        unit.SetPosition(to);

        OnUnitMove?.Invoke(new UnitMoveContext(unit, from, to));
    }

    public bool TryMove(Unit unit, Vector2Int to)
    {
        if (IsCellOccupied(to) || unit.gridPosition == to)
            return false;
        MoveUnit(unit, to);
        return true;
    }
}
