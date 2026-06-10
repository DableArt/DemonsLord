using System;
using System.Collections;
using UnityEngine;

public enum BattleActionType
{
    Move,
    Attack,
    Defend,
    Wait,
    CastSpell,
    UseItem,
    Ultimate
}

[Serializable]
public class BattleAction
{
    public BattleActionType type;
    public string displayName;
    public SpellBase spell;

    public int MpCost => spell != null ? spell.mpCost : 0;
    public int MinRange => 1;
    public int MaxRange => type == BattleActionType.CastSpell && spell != null ? spell.range : 1;

    public BattleAction(BattleActionType type, string name, SpellBase spellData = null)
    {
        this.type = type;
        displayName = name;
        spell = spellData;
    }

    public bool RequiresTarget
    {
        get
        {
            if (type == BattleActionType.Defend || type == BattleActionType.Wait)
                return false;
            return true;
        }
    }

    public bool CanExecute(Unit actor, Unit targetUnit, BattleGrid grid, UnitSquad enemySquad)
    {
        if (actor == null || !actor.IsAlive) return false;

        switch (type)
        {
            case BattleActionType.Move:
                return true;

            case BattleActionType.Attack:
                if (targetUnit == null || !targetUnit.IsAlive) return false;
                if (!enemySquad.units.Contains(targetUnit)) return false;
                return IsInRange(actor.gridPosition, targetUnit.gridPosition);

            case BattleActionType.Defend:
            case BattleActionType.Wait:
                return true;

            case BattleActionType.CastSpell:
                if (spell == null) return false;
                if (actor.currentMP < spell.mpCost) return false;
                if (targetUnit == null || !targetUnit.IsAlive) return false;
                return IsInRange(actor.gridPosition, targetUnit.gridPosition);

            default:
                return false;
        }
    }

    public bool IsInRange(Vector2Int from, Vector2Int to)
    {
        int dist = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        return dist >= MinRange && dist <= MaxRange;
    }
}
