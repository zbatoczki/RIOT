using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections;

/// <summary>
/// Used to connect to the Twitch IRC using a server, port, username and password (oauth token).
/// Parses message recieved from IRC for further parsing in-game
/// </summary>
public class TwitchIRCObj : MonoBehaviour
{

    private static TwitchIRCObj twitchIRC; //only one instance will be made

    readonly private string server = "irc.chat.twitch.tv"; //the server to connect to
    readonly private int port = 6667; //port number
    private string username; //will also be used for channel
    private string channelName;
    private string oAuthToken;

    [HideInInspector] public static TcpClient tcpClient;
    [HideInInspector] public static StreamReader inputStream;
    [HideInInspector] public static StreamWriter outputStream;
    readonly private string PRIVMSG = "PRIVMSG";
    readonly private string PING = "PING :tmi.twitch.tv";
    readonly private string PONG = "PONG :tmi.twitch.tv";
    readonly private string NO_MSG = "NO MESSAGE";

    connectUIManager uiManager;

    private void Awake()
    {
        if (twitchIRC == null)
        {
            DontDestroyOnLoad(gameObject);
            twitchIRC = this;
        }
        else if(twitchIRC != null)
            DestroyObject(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        uiManager = GameObject.Find("connectUIManager").GetComponent<connectUIManager>();
        tcpClient = new TcpClient();
    }

    /// <summary>
    /// Attempts to connect to chat using server, port, and username and OAuth provided by player
    /// </summary>
    public void ConnectToChat()
    {
        //grab text value from user input
        channelName = uiManager.channelNameInput.text.ToLower();
        username = channelName;
        print(channelName);
        oAuthToken = uiManager.oAuthInput.text;
        print(oAuthToken);

        //initialize tcp and input/output streams
        try
        {
            tcpClient.Connect(server, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());
            outputStream.AutoFlush = true;
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to Twitch IRC: " + e.Message);
        }


        //send PASS (oauth) and NICK (username) to connect to irc server
        //flushes after each WriteLine() since AutoFlush is on
        outputStream.WriteLine("PASS " + oAuthToken);
        outputStream.WriteLine("NICK " + username);
        //join channel
        outputStream.WriteLine("JOIN #" + channelName);
        string successMsg = "Successfully connected to " + channelName + "'s chat!";
        uiManager.displaySuccessMsg(successMsg);
        uiManager.EnableBattleBtn();
    }


    /// <summary>
    /// Checks for PING from IRC server. Respond back with PONG to prevent disconnection
    /// </summary>
    /// <param name="msg">Message to check</param>
    /// <returns>Returns true if a PING was recieved, false otherwise</returns>
    private bool CheckForPing(string msg)
    {
        if (msg.Equals(PING))
        {
            Debug.Log(msg);
            SendPong();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sends a PONG message back to the IRC server to prevent early dissconnection
    /// </summary>
    private void SendPong()
    {
        outputStream.WriteLine(PONG);
        Debug.Log(PONG);
    }

    /// <summary>
    /// If data is availbe from IRC server, read the data
    /// </summary>
    /// <returns>Returns message from IRC server, otherwise return a default "No message"</returns>
    public string ReadMessage()
    {
        if (tcpClient.Available > 0)
            return inputStream.ReadLine();
        else
            return NO_MSG;
    }

    /// <summary>
    /// Send a message to the IRC server
    /// </summary>
    /// <param name="message">Message to be sent</param>
    public void SendTwitchMessage(string message)
    {
        string command = PRIVMSG + " #" + channelName + " :" + message;
        outputStream.WriteLine(command);
    }

    /// <summary>
    /// Processes a message recieved from the irc server; splits into message written by user, then username
    /// </summary>
    /// <param name="msg">A message recieved from the irc server to be processed. 
    /// Formatted as <c>:&lt;user&gt;!&lt;user&gt;@&lt;user&gt;.tmi.twitch.tv PRIVMSG #&lt;channel&gt; :This is a sample message</c>
    /// </param>
    /// <returns>Processed string in the format "message:user"</returns>
    public string ProcessMessage(string msg)
    {
        string chatMessage = "";
        if (!CheckForPing(msg))
        {          
            //find second instance of ':'
            //EXAMPLE- :<user>!<user>@<user>.tmi.twitch.tv PRIVMSG #<channel> :This is a sample message
            int colon = msg.IndexOf(':', 1); //start at position 1 (t), not 0 to avoid reading the first ':'
            if (colon > 1)
            {
                //look for PRIVMSG command in message header; if it exists then display content
                string msgHeader = msg.Substring(1, colon); //:<user>!<user>@<user>.tmi.twitch.tv PRIVMSG #<channel> :
                if (msgHeader.Contains(PRIVMSG))
                {
                    string user = msg.Substring(1, msg.IndexOf('!') - 1); //get username
                    //get string after ':'
                    chatMessage = msg.Substring(colon + 1) + ":" + user; //chatMessage = "This is a sample message:USERNAME"              
                }
            }
        }       
        return chatMessage;
    }
}
