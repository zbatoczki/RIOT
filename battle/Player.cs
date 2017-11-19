using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains various stats such as health and damage
/// </summary>
public class Player : BasePlayer {

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
}
