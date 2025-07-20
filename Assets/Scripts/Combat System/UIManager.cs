using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text unitNameText;
    public Text unitLevelText;
    public Slider hpSlider;
    public Text hpText;
    public Slider mpSlider;
    public Text mpText;
    public Text statsText;
    public Text effectsText;
    public Text bonusText;

    public void UpdateUnitUI(Unit unit)
    {
        if (unit == null) return;
        unitNameText.text = unit.unitName;
        unitLevelText.text = $"Lvl. {unit.unitLevel}";
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
        hpText.text = $"{unit.currentHP}/{unit.maxHP}";
        mpSlider.maxValue = unit.maxMP;
        mpSlider.value = unit.currentMP;
        mpText.text = $"{unit.currentMP}/{unit.maxMP}";
        statsText.text = $"ATK: {unit.attack}  DEF: {unit.defense}\nAGI: {unit.agility}  LUK: {unit.luck}";
        effectsText.text = GetEffectsString(unit);
        bonusText.text = GetBonusesString(unit);
    }

    private string GetEffectsString(Unit unit)
    {
        // TODO: Реализация на основе системы эффектов
        return "";
    }

    private string GetBonusesString(Unit unit)
    {
        // TODO: Реализация на основе системы бонусов
        return "";
    }
}