using UnityEngine;

public struct PositionModifier
{
    public float multiplier;
    public int flatBonus;
    public bool isFlanking;
    public bool isBackAttack;
    public bool isHeightAdvantage;
    public bool isSurrounded;

    public static PositionModifier Default => new PositionModifier
    {
        multiplier = 1f,
        flatBonus = 0,
        isFlanking = false,
        isBackAttack = false,
        isHeightAdvantage = false,
        isSurrounded = false
    };
}

public enum RangeType
{
    Melee,
    Ranged
}

public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(Unit attacker, Unit defender,
        PositionModifier posMod, RangeType rangeType, bool isCrit)
    {
        int baseDamage = attacker.attack;
        int defense = defender.defense;

        float multiplier = posMod.multiplier;
        if (isCrit) multiplier += 0.5f;

        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier) - defense + posMod.flatBonus);

        if (rangeType == RangeType.Ranged)
            damage = Mathf.RoundToInt(damage * 0.75f);

        return damage;
    }

    public static int CalculateSpellDamage(Unit caster, Unit target, SpellBase spell)
    {
        int basePower = spell.power + Mathf.RoundToInt(caster.intelligence * 0.5f);
        int resistance = Mathf.RoundToInt(target.intelligence * 0.3f);

        float multiplier = GetTypeEffectiveness(spell.damageType, target);
        int damage = Mathf.Max(1, Mathf.RoundToInt(basePower * multiplier) - resistance);

        return damage;
    }

    public static float GetTypeEffectiveness(DamageType attackType, Unit target)
    {
        switch (attackType)
        {
            case DamageType.Fire:
                if (target.habitatType == UnitHabitatType.Water) return 0.5f;
                if (target.habitatType == UnitHabitatType.Ethereal) return 0.7f;
                return 1.0f;

            case DamageType.Ice:
                if (target.habitatType == UnitHabitatType.Water) return 1.5f;
                if (target.habitatType == UnitHabitatType.Ground) return 1.2f;
                return 1.0f;

            case DamageType.Lightning:
                if (target.habitatType == UnitHabitatType.Water) return 1.5f;
                if (target.habitatType == UnitHabitatType.Air) return 0.7f;
                return 1.0f;

            case DamageType.Light:
                if (target.habitatType == UnitHabitatType.Ethereal) return 1.5f;
                if (target.habitatType == UnitHabitatType.Underground) return 0.7f;
                return 1.0f;

            case DamageType.Dark:
                if (target.habitatType == UnitHabitatType.Underground) return 1.3f;
                if (target.habitatType == UnitHabitatType.Air) return 1.2f;
                return 1.0f;

            case DamageType.Earth:
                if (target.habitatType == UnitHabitatType.Air) return 1.5f;
                if (target.habitatType == UnitHabitatType.Ground) return 1.2f;
                return 1.0f;

            case DamageType.Air:
                if (target.habitatType == UnitHabitatType.Ground) return 1.3f;
                if (target.habitatType == UnitHabitatType.Underground) return 0.7f;
                return 1.0f;

            case DamageType.Time:
                return 1.2f;

            case DamageType.True:
                return 1.5f;

            default:
                return 1.0f;
        }
    }

    public static bool IsCriticalHit(Unit attacker)
    {
        int chance = 5 + attacker.luck;
        return Random.Range(0, 100) < chance;
    }

    public static bool CanCounterAttack(Unit defender)
    {
        if (!defender.IsAlive) return false;
        return true;
    }

    public static PositionModifier GetPositionModifier(Vector2Int attackerPos,
        Vector2Int defenderPos, BattleGrid grid)
    {
        var mod = PositionModifier.Default;

        int heightDiff = grid.GetHeight(attackerPos) - grid.GetHeight(defenderPos);
        if (heightDiff > 0)
        {
            mod.isHeightAdvantage = true;
            mod.flatBonus += heightDiff * 2;
            mod.multiplier += 0.1f * heightDiff;
        }

        return mod;
    }
}
