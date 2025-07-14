using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // ��������� ������������ ���� ��� NavMeshAgent

public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;

    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;
    public int attack;
    public int defense;
    public int agility;
    public int luck;

    public bool IsAlive => currentHP > 0;

    public void Init(string name, int level, int hp, int mp, int atk, int def, int agi, int luk)
    {
        unitName = name;
        unitLevel = level;
        maxHP = currentHP = hp;
        maxMP = currentMP = mp;
        attack = atk;
        defense = def;
        agility = agi;
        luck = luk;
    }

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;
        return currentHP <= 0;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }

    public void UseMana(int amount)
    {
        currentMP -= amount;
        if (currentMP < 0) currentMP = 0;
    }

    public void RestoreMana(int amount)
    {
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP;
    }
}