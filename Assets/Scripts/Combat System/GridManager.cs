using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    public int width => Grid.width;
    public int height => Grid.height;

    public Grid Grid;
    public GridSO gridSO;

    public UnityEvent<UnitMoveContext> OnUnitMove;

    void Awake()
    {

    }

    public void InitGrid(GridSO gridSO)
    {
        //TODO: валидация и инициализация
        throw new NotImplementedException();
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        //TODO: к реализации
        throw new NotImplementedException();
    }

    public void SetCellOccupied(Vector2Int cell, bool value)
    {
        //TODO: к реализации
        throw new NotImplementedException();
    }

    public bool[,] GetOccupiedGrid()
    {
        //TODO: к реализации
        throw new NotImplementedException();
    }

    public void MoveUnit(Unit unit, Vector2Int to)
    {
        var from = unit.gridPosition;

        //TODO: валидация
        //TODO: перещение по сетке

        OnUnitMove?.Invoke(new(unit, from, to));
    }

    public bool TryMove(Unit unit, Vector2Int to)
    {
        //TODO: к реализации
        throw new NotImplementedException();
    }
}

[Serializable]
public class Grid
{
    public int width = 12;
    public int height = 8;
    public List<Cell> OccuptedCell => Cells.Where((h) => h.Occupied).ToList();

    public Cell[] Cells;

    public Grid(int width, int height, Cell[] cells)
    {
        this.width = width;
        this.height = height;
        Cells = cells;
    }
}

[Serializable]
public class Cell
{
    public Vector2Int Point;
    public IList<string> Tags;
    public bool Occupied;

    // Свойства
    public Unit Unit;

    public Cell(Vector2Int point, IList<string> tags, bool occupied)
    {
        Point = point;
        Tags = tags;
        Occupied = occupied;
    }
}

[CreateAssetMenu(fileName = "GrisSo", menuName = "SO")]
public class GridSO : ScriptableObject
{
    public int width = 5;
    public int height = 5;

    public Cell[] Cells;
}

public readonly struct UnitMoveContext
{
    public readonly Unit Unit;
    public readonly Vector2Int From;
    public readonly Vector2Int To;

    public UnitMoveContext(Unit unit, Vector2Int from, Vector2Int to)
    {
        Unit = unit;
        From = from;
        To = to;
    }
}