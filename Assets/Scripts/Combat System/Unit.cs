using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Info")]
    public string unitName;
    public int unitLevel;
    public UnitRank rank = UnitRank.R;
    public UnitHabitatType habitatType = UnitHabitatType.Ground;
    public UnitSize size = UnitSize.Small;

    [Header("Primary Stats (GDD)")]
    public int strength;
    public int agility;
    public int endurance;
    public int intelligence;
    public int charisma;
    public int luck;

    [Header("Combat Stats (derived)")]
    public int attack;
    public int defense;

    [Header("Experience")]
    public int currentExp;
    public int expToNextLevel = 100;

    [Header("Resources")]
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;

    [Header("Ultimate")]
    public int maxUltimateCharge = 100;
    public int currentUltimateCharge;

    [Header("Visual")]
    public GameObject selectionIndicator;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (selectionIndicator != null)
                selectionIndicator.SetActive(value);
        }
    }

    public bool IsAlive => currentHP > 0;

    public Vector2Int gridPosition;

    public void Init(string name, int level,
        int str, int agi, int end, int intel, int cha, int luk,
        int atk, int def,
        int hp, int mp,
        UnitRank unitRank = UnitRank.R,
        UnitHabitatType unitHabitat = UnitHabitatType.Ground,
        UnitSize unitSize = UnitSize.Small)
    {
        unitName = name;
        unitLevel = level;

        strength = str;
        agility = agi;
        endurance = end;
        intelligence = intel;
        charisma = cha;
        luck = luk;

        rank = unitRank;
        habitatType = unitHabitat;
        size = unitSize;

        StatCalculator.RecalculateUnitStats(this);
        currentHP = Mathf.Min(currentHP, maxHP);
        if (currentHP <= 0) currentHP = maxHP;
        currentMP = Mathf.Min(currentMP, maxMP);
        if (currentMP <= 0) currentMP = maxMP;
    }

    public void SyncWorldPosition()
    {
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;
        return currentHP <= 0;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }

    public void UseMana(int amount)
    {
        currentMP -= amount;
        if (currentMP < 0) currentMP = 0;
    }

    public void RestoreMana(int amount)
    {
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP;
    }

    public void GainUltimateCharge(int amount)
    {
        currentUltimateCharge += amount;
        if (currentUltimateCharge > maxUltimateCharge)
            currentUltimateCharge = maxUltimateCharge;
    }

    public bool TryUseUltimate()
    {
        if (currentUltimateCharge < maxUltimateCharge) return false;
        currentUltimateCharge = 0;
        return true;
    }
}
