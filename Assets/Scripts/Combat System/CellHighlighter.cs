using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter : MonoBehaviour
{
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;
    public GridManager gridManager;

    private readonly List<GameObject> _highlights = new List<GameObject>();

    /// <summary>Shows blue move-range highlights for the given unit using BFS.</summary>
    public void ShowMoveRange(Unit unit)
    {
        Clear();
        if (moveHighlightPrefab == null || gridManager == null) return;

        var reachable = BattleAI.BFSReachable(
            unit.gridPosition, gridManager.grid, unit.agility, unit.isFlying);

        foreach (var cell in reachable)
        {
            var go = Instantiate(
                moveHighlightPrefab,
                gridManager.GetWorldPosition(cell),
                Quaternion.identity);
            _highlights.Add(go);
        }
    }

    /// <summary>Shows red attack highlights on enemies adjacent (Manhattan = 1) to the unit.</summary>
    public void ShowAttackTargets(Unit unit, List<Unit> enemies)
    {
        Clear();
        if (attackHighlightPrefab == null || gridManager == null) return;

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            int dist = Mathf.Abs(unit.gridPosition.x - enemy.gridPosition.x)
                     + Mathf.Abs(unit.gridPosition.y - enemy.gridPosition.y);
            if (dist != 1) continue;

            var go = Instantiate(
                attackHighlightPrefab,
                gridManager.GetWorldPosition(enemy.gridPosition),
                Quaternion.identity);
            _highlights.Add(go);
        }
    }

    /// <summary>Removes all highlight objects.</summary>
    public void Clear()
    {
        foreach (var go in _highlights)
        {
            if (go != null) Destroy(go);
        }
        _highlights.Clear();
    }
}
