using System.Collections;
using System.Collections.Generic;
using LobbyRelaySample;
using UnityEngine;

public class PopUp_Settings : PopUpUI
{
    // Start is called before the first frame update
    void Start()
    {
        // get all of the settings from player prefs. 
        
    }

    public void Button_Close()
    {
        ClearPopup();
    }
    
    public void Button_Open()
    {
        ShowPopup();
    }
}
