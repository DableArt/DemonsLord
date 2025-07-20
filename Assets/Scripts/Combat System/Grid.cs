using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid
{
    public int width = 12;
    public int height = 8;
    public List<Vector2Int> OccuptedCell => Cells.Where((h) => h.Value.Occupied).Select(h => h.Key).ToList();
    public Dictionary<Vector2Int, Cell> Cells = new Dictionary<Vector2Int, Cell>();

    public Grid(int width, int height, Dictionary<Vector2Int, Cell> cells)
    {
        this.width = width;
        this.height = height;
        Cells = cells;
    }

    public Grid(int width, int height, IEnumerable<KeyValuePair<Vector2Int, Cell>> cells)
    {
        this.width = width;
        this.height = height;
        
        Cells = new Dictionary<Vector2Int, Cell>();

        foreach (var cell in cells)
        {
            Cells.Add(cell.Key,cell.Value);
        }
    }

    /// <summary>
    /// Получить клетку, если находится в диапазоне сетки: 
    /// если есть в словаре — вернуть ее;
    /// если нет — вернуть клетку по умолчанию;
    /// если точка вне диапазона — выбросить исключение.
    /// </summary>
    public Cell Get(Vector2Int point)
    {
        if (!IsWithinBounds(point))
            throw new ArgumentOutOfRangeException(nameof(point), $"Point {point} is out of grid bounds.");

        if (Cells.TryGetValue(point, out var cell))
            return cell;
        return new Cell(); // клетка по умолчанию
    }

    /// <summary>
    /// Устанавливаем клетку в сетку (если в границах), если вне границ — ошибка.
    /// </summary>
    public Cell Set(Vector2Int point)
    {
        if (!IsWithinBounds(point))
            throw new ArgumentOutOfRangeException(nameof(point), $"Point {point} is out of grid bounds.");

        if (!Cells.TryGetValue(point, out var cell))
        {
            cell = new Cell();
            Cells[point] = cell;
        }
        return cell;
    }

    // Проверка, входит ли точка в диапазон сетки
    public bool IsWithinBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < width
            && point.y >= 0 && point.y < height;
    }
}
