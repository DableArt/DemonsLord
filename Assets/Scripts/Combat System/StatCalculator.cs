using UnityEngine;

public static class StatCalculator
{
    public static int CalculateMaxHP(int endurance, int level, UnitRank rank)
    {
        float rankMult = GetRankMultiplier(rank);
        return Mathf.RoundToInt((50 + endurance * 5) * rankMult * (1f + (level - 1) * 0.08f));
    }

    public static int CalculateMaxMP(int intelligence, int level, UnitRank rank)
    {
        float rankMult = GetRankMultiplier(rank);
        return Mathf.RoundToInt((20 + intelligence * 3) * rankMult * (1f + (level - 1) * 0.08f));
    }

    public static int CalculateAttack(int strength, int level, UnitRank rank)
    {
        float rankMult = GetRankMultiplier(rank);
        return Mathf.RoundToInt(strength * 2 * rankMult * (1f + (level - 1) * 0.1f));
    }

    public static int CalculateDefense(int endurance, int level, UnitRank rank)
    {
        float rankMult = GetRankMultiplier(rank);
        return Mathf.RoundToInt(endurance * 1.5f * rankMult * (1f + (level - 1) * 0.1f));
    }

    public static int CalculateDamage(Unit attacker, Unit defender, int basePower, float multiplier)
    {
        float atk = attacker.attack;
        float def = defender.defense;
        int raw = Mathf.RoundToInt((atk * multiplier + basePower) - def * 0.5f);
        return Mathf.Max(1, raw);
    }

    public static void RecalculateUnitStats(Unit unit)
    {
        if (unit == null) return;
        float hpRatio = unit.maxHP > 0 ? (float)unit.currentHP / unit.maxHP : 1f;
        float mpRatio = unit.maxMP > 0 ? (float)unit.currentMP / unit.maxMP : 1f;

        unit.maxHP = CalculateMaxHP(unit.endurance, unit.unitLevel, unit.rank);
        unit.maxMP = CalculateMaxMP(unit.intelligence, unit.unitLevel, unit.rank);
        unit.attack = CalculateAttack(unit.strength, unit.unitLevel, unit.rank);
        unit.defense = CalculateDefense(unit.endurance, unit.unitLevel, unit.rank);

        unit.currentHP = Mathf.RoundToInt(unit.maxHP * hpRatio);
        unit.currentMP = Mathf.RoundToInt(unit.maxMP * mpRatio);
    }

    public static float GetRankMultiplier(UnitRank rank)
    {
        switch (rank)
        {
            case UnitRank.R: return 1.0f;
            case UnitRank.SR: return 1.3f;
            case UnitRank.SSR: return 1.7f;
            case UnitRank.UR: return 2.2f;
            case UnitRank.LR: return 3.0f;
            default: return 1.0f;
        }
    }
}
