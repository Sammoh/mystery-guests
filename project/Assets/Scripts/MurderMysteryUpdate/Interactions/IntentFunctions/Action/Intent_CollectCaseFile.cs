using System;
using MurderMystery;
using UnityEngine;

/// <summary>
/// This card should be used to collect the case file and reveal it to the player.
/// </summary>
public class Intent_CollectCaseFile : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // Implementation for processing 
        // 1. Get the case file from the game runner.
        // 2. Show the case file information from a player.
        // 3. Complete the intent.
        Debug.LogError($"There is no intent processor for this card.");
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}
