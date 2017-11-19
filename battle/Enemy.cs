using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Contains various stats such as health and damage
/// </summary>
[System.Serializable]
public class Enemy : BasePlayer
{
    public enum STATS
    {
        CURRENTHP,
        MAXHP,
        CURRENTMP,
        MAXMP
    }

}
