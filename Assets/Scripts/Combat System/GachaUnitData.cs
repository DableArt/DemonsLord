using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaUnit", menuName = "SO/Gacha Unit")]
public class GachaUnitData : ScriptableObject
{
    public string unitName;
    public UnitRank baseRank = UnitRank.R;
    public UnitHabitatType habitat = UnitHabitatType.Ground;
    public UnitSize size = UnitSize.Small;
    public GameObject unitPrefab;

    [Header("Base Stats (at level 1, rank R)")]
    public int baseSTR = 5;
    public int baseAGI = 5;
    public int baseEND = 5;
    public int baseINT = 5;
    public int baseCHA = 5;
    public int baseLUK = 5;
    public int baseATK = 10;
    public int baseDEF = 5;
    public int baseATKRange = 1;
    public int baseMoveRange = 3;
    public int baseHP = 50;
    public int baseMP = 20;

    [Header("Abilities")]
    public List<SpellBase> defaultSpells = new List<SpellBase>();
    public List<AbilityBase> passiveAbilities = new List<AbilityBase>();
    public List<AbilityBase> activeAbilities = new List<AbilityBase>();
    public AbilityBase ultimateAbility;

    [Header("Gacha Weights")]
    [Range(0, 100)] public int weightR = 50;
    [Range(0, 100)] public int weightSR = 30;
    [Range(0, 100)] public int weightSSR = 15;
    [Range(0, 100)] public int weightUR = 4;
    [Range(0, 100)] public int weightLR = 1;

    public UnitRank RollRank()
    {
        int total = weightR + weightSR + weightSSR + weightUR + weightLR;
        int roll = Random.Range(0, total);
        if (roll < weightR) return UnitRank.R;
        roll -= weightR;
        if (roll < weightSR) return UnitRank.SR;
        roll -= weightSR;
        if (roll < weightSSR) return UnitRank.SSR;
        roll -= weightSSR;
        if (roll < weightUR) return UnitRank.UR;
        return UnitRank.LR;
    }
}
