using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 12;
    public int height = 8;

    [Header("Visual")]
    [Tooltip("Размер одной клетки в юнитах Unity")]
    public float cellSize = 1f;

    public UnityEvent<UnitMoveContext> OnUnitMove;

    public Grid grid { get; private set; }

    // ─── Инициализация ────────────────────────────────────────────────────────

    private void Awake()
    {
        BuildGrid();
    }

    /// <summary>Создаёт сетку автоматически — вручную ничего заполнять не нужно.</summary>
    void BuildGrid()
    {
        var cells = new Dictionary<Vector2Int, Cell>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[new Vector2Int(x, y)] = new Cell();

        grid = new Grid(width, height, cells);
    }

    // ─── Координаты ───────────────────────────────────────────────────────────

    /// <summary>Мировые координаты → индекс клетки.</summary>
    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector2 local = (Vector2)worldPos - (Vector2)transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellSize)
        );
    }

    /// <summary>Индекс клетки → центр клетки в мировых координатах.</summary>
    public Vector3 GetWorldPosition(Vector2Int cell)
    {
        return transform.position + new Vector3(
            cell.x * cellSize + cellSize * 0.5f,
            cell.y * cellSize + cellSize * 0.5f,
            0f
        );
    }

    // ─── Логика занятости ─────────────────────────────────────────────────────

    /// <summary>Занята ли клетка (или вне границ сетки).</summary>
    public bool IsCellOccupied(Vector2Int cell)
    {
        // Клетки вне сетки считаются непроходимыми
        if (!grid.IsWithinBounds(cell)) return true;

        if (grid.Cells.TryGetValue(cell, out var gridCell))
            return gridCell.Occupied;

        return false;
    }

    /// <summary>Установить флаг занятости клетки.</summary>
    public void SetCellOccupied(Vector2Int cell, bool value)
    {
        if (!grid.IsWithinBounds(cell)) return;

        if (grid.Cells.TryGetValue(cell, out var gridCell))
            gridCell.Occupied = value;
    }

    // ─── Перемещение юнита ────────────────────────────────────────────────────

    /// <summary>Переместить юнита на клетку to. Обновляет gridPosition и занятость.</summary>
    public void MoveUnit(Unit unit, Vector2Int to)
    {
        if (unit == null) return;
        var from = unit.gridPosition;
        if (from == to || IsCellOccupied(to)) return;

        SetCellOccupied(from, false);
        SetCellOccupied(to, true);
        unit.gridPosition = to;                          // ← важно: обновляем позицию юнита

        // Физически перемещаем GameObject
        unit.transform.position = GetWorldPosition(to);

        OnUnitMove?.Invoke(new UnitMoveContext(unit, from, to));
    }

    public bool TryMove(Unit unit, Vector2Int to)
    {
        if (IsCellOccupied(to) || unit.gridPosition == to) return false;
        MoveUnit(unit, to);
        return true;
    }
}
