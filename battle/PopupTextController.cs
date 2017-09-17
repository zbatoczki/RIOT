using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Class to represent instance of popup text
/// </summary>
public class PopupTextController : MonoBehaviour {

    private static PopUpText popupText;
    private static GameObject ui;

    public enum Colors
    {
        GREEN,
        BLUE,
        MAGENTA,
        DEFAULT
    }

    /// <summary>
    /// Initialize references
    /// </summary>
    public static void Initialize()
    {
        try
        {
            if(!ui) ui = GameObject.FindGameObjectWithTag("UI");
            if(!popupText) popupText = Resources.Load<PopUpText>("Prefabs/PopUpParent");
            Debug.Log("Popup initialization successful");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }

    /// <summary>
    /// Create an instance of PopUpText
    /// </summary>
    /// <param name="text">Set text to</param>
    /// <param name="position">Position where popup will happen</param>
    /// <param name="color">Set color of popup text</param>
    public static void CreatePopupText(string text, Transform position, Colors color = Colors.DEFAULT)
    {
        PopUpText instance = Instantiate(popupText);     
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(position.position);
        instance.transform.SetParent(ui.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
        if (color != Colors.DEFAULT)
            instance.SetColor(color);     
    }
}
