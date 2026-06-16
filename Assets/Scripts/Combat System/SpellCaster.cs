using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    public List<SpellBase> knownSpells = new List<SpellBase>();
    private Unit _unit;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public bool CanCast(SpellBase spell)
    {
        if (spell == null) return false;
        if (_unit == null) return false;
        if (!_unit.IsAlive) return false;
        if (_unit.currentMP < spell.mpCost) return false;
        return true;
    }

    public IEnumerator CastSpell(SpellBase spell, Vector2Int targetCell,
        BattleGrid grid, UnitSquad enemySquad, UnitSquad playerSquad,
        System.Action<int> onDamage)
    {
        if (!CanCast(spell))
        {
            onDamage?.Invoke(0);
            yield break;
        }

        _unit.UseMana(spell.mpCost);

        var vfx = GetComponent<VfxTrigger>();
        vfx?.TriggerSpellCast(spell.spellName);
        Debug.Log($"{_unit.unitName} кастует {spell.spellName} [MP:{_unit.currentMP}/{_unit.maxMP}] → клетка {targetCell}");

        List<Unit> targets = GetTargets(spell, targetCell, grid, enemySquad, playerSquad);
        Debug.Log($"{spell.spellName}: найдено целей — {targets.Count}");
        int totalDamage = 0;

        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive) continue;

            if (spell.targetType == SpellTargetType.Ally || spell.targetType == SpellTargetType.AllAllies || spell.targetType == SpellTargetType.Self)
            {
                int healAmount = spell.power + Mathf.RoundToInt(_unit.intelligence * 0.5f);
                target.Heal(healAmount);
                totalDamage -= healAmount;
                Debug.Log($"{spell.spellName} → {target.unitName}: +{healAmount} HP (теперь {target.currentHP}/{target.maxHP})");

                FloatingDamage.ShowHeal(target, healAmount);

                var targetVfx = target.GetComponent<VfxTrigger>();
                targetVfx?.TriggerHealReceived(healAmount);
            }
            else
            {
                int damage = DamageCalculator.CalculateSpellDamage(_unit, target, spell);
                damage = BattleManager.ApplyBiomeDamage(_unit, damage);
                target.TakeDamage(damage);
                totalDamage += damage;
                Debug.Log($"{spell.spellName} → {target.unitName}: {damage} урона (осталось {target.currentHP}/{target.maxHP})");

                _unit.GainUltimateCharge(damage / 2);

                FloatingDamage.ShowDamage(target, damage);

                var targetVfx = target.GetComponent<VfxTrigger>();
                targetVfx?.TriggerDamageTaken(damage);
                vfx?.TriggerDamageDealt(damage);
            }

            var statusManager = target.GetComponent<StatusManager>();
            if (statusManager != null && spell.statusEffects.Count > 0)
            {
                statusManager.ApplyStatusFromSpell(spell, _unit.unitName);
                foreach (var se in spell.statusEffects)
                    Debug.Log($"  статус {se.type} шанс {se.applyChance}% длит.{se.duration} сила {se.potency}");
            }

            yield return new WaitForSeconds(0.3f);
        }

        onDamage?.Invoke(totalDamage);
        if (targets.Count > 0)
            Debug.Log($"{_unit.unitName} завершил каст {spell.spellName}, итоговый урон/хил: {totalDamage}");
    }

    private List<Unit> GetTargets(SpellBase spell, Vector2Int targetCell,
        BattleGrid grid, UnitSquad enemySquad, UnitSquad playerSquad)
    {
        var result = new List<Unit>();

        if (spell.areaOfEffect > 0)
        {
            var affectedCells = GetAreaCells(targetCell, spell.areaOfEffect, grid);
            foreach (var cell in affectedCells)
            {
                Unit unit = enemySquad.GetUnitAtPosition(cell);
                if (unit == null)
                    unit = playerSquad.GetUnitAtPosition(cell);
                if (unit != null)
                {
                    if (unit == _unit && (spell.targetType == SpellTargetType.Ally ||
                        spell.targetType == SpellTargetType.AllAllies))
                        result.Add(unit);
                    else if (unit != _unit)
                        result.Add(unit);
                }
            }
        }
        else
        {
            Unit target = null;
            if (spell.targetType == SpellTargetType.Enemy || spell.targetType == SpellTargetType.AllEnemies)
                target = enemySquad.GetUnitAtPosition(targetCell);
            else if (spell.targetType == SpellTargetType.Ally || spell.targetType == SpellTargetType.AllAllies)
                target = playerSquad.GetUnitAtPosition(targetCell);
            else if (spell.targetType == SpellTargetType.Self)
                target = _unit;

            if (target != null)
            {
                if (target == _unit && spell.targetType == SpellTargetType.Ally)
                    result.Add(target);
                else if (target != _unit)
                    result.Add(target);
            }
        }

        return result;
    }

    private List<Vector2Int> GetAreaCells(Vector2Int center, int radius, BattleGrid grid)
    {
        var cells = new List<Vector2Int>();
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                var pt = new Vector2Int(center.x + dx, center.y + dy);
                if (grid.IsWithinBounds(pt))
                    cells.Add(pt);
            }
        }
        return cells;
    }
}
