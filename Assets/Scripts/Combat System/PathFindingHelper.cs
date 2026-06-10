using System.Collections.Generic;
using UnityEngine;

public static class PathFindingHelper
{
    public static Path FindPath(BattleGrid grid, Vector2Int start, Vector2Int goal, Unit unit = null)
    {
        if (!grid.IsWithinBounds(start) || !grid.IsWithinBounds(goal))
            return new Path();

        if (!grid.IsPassable(goal))
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

            foreach (var neighbor in grid.GetNeighbors(current))
            {
                if (!grid.IsPassable(neighbor) && neighbor != goal)
                    continue;

                int moveCost = grid.GetMovementCost(neighbor);

                if (unit != null && unit.habitatType == UnitHabitatType.Air)
                    moveCost = 1;

                int tentativeG = gScore[current] + moveCost;
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
        return new Path();
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
