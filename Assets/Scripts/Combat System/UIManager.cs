using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Left Panel - Unit Name")]
    public TMP_Text unitNameText;
    public TMP_Text unitLevelText;

    [Header("Left Panel - Bars")]
    [Tooltip("Image с Image Type = Filled, Fill Method = Horizontal")]
    public Image hpBar;
    public TMP_Text hpText;

    [Tooltip("Image с Image Type = Filled, Fill Method = Horizontal")]
    public Image mpBar;
    public TMP_Text mpText;

    [Tooltip("Image с Image Type = Filled, Fill Method = Horizontal")]
    public Image ultimateBar;
    public TMP_Text ultimateText;

    [Header("Left Panel - Stats (значения, не лейблы)")]
    public TMP_Text atkValueText;   // текст рядом с ATK — число
    public TMP_Text defValueText;   // текст рядом с DEF — число
    public TMP_Text agiValueText;   // текст рядом с AGI — число
    public TMP_Text intValueText;   // текст рядом с INT — число (в коде = 0, задел)
    public TMP_Text lukValueText;   // текст рядом с LUK — число

    [Header("Left Panel - Effects & Bonus")]
    public TMP_Text effectsText;
    public TMP_Text bonusText;

    [Header("Action Buttons")]
    public Button btnAttack;
    public Button btnDefend;
    public Button btnWait;
    public Button btnEscape;
    public Button btnItems;     // Items_button — движение
    public Button btnMagic;     // выключена в прототипе
    public Button btnSpecial;   // выключена в прототипе

    [Header("Top Panel")]
    public TMP_Text turnText;
    public TMP_Text phaseText;
    public TMP_Text currentUnitText;

    [Header("Right Panel - Battle Stats")]
    public TMP_Text alliesAliveText;
    public TMP_Text enemiesAliveText;

    private BattleSystem _battleSystem;

    // ─── Инициализация ────────────────────────────────────────────────────────

    /// <summary>Вызывается из BattleSystem.Start() — сохраняет ссылку.</summary>
    public void Init(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;
    }

    private void Start()
    {
        if (_battleSystem == null)
            _battleSystem = FindObjectOfType<BattleSystem>();

        // Подключаем кнопки
        btnAttack?.onClick.AddListener(() => _battleSystem?.OnPlayerSelectAttack());
        btnDefend?.onClick.AddListener(() => _battleSystem?.OnPlayerDefend());
        btnWait?.onClick.AddListener(() => _battleSystem?.OnPlayerWait());
        btnEscape?.onClick.AddListener(() => _battleSystem?.OnPlayerEscape());
        btnItems?.onClick.AddListener(() => _battleSystem?.OnPlayerSelectMove());

        // Magic и Special — заблокированы в прототипе
        if (btnMagic != null) btnMagic.interactable = false;
        if (btnSpecial != null) btnSpecial.interactable = false;
    }

    // ─── Обновление UI ────────────────────────────────────────────────────────

    /// <summary>Обновить левую панель под выбранного юнита.</summary>
    public void UpdateUnitUI(Unit unit)
    {
        if (unit == null) return;

        // Имя и уровень
        if (unitNameText != null) unitNameText.text = unit.unitName;
        if (unitLevelText != null) unitLevelText.text = $"Lvl. {unit.unitLevel}";

        // HP bar
        if (hpBar != null) hpBar.fillAmount = unit.maxHP > 0 ? (float)unit.currentHP / unit.maxHP : 0f;
        if (hpText != null) hpText.text = $"{unit.currentHP}/{unit.maxHP}";

        // MP bar
        if (mpBar != null) mpBar.fillAmount = unit.maxMP > 0 ? (float)unit.currentMP / unit.maxMP : 0f;
        if (mpText != null) mpText.text = $"{unit.currentMP}/{unit.maxMP}";

        // Ultimate bar (заглушка 100%)
        if (ultimateBar != null) ultimateBar.fillAmount = 1f;
        if (ultimateText != null) ultimateText.text = "100%";

        // Статы — каждый в своё поле
        if (atkValueText != null) atkValueText.text = unit.attack.ToString();
        if (defValueText != null) defValueText.text = unit.defense.ToString();
        if (agiValueText != null) agiValueText.text = unit.agility.ToString();
        if (intValueText != null) intValueText.text = "0";   // INT пока не используется
        if (lukValueText != null) lukValueText.text = unit.luck.ToString();

        // Эффекты и бонусы
        if (effectsText != null) effectsText.text = GetEffectsString(unit);
        if (bonusText != null) bonusText.text = GetBonusesString(unit);
    }

    /// <summary>Обновить верхнюю панель хода.</summary>
    public void UpdateTurnInfo(int roundNumber, bool isPlayerTurn, string activeUnitName)
    {
        string phaseStr = isPlayerTurn ? "Player Turn" : "Enemy Turn";
        if (turnText != null) turnText.text = roundNumber.ToString();
        if (phaseText != null) phaseText.text = phaseStr;
        if (currentUnitText != null) currentUnitText.text = activeUnitName;
    }

    /// <summary>Обновить счётчики живых юнитов (правая панель).</summary>
    public void UpdateAliveCounts(int alliesAlive, int alliesTotal, int enemiesAlive, int enemiesTotal)
    {
        if (alliesAliveText != null) alliesAliveText.text = $"{alliesAlive}/{alliesTotal}";
        if (enemiesAliveText != null) enemiesAliveText.text = $"{enemiesAlive}/{enemiesTotal}";
    }

    /// <summary>Включить/выключить кнопки действий игрока.</summary>
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

    // ─── Приватные хелперы ────────────────────────────────────────────────────

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
