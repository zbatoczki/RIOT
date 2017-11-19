using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayer {

    private int maxHP = 9999;
    private int currentHP = 9999;

    private int maxMP = 999;
    private int currentMP = 999;

    private int attackDmg = 1000;
    private int magicDmg = 2000;

    public int GetMaxHP()
    {
        return maxHP;
    }
    public int GetMaxMP()
    {
        return maxMP;
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }
    public int GetCurrentMP()
    {
        return currentMP;
    }

    public int GetMissingHP()
    {
        return maxHP - currentHP;
    }

    public int GetMissingMP()
    {
        return maxMP - currentMP;
    }

    public int GetMagicDmg()
    {
        return magicDmg;
    }

    public void SetHP(int hp)
    {
        currentHP = hp;
    }

    public void SetMP(int mp)
    {
        currentMP = mp;
    }

    public void AddHP(int health)
    {
        currentHP += health;
    }

    public void AddMP(int magic)
    {
        currentMP += magic;
    }

    public void SubtractHP(int cost)
    {
        currentHP -= cost;
    }

    public void SubtractMP(int cost)
    {
        currentMP -= cost;
    }

    public int calcDamage()
    {
        return Random.Range(1000, 1501);
    }

    public int calcMagicDamage()
    {
        return Random.Range(1000, 1501);
    }
}
