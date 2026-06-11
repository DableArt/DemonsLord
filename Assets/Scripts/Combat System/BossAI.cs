using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BossPhaseConfig
{
    public string phaseName = "Phase 1";
    [Range(0, 100)] public int hpThreshold = 100;
    public bool canMove = true;
    public float actionDelay = 0.8f;
    public int bonusAttack;
    public int bonusDefense;
    public List<SpellBase> phaseSpells = new List<SpellBase>();
    public int ultimateChargeOnEntry;
}

public class BossAI : EnemyAI
{
    [Header("Boss Config")]
    public string bossTitle = "Boss";
    public List<BossPhaseConfig> phases = new List<BossPhaseConfig>();

    [Header("Unique Mechanics")]
    public int enrageTurns;
    public int summonCount;
    public bool hasAoeAttack;

    private int _currentPhaseIndex;
    private int _enrageCounter;
    private int _turnCount;
    private bool _phaseJustChanged;
    private bool _bossActed;

    private void Start()
    {
        if (phases.Count == 0)
        {
            phases.Add(new BossPhaseConfig
            {
                phaseName = "Phase 1",
                hpThreshold = 100,
                canMove = true,
                actionDelay = 0.8f
            });
        }
        _currentPhaseIndex = 0;
        _enrageCounter = 0;
        _turnCount = 0;
    }

    public override IEnumerator ExecuteTurn(BattleManager battleManager)
    {
        _battleManager = battleManager;
        _gridManager = battleManager.gridManager;
        _playerSquad = battleManager.playerSquad;
        _enemySquad = battleManager.enemySquad;

        if (_unit == null || !_unit.IsAlive || _playerSquad == null || !_playerSquad.IsAlive)
            yield break;

        _turnCount++;
        CheckPhaseTransition();

        var currentPhase = phases[_currentPhaseIndex];
        yield return new WaitForSeconds(currentPhase.actionDelay);

        if (_phaseJustChanged)
        {
            OnPhaseEnter(currentPhase);
            _phaseJustChanged = false;
        }

        if (_enrageCounter > 0)
            _enrageCounter--;

        if (TryEnrageMechanic()) yield break;

        yield return ExecuteBossAction(currentPhase);
    }

    private void CheckPhaseTransition()
    {
        float hpPercent = (float)_unit.currentHP / _unit.maxHP * 100f;
        for (int i = phases.Count - 1; i >= 0; i--)
        {
            if (hpPercent <= phases[i].hpThreshold)
            {
                if (i != _currentPhaseIndex)
                {
                    _currentPhaseIndex = i;
                    _phaseJustChanged = true;
                    Debug.Log($"[BOSS] {_unit.unitName} enters {phases[i].phaseName}!");
                }
                break;
            }
        }
    }

    private void OnPhaseEnter(BossPhaseConfig phase)
    {
        if (phase.ultimateChargeOnEntry > 0)
            _unit.GainUltimateCharge(phase.ultimateChargeOnEntry);

        var vfx = GetComponent<VfxTrigger>();
        vfx?.TriggerUltimate();

        var abilityComp = GetComponent<AbilityComponent>();
        if (abilityComp != null)
            abilityComp.ultimateAvailable = _unit.currentUltimateCharge >= _unit.maxUltimateCharge;

        Debug.Log($"[BOSS] {_unit.unitName} {phase.phaseName}: ATK+{phase.bonusAttack} DEF+{phase.bonusDefense}");
    }

    private bool TryEnrageMechanic()
    {
        if (_enrageCounter > 0 && _turnCount % 3 == 0)
        {
            var target = FindNearestEnemy();
            if (target != null)
            {
                int bonusDmg = Mathf.RoundToInt(_unit.attack * 0.5f);
                var posMod = DamageCalculator.GetPositionModifier(
                    _unit.gridPosition, target.gridPosition, _gridManager.grid, _enemySquad.units);
                int damage = DamageCalculator.CalculatePhysicalDamage(
                    _unit, target, posMod, RangeType.Melee, true);
                damage += bonusDmg;
                damage = BattleManager.ApplyBiomeDamage(_unit, damage);

                target.TakeDamage(damage);

                FloatingDamage.ShowDamage(target, damage, true);

                var vfx = GetComponent<VfxTrigger>();
                vfx?.TriggerDamageDealt(damage);
                var targetVfx = target.GetComponent<VfxTrigger>();
                targetVfx?.TriggerDamageTaken(damage);

                Debug.Log($"[BOSS ENRAGE] {_unit.unitName} deals {damage} bonus damage!");

                if (!target.IsAlive)
                {
                    targetVfx?.TriggerDeath();
                    _gridManager.RemoveUnitFromGrid(target);
                    _battleManager.turnManager.RemoveUnit(target);
                }
                return true;
            }
        }
        return false;
    }

    private IEnumerator ExecuteBossAction(BossPhaseConfig phase)
    {
        var target = FindBestTarget();
        if (target == null) yield break;

        int dist = GetDistance(_unit.gridPosition, target.gridPosition);

        if (TryUltimate(target)) yield break;

        yield return TryBossSpell(phase, target, dist);
        if (_bossActed) yield break;

        if (hasAoeAttack && dist <= 2)
        {
            var aoeTargets = GetUnitsInRadius(_unit.gridPosition, 1);
            if (aoeTargets.Count >= 2)
            {
                foreach (var aoeTarget in aoeTargets)
                {
                    int damage = Mathf.RoundToInt(_unit.attack * 0.8f);
                    damage = BattleManager.ApplyBiomeDamage(_unit, damage);
                    aoeTarget.TakeDamage(damage);

                    FloatingDamage.ShowDamage(aoeTarget, damage);

                    var vfx = aoeTarget.GetComponent<VfxTrigger>();
                    vfx?.TriggerDamageTaken(damage);

                    if (!aoeTarget.IsAlive)
                    {
                        vfx?.TriggerDeath();
                        _gridManager.RemoveUnitFromGrid(aoeTarget);
                        _battleManager.turnManager.RemoveUnit(aoeTarget);
                    }
                }
                Debug.Log($"[BOSS] {_unit.unitName} uses AOE attack!");
                yield return new WaitForSeconds(0.5f);
                yield break;
            }
        }

        if (TryMeleeAttack(target)) yield break;

        if (phase.canMove)
            yield return MoveToward(target.gridPosition);
    }

    private IEnumerator TryBossSpell(BossPhaseConfig phase, Unit target, int dist)
    {
        _bossActed = false;
        var caster = GetComponent<SpellCaster>();
        if (caster == null) yield break;

        var availableSpells = phase.phaseSpells.Count > 0 ? phase.phaseSpells : caster.knownSpells;

        foreach (var spell in availableSpells)
        {
            if (!caster.CanCast(spell)) continue;
            if (dist > spell.range) continue;
            if (spell.targetType == SpellTargetType.Ally || spell.targetType == SpellTargetType.AllAllies || spell.targetType == SpellTargetType.Self)
                continue;

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
            _bossActed = true;
            yield break;
        }
    }

    public void TriggerEnrage(int turns = 3)
    {
        _enrageCounter = turns;
        Debug.Log($"[BOSS] {_unit.unitName} ENRAGED for {turns} turns!");
    }

    private List<Unit> GetUnitsInRadius(Vector2Int center, int radius)
    {
        var result = new List<Unit>();
        foreach (var unit in _playerSquad.units)
        {
            if (unit == null || !unit.IsAlive) continue;
            if (GetDistance(center, unit.gridPosition) <= radius)
                result.Add(unit);
        }
        return result;
    }
}
