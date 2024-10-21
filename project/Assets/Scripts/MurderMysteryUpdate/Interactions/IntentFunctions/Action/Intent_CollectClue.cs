using System;
using MurderMystery;
using UnityEngine;

/// <summary>
/// select a player to collect a random or selected card from the other player's hand.
/// </summary>
public class Intent_CollectClue : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        Debug.LogError($"Starting a minigame for {caller}");

        var minigameType = MinigameType.Search;
        // This is something that the player has already asked for, it should have been validated already.
        // The minigame will  be started on the server side.
        MinigameManager.Instance.StartMinigame(caller, minigameType, (didPass) =>
        {       
            // This is a minigame that the player has to complete.
            // Need to be able to the tell the client that there is a minigame. 
            // The game should be validated on the server side.
            
            intent.hasPassed = true;

            if (didPass)
            {
                onComplete?.Invoke(intent);
            }
            else
            {
                onFail?.Invoke();
            }
            
        });
        
        // intent.hasPassed = true;
        // onComplete?.Invoke(intent);
    }
}
