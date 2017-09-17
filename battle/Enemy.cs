using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Contains various stats such as health and damage
/// </summary>
[System.Serializable]
public class Enemy
{
    public int maxHP = 9999;
    public int currentHP = 9999;

    public int maxMP = 999;
    public int currentMP = 999;

    public int attackDmg = 1000;
    public int magicDmg = 2000;

    public enum STATS
    {
        CURRENTHP,
        MAXHP,
        CURRENTMP,
        MAXMP
    }
}
