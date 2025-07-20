using System.Collections.Generic;
using UnityEngine;

public static class PathFindingHelper
{
    public static Path FindPath(Grid grid, Vector2Int start, Vector2Int goal)
    {
        if (GetCell(grid, start) == null || GetCell(grid, goal) == null)
            return new Path();

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
                if (cell == null || cell.Occupied) continue;

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

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

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

    private static Cell GetCell(Grid grid, Vector2Int pos)
    {
        int index = pos.y * grid.width + pos.x;
        if (index < 0 || index >= grid.Cells.Length) return null;
        var cell = grid.Cells[index];
        if (cell == null || cell.Point != pos) return null;
        return cell;
    }

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
}
