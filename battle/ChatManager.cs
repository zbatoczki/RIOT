using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages chat voting during enemy turn
/// </summary>
public class ChatManager : MonoBehaviour {

    private HashSet<string> listOfVoters; //keep track of users who voted
    private TwitchIRCObj chat;
    private EnemyStateMachine esm;

    private BattleUIManager ui;

    private readonly int TOTAL_TIME = 30; //time limit for voting
    private int currentTime; //time in seconds

    //used to tally votes: attack, magic, heal
    private Dictionary<string, int> votes = new Dictionary<string, int>(3);
    private readonly string SWIFT_RAGE = "SwiftRage";
    private readonly string GIVE_PLZ = "GivePLZ";
    private readonly string KAPPA = "Kappa";

    // Use this for initialization
    void Awake () {
        currentTime = 0;
        listOfVoters = new HashSet<string>();
        AddEntries();
        chat = GameObject.FindGameObjectWithTag("IRC").GetComponent<TwitchIRCObj>();
        esm = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyStateMachine>();
        ui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<BattleUIManager>();
    }

    /// <summary>
    /// Used to initialze <c>votes</c> with entries
    /// </summary>
    private void AddEntries()
    {
        votes.Add(SWIFT_RAGE, 0);
        votes.Add(GIVE_PLZ, 0);
        votes.Add(KAPPA, 0);
    }
	
    /// <summary>
    /// Resets list of voters and tallies. Sends a message to the chat to begin voting
    /// </summary>
    public void StartVoting()
    {
        if (listOfVoters.Count > 0)
            listOfVoters.Clear();
        if (votes.Count > 0)
        {
            votes.Clear();
            AddEntries();
        }
        currentTime = 0;
        StartCoroutine(StartTimer());
        chat.SendTwitchMessage("Begin voting now! Type in one of the following commands");
        chat.SendTwitchMessage("SwiftRage for 'Attack'");
        chat.SendTwitchMessage("GivePLZ for 'Magic'");
        chat.SendTwitchMessage("Kappa for 'Heal'");
        Debug.Log("Begin voting!");              
        StartCoroutine(CollectVotes());
    }

    /// <summary>
    /// A courotuine that runs a timer to allow users to vote. Updates the enemy UI progress bar in the meantime.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartTimer()
    {
        while(currentTime <= TOTAL_TIME)
        {
            ui.UpdateEnemyLimitBar((float)currentTime / TOTAL_TIME);
            //Debug.Log("Time: " + currentTime);
            yield return new WaitForSeconds(1.0f);
            currentTime++;         
        }
    }

    /// <summary>
    /// Read user's input. First check to see if a valid option was entered; if so, tally the vote and add in the user of people voted to avoid multiple votes from the same user.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CollectVotes()
    {
        while(currentTime <= TOTAL_TIME)
        {
            string msg = chat.ProcessMessage(chat.ReadMessage()); //text:user
            if (!string.IsNullOrEmpty(msg))
            {
                int colon = msg.IndexOf(':');
                string vote = msg.Substring(0, colon);
                string userName = msg.Substring(colon + 1);
                if (!UserVoted(userName) && ValidVote(vote))
                    EnterVote(userName, vote);
                else Debug.Log("Something went wrong with the vote. Either they voted or it was an invalid vote.");
            }            
            yield return null;
        }
        //determine and execute action
        esm.SelectAction( DetermineWinner() );    
    }

    /// <summary>
    /// Determines action to be executed
    /// </summary>
    /// <returns>int representing action: 0 to attack, 1 to cast magic, 2 to heal. Default set to 0 if error or no votes</returns>
    private int DetermineWinner()
    {
        int result = 0; //default to attack

        foreach (KeyValuePair<string, int> vote in votes)
        {
            string msg = string.Format("{0}: {1} votes", vote.Key, vote.Value);
            Debug.Log(msg);
        }
        
        string winner = votes.Aggregate((l, r) => l.Value >= r.Value ? l : r).Key;
        Debug.Log("winer=" + winner);
        if (winner.Equals(GIVE_PLZ)) result = 1; //magic
        else if (winner.Equals(KAPPA)) result = 2; //heal
        Debug.Log("Winner: " + result);
        return result;
    }

    /// <summary>
    /// Checks to see if user vote is valid.
    /// </summary>
    /// <param name="vote">User's vote</param>
    /// <returns>true if valid, false otherwise</returns>
    private bool ValidVote(string vote)
    {
        return vote.Equals(SWIFT_RAGE) || vote.Equals(GIVE_PLZ) || vote.Equals(KAPPA);
    }

    /// <summary>
    /// Enters user's vote to <param>listOfVotes</param> and tallies to appropriate counter
    /// </summary>
    /// <param name="user">name of user</param>
    /// <param name="vote">Vote user sent</param>
    private void EnterVote(string user, string vote)
    {
        listOfVoters.Add(user);
        votes[vote]++;
    }

    /// <summary>
    /// Check if user already voted
    /// </summary>
    /// <returns>true if user already voted, false otherwise</returns>
    public bool UserVoted(string user)
    {
        return listOfVoters.Contains(user);
    }
}
