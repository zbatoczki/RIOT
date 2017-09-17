using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/// <summary>
/// Manages UI in connect screen; includes input fields, output, and button functionality
/// </summary>
public class connectUIManager : MonoBehaviour {

    public InputField channelNameInput;
    public InputField oAuthInput;
    private Text successMsg;
    public Button startBattleBtn;
    public chatBox chatBox;

    private TwitchIRCObj irc;

    // Use this for initialization
    void Start () {
        successMsg = GameObject.Find("successMsg").GetComponent<Text>();
        successMsg.enabled = false;
        startBattleBtn.interactable = false;
        startBattleBtn.gameObject.SetActive(false);
        irc = GameObject.FindGameObjectWithTag("IRC").GetComponent<TwitchIRCObj>();
    }
    /// <summary>
    /// Every frame if the irc has been initialized, read any new message and display them on the screen 
    /// </summary>
    private void Update()
    {
        if (irc)
        {
            string message = irc.ProcessMessage(irc.ReadMessage());
            chatBox.AddMessage(message);
        }
    }

    /// <summary>
    /// Opens link in user's default browser. Allows them to recieve a OAuth token for IRC access.
    /// </summary>
    public void getOAuthToken()
    {
        Application.OpenURL("https://twitchapps.com/tmi/");
    }

    /// <summary>
    /// Displays message in game window.
    /// </summary>
    /// <param name="msg">Message to be displayed</param>
   public void displaySuccessMsg(string msg)
    {
        successMsg.text = msg;
        successMsg.enabled = true;
        Invoke("HideSuccessMsg", 3.0f);
    }

    /// <summary>
    /// Hide success message after connecting
    /// </summary>
    private void HideSuccessMsg()
    {
        successMsg.enabled = false;
    }

    /// <summary>
    /// Displays 'Begin Battle' button after a connection to the IRC has been made.
    /// </summary>
    public void EnableBattleBtn()
    {
        startBattleBtn.interactable = true;
        startBattleBtn.gameObject.SetActive(true);
    }

    /// <summary>
    /// Loads to main battle
    /// </summary>
    public void BeginBattle()
    {
        //TwitchIRCObj.twitchIRC.ToggleChat();
        SceneManager.LoadScene("battle");
    }
}
