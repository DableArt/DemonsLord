using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Left Panel – Selected Unit")]
    public Text unitNameText;
    public Text unitLevelText;
    public Slider hpSlider;
    public Text hpText;
    public Slider mpSlider;
    public Text mpText;
    public Text statsText;
    public Text effectsText;
    public Text bonusText;

    [Header("Action Buttons")]
    public Button btnAttack;
    public Button btnDefend;
    public Button btnWait;
    public Button btnEscape;
    public Button btnMove;
    public Button btnMagic;
    public Button btnSpecial;

    [Header("Top Panel")]
    public Text turnText;
    public Text phaseText;
    public Text currentUnitText;

    [Header("Right Panel")]
    public Text alliesAliveText;
    public Text enemiesAliveText;

    private BattleSystem _battleSystem;

    public void Init(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;

        if (btnAttack)  btnAttack.onClick.AddListener(_battleSystem.OnPlayerSelectAttack);
        if (btnDefend)  btnDefend.onClick.AddListener(_battleSystem.OnPlayerDefend);
        if (btnWait)    btnWait.onClick.AddListener(_battleSystem.OnPlayerWait);
        if (btnEscape)  btnEscape.onClick.AddListener(_battleSystem.OnPlayerEscape);
        if (btnMove)    btnMove.onClick.AddListener(_battleSystem.OnPlayerSelectMove);

        // Magic and Special are not implemented in this prototype
        if (btnMagic)   btnMagic.interactable   = false;
        if (btnSpecial) btnSpecial.interactable = false;
    }

    public void UpdateUnitUI(Unit unit)
    {
        if (unit == null) return;

        if (unitNameText)  unitNameText.text  = unit.unitName;
        if (unitLevelText) unitLevelText.text = $"Lvl. {unit.unitLevel}";

        if (hpSlider) { hpSlider.maxValue = unit.maxHP; hpSlider.value = unit.currentHP; }
        if (hpText)   hpText.text = $"{unit.currentHP}/{unit.maxHP}";

        if (mpSlider) { mpSlider.maxValue = unit.maxMP; mpSlider.value = unit.currentMP; }
        if (mpText)   mpText.text = $"{unit.currentMP}/{unit.maxMP}";

        if (statsText)   statsText.text   = $"ATK: {unit.attack}  DEF: {unit.defense}\nAGI: {unit.agility}  LUK: {unit.luck}";
        if (effectsText) effectsText.text = "";  // TODO: effects system
        if (bonusText)   bonusText.text   = "";  // TODO: terrain bonuses
    }

    public void UpdateTurnInfo(int turnNumber, bool isPlayerTurn, string currentUnitName)
    {
        if (turnText)       turnText.text       = $"Ход: {turnNumber}";
        if (phaseText)      phaseText.text      = isPlayerTurn ? "Player Turn" : "Enemy Turn";
        if (currentUnitText) currentUnitText.text = currentUnitName;
    }

    public void UpdateAliveCounts(int alliesAlive, int alliesTotal, int enemiesAlive, int enemiesTotal)
    {
        if (alliesAliveText)  alliesAliveText.text  = $"{alliesAlive}/{alliesTotal}";
        if (enemiesAliveText) enemiesAliveText.text = $"{enemiesAlive}/{enemiesTotal}";
    }

    /// <summary>Enables or disables all player action buttons.</summary>
    public void SetActionButtonsInteractable(bool value)
    {
        if (btnAttack) btnAttack.interactable = value;
        if (btnDefend) btnDefend.interactable = value;
        if (btnWait)   btnWait.interactable   = value;
        if (btnEscape) btnEscape.interactable = value;
        if (btnMove)   btnMove.interactable   = value;
    }
}