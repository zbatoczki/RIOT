using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Overall game manager; manages flow of the game; determines active turn and updates game
/// </summary>
public class BattleSystem : MonoBehaviour {
  
    //list of states in the battle system
    public enum BattleStates{
        START, //initialize battle
        PLAYERTURN,
        ENEMYTURN,
        UPDATE,
        WAIT,
        ENDGAME
    }

    private BattleStates currentState; //current state for the battke system manager

    private PlayerStateMachine psm;
    private EnemyStateMachine esm;
    [SerializeField] private BattleUIManager battleUI;

    public static bool isPlayersTurn = true;
    public static bool skipEnemyTurn = false;

    private bool updating = false;
    
    public static System.Random random = new System.Random();

    // Use this for initialization
    void Awake () {
        //set up variables
        currentState = BattleStates.START;
        if (currentState == BattleStates.START)
        {
            psm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>();
            esm = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyStateMachine>();           
        }
        PopupTextController.Initialize();
    }

    private void Start()
    {
        //decide who goes first
        BattleStates[] turn = { BattleStates.PLAYERTURN, BattleStates.ENEMYTURN };
        System.Random random = new System.Random();
        //currentState = turn[random.Next(turn.Length)];

        battleUI.UpdateUI(); //set UI
        currentState = BattleStates.PLAYERTURN;
    }

    // Update is called once per frame
    void Update () {
        Debug.Log("Battle State: " + currentState);
        switch (currentState)
        {
            case (BattleStates.WAIT): break;
            case (BattleStates.PLAYERTURN):
                psm.StartTurn();
                currentState = BattleStates.WAIT;
                break;
            case (BattleStates.ENEMYTURN):
                esm.StartTurn();
                currentState = BattleStates.WAIT;
                break;
            case (BattleStates.UPDATE): //update UI and see if any conditions are met               
                if (!updating) {
                    updating = true;
                    UpdateGame();
                    updating = false;                   
                }
                break;
            case (BattleStates.ENDGAME):
                currentState = BattleStates.WAIT;
                EndGame();
                break;
        }
	}

    /// <summary>
    /// At the end of each turn (player or enemy), check if the game should end or determine who goes next
    /// </summary>
    private void UpdateGame()
    {
        
        //check if player or enemy is dead or if enemy has to skip turn
        if (psm.GetStat(Player.STATS.CURRENTHP) <= 0 || esm.GetStat(Enemy.STATS.CURRENTHP) <= 0)
            currentState = BattleStates.ENDGAME;
        else if (!isPlayersTurn) //prepare for enemy turn
        {
            if (skipEnemyTurn) //skip enemy turn; alert through UI
            {
                currentState = BattleStates.PLAYERTURN;
                //Debug.Log("Enemy turn skipped.");
            }
            else
            {
                currentState = BattleStates.ENEMYTURN;
            }          
        }
        else if(isPlayersTurn)
        {
            currentState = BattleStates.PLAYERTURN;
        }
        else
            Debug.Log("SOMETHING WENT WRONG!");
        //updating = false;
    }

    /// <summary>
    /// End the game and determine winner.
    /// </summary>
    private void EndGame()
    {
        StartCoroutine(End());
    }

    private IEnumerator End()
    {
        yield return new WaitForSeconds(2);
        string winner = "Streamer won!"; //default message
        if (psm.GetStat(Player.STATS.CURRENTHP) <= 0)
            winner = "Twitch chat won!";
        battleUI.SetGameOverMenu(winner);
        currentState = BattleStates.WAIT;
    }

    /// <summary>
    /// Resets game.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene("battle");
    }

    /// <summary>
    /// Closes the application
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetPlayerAction(PlayerStateMachine.PlayerActions action)
    {
        psm.SetAction(action);
        psm.currentState = PlayerStateMachine.PlayerStates.PERFORMACTION;
    }

    public void SetEnemyAction(EnemyStateMachine.EnemyActions action)
    {
        esm.SetAction(action);
        esm.currentState = EnemyStateMachine.EnemyStates.PERFORMACTION;
    }

    /// <summary>
    /// End a turn and update the game
    /// </summary>
    public void EndTurn()
    {
        currentState = BattleStates.UPDATE;
    }
}
