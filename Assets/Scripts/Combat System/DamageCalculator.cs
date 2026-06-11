using System.Collections.Generic;
using System.Linq;
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
        int baseDamage = Mathf.Max(0, attacker.attack + GetAttackModifier(attacker));
        int defense = Mathf.Max(0, defender.defense + GetDefenseModifier(defender));

        float multiplier = posMod.multiplier;
        if (isCrit) multiplier += 0.5f;

        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier) - defense + posMod.flatBonus);

        if (rangeType == RangeType.Ranged)
            damage = Mathf.RoundToInt(damage * 0.75f);

        return damage;
    }

    private static int GetAttackModifier(Unit unit)
    {
        var sm = unit.GetComponent<StatusManager>();
        return sm != null ? sm.GetAttackModifier() : 0;
    }

    private static int GetDefenseModifier(Unit unit)
    {
        var sm = unit.GetComponent<StatusManager>();
        return sm != null ? sm.GetDefenseModifier() : 0;
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
        Vector2Int defenderPos, BattleGrid grid, IEnumerable<Unit> alliedUnits = null)
    {
        var mod = PositionModifier.Default;

        int heightDiff = grid.GetHeight(attackerPos) - grid.GetHeight(defenderPos);
        if (heightDiff > 0)
        {
            mod.isHeightAdvantage = true;
            mod.flatBonus += heightDiff * 2;
            mod.multiplier += 0.1f * heightDiff;
        }

        if (alliedUnits == null) return mod;

        var adjacentAllies = new List<Vector2Int>();
        foreach (var ally in alliedUnits)
        {
            if (ally == null || !ally.IsAlive) continue;
            if (ally.gridPosition == attackerPos) continue;
            if (IsAdjacent(ally.gridPosition, defenderPos))
                adjacentAllies.Add(ally.gridPosition);
        }

        if (adjacentAllies.Count == 0) return mod;

        Vector2Int attackerDir = new Vector2Int(
            attackerPos.x - defenderPos.x,
            attackerPos.y - defenderPos.y
        );

        var occupiedDirs = new HashSet<Vector2Int> { NormalizeDir(attackerDir) };

        bool hasOpposite = false;
        foreach (var allyPos in adjacentAllies)
        {
            Vector2Int allyDir = new Vector2Int(
                allyPos.x - defenderPos.x,
                allyPos.y - defenderPos.y
            );
            occupiedDirs.Add(NormalizeDir(allyDir));

            if (attackerDir.x * allyDir.x + attackerDir.y * allyDir.y < 0)
                hasOpposite = true;
        }

        int sideCount = occupiedDirs.Count;

        if (sideCount >= 3)
        {
            mod.isSurrounded = true;
            mod.multiplier += 0.3f;
            mod.flatBonus += 10;
        }
        else if (sideCount >= 2 && hasOpposite)
        {
            mod.isFlanking = true;
            mod.multiplier += 0.2f;
            mod.flatBonus += 5;
        }
        else if (sideCount >= 2)
        {
            mod.isBackAttack = true;
            mod.multiplier += 0.15f;
            mod.flatBonus += 3;
        }

        return mod;
    }

    private static Vector2Int NormalizeDir(Vector2Int dir)
    {
        return new Vector2Int(
            dir.x > 0 ? 1 : (dir.x < 0 ? -1 : 0),
            dir.y > 0 ? 1 : (dir.y < 0 ? -1 : 0)
        );
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }
}
