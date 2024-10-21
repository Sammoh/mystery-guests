using System;
using System.Collections;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Used to make a suggestion to the server.
/// A Sheriff ability that allows the player to make an accusation once per game.
/// </summary>
internal class Intent_MakeSuggestion : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        var populateContent = PopulateContent.Instance;
        populateContent.gameObject.SetActive(true);
        
        // Debug.LogError("Making suggestion");
        
        InGameRunner.Instance.SuggestPlayer(caller, suggestion =>
        {
            intent.hasPassed = true;
            InGameRunner.Instance.PlayerInput_SuggestPlayer(caller, suggestion);
            onComplete?.Invoke(intent);
        }, onFail);
    }
}
