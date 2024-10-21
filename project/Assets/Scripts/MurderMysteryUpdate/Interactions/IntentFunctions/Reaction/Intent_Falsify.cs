using System;
using LobbyRelaySample.ngo;
using MurderMystery;
using MurderMystery.Ai;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This card is used to falsify a card.
/// Can be used defensively or offensively.
/// </summary>
public class Intent_Falsify : IIntentProcessor
{
	public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
		Debug.LogError("Falsify");
        // // get the target's id from the card. 
        // var playerList = InGameRunner.Instance.PlayerList.Values;
        //
        // foreach (var player in playerList)
        // {
        //     if (player.character != intent.selectedCharacter) continue;
        //
        //     var targetId = player.id;
        //     var targetPlayerData = InGameRunner.Instance.PlayerList[targetId];
        //
        //     if (targetPlayerData is PlayerData)
        //     {
        //         var playerObject = NetworkManager.Singleton.ConnectedClients[player.id].PlayerObject
        //             .GetComponent<PlayerInputPanel>();
        //         playerObject?.ShowCard(caller, player.id, intent.selectedCardIndex);
        //     }
        //     else if (targetPlayerData is AiPlayerData aiPlayerData)
        //     {
        //         AiManager.Instance.ShowCard(aiPlayerData, intent.selectedCardIndex);
        //     }
        //
        // }
		
		intent.hasPassed = true;
        onComplete?.Invoke(intent);
        
    }
}