using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class chatBox : MonoBehaviour {

    public GameObject chatMsgPrefab;
    private Queue<GameObject> messageList;
    private readonly int QUEUE_SIZE = 15;
	// Use this for initialization
	void Awake () {
        messageList = new Queue<GameObject>(QUEUE_SIZE);
	}

    public void AddMessage(string msg)
    {
        if(!string.IsNullOrEmpty(msg))
        {
            if (messageList.Count == QUEUE_SIZE)
            {
                GameObject oldestMessage = messageList.Dequeue();
                DestroyObject(oldestMessage);
            }
            //create copy of prefab and add to chatbox; newMsg will be a child of chatBox
            GameObject newMsg = Instantiate(chatMsgPrefab, this.transform) as GameObject;
            newMsg.GetComponent<Text>().text = msg;
            messageList.Enqueue(newMsg);
        }       
    }
}
