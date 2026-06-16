using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Left Panel - Selected Unit")]
    public TMP_Text unitNameText;
    public TMP_Text unitLevelText;
    public Image hpBar;
    public TMP_Text hpText;
    public Image mpBar;
    public TMP_Text mpText;
    public Image ultimateBar;
    public TMP_Text ultimateText;
    public TMP_Text statsText;
    public TMP_Text effectsText;
    public TMP_Text bonusText;

    [Header("Squad Roster")]
    public TMP_Text squadRosterText;

    [Header("Action Buttons")]
    public Button btnDefend;
    public Button btnWait;
    public Button btnEscape;
    public Button btnItems;
    public Button btnMagic;
    public Button btnSpecial;

    [Header("Magic Panel")]
    public GameObject magicPanel;
    public TMP_Text magicListText;
    public TMP_Text magicTargetingText;

    [Header("Top Panel")]
    public TMP_Text turnText;
    public TMP_Text phaseText;
    public TMP_Text currentUnitText;

    [Header("Turn Order")]
    public TurnOrderUI turnOrderUI;

    [Header("Right Panel")]
    public TMP_Text alliesAliveText;
    public TMP_Text enemiesAliveText;
    public TMP_Text enemyRosterText;

    private BattleManager _battleManager;

    private void Start()
    {
        _battleManager = FindObjectOfType<BattleManager>();

        btnDefend?.onClick.AddListener(() => _battleManager?.OnPlayerDefend());
        btnWait?.onClick.AddListener(() => _battleManager?.OnPlayerWait());
        btnMagic?.onClick.AddListener(() => _battleManager?.OnPlayerSelectMagic());
        btnSpecial?.onClick.AddListener(() => _battleManager?.OnPlayerUltimate());

        if (magicPanel != null) magicPanel.SetActive(false);
    }

    public void UpdateUnitUI(Unit unit)
    {
        if (unit == null) return;

        if (unitNameText != null) unitNameText.text = unit.unitName;
        if (unitLevelText != null) unitLevelText.text = $"Lvl. {unit.unitLevel}";

        if (hpBar != null) hpBar.fillAmount = unit.maxHP > 0 ? (float)unit.currentHP / unit.maxHP : 0f;
        if (hpText != null) hpText.text = $"{unit.currentHP}/{unit.maxHP}";

        if (mpBar != null) mpBar.fillAmount = unit.maxMP > 0 ? (float)unit.currentMP / unit.maxMP : 0f;
        if (mpText != null) mpText.text = $"{unit.currentMP}/{unit.maxMP}";

        if (ultimateBar != null) ultimateBar.fillAmount = unit.maxUltimateCharge > 0 ? (float)unit.currentUltimateCharge / unit.maxUltimateCharge : 0f;
        if (ultimateText != null) ultimateText.text = $"{unit.currentUltimateCharge}/{unit.maxUltimateCharge}";

        if (statsText != null)
            statsText.text = $"ATK {unit.attack}  DEF {unit.defense}\n" +
                             $"STR {unit.strength}  AGI {unit.agility}\n" +
                             $"END {unit.endurance}  INT {unit.intelligence}\n" +
                             $"CHA {unit.charisma}  LUK {unit.luck}\n" +
                             $"Rank: {unit.rank}   Type: {unit.habitatType}";

        if (effectsText != null)
        {
            string effects = "";
            var abilityComp = unit.GetComponent<AbilityComponent>();
            if (abilityComp != null)
            {
                if (abilityComp.ultimateAbility != null)
                    effects += $"Ult: {abilityComp.ultimateAbility.abilityName}\n";
                if (abilityComp.passiveAbilities.Count > 0)
                    effects += $"Passives: {abilityComp.passiveAbilities.Count}\n";
            }
            effectsText.text = effects;
        }

        if (btnMagic != null)
        {
            var caster = unit.GetComponent<SpellCaster>();
            btnMagic.interactable = caster != null && caster.knownSpells.Count > 0
                && unit.currentMP > 0 && unit.IsAlive;
        }

        if (btnSpecial != null)
        {
            var abilityComp = unit.GetComponent<AbilityComponent>();
            btnSpecial.interactable = abilityComp != null && abilityComp.CanUseUltimate();
        }
    }

    public void UpdateTopPanel(int turnNumber, string phase, string activeUnitName)
    {
        if (turnText != null) turnText.text = turnNumber.ToString();
        if (phaseText != null) phaseText.text = phase;
        if (currentUnitText != null) currentUnitText.text = activeUnitName;
    }

    public void UpdateBattleStats(int alliesAlive, int alliesTotal, int enemiesAlive, int enemiesTotal)
    {
        if (alliesAliveText != null) alliesAliveText.text = $"{alliesAlive}/{alliesTotal}";
        if (enemiesAliveText != null) enemiesAliveText.text = $"{enemiesAlive}/{enemiesTotal}";
    }

    public void UpdateSquadList(UnitSquad squad, Unit selected)
    {
        if (squadRosterText == null) return;

        string roster = "<b>Squad</b>\n";
        for (int i = 0; i < squad.units.Count; i++)
        {
            var u = squad.units[i];
            if (u == null) continue;

            string marker = u == selected ? "▶ " : "  ";
            string hpInfo = u.IsAlive
                ? $"<color=#{ColorToHex(GetHpColor(u))}>HP:{u.currentHP}/{u.maxHP}</color>"
                : "<color=red>DEAD</color>";
            string mpInfo = u.IsAlive ? $" MP:{u.currentMP}/{u.maxMP}" : "";
            roster += $"{marker}{i + 1}. {u.unitName} [{hpInfo}{mpInfo}]\n";
        }
        squadRosterText.text = roster;
    }

    public void UpdateEnemySquadList(UnitSquad squad)
    {
        if (enemyRosterText == null) { Debug.LogWarning("UIManager: enemyRosterText not assigned"); return; }
        if (squad == null) { enemyRosterText.text = "No enemies"; Debug.LogWarning("UIManager: enemySquad is null"); return; }
        string roster = "<b>Enemies</b>\n";
        for (int i = 0; i < squad.units.Count; i++)
        {
            var u = squad.units[i];
            if (u == null) continue;
            string hpInfo = u.IsAlive
                ? $"<color=#{ColorToHex(GetHpColor(u))}>HP:{u.currentHP}/{u.maxHP}</color>"
                : "<color=red>DEAD</color>";
            roster += $"{i + 1}. {u.unitName} [{hpInfo}]\n";
        }
        enemyRosterText.text = roster;
    }

    public void UpdateTurnOrderDisplay()
    {
        if (turnOrderUI == null || _battleManager?.turnManager == null) return;
        var queue = _battleManager.turnManager.GetTurnOrder();
        var current = _battleManager.turnManager.CurrentUnit;
        int idx = queue.IndexOf(current);
        if (idx < 0) idx = 0;
        turnOrderUI.UpdateTurnOrder(queue, idx, current);
    }

    public void ShowMagicPanel(SpellCaster caster)
    {
        if (magicPanel != null) magicPanel.SetActive(true);
        if (magicListText != null && caster != null)
        {
            string list = "<b>Select Spell:</b>\n";
            for (int i = 0; i < caster.knownSpells.Count; i++)
            {
                var s = caster.knownSpells[i];
                string canCast = caster.CanCast(s)
                    ? "<color=green>✓</color>"
                    : "<color=red>✗</color>";
                string schoolColor = GetSchoolColor(s.school);
                list += $"[{i + 1}] <color={schoolColor}>{s.spellName}</color> {canCast}\n" +
                        $"     MP:{s.mpCost} PWR:{s.power} RNG:{s.range}";
                if (s.areaOfEffect > 0)
                    list += $" AOE:{s.areaOfEffect}";
                list += "\n";
            }
            list += "\nPress number key to select spell";
            magicListText.text = list;
        }
        if (magicTargetingText != null) magicTargetingText.text = "";
    }

    public void ShowTargetingInfo(SpellBase spell)
    {
        if (magicPanel != null) magicPanel.SetActive(true);
        if (magicListText != null)
        {
            string schoolColor = GetSchoolColor(spell.school);
            magicListText.text = $"<color={schoolColor}><b>{spell.spellName}</b></color>\n" +
                                 $"PWR: {spell.power}  RNG: {spell.range}\n" +
                                 $"<color=yellow>Click a target cell</color>\n" +
                                 $"Press ESC to cancel";
        }
    }

    public void HideMagicPanel()
    {
        if (magicPanel != null) magicPanel.SetActive(false);
    }

    public void SetActionButtonsInteractable(bool value)
    {
        if (btnDefend != null) btnDefend.interactable = value;
        if (btnWait != null) btnWait.interactable = value;
        if (btnEscape != null) btnEscape.interactable = value;
        if (btnItems != null) btnItems.interactable = value;
        if (btnMagic != null) btnMagic.interactable = value;
        if (btnSpecial != null)
        {
            var current = _battleManager?.turnManager?.CurrentUnit;
            var abilityComp = current?.GetComponent<AbilityComponent>();
            btnSpecial.interactable = value && abilityComp != null && abilityComp.CanUseUltimate();
        }
    }

    private static Color GetHpColor(Unit unit)
    {
        if (unit == null) return Color.white;
        float pct = (float)unit.currentHP / unit.maxHP;
        if (pct > 0.6f) return Color.green;
        if (pct > 0.3f) return Color.yellow;
        return Color.red;
    }

    private static string GetSchoolColor(MagicSchool school)
    {
        switch (school)
        {
            case MagicSchool.Fire: return "#FF4444";
            case MagicSchool.Ice: return "#44CCFF";
            case MagicSchool.Lightning: return "#FFD700";
            case MagicSchool.Dark: return "#AA44FF";
            case MagicSchool.Light: return "#FFFFFF";
            case MagicSchool.Earth: return "#8B4513";
            case MagicSchool.Air: return "#87CEEB";
            case MagicSchool.Time: return "#FF69B4";
            default: return "white";
        }
    }

    private static string ColorToHex(Color c)
    {
        return ColorUtility.ToHtmlStringRGB(c);
    }
}
