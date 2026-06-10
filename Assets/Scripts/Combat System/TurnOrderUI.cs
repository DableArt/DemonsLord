using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnOrderUI : MonoBehaviour
{
    [Header("Turn Order")]
    public TMP_Text turnOrderText;
    public int displayCount = 6;

    public void UpdateTurnOrder(List<Unit> turnQueue, int currentIndex, Unit currentUnit)
    {
        if (turnOrderText == null || turnQueue == null || turnQueue.Count == 0) return;

        string display = "<b>Turn Order</b>\n";
        for (int i = 0; i < displayCount && i < turnQueue.Count; i++)
        {
            int idx = (currentIndex + i) % turnQueue.Count;
            var unit = turnQueue[idx];
            if (unit == null) continue;

            string prefix = i == 0 ? "► " : "  ";
            string hpBar = UnitHealthIndicator(unit);
            string marker = i == 0 && unit == currentUnit ? " <color=yellow>●</color>" : "";
            display += $"{prefix}{unit.unitName}{marker} {hpBar}\n";
        }
        turnOrderText.text = display;
    }

    private static string UnitHealthIndicator(Unit unit)
    {
        if (unit == null || !unit.IsAlive) return "<color=red>DEAD</color>";
        float pct = (float)unit.currentHP / unit.maxHP;
        if (pct > 0.6f) return "";
        if (pct > 0.3f) return "<color=yellow>⬤</color>";
        return "<color=red>⬤</color>";
    }
}
