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

    public GridManager gridManager;
    public UIManager uiManager;
    private Unit selectedUnit;

    void Start()
    {
        state = BattleState.START;
        SetupBattle();
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

            if (playerUnit.gridPosition == cell)
            {
                selectedUnit = playerUnit;
                // TODO: Подсветка
                return;
            }
            if (selectedUnit != null && playerUnit == selectedUnit)
            {
                if (gridManager.IsCellOccupied(cell)) return;
                var path = PathFindingHelper.FindPath(gridManager.grid, selectedUnit.gridPosition, cell);

                if (path.IsValid && path.Length > 1)
                {
                    gridManager.MoveUnit(selectedUnit, path.End);
                    uiManager.UpdateUnitUI(selectedUnit);
                    selectedUnit = null;
                    NextTurn();
                    return;
                }
            }
            if (selectedUnit != null && cell == enemyUnit.gridPosition)
            {
                if (IsAdjacent(selectedUnit.gridPosition, cell))
                {
                    OnPlayerAttack();
                    selectedUnit = null;
                }
            }
        }
    }

    void SetupBattle()
    {
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
            selectedUnit = null;
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
        uiManager.UpdateUnitUI(playerUnit);
        uiManager.UpdateUnitUI(enemyUnit);
        // Show player actions if needed
    }

    public void OnPlayerAttack()
    {
        if (state != BattleState.PLAYERTURN) return;
        int damage = Mathf.Max(1, playerUnit.attack - enemyUnit.defense);
        enemyUnit.TakeDamage(damage);
        uiManager.UpdateUnitUI(enemyUnit);
        NextTurn();
    }

    public void OnPlayerDefend()
    {
        if (state != BattleState.PLAYERTURN) return;
        // TODO: Добавить механику защиты (например, временно увеличить defense)
        NextTurn();
    }

    public void OnPlayerWait()
    {
        if (state != BattleState.PLAYERTURN) return;
        NextTurn();
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);

        if (!playerUnit.IsAlive)
        {
            NextTurn();
            yield break;
        }

        // Если враг рядом с игроком — атакует
        if (IsAdjacent(enemyUnit.gridPosition, playerUnit.gridPosition))
        {
            int damage = Mathf.Max(1, enemyUnit.attack - playerUnit.defense);
            playerUnit.TakeDamage(damage);
            uiManager.UpdateUnitUI(playerUnit);
        }
        else
        {
            // Поиск пути к игроку
            var path = PathFindingHelper.FindPath(
                gridManager.grid,
                enemyUnit.gridPosition,
                playerUnit.gridPosition
            );
            if (path.IsValid && path.Length > 1)
            {
                Vector2Int nextCell = path[1];
                if (!gridManager.IsCellOccupied(nextCell))
                    gridManager.MoveUnit(enemyUnit, nextCell);
            }
        }
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    void OnBattleEnd(bool playerWon)
    {
        // TODO: Вывести UI победы/поражения
        if (playerWon)
            Debug.Log("Игрок победил!");
        else
            Debug.Log("Игрок проиграл!");
    }
}
