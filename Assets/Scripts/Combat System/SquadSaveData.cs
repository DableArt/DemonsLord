using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedUnitData
{
    public string unitName;
    public int level;
    public UnitRank rank;
    public UnitHabitatType habitat;
    public UnitSize size;
    public int strength, agility, endurance, intelligence, charisma, luck;
    public int attack, defense;
    public int maxHP, currentHP, maxMP, currentMP;
    public int currentExp, expToNextLevel;
    public int currentUltimateCharge, maxUltimateCharge;
    public List<string> spellNames = new List<string>();
}

[Serializable]
public class SquadSaveData
{
    public List<SavedUnitData> units = new List<SavedUnitData>();
}

public static class SquadSaveExtensions
{
    public static SquadSaveData CaptureSquad(UnitSquad squad)
    {
        var data = new SquadSaveData();
        foreach (var unit in squad.units)
        {
            if (unit == null) continue;
            var ud = new SavedUnitData
            {
                unitName = unit.unitName,
                level = unit.unitLevel,
                rank = unit.rank,
                habitat = unit.habitatType,
                size = unit.size,
                strength = unit.strength,
                agility = unit.agility,
                endurance = unit.endurance,
                intelligence = unit.intelligence,
                charisma = unit.charisma,
                luck = unit.luck,
                attack = unit.attack,
                defense = unit.defense,
                maxHP = unit.maxHP,
                currentHP = unit.currentHP,
                maxMP = unit.maxMP,
                currentMP = unit.currentMP,
                currentExp = unit.currentExp,
                expToNextLevel = unit.expToNextLevel,
                currentUltimateCharge = unit.currentUltimateCharge,
                maxUltimateCharge = unit.maxUltimateCharge
            };
            var caster = unit.GetComponent<SpellCaster>();
            if (caster != null)
                foreach (var s in caster.knownSpells)
                    ud.spellNames.Add(s.spellName);
            data.units.Add(ud);
        }
        return data;
    }

    public static void RestoreSquad(SquadSaveData data, UnitSquad squad, GameObject unitPrefab)
    {
        squad.units.Clear();
        if (data == null) return;

        foreach (var ud in data.units)
        {
            GameObject go;
            if (unitPrefab != null)
                go = UnityEngine.Object.Instantiate(unitPrefab);
            else
                go = new GameObject(ud.unitName);

            var unit = go.GetComponent<Unit>();
            if (unit == null) unit = go.AddComponent<Unit>();

            RestoreUnit(unit, ud);
            squad.AddUnit(unit);
        }
    }

    private static void RestoreUnit(Unit unit, SavedUnitData ud)
    {
        unit.unitName = ud.unitName;
        unit.unitLevel = ud.level;
        unit.rank = ud.rank;
        unit.habitatType = ud.habitat;
        unit.size = ud.size;
        unit.strength = ud.strength;
        unit.agility = ud.agility;
        unit.endurance = ud.endurance;
        unit.intelligence = ud.intelligence;
        unit.charisma = ud.charisma;
        unit.luck = ud.luck;
        unit.attack = ud.attack;
        unit.defense = ud.defense;
        unit.maxHP = ud.maxHP;
        unit.currentHP = ud.currentHP;
        unit.maxMP = ud.maxMP;
        unit.currentMP = ud.currentMP;
        unit.currentExp = ud.currentExp;
        unit.expToNextLevel = ud.expToNextLevel;
        unit.currentUltimateCharge = ud.currentUltimateCharge;
        unit.maxUltimateCharge = ud.maxUltimateCharge;

        var caster = unit.GetComponent<SpellCaster>();
        if (caster == null) caster = unit.gameObject.AddComponent<SpellCaster>();
        caster.knownSpells.Clear();
        foreach (var sn in ud.spellNames)
        {
            var found = FindSpellByName(sn);
            if (found != null) caster.knownSpells.Add(found);
        }

        var abilityComp = unit.GetComponent<AbilityComponent>();
        if (abilityComp == null) unit.gameObject.AddComponent<AbilityComponent>();

        var vfx = unit.GetComponent<VfxTrigger>();
        if (vfx == null) unit.gameObject.AddComponent<VfxTrigger>();
    }

    private static SpellBase FindSpellByName(string name)
    {
        var allSpells = Resources.LoadAll<SpellBase>("");
        foreach (var s in allSpells)
            if (s.spellName == name) return s;

        var type = typeof(DefaultSpells);
        var props = type.GetProperties();
        foreach (var p in props)
        {
            var val = p.GetValue(null) as SpellBase;
            if (val != null && val.spellName == name) return val;
        }
        return null;
    }
}
