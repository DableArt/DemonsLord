using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattlePhase
{
    Start,
    SelectUnit,
    SelectAction,
    SelectMoveCell,
    SelectAttackTarget,
    EnemyTurn,
    BattleWon,
    BattleLost
}

public class BattleSystem : MonoBehaviour
{
    [Header("Units")]
    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    [Header("Settings & References")]
    public BattleSettings battleSettings;
    public GridManager gridManager;
    public UIManager uiManager;
    public CellHighlighter cellHighlighter;

    public BattlePhase phase { get; private set; }

    private List<Unit> _turnOrder = new List<Unit>();
    private int _turnIndex;
    private int _roundNumber;
    private Unit _currentUnit;
    private Unit _selectedPlayerUnit;

    private List<Vector2Int> _validMoveCells = new List<Vector2Int>();
    private List<Unit> _validAttackTargets = new List<Unit>();

    void Start()
    {
        phase = BattlePhase.Start;
        uiManager?.Init(this);
        SetupBattle();
    }

    void Update()
    {
        if (phase == BattlePhase.SelectUnit ||
            phase == BattlePhase.SelectMoveCell ||
            phase == BattlePhase.SelectAttackTarget)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                Vector2Int cell = gridManager.WorldToCell(mouseWorld);
                HandleCellClick(cell);
            }
        }
    }

    void SetupBattle()
    {
        // Default initialisation for units not yet configured in Inspector
        if (playerUnits.Count > 0 && playerUnits[0] != null && playerUnits[0].maxHP == 0)
            playerUnits[0].Init("Hero", 1, 100, 30, 20, 10, 15, 5);
        if (enemyUnits.Count > 0 && enemyUnits[0] != null && enemyUnits[0].maxHP == 0)
            enemyUnits[0].Init("Skeleton", 1, 80, 10, 15, 8, 10, 2);

        // Register starting positions on the grid
        foreach (var u in playerUnits)
            if (u != null) gridManager.SetCellOccupied(u.gridPosition, true);
        foreach (var u in enemyUnits)
            if (u != null) gridManager.SetCellOccupied(u.gridPosition, true);

        _roundNumber = 0;
        BuildTurnOrder();
        _turnIndex = 0;

        UpdateAliveCountsUI();
        NextTurn();
    }

    void BuildTurnOrder()
    {
        _turnOrder = TurnOrderHelper.BuildTurnOrder(playerUnits, enemyUnits, gridManager.width);
    }

    void NextTurn()
    {
        if (CheckBattleEnd()) return;

        // Skip units that died mid-round
        while (_turnIndex < _turnOrder.Count && (_turnOrder[_turnIndex] == null || !_turnOrder[_turnIndex].IsAlive))
            _turnIndex++;

        if (_turnIndex >= _turnOrder.Count)
        {
            // New round: rebuild order with survivors
            _roundNumber++;
            BuildTurnOrder();
            _turnIndex = 0;
        }

        if (CheckBattleEnd()) return;

        _currentUnit = _turnOrder[_turnIndex];
        _turnIndex++;

        // Reset defending state at the start of a unit's turn
        _currentUnit.isDefending = false;

        UpdateAliveCountsUI();

        if (playerUnits.Contains(_currentUnit))
            StartPlayerTurn(_currentUnit);
        else
            StartEnemyTurn(_currentUnit);
    }

    void StartPlayerTurn(Unit unit)
    {
        _selectedPlayerUnit = unit;
        _validMoveCells.Clear();
        _validAttackTargets.Clear();

        uiManager?.UpdateUnitUI(unit);
        uiManager?.UpdateTurnInfo(_roundNumber, true, unit.unitName);
        uiManager?.SetActionButtonsInteractable(true);

        phase = BattlePhase.SelectAction;
    }

    void StartEnemyTurn(Unit unit)
    {
        phase = BattlePhase.EnemyTurn;
        uiManager?.UpdateUnitUI(unit);
        uiManager?.UpdateTurnInfo(_roundNumber, false, unit.unitName);
        uiManager?.SetActionButtonsInteractable(false);
        StartCoroutine(EnemyTurnCoroutine(unit));
    }

    // ─── Player action handlers (called by UI buttons) ───────────────────────

    public void OnPlayerSelectMove()
    {
        if (phase != BattlePhase.SelectAction || _selectedPlayerUnit == null) return;

        _validMoveCells = BattleAI.BFSReachable(
            _selectedPlayerUnit.gridPosition, gridManager.grid,
            _selectedPlayerUnit.agility, _selectedPlayerUnit.isFlying);

        cellHighlighter?.ShowMoveRange(_selectedPlayerUnit);
        phase = BattlePhase.SelectMoveCell;
    }

    public void OnPlayerSelectAttack()
    {
        if (phase != BattlePhase.SelectAction || _selectedPlayerUnit == null) return;

        _validAttackTargets = enemyUnits
            .Where(e => e.IsAlive && IsAdjacent(_selectedPlayerUnit.gridPosition, e.gridPosition))
            .ToList();

        cellHighlighter?.ShowAttackTargets(_selectedPlayerUnit, enemyUnits);
        phase = BattlePhase.SelectAttackTarget;
    }

    public void OnPlayerDefend()
    {
        if (phase != BattlePhase.SelectAction) return;
        _selectedPlayerUnit.isDefending = true;
        FinishPlayerAction();
    }

    public void OnPlayerWait()
    {
        if (phase != BattlePhase.SelectAction) return;
        FinishPlayerAction();
    }

    public void OnPlayerEscape()
    {
        if (phase != BattlePhase.SelectAction) return;

        int totalPlayerLUK = playerUnits.Where(u => u.IsAlive).Sum(u => u.luck);
        int totalEnemyLUK  = enemyUnits.Where(u => u.IsAlive).Sum(u => u.luck);
        float denom = totalPlayerLUK + totalEnemyLUK;
        float escapeChance = denom > 0 ? totalPlayerLUK / denom : 0.5f;

        if (Random.value < escapeChance)
        {
            Debug.Log("Побег удался!");
            OnBattleEscaped();
        }
        else
        {
            Debug.Log("Побег не удался");
            FinishPlayerAction();
        }
    }

    // ─── Cell click handler ───────────────────────────────────────────────────

    void HandleCellClick(Vector2Int cell)
    {
        switch (phase)
        {
            case BattlePhase.SelectUnit:
                // Allow clicking the current player unit to confirm selection
                var clicked = playerUnits.FirstOrDefault(u => u.IsAlive && u.gridPosition == cell);
                if (clicked != null && clicked == _currentUnit)
                {
                    _selectedPlayerUnit = clicked;
                    phase = BattlePhase.SelectAction;
                }
                break;

            case BattlePhase.SelectMoveCell:
                if (_validMoveCells.Contains(cell))
                {
                    cellHighlighter?.Clear();
                    gridManager.MoveUnit(_selectedPlayerUnit, cell);
                    uiManager?.UpdateUnitUI(_selectedPlayerUnit);
                    FinishPlayerAction();
                }
                break;

            case BattlePhase.SelectAttackTarget:
                var target = _validAttackTargets.FirstOrDefault(e => e.gridPosition == cell);
                if (target != null)
                {
                    int damage = CalculateDamage(_selectedPlayerUnit, target);
                    target.TakeDamage(damage);
                    Debug.Log($"{_selectedPlayerUnit.unitName} атакует {target.unitName} на {damage} урона. HP: {target.currentHP}/{target.maxHP}");
                    cellHighlighter?.Clear();
                    uiManager?.UpdateUnitUI(_selectedPlayerUnit);
                    FinishPlayerAction();
                }
                break;
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    void FinishPlayerAction()
    {
        cellHighlighter?.Clear();
        uiManager?.SetActionButtonsInteractable(false);
        NextTurn();
    }

    int CalculateDamage(Unit attacker, Unit target)
    {
        int rawDmg = Mathf.Max(1, attacker.attack - target.defense);
        if (target.isDefending && battleSettings != null)
            return Mathf.Max(1, Mathf.RoundToInt(rawDmg / battleSettings.defendDamageMultiplier));
        return rawDmg;
    }

    bool CheckBattleEnd()
    {
        bool anyPlayerAlive = playerUnits.Any(u => u.IsAlive);
        bool anyEnemyAlive  = enemyUnits.Any(u => u.IsAlive);

        if (!anyPlayerAlive)
        {
            phase = BattlePhase.BattleLost;
            cellHighlighter?.Clear();
            uiManager?.SetActionButtonsInteractable(false);
            Debug.Log("Игрок проиграл!");
            return true;
        }
        if (!anyEnemyAlive)
        {
            phase = BattlePhase.BattleWon;
            cellHighlighter?.Clear();
            uiManager?.SetActionButtonsInteractable(false);
            Debug.Log("Игрок победил!");
            return true;
        }
        return false;
    }

    void OnBattleEscaped()
    {
        phase = BattlePhase.BattleLost;
        cellHighlighter?.Clear();
        uiManager?.SetActionButtonsInteractable(false);
        // Further handling (e.g., return to world map) can be added here
    }

    void UpdateAliveCountsUI()
    {
        uiManager?.UpdateAliveCounts(
            playerUnits.Count(u => u.IsAlive), playerUnits.Count,
            enemyUnits.Count(u => u.IsAlive), enemyUnits.Count);
    }

    // ─── Enemy AI coroutine ───────────────────────────────────────────────────

    private IEnumerator EnemyTurnCoroutine(Unit unit)
    {
        yield return new WaitForSeconds(0.8f);

        if (battleSettings != null)
        {
            var decision = BattleAI.Evaluate(unit, enemyUnits, playerUnits, gridManager.grid, battleSettings);
            ExecuteAIDecision(unit, decision);
        }
        else
        {
            FallbackEnemyAI(unit);
        }

        yield return new WaitForSeconds(0.5f);
        NextTurn();
    }

    void ExecuteAIDecision(Unit unit, AIDecision decision)
    {
        switch (decision.action)
        {
            case AIAction.Move:
                if (!gridManager.IsCellOccupied(decision.moveTarget))
                    gridManager.MoveUnit(unit, decision.moveTarget);
                break;
            case AIAction.Attack:
                if (decision.attackTarget != null && decision.attackTarget.IsAlive)
                {
                    int dmg = CalculateDamage(unit, decision.attackTarget);
                    decision.attackTarget.TakeDamage(dmg);
                    Debug.Log($"{unit.unitName} атакует {decision.attackTarget.unitName} на {dmg} урона.");
                    uiManager?.UpdateUnitUI(decision.attackTarget);
                }
                break;
            case AIAction.Defend:
                unit.isDefending = true;
                break;
            case AIAction.Wait:
                break;
        }
    }

    void FallbackEnemyAI(Unit unit)
    {
        var adjacent = playerUnits.FirstOrDefault(p => p.IsAlive && IsAdjacent(unit.gridPosition, p.gridPosition));
        if (adjacent != null)
        {
            int dmg = CalculateDamage(unit, adjacent);
            adjacent.TakeDamage(dmg);
            Debug.Log($"{unit.unitName} атакует {adjacent.unitName} на {dmg} урона.");
        }
        else
        {
            var nearest = playerUnits.Where(p => p.IsAlive)
                .OrderBy(p => Manhattan(unit.gridPosition, p.gridPosition))
                .FirstOrDefault();
            if (nearest != null)
            {
                var path = PathFindingHelper.FindPath(gridManager.grid, unit.gridPosition, nearest.gridPosition);
                if (path.IsValid && path.Length > 1)
                    gridManager.MoveUnit(unit, path[1]);
            }
        }
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;

    private static int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}

