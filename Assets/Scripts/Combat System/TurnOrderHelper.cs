using System.Collections.Generic;
using System.Linq;

public static class TurnOrderHelper
{
    /// <summary>
    /// Builds the turn order for a round from alive player and enemy units.
    /// Sorting: frontDistance ascending, then agility descending.
    /// For player units: frontDistance = (gridWidth - 1 - pos.x)
    /// For enemy units:  frontDistance = pos.x
    /// </summary>
    public static List<Unit> BuildTurnOrder(List<Unit> playerUnits, List<Unit> enemyUnits, int gridWidth)
    {
        var combined = new List<(Unit unit, bool isPlayer)>();
        foreach (var u in playerUnits)
            if (u != null && u.IsAlive) combined.Add((u, true));
        foreach (var u in enemyUnits)
            if (u != null && u.IsAlive) combined.Add((u, false));

        return combined
            .OrderBy(e => e.isPlayer ? (gridWidth - 1 - e.unit.gridPosition.x) : e.unit.gridPosition.x)
            .ThenByDescending(e => e.unit.agility)
            .Select(e => e.unit)
            .ToList();
    }
}
