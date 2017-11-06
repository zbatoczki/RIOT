﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages enemy's turn. Instance created for Enemy gameObject
/// </summary>
public class EnemyStateMachine : MonoBehaviour {

    //references
    [SerializeField] private Enemy enemy;
    private PlayerStateMachine psm;
    private BattleSystem battleSystem;
    private BattleUIManager battleUI;
    private ChatManager chatManager;
    private bool autoPlay = true;

    public enum EnemyStates
    {
        START,
        WAIT,
        PERFORMACTION,
        WAITFORACTION,
        PROCESS,
        END
    }

    public enum EnemyActions
    {
        ATTACK,
        MAGIC,
        HEAL,
    }

    public EnemyStates currentState; //keeps track of current position in the enemy's turn

    //used for performing enemy action
    private EnemyActions chosenAction;
    private GameObject playerGameObj;
    private Vector3 startPosition;
    private Animator anim;
    private float animSpeed = 5f;
    [SerializeField] private Sprite defendSprite;
    [SerializeField] private Sprite magicSprite;
    [SerializeField] private Sprite healSprite;

    //audio
    private AudioManager audioManager;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip magicSound;

    // Use this for initialization
    void Awake()
    {
        enemy = new Enemy();
        anim = GetComponent<Animator>();
        startPosition = transform.position;
        currentState = EnemyStates.WAIT;
        battleSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<BattleSystem>();
        battleUI = GameObject.FindGameObjectWithTag("UIManager").GetComponent<BattleUIManager>();
        playerGameObj = GameObject.FindGameObjectWithTag("Player");
        chatManager = GameObject.FindGameObjectWithTag("ChatManager").GetComponent<ChatManager>();
        psm = playerGameObj.GetComponent<PlayerStateMachine>();
        audioManager = AudioManager.instance;
    }

    public void SetAutoPlay()
    {
        autoPlay = !autoPlay;
        battleUI.SetAutoplayBtn(false, autoPlay);
    }

    private IEnumerator Wait()
    {
        Debug.Log("Waiting...");
        yield return new WaitForSeconds(2);
        Debug.Log("Done waiting!");
    }

    /// <summary>
    /// Begin enemy's turn. Enables animation and menu
    /// </summary>
    public void StartTurn()
    {
        StartCoroutine(Begin());
    }

    private IEnumerator Begin()
    {
        //Debug.Log("Starting enemy's turn");
        yield return StartCoroutine(Wait());
        if (autoPlay)
        {
            Debug.Log("Enemy is auto running.");
            AutoPlayTurn();
        }
        else
        {
            if (!anim.enabled) TurnOnAnim();
            battleUI.EnemyActionMenuOn();
            StartCoroutine(StartVote());
        }
    }

    private void AutoPlayTurn()
    {
        SetAction(EnemyActions.ATTACK);
        SelectAction( (int)EnemyActions.ATTACK );
    }

    /// <summary>
    /// Coroutine that starts voting
    /// </summary>
    private IEnumerator StartVote()
    {
        //wait until sound effect is finished playing, then begin coroutine
        audioManager.PlaySoundEffect(audioManager.menuSelection);
        while (audioManager.SoundEffectPlaying())
            yield return null;
        //begin processing chat input
        chatManager.StartVoting();
    }

    /// <summary>
    /// Choose the action to be executed
    /// </summary>
    /// <param name="action">int representing action to execute</param>
    public void SelectAction(int action)
    {
        battleUI.EnemyMenusOff();
        battleUI.HideButtonDesc();
        chosenAction = (EnemyActions)action;
        StartCoroutine(PerformAction());
    }

    /// <summary>
    /// Sets maching state
    /// </summary>
    /// <param name="state">State to be set</param>
    public void SetState(EnemyStates state) { currentState = state; }
    public void SetAction(EnemyActions action) { chosenAction = action; }


    /// <summary>
    /// Executes selected action
    /// </summary>
    private IEnumerator PerformAction()
    {
        switch (chosenAction)
        {
            case (EnemyActions.ATTACK): //basic attack
                yield return StartCoroutine(Attack());
                break;
            case (EnemyActions.MAGIC): //applies a % to mitagate on enemy's next attack; also restores minor health
                CastMagic();
                break;
            case (EnemyActions.HEAL):
                yield return StartCoroutine(Restore());
                break;
        }
        //Invoke("End", 1.5f); //once action is done, wait for time then end the turn
        End();
    }

    /// <summary>
    /// End the enemy's turn
    /// </summary>
    public void End()
    {
        BattleSystem.isPlayersTurn = true;
        battleUI.EnemyMenusOff();
        battleSystem.EndTurn();
    }

    /// <summary>
    /// Return requested enemty stat
    /// </summary>
    /// <param name="s">Requested stat</param>
    /// <returns>Returns stat (int)</returns>
    public int GetStat(Enemy.STATS s)
    {
        int stat = -1;
        switch (s)
        {
            case (Enemy.STATS.CURRENTHP): stat = enemy.currentHP; break;
            case (Enemy.STATS.MAXHP): stat = enemy.maxHP; break;
            case (Enemy.STATS.CURRENTMP): stat = enemy.currentMP; break;
            case (Enemy.STATS.MAXMP): stat = enemy.maxMP; break;
        }
        return stat;
    }

    /* /////////////////ENEMY ANIMATIONS/////////////////// */

    private bool MoveToPlayer(Vector3 targetPosition)
    {
        if (!anim.GetBool("isMoving"))
            anim.SetBool("isMoving", true);
        return targetPosition != (transform.position = Vector3.MoveTowards(transform.position, targetPosition, animSpeed * Time.deltaTime));
    }

    private bool MoveToStart(Vector3 targetPosition)
    {
        gameObject.GetComponent<SpriteRenderer>().flipX = false;
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

    /* /////////////////ENEMY ACTIONS/////////////////// */

    private IEnumerator Attack()
    {
        //ANIMATE ENEMY SPRITE TO PLAYER
        Vector3 playerPosition = new Vector3(playerGameObj.transform.position.x + 1.5f, playerGameObj.transform.position.y);
        //while the enemy is moving
        while (MoveToPlayer(playerPosition))
            yield return null;
        anim.SetBool("isMoving", false);
        //perform chosen action animation and calculate damage and/or heals
        yield return new WaitForSeconds(0.5f);
        anim.Play("EnemyAttack");
        audioManager.PlaySoundEffect(attackSound);
        DoDamage(enemy.attackDmg);       
        yield return new WaitForSeconds(0.5f);
        //animate back to starting position
        while (MoveToStart(startPosition))
            yield return null;
        //reset sprite orientation
        gameObject.GetComponent<SpriteRenderer>().flipX = true;
        anim.SetBool("isMoving", false);
    }

    private void CastMagic()
    {
        //Debug.Log("Enemy casts Magic!");
        TurnOffAnim();
        GetComponent<SpriteRenderer>().sprite = magicSprite;
        audioManager.PlaySoundEffect(magicSound);
        enemy.currentMP -= 200;
        Invoke("TurnOnAnim", 1f);
        DoDamage(enemy.magicDmg);
    }

    private void CheckHPOverflow()
    {
        if (enemy.currentHP > enemy.maxHP) enemy.currentHP = enemy.maxHP;
    }

    private void CheckMPOverflow()
    {
        if (enemy.currentMP > enemy.maxMP) enemy.currentMP = enemy.maxMP;
    }

    /// <summary>
    /// Restore % of max HP and MP
    /// </summary>
    private IEnumerator Restore()
    {
        TurnOffAnim();
        GetComponent<SpriteRenderer>().sprite = healSprite;

        float percentage = Random.value;
        int healthToRestore = (int)(enemy.maxHP * percentage);
        enemy.currentHP += healthToRestore;
        CheckHPOverflow();

        percentage = Random.value;
        int mpToRestore = (int)(enemy.maxMP * percentage);
        enemy.currentMP += mpToRestore;
        CheckMPOverflow();

        PopupTextController.CreatePopupText(healthToRestore.ToString(), transform, PopupTextController.Colors.GREEN);
        yield return new WaitForSeconds(0.5f);
        audioManager.PlaySoundEffect(audioManager.recovery);
        PopupTextController.CreatePopupText(mpToRestore.ToString(), transform, PopupTextController.Colors.BLUE);
        Invoke("TurnOnAnim", 1.5f);
        battleUI.UpdateUI();
    }

    
    private void DoDamage(int dmg)
    {
        psm.TakeDamage(dmg);
    }

    /// <summary>
    /// Recieve x amount of damage; update UI accordingly
    /// </summary>
    /// <param name="dmg">Damage to be taken</param>
    public void TakeDamage(int dmg)
    {
        if (enemy.currentHP < dmg)
            enemy.currentHP = 0;
        else
            enemy.currentHP -= dmg;
        PopupTextController.CreatePopupText(dmg.ToString(), transform);
        battleUI.UpdateUI();
    }
}
