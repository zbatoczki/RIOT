using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Used to create an instance of text to popup when certain actions have been made, such as an attack or heal
/// </summary>
public class PopUpText : MonoBehaviour {

    public Animator anim;
    private Text dmgTxt;

    private void Awake()
    {
        dmgTxt = anim.GetComponent<Text>();
    }

    private void Start()
    {
        AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipInfo[0].clip.length);        
    }

    /// <summary>
    /// Set text to instance of class
    /// </summary>
    /// <param name="text">Text to be set to</param>
    public void SetText(string text)
    {
        dmgTxt.text = text;
    }

    /// <summary>
    /// Sets color of popup text
    /// </summary>
    /// <param name="color">Color to be used</param>
    public void SetColor(PopupTextController.Colors color)
    {
        if (color == PopupTextController.Colors.GREEN)
            dmgTxt.color = Color.green;        
        else if (color == PopupTextController.Colors.BLUE)
            dmgTxt.color = Color.blue;
        else if (color == PopupTextController.Colors.MAGENTA)
            dmgTxt.color = new Color(1.0f, 0.0f, 1.0f);
        else Debug.Log("Something went wrong with colors!");
    }

}
