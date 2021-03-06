﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manages player's turn
/// </summary>
public class PlayerStateMachine : MonoBehaviour {

    private Player player;
    private EnemyStateMachine esm;
    private BattleSystem battleSystem;
    private BattleUIManager battleUI;
    private bool autoPlay = true;

    private readonly int MAGIC_COST = 300;

    public enum PlayerStates
    {
        START,
        WAIT,
        PERFORMACTION,
        WAITFORACTION,
        PROCESS,
        END
    }

    public enum PlayerActions
    {
        ATTACK,
        DEFEND,
        MAGIC,
        POTION,
        MAGICPOTION,
        BAN,
        RESTORE,
        LIMIT
    }

    public PlayerStates currentState; //keeps track of current state in the player's turn

    //used for performing player action
    private PlayerActions chosenAction;
    private GameObject enemyGameObj;
    private Vector3 startPosition;
    private Animator anim;
    private float animSpeed = 5f;
    [SerializeField] private Sprite defendSprite;
    [SerializeField] private Sprite magicSprite;
    [SerializeField] private Sprite democracySprite;

    //audio
    private AudioManager audioManager;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip magicSound;

    // Use this for initialization
    void Awake () {
        player = new Player();    
        anim = GetComponent<Animator>();
        startPosition = transform.position;
        currentState = PlayerStates.WAIT;
        battleSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<BattleSystem>();
        battleUI = GameObject.FindGameObjectWithTag("UIManager").GetComponent<BattleUIManager>();
        enemyGameObj = GameObject.FindGameObjectWithTag("Enemy");
        esm = enemyGameObj.GetComponent<EnemyStateMachine>();
        audioManager = AudioManager.instance;
    }

    public void SetAutoPlay()
    {
        autoPlay = !autoPlay;
        battleUI.SetAutoplayBtn(true, autoPlay);
    }

    private IEnumerator Wait()
    {
        Debug.Log("Waiting...");
        yield return new WaitForSeconds(2);
        Debug.Log("Done waiting!");
    }

    /// <summary>
    /// Start player's turn
    /// </summary>
    public void StartTurn()
    {
        StartCoroutine(Begin());      
    }

    private IEnumerator Begin()
    {
        yield return StartCoroutine(Wait());
        if (!anim.enabled) TurnOnAnim();
        player.defense = 0f;
        BattleSystem.skipEnemyTurn = false;
        CheckStats();
        if (autoPlay)
        {
            Debug.Log("Player is auto running.");
            AutoPlayTurn();
        }
        else
        {
            battleUI.PlayerActionMenuOn();
            audioManager.PlaySoundEffect(audioManager.menuSelection);
        }
    }

    private void AutoPlayTurn()
    {
        int health = player.GetCurrentHP();
        int magic = player.GetCurrentMP();
        int enemyHealth = esm.GetStat(Enemy.STATS.CURRENTHP);
        int enemyMagic = esm.GetStat(Enemy.STATS.CURRENTMP);

        double percentHealthMissing = (double)health / player.GetMaxHP();

        System.Random random = new System.Random();
        chosenAction = BattleSystem.random.NextDouble() >= 0.5 ? PlayerActions.ATTACK : PlayerActions.MAGIC;

         if (chosenAction == PlayerActions.MAGIC && player.GetCurrentMP() < MAGIC_COST)
            if(player.MagicPotionsAvail())
                chosenAction = PlayerActions.MAGICPOTION;
            else
                chosenAction = PlayerActions.ATTACK;

        if (battleUI.LimitReady() && percentHealthMissing >= 0.25)
            chosenAction = PlayerActions.LIMIT;
        else if (percentHealthMissing <= 0.25 && player.PotionsAvail())
            chosenAction = PlayerActions.POTION;
        else if (percentHealthMissing <= 0.1 && player.RestoreAvail())
            chosenAction = PlayerActions.RESTORE;    

        StartCoroutine(PerformAction());

    }

    public void SetState(PlayerStates state) { currentState = state; }
    public void SetAction(PlayerActions action) { chosenAction = action; }

    /// <summary>
    /// Choose action; turn off menus
    /// </summary>
    /// <param name="action">Action to be chosen</param>
    public void SelectAction(int action)
    {
        battleUI.PlayerMenusOff();
        battleUI.HideButtonDesc();
        chosenAction = (PlayerActions)action;
        StartCoroutine(PerformAction());
    }

    /// <summary>
    /// Execute slected action
    /// </summary>
    private IEnumerator PerformAction()
    {
        switch (chosenAction)
        {
            case (PlayerActions.ATTACK): //basic attack
                yield return StartCoroutine(Attack());
                break;
            case (PlayerActions.DEFEND): //applies a % to mitagate on enemy's next attack; also restores minor health
                Defend();
                break;
            case (PlayerActions.MAGIC):
                CastMagic();
                break;
            case (PlayerActions.POTION):
                RestoreHP(0.25f);
                break;
            case (PlayerActions.MAGICPOTION):
                RestoreMP(0.25f);
                break;
            /*case (PlayerActions.BAN):
                Ban();
                break;*/
            case (PlayerActions.RESTORE):
                Restore();
                break;
            case (PlayerActions.LIMIT):
                Democracy();
                break;
        }
        //Invoke("End", 2f);
        End();
    }

    /// <summary>
    /// End player's turn
    /// </summary>
    private void End()
    {
        //Debug.Log("Player has ended their turn.");
        //currentState = PlayerStates.WAIT;
        BattleSystem.isPlayersTurn = false;
        battleSystem.EndTurn();
    }

    /* ///////////////////ACCESS METHODS////////////////////// */
    /// <summary>
    /// Return a sleceted player stat
    /// </summary>
    /// <param name="s">Stat to be returned</param>
    /// <returns>Returns requested stat</returns>
    public int GetStat(Player.STATS s)
    {
        int stat = -1;
        switch(s)
        {
            case (Player.STATS.CURRENTHP): stat = player.GetCurrentHP(); break;
            case (Player.STATS.MAXHP): stat = player.GetMaxHP(); break;
            case (Player.STATS.CURRENTMP): stat = player.GetCurrentMP(); break;
            case (Player.STATS.MAXMP): stat = player.GetMaxMP(); break;
            //case (Player.STATS.BANQUANTITY): stat = player.banQuantity; break;
        }
        return stat;
    }

    /// <summary>
    /// Check player's stats and item quantities and updates menus accordingly
    /// </summary>
    private void CheckStats()
    {
        //check MP
        if (player.GetCurrentMP() < 200)
            battleUI.toggleMagic(false);
        else 
            battleUI.toggleMagic(true);
        //check limit
        if (battleUI.checkPlayerLimit() >= 1.0f)
            battleUI.enableLimitButton();
        //check item counts
        if (player.PotionsAvail())
            battleUI.toggleItems("potion", true);
        else
            battleUI.toggleItems("potion", false);
        if (player.MagicPotionsAvail())
            battleUI.toggleItems("magicpotion", true);
        else
            battleUI.toggleItems("magicpotion", false);
    }

    /* /////////////////PLAYER ANIMATIONS/////////////////// */

    private bool MoveToEnemy(Vector3 targetPosition)
    {
        if (!anim.GetBool("isMoving"))
            anim.SetBool("isMoving", true);
        return targetPosition != (transform.position = Vector3.MoveTowards(transform.position, targetPosition, animSpeed * Time.deltaTime));
    }

    private bool MoveToStart(Vector3 targetPosition)
    {
        gameObject.GetComponent<SpriteRenderer>().flipX = true;
        if (!anim.GetBool("isMoving"))
            anim.SetBool("isMoving", true);
        return targetPosition != (transform.position = Vector3.MoveTowards(transform.position, targetPosition, animSpeed * Time.deltaTime));
    }

    private void TurnOnAnim()
    {
        anim.enabled = true;
    }

    private void TurnOffAnim()
    {
        anim.enabled = false;
    }

    /* /////////////////PLAYER ACTIONS/////////////////// */

    private IEnumerator Attack()
    {
        //Debug.Log("Player is attacking.");
        //ANIMATE PLAYER SPRITE TO ENEMY
        Vector3 enemyPosition = new Vector3(enemyGameObj.transform.position.x - 1.5f, enemyGameObj.transform.position.y);
        //while the player is moving
        while (MoveToEnemy(enemyPosition))
            yield return null;
        anim.SetBool("isMoving", false);
        //perform chosen action animation and calculate damage and/or heals
        yield return new WaitForSeconds(0.5f);
        anim.Play("PlayerAttack");
        audioManager.PlaySoundEffect(attackSound);
        DoDamage(player.calcDamage());        
        yield return new WaitForSeconds(0.5f);
        //animate back to starting position
        while (MoveToStart(startPosition))
            yield return null;
        //reset sprite orientation
        gameObject.GetComponent<SpriteRenderer>().flipX = false;
        anim.SetBool("isMoving", false);
        //Debug.Log("Player has finished attacking.");
    }
    private void Defend()
    {
        //apply a random percentage to mitagte on enemy turn
        player.defense = Mathf.Round(Random.value * 100f) / 100f;
        //Debug.Log("Player will mitigate " + player.defense.ToString("##%") + " of enemy's next attack");
        //restore between 50 - 200 HP
        int extraHealth = BattleSystem.random.Next(50, 201);
        player.AddHP(extraHealth);
        CheckHPOverflow();
        //Debug.Log("Player restores " + extraHealth + " HP.");
        //set the defend sprite until next turn
        TurnOffAnim();
        GetComponent<SpriteRenderer>().sprite = defendSprite;
        PopupTextController.CreatePopupText(extraHealth.ToString(), transform, PopupTextController.Colors.GREEN);
        battleUI.UpdateUI();
    }

    private void CastMagic()
    {
        //Debug.Log("Player casts Magic!");
        TurnOffAnim();
        GetComponent<SpriteRenderer>().sprite = magicSprite;
        audioManager.PlaySoundEffect(magicSound);
        player.SubtractMP(MAGIC_COST);
        Invoke("TurnOnAnim", 1.5f);
        DoDamage(player.GetMagicDmg());
    }

    /// <summary>
    /// Execute limit break ability
    /// </summary>
    private void Democracy()
    {
        TurnOffAnim();
        GetComponent<SpriteRenderer>().sprite = democracySprite;
        audioManager.PlaySoundEffect(magicSound);
        Invoke("TurnOnAnim", 1.5f);

        int dmg = Mathf.RoundToInt(Random.value * esm.GetStat(Enemy.STATS.MAXHP) );

        DoDamage(dmg);
        battleUI.UpdatePlayerLimitBar(-1f);
        battleUI.disableLimitButton();
    }

    /// <summary>
    /// Set HP to max if current exceeds it
    /// </summary>
    private void CheckHPOverflow()
    {
       if (player.GetCurrentHP() > player.GetMaxHP() ) player.SetHP(player.GetMaxHP());
    }

    /// <summary>
    /// Sets MP to max if current exceeds it
    /// </summary>
    private void CheckMPOverflow()
    {
        if (player.GetCurrentMP() > player.GetMaxMP()) player.SetMP(player.GetMaxMP());
    }

    /// <summary>
    /// restore percentage of missing health to player (round to nearest integer)
    /// </summary>
    /// <param name="percentage">Percentage to be restored</param>
    private void RestoreHP(float percentage)
    {
        int missingHP = player.GetMissingHP();
        int healthToRestore = Mathf.RoundToInt(missingHP * percentage);
        player.AddHP(healthToRestore);
        CheckHPOverflow();
        PopupTextController.CreatePopupText(healthToRestore.ToString(), transform, PopupTextController.Colors.GREEN);
        audioManager.PlaySoundEffect(audioManager.recovery);
        battleUI.UpdateUI();
        player.SubtractPotion();
    }

    /// <summary>
    /// restore percentage of missing MP to player (round to nearest integer)
    /// </summary>
    /// <param name="percentage">Percentage to be restored</param>
    private void RestoreMP(float percentage)
    {
        int missingMP = player.GetMissingMP();
        int mpToRestore = Mathf.RoundToInt(missingMP * percentage);
        player.AddMP(mpToRestore);
        CheckMPOverflow();
        PopupTextController.CreatePopupText(mpToRestore.ToString(), transform, PopupTextController.Colors.BLUE);
        audioManager.PlaySoundEffect(audioManager.recovery);
        battleUI.UpdateUI();
        player.SubtractMagicPotion();
    }

    /*private void Ban()
    {
        player.banQuantity--;
        BattleSystem.skipEnemyTurn = true;
        battleUI.UpdateUI();
    }
    */

    /// <summary>
    /// Restore all HP and MP
    /// </summary>
    private void Restore()
    {
        player.SetHP(player.GetMaxHP());
        player.SetMP(player.GetMaxMP());
        player.SubtractRestore();
        battleUI.RestoreOff();
        PopupTextController.CreatePopupText("RESTORE", transform, PopupTextController.Colors.MAGENTA);
        audioManager.PlaySoundEffect(audioManager.recovery);
        battleUI.UpdateUI();
    }

    /// <summary>
    /// Do damge calculated to enemy
    /// </summary>
    /// <param name="dmg">Damage to be done</param>
    private void DoDamage(int dmg)
    {
        
       // Debug.Log("Player does " + dmg + " points of damage!");
        esm.TakeDamage(dmg);
    }

    /// <summary>
    /// Recieve damge calucalted from enemy; update player limit accordingly
    /// </summary>
    /// <param name="dmg">Damge to be taken</param>
    public void TakeDamage(int dmg)
    {
        //mitigate damge if any defense is applied
        if (player.defense > 0)
            dmg = dmg - (int)(dmg * player.defense);

        if (player.GetCurrentHP() < dmg)
            player.SetHP(0);
        else
            player.SubtractHP(dmg);
        PopupTextController.CreatePopupText(dmg.ToString(), transform);

        float missingHP = (float)dmg / player.GetMaxHP();
        battleUI.UpdatePlayerLimitBar(missingHP);
        battleUI.UpdateUI();
    }
}
