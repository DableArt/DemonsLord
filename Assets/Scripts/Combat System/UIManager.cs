using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Left Panel - Selected Unit")]
    public TMP_Text unitNameText;
    public TMP_Text unitLevelText;

    [Tooltip("Image с Image Type = Filled, Fill Method = Horizontal")]
    public Image hpBar;
    public TMP_Text hpText;

    [Tooltip("Image с Image Type = Filled, Fill Method = Horizontal")]
    public Image mpBar;
    public TMP_Text mpText;

    [Tooltip("Image с Image Type = Filled, Fill Method = Horizontal")]
    public Image ultimateBar;
    public TMP_Text ultimateText;

    public TMP_Text statsText;
    public TMP_Text effectsText;
    public TMP_Text bonusText;

    [Header("Action Buttons")]
    public Button btnAttack;
    public Button btnDefend;
    public Button btnWait;
    public Button btnEscape;
    public Button btnItems;   // кнопка Items_button (движение/предметы)
    public Button btnMagic;
    public Button btnSpecial;

    [Header("Top Panel")]
    public TMP_Text turnText;
    public TMP_Text phaseText;
    public TMP_Text currentUnitText;

    [Header("Right Panel")]
    public TMP_Text alliesAliveText;
    public TMP_Text enemiesAliveText;

    private BattleSystem _battleSystem;

    private void Start()
    {
        _battleSystem = FindObjectOfType<BattleSystem>();

        // Кнопки подключаются через Inspector OnClick() или здесь:
        btnAttack?.onClick.AddListener(() => _battleSystem?.OnPlayerSelectAttack());
        btnDefend?.onClick.AddListener(() => _battleSystem?.OnPlayerDefend());
        btnWait?.onClick.AddListener(() => _battleSystem?.OnPlayerWait());
        btnEscape?.onClick.AddListener(() => _battleSystem?.OnPlayerEscape());
        btnItems?.onClick.AddListener(() => _battleSystem?.OnPlayerSelectMove());

        // Magic и Special отключены в прототипе
        btnMagic?.gameObject.GetComponent<Button>()?.GetComponent<Button>()
            .GetComponent<Graphic>()?.CrossFadeAlpha(0.5f, 0f, true);
        if (btnMagic != null) btnMagic.interactable = false;
        if (btnSpecial != null) btnSpecial.interactable = false;
    }

    /// <summary>Обновить левую панель под выбранного юнита.</summary>
    public void UpdateUnitUI(Unit unit)
    {
        if (unit == null) return;

        if (unitNameText != null) unitNameText.text = unit.unitName;
        if (unitLevelText != null) unitLevelText.text = $"Lvl. {unit.unitLevel}";

        // HP Bar
        if (hpBar != null) hpBar.fillAmount = unit.maxHP > 0 ? (float)unit.currentHP / unit.maxHP : 0f;
        if (hpText != null) hpText.text = $"{unit.currentHP}/{unit.maxHP}";

        // MP Bar
        if (mpBar != null) mpBar.fillAmount = unit.maxMP > 0 ? (float)unit.currentMP / unit.maxMP : 0f;
        if (mpText != null) mpText.text = $"{unit.currentMP}/{unit.maxMP}";

        // Ultimate Bar (100% заглушка для прототипа)
        if (ultimateBar != null) ultimateBar.fillAmount = 1f;
        if (ultimateText != null) ultimateText.text = "100%";

        // Stats
        if (statsText != null)
            statsText.text = $"ATK  {unit.attack}    DEF  {unit.defense}\n" +
                             $"AGI  {unit.agility}    INT  0\n" +
                             $"LUK  {unit.luck}";

        if (effectsText != null) effectsText.text = GetEffectsString(unit);
        if (bonusText != null) bonusText.text = GetBonusesString(unit);
    }

    /// <summary>Обновить верхнюю панель.</summary>
    public void UpdateTopPanel(int turnNumber, string phase, string activeUnitName)
    {
        if (turnText != null) turnText.text = turnNumber.ToString();
        if (phaseText != null) phaseText.text = phase;
        if (currentUnitText != null) currentUnitText.text = activeUnitName;
    }

    /// <summary>Обновить правую панель статистики боя.</summary>
    public void UpdateBattleStats(int alliesAlive, int alliesTotal, int enemiesAlive, int enemiesTotal)
    {
        if (alliesAliveText != null) alliesAliveText.text = $"{alliesAlive}/{alliesTotal}";
        if (enemiesAliveText != null) enemiesAliveText.text = $"{enemiesAlive}/{enemiesTotal}";
    }

    /// <summary>Включить/выключить все кнопки действий игрока.</summary>
    public void SetActionButtonsInteractable(bool value)
    {
        if (btnAttack != null) btnAttack.interactable = value;
        if (btnDefend != null) btnDefend.interactable = value;
        if (btnWait != null) btnWait.interactable = value;
        if (btnEscape != null) btnEscape.interactable = value;
        if (btnItems != null) btnItems.interactable = value;
        // Magic и Special всегда выключены
        if (btnMagic != null) btnMagic.interactable = false;
        if (btnSpecial != null) btnSpecial.interactable = false;
    }

    private string GetEffectsString(Unit unit)
    {
        // TODO: активные эффекты (яд, усиление и т.д.)
        return "";
    }

    private string GetBonusesString(Unit unit)
    {
        // TODO: бонусы от местности
        return "";
    }
}
