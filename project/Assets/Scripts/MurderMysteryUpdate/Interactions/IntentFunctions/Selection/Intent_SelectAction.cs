using System;
using System.Collections;
using System.Collections.Generic;
using MurderMystery;
using UnityEngine;

public class Intent_SelectAction : IIntentProcessor
{
    // This is a tool used to select a specific clue from this player's hand. 
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
    }
}
