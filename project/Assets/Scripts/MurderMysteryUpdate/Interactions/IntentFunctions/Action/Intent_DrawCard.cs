using System;
using System.Linq;
using LobbyRelaySample.ngo;
using MurderMystery;
using MurderMystery.Ai;
using Unity.Netcode;
using UnityEngine;

public class Intent_DrawCard : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // get the target's id from the card. 
        // var playerList = InGameRunner.Instance.PlayerList.Values;
        // var target = playerList.FirstOrDefault(x => x.character == intent.selectedCharacter);
        // NewCardSelector.Instance.DrawCards(target, 2);
        
        Debug.LogError("Drawing a card is not implemented yet.");
        
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
        
    }
}