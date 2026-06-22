using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnOrderUI : MonoBehaviour
{
    [Header("Turn Order")]
    public TMP_Text turnOrderText;
    public int displayCount = 4;

    public void UpdateTurnOrder(List<Unit> turnQueue, int currentIndex, Unit currentUnit, UnitSquad playerSquad)
    {
        if (turnOrderText == null || turnQueue == null || turnQueue.Count == 0) return;

        string display = "";
        for (int i = 0; i < displayCount && i < turnQueue.Count; i++)
        {
            int idx = (currentIndex + i) % turnQueue.Count;
            var unit = turnQueue[idx];
            if (unit == null) continue;

            bool isPlayer = playerSquad != null && playerSquad.units.Contains(unit);

            if (i > 0)
                display += " ";

            if (i == 0 && isPlayer)
                display += $"<mark=#FFFF00><color=white>{unit.unitName}</color></mark>";
            else if (isPlayer)
                display += $"<color=white>{unit.unitName}</color>";
            else
                display += $"<mark=#FF0000><color=white>{unit.unitName}</color></mark>";
        }
        turnOrderText.text = display;
    }
}
