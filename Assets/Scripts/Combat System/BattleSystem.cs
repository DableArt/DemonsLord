using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Добавляем пространство имен для NavMeshAgent

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

    void Start()
    {
        state = BattleState.START;
        SetupBattle();
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

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);

        // Enemy AI Logic
        float distanceToPlayer = Vector3.Distance(enemyUnit.transform.position, playerUnit.transform.position);

        if (distanceToPlayer <= enemyAttackRange && Time.time >= nextAttackTime)
        {
            // Attack the Player
            int damage = Mathf.Max(1, enemyUnit.attack - playerUnit.defense);
            playerUnit.TakeDamage(damage);
            // Здесь можно обновить UI
            nextAttackTime = Time.time + timeBetweenAttacks;
        }
        else
        {
            // Move towards the Player
            NavMeshAgent agent = enemyUnit.GetComponent<NavMeshAgent>();

            if (agent != null)
            {
                agent.SetDestination(playerUnit.transform.position);
            }
            else
            {
                Debug.LogError("NavMeshAgent not found on enemy unit.  Add a NavMeshAgent component to the enemy prefab.");
            }
        }
        yield return new WaitForSeconds(1f); // Added small delay after enemy action

        NextTurn();
    }

    void OnBattleEnd(bool playerWon)
    {
        // Вызов UI: победа/поражение
    }
}