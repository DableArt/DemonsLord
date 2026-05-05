using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public CellWraper[] Cells;

    public UnityEvent<UnitMoveContext> OnUnitMove;

    public Grid grid;

    [Serializable]
    public class CellWraper
    {
        public Vector2Int pont;
        public Cell Cell;
    }

    private void OnValidate()
    {
        // валидация
    }


    public bool IsCellOccupied(Vector2Int cell)
    {
        if(grid.IsWithinBounds(cell)) return true;

        if (grid.Cells.TryGetValue(cell, out var gridCell))
        {
            return gridCell.Occupied;
        }
        else
        {
            return false;
        }
    }

    public void SetCellOccupied(Vector2Int cell, bool value)
    {
        if (grid.IsWithinBounds(cell))
            return;

        if(grid.Cells.TryGetValue(cell, out var gridCell))
        {
            gridCell.Occupied = value;
        }
    }

    public void MoveUnit(Unit unit, Vector2Int to)
    {
        var from = unit.gridPosition;
        if (from == to || IsCellOccupied(to)) return;
        SetCellOccupied(from, false);
        SetCellOccupied(to, true);

        OnUnitMove?.Invoke(new UnitMoveContext(unit, from, to));
    }

    public bool TryMove(Unit unit, Vector2Int to)
    {
        if (IsCellOccupied(to) || unit.gridPosition == to)
            return false;
        MoveUnit(unit, to);
        return true;
    }

    private void Start()
    {
        grid = new(width, height, Cells.Select(item => new KeyValuePair<Vector2Int, Cell>(item.pont, item.Cell)));
    }
}
