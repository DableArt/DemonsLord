using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    private Unit _unit;
    private List<StatusEffect> _statuses = new List<StatusEffect>();

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public void AddStatus(StatusEffectType type, int duration, int potency, string source)
    {
        var existing = _statuses.FirstOrDefault(s => s.type == type);
        if (existing != null)
        {
            existing.remainingDuration = Mathf.Max(existing.remainingDuration, duration);
            existing.potency = potency;
            existing.sourceName = source;
        }
        else
        {
            _statuses.Add(new StatusEffect(type, duration, potency, source));
        }

        Debug.Log($"[Status] {_unit.unitName} получил {type} на {duration} ходов ( potency={potency}) от {source}");
    }

    public void RemoveStatus(StatusEffectType type)
    {
        _statuses.RemoveAll(s => s.type == type);
    }

    public void RemoveAllStatuses()
    {
        _statuses.Clear();
    }

    public bool HasStatus(StatusEffectType type)
    {
        return _statuses.Any(s => s.type == type);
    }

    public bool CanAct()
    {
        return !_statuses.Any(s => s.PreventsAction);
    }

    public void OnTurnStart()
    {
        foreach (var status in _statuses.ToList())
        {
            if (status.IsDot)
            {
                switch (status.type)
                {
                    case StatusEffectType.Burn:
                        _unit.TakeDamage(status.potency);
                        FloatingDamage.ShowDamage(_unit, status.potency);
                        Debug.Log($"[Status] {_unit.unitName} получает {status.potency} урона от {status.type}");
                        break;
                    case StatusEffectType.Poison:
                        _unit.TakeDamage(status.potency);
                        FloatingDamage.ShowDamage(_unit, status.potency);
                        Debug.Log($"[Status] {_unit.unitName} получает {status.potency} урона от {status.type}");
                        break;
                    case StatusEffectType.Bleed:
                        _unit.TakeDamage(status.potency);
                        FloatingDamage.ShowDamage(_unit, status.potency);
                        Debug.Log($"[Status] {_unit.unitName} получает {status.potency} урона от {status.type}");
                        break;
                    case StatusEffectType.Regeneration:
                        _unit.Heal(status.potency);
                        FloatingDamage.ShowHeal(_unit, status.potency);
                        Debug.Log($"[Status] {_unit.unitName} восстанавливает {status.potency} HP от {status.type}");
                        break;
                }
            }

            status.remainingDuration--;
        }

        _statuses.RemoveAll(s => s.IsExpired);
    }

    public void ApplyStatusFromSpell(SpellBase spell, string sourceName)
    {
        foreach (var effectData in spell.statusEffects)
        {
            if (UnityEngine.Random.Range(0, 100) < effectData.applyChance)
            {
                AddStatus(effectData.type, effectData.duration, effectData.potency, sourceName);
            }
        }
    }

    public int GetAttackModifier()
    {
        int mod = 0;
        if (HasStatus(StatusEffectType.Weaken))
            mod -= _statuses.First(s => s.type == StatusEffectType.Weaken).potency;
        return mod;
    }

    public int GetDefenseModifier()
    {
        int mod = 0;
        if (HasStatus(StatusEffectType.Shield))
            mod += _statuses.First(s => s.type == StatusEffectType.Shield).potency;
        if (HasStatus(StatusEffectType.Weaken))
            mod -= Mathf.Abs(_statuses.First(s => s.type == StatusEffectType.Weaken).potency) / 2;
        return mod;
    }

    public bool ShouldSkipTurn(out string reason)
    {
        reason = null;
        if (HasStatus(StatusEffectType.Stun))
        {
            reason = "оглушён";
            return true;
        }
        if (HasStatus(StatusEffectType.Freeze))
        {
            reason = "заморожен";
            return true;
        }
        return false;
    }

    public List<StatusEffect> GetActiveStatuses()
    {
        return new List<StatusEffect>(_statuses);
    }
}
