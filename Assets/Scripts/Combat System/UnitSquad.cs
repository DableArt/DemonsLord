using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UnitSquad
{
    public string squadName = "Squad";
    public List<Unit> units = new List<Unit>();

    public Unit leader;
    public int MaxSize => 5;

    public int AliveCount => units.Count(u => u != null && u.IsAlive);
    public bool IsAlive => AliveCount > 0;

    public void AddUnit(Unit unit)
    {
        if (unit == null) return;
        if (units.Count >= MaxSize)
        {
            Debug.LogWarning($"Squad {squadName} is full ({MaxSize}/{MaxSize}).");
            return;
        }
        units.Add(unit);
        if (leader == null) leader = unit;
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        if (leader == unit)
            leader = units.FirstOrDefault();
    }

    public Unit GetUnitByIndex(int index)
    {
        if (index >= 0 && index < units.Count)
            return units[index];
        return null;
    }

    public Unit GetAliveUnitBySlot(int slotIndex)
    {
        var alive = units.Where(u => u != null && u.IsAlive).ToList();
        if (slotIndex < 0 || slotIndex >= alive.Count) return null;
        return alive[slotIndex];
    }

    public int GetAliveUnitSlot(Unit unit)
    {
        var alive = units.Where(u => u != null && u.IsAlive).ToList();
        return alive.IndexOf(unit);
    }

    public Unit GetNextAliveUnit(Unit current)
    {
        var alive = units.Where(u => u != null && u.IsAlive).ToList();
        if (alive.Count == 0) return null;
        int idx = alive.IndexOf(current);
        if (idx < 0) return alive[0];
        return alive[(idx + 1) % alive.Count];
    }

    public Unit GetPreviousAliveUnit(Unit current)
    {
        var alive = units.Where(u => u != null && u.IsAlive).ToList();
        if (alive.Count == 0) return null;
        int idx = alive.IndexOf(current);
        if (idx < 0) return alive[alive.Count - 1];
        return alive[(idx - 1 + alive.Count) % alive.Count];
    }

    public void DeployOnGrid(BattleGrid grid, int startX, int startY, bool horizontal = true)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            if (unit == null) continue;

            Vector2Int pos;
            if (horizontal)
                pos = new Vector2Int(startX + i, startY);
            else
                pos = new Vector2Int(startX, startY + i);

            if (!grid.IsWithinBounds(pos))
            {
                Debug.LogWarning($"Cell {pos} out of bounds for unit {unit.unitName}");
                continue;
            }

            grid.SetOccupied(pos, true);
            unit.gridPosition = pos;
            grid.GetCell(pos).unit = unit;
            unit.SyncWorldPosition();
        }
    }

    public Unit GetUnitAtPosition(Vector2Int pos)
    {
        return units.FirstOrDefault(u => u != null && u.IsAlive && u.gridPosition == pos);
    }
}
