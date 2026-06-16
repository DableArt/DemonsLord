using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    public List<GachaUnitData> availableUnits = new List<GachaUnitData>();
    private static Sprite _unitCircleSprite;

    public Unit PullUnit(int level = 1)
    {
        if (availableUnits.Count == 0)
        {
            Debug.LogError("GachaSystem: No available units defined.");
            return null;
        }

        var data = availableUnits[Random.Range(0, availableUnits.Count)];
        UnitRank rolledRank = data.RollRank();

        GameObject prefab = data.unitPrefab;
        if (prefab == null)
        {
            Debug.LogError($"GachaSystem: No prefab for {data.unitName}");
            return null;
        }

        GameObject go = Instantiate(prefab);
        go.name = data.unitName;
        Unit unit = go.GetComponent<Unit>();
        if (unit == null)
            unit = go.AddComponent<Unit>();

        SetupUnit(unit, data, rolledRank, level);
        return unit;
    }

    public static Unit CreateUnitFromData(GachaUnitData data, UnitRank rank, int level)
    {
        GameObject go = new GameObject(data.unitName);
        Unit unit = go.AddComponent<Unit>();
        SetupUnit(unit, data, rank, level);
        return unit;
    }

    public static void SetupUnit(Unit unit, GachaUnitData data, UnitRank rank, int level)
    {
        unit.unitName = data.unitName;
        unit.unitLevel = level;
        unit.rank = rank;
        unit.habitatType = data.habitat;
        unit.size = data.size;

        float rankMult = GetRankMultiplier(rank);
        float levelMult = 1f + (level - 1) * 0.1f;

        unit.strength = Mathf.RoundToInt(data.baseSTR * rankMult * levelMult);
        unit.agility = Mathf.RoundToInt(data.baseAGI * rankMult * levelMult);
        unit.endurance = Mathf.RoundToInt(data.baseEND * rankMult * levelMult);
        unit.intelligence = Mathf.RoundToInt(data.baseINT * rankMult * levelMult);
        unit.charisma = Mathf.RoundToInt(data.baseCHA * rankMult * levelMult);
        unit.luck = Mathf.RoundToInt(data.baseLUK * rankMult * levelMult);

        unit.attack = Mathf.RoundToInt(data.baseATK * rankMult * levelMult);
        unit.defense = Mathf.RoundToInt(data.baseDEF * rankMult * levelMult);
        unit.attackRange = data.baseATKRange;

        unit.maxHP = Mathf.RoundToInt(data.baseHP * rankMult * levelMult);
        unit.currentHP = unit.maxHP;
        unit.maxMP = Mathf.RoundToInt(data.baseMP * rankMult * levelMult);
        unit.currentMP = unit.maxMP;

        unit.currentUltimateCharge = 0;
        unit.maxUltimateCharge = 100;

        var caster = unit.GetComponent<SpellCaster>();
        if (caster == null) caster = unit.gameObject.AddComponent<SpellCaster>();
        caster.knownSpells = new List<SpellBase>(data.defaultSpells);

        var abilityComp = unit.GetComponent<AbilityComponent>();
        if (abilityComp == null) abilityComp = unit.gameObject.AddComponent<AbilityComponent>();
        abilityComp.passiveAbilities = new List<AbilityBase>(data.passiveAbilities);
        abilityComp.activeAbilities = new List<AbilityBase>(data.activeAbilities);
        abilityComp.ultimateAbility = data.ultimateAbility;

        var vfx = unit.GetComponent<VfxTrigger>();
        if (vfx == null) unit.gameObject.AddComponent<VfxTrigger>();

        var status = unit.GetComponent<StatusManager>();
        if (status == null) unit.gameObject.AddComponent<StatusManager>();

        var sr = unit.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = unit.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.sortingOrder = 0;
            sr.color = Color.white;
        }
    }

    private static Sprite GetCircleSprite()
    {
        if (_unitCircleSprite != null) return _unitCircleSprite;
        int size = 32;
        var tex = new Texture2D(size, size);
        var colors = new Color[size * size];
        float cx = size / 2f, cy = size / 2f, r = size / 2f - 1;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                colors[y * size + x] = (dx * dx + dy * dy <= r * r) ? Color.white : Color.clear;
            }
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        _unitCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return _unitCircleSprite;
    }

    public static float GetRankMultiplier(UnitRank rank)
    {
        switch (rank)
        {
            case UnitRank.R: return 1.0f;
            case UnitRank.SR: return 1.3f;
            case UnitRank.SSR: return 1.7f;
            case UnitRank.UR: return 2.2f;
            case UnitRank.LR: return 3.0f;
            default: return 1.0f;
        }
    }
}
