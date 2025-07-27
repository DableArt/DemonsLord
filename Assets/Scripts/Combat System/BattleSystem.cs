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

 //   public UIManager uiManager;
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
    }

    void OnPlayerTurn()
    {
       // uiManager.UpdateUnitUI(playerUnit);
       // uiManager.UpdateUnitUI(enemyUnit);
        // Show player actions if needed
    }

    public void OnPlayerAttack()
    {
        if (state != BattleState.PLAYERTURN) return;
        int damage = Mathf.Max(1, playerUnit.attack - enemyUnit.defense);
        enemyUnit.TakeDamage(damage);
        //uiManager.UpdateUnitUI(enemyUnit);
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

    void OnBattleEnd(bool playerWon)
    {
        // TODO: Вывести UI победы/поражения
        if (playerWon)
            Debug.Log("Игрок победил!");
        else
            Debug.Log("Игрок проиграл!");
    }
}
