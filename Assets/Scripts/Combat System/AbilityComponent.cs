using System.Collections.Generic;
using UnityEngine;

public class AbilityComponent : MonoBehaviour
{
    public List<AbilityBase> passiveAbilities = new List<AbilityBase>();
    public List<AbilityBase> activeAbilities = new List<AbilityBase>();
    public AbilityBase ultimateAbility;

    [System.NonSerialized] public List<int> activeCooldowns = new List<int>();
    [System.NonSerialized] public bool ultimateAvailable;

    private Unit _unit;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public void Initialize()
    {
        foreach (var p in passiveAbilities)
            ApplyPassive(p);

        activeCooldowns.Clear();
        foreach (var a in activeAbilities)
            activeCooldowns.Add(0);

        ultimateAvailable = false;
    }

    public void OnTurnStart()
    {
        for (int i = 0; i < activeCooldowns.Count; i++)
            if (activeCooldowns[i] > 0) activeCooldowns[i]--;

        if (_unit != null && _unit.currentUltimateCharge >= _unit.maxUltimateCharge)
            ultimateAvailable = true;
    }

    private void ApplyPassive(AbilityBase ability)
    {
        Debug.Log($"[Passive] {_unit.unitName}: {ability.abilityName}");
    }

    public bool CanUseActive(int index)
    {
        if (index < 0 || index >= activeAbilities.Count) return false;
        if (_unit == null || !_unit.IsAlive) return false;
        if (activeCooldowns[index] > 0) return false;
        if (_unit.currentMP < activeAbilities[index].mpCost) return false;
        return true;
    }

    public bool CanUseUltimate()
    {
        if (ultimateAbility == null) return false;
        if (!ultimateAvailable) return false;
        if (_unit == null || !_unit.IsAlive) return false;
        return true;
    }

    public void UseActive(int index)
    {
        if (!CanUseActive(index)) return;
        var ability = activeAbilities[index];
        _unit.UseMana(ability.mpCost);
        activeCooldowns[index] = ability.cooldown;

        var vfx = GetComponent<VfxTrigger>();
        vfx?.TriggerAbility(ability.abilityName);
    }

    public void UseUltimate()
    {
        if (!CanUseUltimate()) return;
        ultimateAvailable = false;
        _unit.currentUltimateCharge = 0;

        var vfx = GetComponent<VfxTrigger>();
        vfx?.TriggerUltimate();
    }
}
