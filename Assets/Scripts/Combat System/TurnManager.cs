using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager
{
    private List<Unit> turnQueue = new List<Unit>();
    private int currentIndex = 0;

    public Unit CurrentUnit => turnQueue.Count > 0 ? turnQueue[currentIndex] : null;
    public int TurnNumber { get; private set; } = 1;

    public void BuildOrder(IEnumerable<Unit> units)
    {
        turnQueue = units.OrderByDescending(u => u.agility).ToList();
        currentIndex = 0;
        TurnNumber = 1;
    }

    public void AddUnit(Unit unit)
    {
        turnQueue.Add(unit);
        turnQueue = turnQueue.OrderByDescending(u => u.agility).ToList();
    }

    public void RemoveUnit(Unit unit)
    {
        int idx = turnQueue.IndexOf(unit);
        turnQueue.Remove(unit);
        if (idx < currentIndex) currentIndex--;
        else if (idx == currentIndex && currentIndex >= turnQueue.Count)
            currentIndex = Mathf.Max(0, turnQueue.Count - 1);
    }

    public void NextTurn()
    {
        currentIndex++;
        if (currentIndex >= turnQueue.Count)
        {
            currentIndex = 0;
            TurnNumber++;
        }
    }

    public List<Unit> PeekNextUnits(int count = 3)
    {
        var result = new List<Unit>();
        for (int i = 0; i < count && i < turnQueue.Count; i++)
        {
            result.Add(turnQueue[(currentIndex + i) % turnQueue.Count]);
        }
        return result;
    }

    public List<Unit> GetTurnOrder() => new List<Unit>(turnQueue);
}
