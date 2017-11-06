using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains various stats such as health and damage
/// </summary>
public class Player {

    private int maxHP = 9999;
    private int currentHP = 9999;

    private int maxMP = 999;
    private int currentMP = 999;

    private int attackDmg = 100;
    private int magicDmg = 2000;
    [HideInInspector] public float defense = 0f;

    private int potionMaxQuantity = 3;
    private int potionQuantity = 3; //only item that requires incrementing/decrementing

    private int magicPotionMaxQuantity = 3;
    private int magicPotionQuantity = 3; //only item that requires incrementing/decrementing

    private int restoreQuantity = 1;


    public enum STATS
    {
        CURRENTHP,
        MAXHP,
        CURRENTMP,
        MAXMP,
        BANQUANTITY,
        MAXBANQUANTITY 
    }

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

    public void SubtractRestore()
    {
        restoreQuantity--;
    }

    public bool PotionsAvail()
    {
        return potionQuantity > 0;
    }

    public bool MagicPotionsAvail()
    {
        return magicPotionQuantity > 0;
    }

    public bool RestoreAvail()
    {
        return restoreQuantity > 0;
    }

    public void SubtractPotion()
    {
        potionQuantity--;
    }

    public void SubtractMagicPotion()
    {
        magicPotionQuantity--;
    }

    public int calcDamage()
    {
        return Random.Range(1000, 1501);
    }

    
}
