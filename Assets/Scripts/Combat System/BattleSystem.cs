using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public Unit playerUnit;
    public Unit enemyUnit;

    public BattleState state;
    private Queue<Unit> turnQueue = new Queue<Unit>();

    // AI Related
    public float enemyAttackRange = 2f; // Дистанция, с которой враг атакует.
    public float timeBetweenAttacks = 2f; // Интервал между атаками.
    private float nextAttackTime;

    public GridManager gridManager;
    private Unit selectedUnit;

    void Start()
    {
        state = BattleState.START;
        SetupBattle();
        // gridManager должен быть назначен в инспекторе или через FindObjectOfType
    }

    void Update()
    {
        if (state == BattleState.PLAYERTURN)
        {
            HandlePlayerInput();
        }
    }

    void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(mouseWorld.x), Mathf.RoundToInt(mouseWorld.y));
            // Проверка: клик по юниту
            if (playerUnit.gridPosition == cell)
            {
                selectedUnit = playerUnit;
                // TODO: подсветить выбранного юнита
                return;
            }
            // Если выбран юнит и клик по клетке
            if (selectedUnit != null && !gridManager.IsCellOccupied(cell))
            {
                // Построить путь
                var path = AStarPathfinder.FindPath(selectedUnit.gridPosition, cell, InvertOccupiedForUnit(selectedUnit));
                if (path.Count > 1)
                {
                    // Освободить старую клетку
                    gridManager.SetCellOccupied(selectedUnit.gridPosition, false);
                    // Занять новую (последнюю в пути)
                    gridManager.SetCellOccupied(cell, true);
                    selectedUnit.MoveAlongPath(path);
                    selectedUnit.gridPosition = cell;
                    selectedUnit = null;
                    // После движения — завершить ход
                    NextTurn();
                }
            }
            // Если клик по врагу и он в соседней клетке — атаковать
            if (selectedUnit != null && cell == enemyUnit.gridPosition && IsAdjacent(selectedUnit.gridPosition, cell))
            {
                OnPlayerAttack();
                selectedUnit = null;
            }
        }
    }

    void SetupBattle()
    {
        // Пример инициализации (заменить на создание через префабы/данные)
        playerUnit.Init("Hero", 1, 100, 30, 20, 10, 15, 5);
        enemyUnit.Init("Skeleton", 1, 80, 10, 15, 8, 10, 2);

        turnQueue.Clear();
        if (playerUnit.agility >= enemyUnit.agility)
        {
            turnQueue.Enqueue(playerUnit);
            turnQueue.Enqueue(enemyUnit);
        }
        else
        {
            turnQueue.Enqueue(enemyUnit);
            turnQueue.Enqueue(playerUnit);
        }
        NextTurn();
    }

    void NextTurn()
    {
        if (!playerUnit.IsAlive)
        {
            state = BattleState.LOST;
            OnBattleEnd(false);
            return;
        }
        if (!enemyUnit.IsAlive)
        {
            state = BattleState.WON;
            OnBattleEnd(true);
            return;
        }
        Unit current = turnQueue.Dequeue();
        turnQueue.Enqueue(current);
        if (current == playerUnit)
        {
            state = BattleState.PLAYERTURN;
            OnPlayerTurn();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    void OnPlayerTurn()
    {
        // Здесь должен быть вызов UI для выбора действия
        // Пример: ShowPlayerActions();
    }

    public void OnPlayerAttack()
    {
        if (state != BattleState.PLAYERTURN) return;
        int damage = Mathf.Max(1, playerUnit.attack - enemyUnit.defense);
        enemyUnit.TakeDamage(damage);
        // Здесь можно обновить UI
        NextTurn();
    }

    public void OnPlayerDefend()
    {
        if (state != BattleState.PLAYERTURN) return;
        // Пример: увеличиваем защиту на 1 ход (реализовать баффы позже)
        NextTurn();
    }

    public void OnPlayerWait()
    {
        if (state != BattleState.PLAYERTURN) return;
        // Пропуск хода
        NextTurn();
    }

    bool[,] InvertOccupiedForUnit(Unit unit)
    {
        // Копия сетки, где клетка юнита считается свободной (чтобы он мог выйти из неё)
        var occ = (bool[,])gridManager.GetOccupiedGrid().Clone();
        occ[unit.gridPosition.x, unit.gridPosition.y] = false;
        return occ;
    }

    bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        // AI: если враг рядом с игроком — атаковать
        if (IsAdjacent(enemyUnit.gridPosition, playerUnit.gridPosition))
        {
            int damage = Mathf.Max(1, enemyUnit.attack - playerUnit.defense);
            playerUnit.TakeDamage(damage);
            // TODO: обновить UI
        }
        else
        {
            // Построить путь к игроку
            var path = AStarPathfinder.FindPath(enemyUnit.gridPosition, playerUnit.gridPosition, InvertOccupiedForUnit(enemyUnit));
            if (path.Count > 1)
            {
                // Освободить старую клетку
                gridManager.SetCellOccupied(enemyUnit.gridPosition, false);
                // Занять новую (следующую по пути)
                Vector2Int nextCell = path[1];
                gridManager.SetCellOccupied(nextCell, true);
                enemyUnit.MoveAlongPath(new List<Vector2Int> { nextCell });
                enemyUnit.gridPosition = nextCell;
            }
        }
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    void OnBattleEnd(bool playerWon)
    {
        // Вызов UI: победа/поражение
    }
}