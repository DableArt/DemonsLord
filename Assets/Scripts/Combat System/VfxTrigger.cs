using System;
using UnityEngine;

public class VfxTrigger : MonoBehaviour
{
    public event Action<string> OnSpellCast;
    public event Action<string> OnAbilityUsed;
    public event Action OnUltimateUsed;
    public event Action<int> OnDamageTaken;
    public event Action<int> OnDamageDealt;
    public event Action<int> OnHealReceived;
    public event Action OnDeath;

    public void TriggerSpellCast(string spellName)
    {
        OnSpellCast?.Invoke(spellName);
        Debug.Log($"[VFX] Spell: {spellName}");
    }

    public void TriggerAbility(string abilityName)
    {
        OnAbilityUsed?.Invoke(abilityName);
        Debug.Log($"[VFX] Ability: {abilityName}");
    }

    public void TriggerUltimate()
    {
        OnUltimateUsed?.Invoke();
        Debug.Log($"[VFX] ULTIMATE!");
    }

    public void TriggerDamageTaken(int amount)
    {
        OnDamageTaken?.Invoke(amount);
    }

    public void TriggerDamageDealt(int amount)
    {
        OnDamageDealt?.Invoke(amount);
    }

    public void TriggerDeath()
    {
        OnDeath?.Invoke();
    }

    public void TriggerHealReceived(int amount)
    {
        OnHealReceived?.Invoke(amount);
    }
}
