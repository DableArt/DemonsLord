using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float cellSize = 1f;
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

    private void Awake()
    {
        grid = new Grid(width, height, Cells.Select(item => new KeyValuePair<Vector2Int, Cell>(item.pont, item.Cell)));
    }

    public Vector3 GetWorldPosition(Vector2Int cell) =>
        transform.position + new Vector3(cell.x * cellSize + cellSize * 0.5f, cell.y * cellSize + cellSize * 0.5f, 0f);

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - transform.position;
        return new Vector2Int(Mathf.FloorToInt(local.x / cellSize), Mathf.FloorToInt(local.y / cellSize));
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (!grid.IsWithinBounds(cell)) return true;

        if (grid.Cells.TryGetValue(cell, out var gridCell))
        {
            return gridCell.Occupied;
        }
        return false;
    }

    public void SetCellOccupied(Vector2Int cell, bool value)
    {
        if (!grid.IsWithinBounds(cell)) return;

        if (!grid.Cells.TryGetValue(cell, out var gridCell))
        {
            gridCell = new Cell();
            grid.Cells[cell] = gridCell;
        }
        gridCell.Occupied = value;
    }

    public void MoveUnit(Unit unit, Vector2Int to)
    {
        var from = unit.gridPosition;
        if (from == to || IsCellOccupied(to)) return;
        SetCellOccupied(from, false);
        SetCellOccupied(to, true);
        unit.gridPosition = to;
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
