using System.Collections.Generic;
using System.Linq;
using UnityEngine; // или напишите свою структуру Vector2Int

public static class PathFindingHelper
{
    public static Path FindPath(Grid grid, Vector2Int start, Vector2Int goal)
    {
        var openSet = new PriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();
        var fScore = new Dictionary<Vector2Int, int>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current, grid))
            {
                var cell = GetCell(grid, neighbor);
                if (cell == null || cell.Occupied) continue; // blocked or out of bounds

                int tentativeG = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return new Path(); // no path found
    }

    // Манхэттенская эвристика
    private static int Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    // Восстановление пути
    private static Path ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return new Path(totalPath);
    }

    // Получение соседей (по 4 направлениям)
    private static IEnumerable<Vector2Int> GetNeighbors(Vector2Int current, Grid grid)
    {
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var neighbor = current + d;
            if (neighbor.x >= 0 && neighbor.x < grid.width && neighbor.y >= 0 && neighbor.y < grid.height)
                yield return neighbor;
        }
    }

    // Поиск клетки по координатам
    private static Cell GetCell(Grid grid, Vector2Int pos)
    {
        int index = pos.y * grid.width + pos.x;
        if (index < 0 || index >= grid.Cells.Length) return null;
        var cell = grid.Cells[index];
        if (cell == null || cell.Point != pos) return null;
        return cell;
    }
}

public struct Path
{
    public readonly List<Vector2Int> Points;

    public Path(IEnumerable<Vector2Int> points)
    {
        Points = points != null ? new List<Vector2Int>(points) : new List<Vector2Int>();
    }

    /// <summary>Путь не пустой?</summary>
    public bool IsValid => Points != null && Points.Count > 0;

    /// <summary>Длина пути (количество точек)</summary>
    public int Length => Points?.Count ?? 0;

    /// <summary>Начальная точка пути (или default, если путь пустой)</summary>
    public Vector2Int Start => (Points != null && Points.Count > 0) ? Points[0] : default;

    /// <summary>Конечная точка пути (или default, если путь пустой)</summary>
    public Vector2Int End => (Points != null && Points.Count > 0) ? Points[Points.Count - 1] : default;

    /// <summary>Доступ к точке по индексу</summary>
    public Vector2Int this[int idx] => Points[idx];
}


