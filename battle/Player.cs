using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains various stats such as health and damage
/// </summary>
public class Player {

    public int maxHP = 9999;
    public int currentHP = 9999;

    public int maxMP = 999;
    public int currentMP = 999;

    public int attackDmg = 100;
    public int magicDmg = 2000;
    [HideInInspector] public float defense = 0f;

    //public int banMaxQuantity = 3;
    //public int banQuantity = 3; //only item that requires incrementing/decrementing

    public enum STATS
    {
        CURRENTHP,
        MAXHP,
        CURRENTMP,
        MAXMP,
        BANQUANTITY,
        MAXBANQUANTITY 
    }
}
