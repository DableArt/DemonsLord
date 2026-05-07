using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AIAction { Move, Attack, Defend, Wait }

public struct AIDecision
{
    public AIAction action;
    public Unit attackTarget;
    public Vector2Int moveTarget;
}

public static class BattleAI
{
    public static AIDecision Evaluate(Unit unit, List<Unit> allies, List<Unit> enemies, Grid grid, BattleSettings settings)
    {
        int moveRange = unit.agility;
        float hpPercent = unit.maxHP > 0 ? (float)unit.currentHP / unit.maxHP : 1f;

        // Priority 1: Attack if an adjacent enemy exists
        var adjacentEnemies = enemies.Where(e => e.IsAlive && IsAdjacent(unit.gridPosition, e.gridPosition)).ToList();
        if (adjacentEnemies.Count > 0)
        {
            // Hard difficulty: defend when low HP instead of attacking
            if (settings.difficulty == AIDifficulty.Hard && hpPercent < 0.4f)
                return new AIDecision { action = AIAction.Defend };

            // Hard difficulty: target the weakest enemy
            Unit target = settings.difficulty == AIDifficulty.Hard
                ? adjacentEnemies.OrderBy(e => e.currentHP).First()
                : adjacentEnemies[0];

            return new AIDecision { action = AIAction.Attack, attackTarget = target };
        }

        // Priority 2: Retreat when HP is critically low
        if (hpPercent < settings.retreatHpPercent)
        {
            var retreatCell = FindRetreatCell(unit, enemies, grid, moveRange);
            if (retreatCell.HasValue)
                return new AIDecision { action = AIAction.Move, moveTarget = retreatCell.Value };
        }

        // Priority 3: Move towards the nearest enemy (with Easy randomness)
        if (settings.difficulty == AIDifficulty.Easy && Random.value < settings.randomMoveProbabilityEasy)
            return new AIDecision { action = AIAction.Wait };

        var liveEnemies = enemies.Where(e => e.IsAlive).ToList();
        if (liveEnemies.Count > 0)
        {
            var nearest = liveEnemies.OrderBy(e => Manhattan(unit.gridPosition, e.gridPosition)).First();
            var path = PathFindingHelper.FindPath(grid, unit.gridPosition, nearest.gridPosition);
            if (path.IsValid && path.Length > 1)
            {
                Vector2Int nextStep = path[1];
                if (Manhattan(unit.gridPosition, nextStep) <= moveRange)
                    return new AIDecision { action = AIAction.Move, moveTarget = nextStep };
            }
        }

        // Priority 4: Wait
        return new AIDecision { action = AIAction.Wait };
    }

    /// <summary>
    /// BFS reachable cells from <paramref name="start"/> within <paramref name="range"/> steps.
    /// Flying units traverse occupied cells but can only land on free ones.
    /// </summary>
    public static List<Vector2Int> BFSReachable(Vector2Int start, Grid grid, int range, bool ignoreOccupied = false)
    {
        var visited = new HashSet<Vector2Int> { start };
        var queue = new Queue<(Vector2Int cell, int dist)>();
        queue.Enqueue((start, 0));
        var result = new List<Vector2Int>();
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();
            if (dist >= range) continue;

            foreach (var dir in dirs)
            {
                var next = current + dir;
                if (!grid.IsWithinBounds(next)) continue;
                if (visited.Contains(next)) continue;
                visited.Add(next);

                bool occupied = grid.Cells.TryGetValue(next, out var c) && c.Occupied;

                if (!occupied)
                {
                    result.Add(next);
                    queue.Enqueue((next, dist + 1));
                }
                else if (ignoreOccupied)
                {
                    // Flying: traverse but cannot land; still propagate BFS
                    queue.Enqueue((next, dist + 1));
                }
            }
        }

        return result;
    }

    private static Vector2Int? FindRetreatCell(Unit unit, List<Unit> enemies, Grid grid, int moveRange)
    {
        var reachable = BFSReachable(unit.gridPosition, grid, moveRange, unit.isFlying);
        if (reachable.Count == 0) return null;

        var liveEnemies = enemies.Where(e => e.IsAlive).ToList();
        if (liveEnemies.Count == 0) return null;

        // Pick the cell that is farthest from the nearest enemy
        return reachable
            .OrderByDescending(cell => liveEnemies.Min(e => Manhattan(cell, e.gridPosition)))
            .First();
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;

    private static int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
