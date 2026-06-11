using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public AIBehaviourType behaviour = AIBehaviourType.Aggressive;

    protected Unit _unit;
    protected BattleManager _battleManager;
    protected GridManager _gridManager;
    protected UnitSquad _playerSquad;
    protected UnitSquad _enemySquad;
    protected bool _lastActionSucceeded;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public virtual IEnumerator ExecuteTurn(BattleManager battleManager)
    {
        _battleManager = battleManager;
        _gridManager = battleManager.gridManager;
        _playerSquad = battleManager.playerSquad;
        _enemySquad = battleManager.enemySquad;

        if (_unit == null || !_unit.IsAlive || _playerSquad == null || !_playerSquad.IsAlive)
            yield break;

        switch (behaviour)
        {
            case AIBehaviourType.Aggressive:
                yield return ExecuteAggressive();
                break;
            case AIBehaviourType.Defensive:
                yield return ExecuteDefensive();
                break;
            case AIBehaviourType.Tactical:
                yield return ExecuteTactical();
                break;
        }
    }

    protected virtual IEnumerator ExecuteAggressive()
    {
        var target = FindBestTarget();
        if (target == null) yield break;

        if (TryUltimate(target)) yield break;
        yield return TryCastSpell(target); if (_lastActionSucceeded) yield break;
        if (TryMeleeAttack(target)) yield break;

        yield return MoveToward(target.gridPosition);
    }

    protected virtual IEnumerator ExecuteDefensive()
    {
        float hpPercent = (float)_unit.currentHP / _unit.maxHP;

        if (hpPercent < 0.3f)
        {
            var healTarget = FindLowestHpAlly();
            if (healTarget != null && healTarget != _unit)
            {
                yield return TryCastHeal(healTarget); if (_lastActionSucceeded) yield break;
            }
            yield return TryCastHeal(_unit); if (_lastActionSucceeded) yield break;
            yield return MoveAwayFromEnemies();
            yield break;
        }

        if (hpPercent < 0.6f)
        {
            yield return TryCastHeal(_unit); if (_lastActionSucceeded) yield break;
        }

        var target = FindNearestEnemy();
        if (target == null) yield break;

        int dist = GetDistance(_unit.gridPosition, target.gridPosition);

        if (dist <= 1)
        {
            if (hpPercent > 0.4f)
            {
                if (TryMeleeAttack(target)) yield break;
            }
            yield return MoveAwayFromEnemies();
            yield break;
        }

        yield return TryCastSpell(target); if (_lastActionSucceeded) yield break;
        yield return MoveToward(target.gridPosition);
    }

    protected virtual IEnumerator ExecuteTactical()
    {
        var groupedTarget = FindBestAoeTarget();
        if (groupedTarget != null)
        {
            yield return TryCastAoe(groupedTarget.Value); if (_lastActionSucceeded) yield break;
        }

        var priorityTarget = FindHighestThreatTarget();
        if (priorityTarget == null)
            priorityTarget = FindLowestHpEnemy();
        if (priorityTarget == null) yield break;

        if (TryUltimate(priorityTarget)) yield break;
        yield return TryCastSpell(priorityTarget); if (_lastActionSucceeded) yield break;
        if (TryMeleeAttack(priorityTarget)) yield break;

        var flankPos = FindFlankPosition(priorityTarget.gridPosition);
        if (flankPos.HasValue)
        {
            yield return MoveToPosition(flankPos.Value);
            if (IsAdjacent(_unit.gridPosition, priorityTarget.gridPosition))
                TryMeleeAttack(priorityTarget);
        }
        else
        {
            yield return MoveToward(priorityTarget.gridPosition);
        }
    }

    protected bool TryMeleeAttack(Unit target)
    {
        if (target == null || !target.IsAlive) return false;
        if (!IsAdjacent(_unit.gridPosition, target.gridPosition)) return false;

        var posMod = DamageCalculator.GetPositionModifier(
            _unit.gridPosition, target.gridPosition, _gridManager.grid, _enemySquad.units);
        bool isCrit = DamageCalculator.IsCriticalHit(_unit);
        int damage = DamageCalculator.CalculatePhysicalDamage(
            _unit, target, posMod, RangeType.Melee, isCrit);
        damage = BattleManager.ApplyBiomeDamage(_unit, damage);

        target.TakeDamage(damage);
        _unit.GainUltimateCharge(damage / 2);
        target.GainUltimateCharge(damage / 4);

        FloatingDamage.ShowDamage(target, damage, isCrit);

        var vfx = GetComponent<VfxTrigger>();
        vfx?.TriggerDamageDealt(damage);
        var targetVfx = target.GetComponent<VfxTrigger>();
        targetVfx?.TriggerDamageTaken(damage);

        if (isCrit)
            Debug.Log($"[AI] CRIT! {_unit.unitName} deals {damage} damage to {target.unitName}");

        if (!target.IsAlive)
        {
            targetVfx?.TriggerDeath();
            _gridManager.RemoveUnitFromGrid(target);
            _battleManager.turnManager.RemoveUnit(target);
        }

        return true;
    }

    protected IEnumerator TryCastSpell(Unit target)
    {
        _lastActionSucceeded = false;
        var caster = GetComponent<SpellCaster>();
        if (caster == null || caster.knownSpells.Count == 0) yield break;

        int dist = GetDistance(_unit.gridPosition, target.gridPosition);

        foreach (var spell in caster.knownSpells)
        {
            if (!caster.CanCast(spell)) continue;
            if (spell.targetType == SpellTargetType.Ally || spell.targetType == SpellTargetType.AllAllies || spell.targetType == SpellTargetType.Self)
                continue;
            if (dist > spell.range) continue;

            _lastActionSucceeded = true;
            yield return caster.CastSpell(spell, target.gridPosition,
                _gridManager.grid, _playerSquad, _enemySquad, (dmg) =>
                {
                    if (!target.IsAlive)
                    {
                        var deathVfx = target.GetComponent<VfxTrigger>();
                        deathVfx?.TriggerDeath();
                        _gridManager.RemoveUnitFromGrid(target);
                        _battleManager.turnManager.RemoveUnit(target);
                    }
                });
            yield break;
        }
    }

    protected IEnumerator TryCastHeal(Unit target)
    {
        _lastActionSucceeded = false;
        var caster = GetComponent<SpellCaster>();
        if (caster == null || caster.knownSpells.Count == 0) yield break;

        int dist = GetDistance(_unit.gridPosition, target.gridPosition);

        foreach (var spell in caster.knownSpells)
        {
            if (!caster.CanCast(spell)) continue;
            if (spell.targetType != SpellTargetType.Ally && spell.targetType != SpellTargetType.AllAllies && spell.targetType != SpellTargetType.Self)
                continue;
            if (dist > spell.range) continue;

            _lastActionSucceeded = true;
            yield return caster.CastSpell(spell, target.gridPosition,
                _gridManager.grid, _playerSquad, _enemySquad, null);
            yield break;
        }
    }

    protected IEnumerator TryCastAoe(Vector2Int center)
    {
        _lastActionSucceeded = false;
        var caster = GetComponent<SpellCaster>();
        if (caster == null || caster.knownSpells.Count == 0) yield break;

        int dist = GetDistance(_unit.gridPosition, center);

        foreach (var spell in caster.knownSpells)
        {
            if (!caster.CanCast(spell)) continue;
            if (spell.areaOfEffect <= 0) continue;
            if (spell.targetType == SpellTargetType.Ally || spell.targetType == SpellTargetType.AllAllies || spell.targetType == SpellTargetType.Self)
                continue;
            if (dist > spell.range) continue;

            _lastActionSucceeded = true;
            yield return caster.CastSpell(spell, center,
                _gridManager.grid, _playerSquad, _enemySquad, null);
            yield break;
        }
    }

    protected bool TryUltimate(Unit target)
    {
        var abilityComp = GetComponent<AbilityComponent>();
        if (abilityComp == null || !abilityComp.CanUseUltimate()) return false;
        if (target == null || !target.IsAlive) return false;

        int dist = GetDistance(_unit.gridPosition, target.gridPosition);
        if (dist > abilityComp.ultimateAbility.range) return false;

        int damage = abilityComp.ultimateAbility.power + Mathf.RoundToInt(_unit.intelligence * 0.5f);
        target.TakeDamage(damage);
        abilityComp.UseUltimate();

        FloatingDamage.ShowUltimate(target, damage);

        var vfx = GetComponent<VfxTrigger>();
        vfx?.TriggerDamageDealt(damage);
        vfx?.TriggerUltimate();
        var targetVfx = target.GetComponent<VfxTrigger>();
        targetVfx?.TriggerDamageTaken(damage);

        Debug.Log($"[AI ULTIMATE] {_unit.unitName} uses {abilityComp.ultimateAbility.abilityName} on {target.unitName} - {damage} damage!");

        if (!target.IsAlive)
        {
            targetVfx?.TriggerDeath();
            _gridManager.RemoveUnitFromGrid(target);
            _battleManager.turnManager.RemoveUnit(target);
        }

        return true;
    }

    protected IEnumerator MoveToward(Vector2Int targetPos)
    {
        var path = PathFindingHelper.FindPath(_gridManager.grid, _unit.gridPosition, targetPos, _unit);
        if (path.IsValid && path.Length > 1)
        {
            Vector2Int nextCell = path[1];
            if (_gridManager.grid.IsWithinBounds(nextCell) && !_gridManager.grid.IsOccupied(nextCell))
            {
                _gridManager.MoveUnit(_unit, nextCell);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    protected IEnumerator MoveToPosition(Vector2Int pos)
    {
        if (!_gridManager.grid.IsWithinBounds(pos) || _gridManager.grid.IsOccupied(pos)) yield break;
        _gridManager.MoveUnit(_unit, pos);
        yield return new WaitForSeconds(0.3f);
    }

    protected IEnumerator MoveAwayFromEnemies()
    {
        var nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) yield break;

        Vector2Int awayDir = new Vector2Int(
            _unit.gridPosition.x - nearestEnemy.gridPosition.x,
            _unit.gridPosition.y - nearestEnemy.gridPosition.y
        );
        awayDir = new Vector2Int(Mathf.Clamp(awayDir.x, -1, 1), Mathf.Clamp(awayDir.y, -1, 1));

        Vector2Int retreatPos = new Vector2Int(
            _unit.gridPosition.x + awayDir.x,
            _unit.gridPosition.y + awayDir.y
        );

        if (_gridManager.grid.IsWithinBounds(retreatPos) && !_gridManager.grid.IsOccupied(retreatPos))
        {
            _gridManager.MoveUnit(_unit, retreatPos);
            yield return new WaitForSeconds(0.3f);
        }
    }

    protected Vector2Int? FindFlankPosition(Vector2Int targetPos)
    {
        var neighbors = _gridManager.grid.GetNeighbors(targetPos);
        foreach (var n in neighbors)
        {
            if (!_gridManager.grid.IsWithinBounds(n)) continue;
            if (_gridManager.grid.IsOccupied(n)) continue;

            Vector2Int dirToTarget = new Vector2Int(
                targetPos.x - _unit.gridPosition.x,
                targetPos.y - _unit.gridPosition.y
            );

            Vector2Int dirFromNeighbor = new Vector2Int(
                n.x - _unit.gridPosition.x,
                n.y - _unit.gridPosition.y
            );

            if (dirToTarget.x != dirFromNeighbor.x || dirToTarget.y != dirFromNeighbor.y)
                return n;
        }
        return null;
    }

    protected Vector2Int? FindBestAoeTarget()
    {
        var caster = GetComponent<SpellCaster>();
        if (caster == null) return null;

        var aoeSpell = caster.knownSpells.FirstOrDefault(s => s.areaOfEffect > 0);
        if (aoeSpell == null) return null;

        int bestCount = 0;
        Vector2Int? bestCell = null;

        var aliveEnemies = _playerSquad.units.Where(u => u != null && u.IsAlive).ToList();
        if (aliveEnemies.Count < 2) return null;

        foreach (var enemy in aliveEnemies)
        {
            int count = 0;
            foreach (var other in aliveEnemies)
            {
                if (other == enemy) continue;
                if (GetDistance(enemy.gridPosition, other.gridPosition) <= aoeSpell.areaOfEffect)
                    count++;
            }
            if (count > bestCount)
            {
                bestCount = count;
                bestCell = enemy.gridPosition;
            }
        }

        return bestCell;
    }

    protected Unit FindBestTarget()
    {
        return FindLowestHpEnemy() ?? FindNearestEnemy();
    }

    protected Unit FindNearestEnemy()
    {
        Unit nearest = null;
        int minDist = int.MaxValue;
        foreach (var unit in _playerSquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            int dist = GetDistance(_unit.gridPosition, unit.gridPosition);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = unit;
            }
        }
        return nearest;
    }

    protected Unit FindLowestHpEnemy()
    {
        Unit lowest = null;
        int minHp = int.MaxValue;
        foreach (var unit in _playerSquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            if (unit.currentHP < minHp)
            {
                minHp = unit.currentHP;
                lowest = unit;
            }
        }
        return lowest;
    }

    protected Unit FindHighestThreatTarget()
    {
        Unit highest = null;
        int maxThreat = int.MinValue;
        foreach (var unit in _playerSquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            int threat = unit.attack + unit.intelligence;
            if (threat > maxThreat)
            {
                maxThreat = threat;
                highest = unit;
            }
        }
        return highest;
    }

    protected Unit FindLowestHpAlly()
    {
        Unit lowest = null;
        int minHp = int.MaxValue;
        foreach (var unit in _enemySquad.units)
        {
            if (unit == null || !unit.IsAlive || unit == _unit) continue;
            if (unit.currentHP < minHp)
            {
                minHp = unit.currentHP;
                lowest = unit;
            }
        }
        return lowest;
    }

    protected static int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    protected static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return GetDistance(a, b) == 1;
    }
}
