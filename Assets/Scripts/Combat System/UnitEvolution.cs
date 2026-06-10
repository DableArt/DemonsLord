using UnityEngine;

public static class UnitEvolution
{
    public static bool CanEvolve(Unit unit, EvolutionData recipe)
    {
        if (unit == null || recipe == null) return false;
        if (unit.rank != recipe.fromRank) return false;
        if (unit.unitLevel < recipe.requiredLevel) return false;
        if (!unit.IsAlive) return false;
        return true;
    }

    public static bool Evolve(Unit unit, EvolutionData recipe)
    {
        if (!CanEvolve(unit, recipe)) return false;

        unit.rank = recipe.toRank;
        StatCalculator.RecalculateUnitStats(unit);

        var abilityComp = unit.GetComponent<AbilityComponent>();
        if (abilityComp != null)
            abilityComp.Initialize();

        var vfx = unit.GetComponent<VfxTrigger>();
        if (vfx != null)
        {
            vfx.TriggerUltimate();
            FloatingDamage.ShowText(unit, $"EVOLVE → {recipe.toRank}", recipe.evolutionColor, 7);
        }

        Debug.Log($"[Evolution] {unit.unitName} evolved from {recipe.fromRank} to {recipe.toRank}!");
        return true;
    }

    public static void LevelUp(Unit unit, int levels = 1)
    {
        if (unit == null) return;
        unit.unitLevel += levels;
        StatCalculator.RecalculateUnitStats(unit);

        var vfx = unit.GetComponent<VfxTrigger>();
        if (vfx != null)
            FloatingDamage.ShowText(unit, $"Level Up! ({unit.unitLevel})", Color.cyan, 5);

        Debug.Log($"[LevelUp] {unit.unitName} reached level {unit.unitLevel}");
    }
}
