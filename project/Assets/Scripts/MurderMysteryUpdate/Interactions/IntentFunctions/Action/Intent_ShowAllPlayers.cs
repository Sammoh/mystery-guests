using System;
using LobbyRelaySample.ngo;
using MurderMystery;
using MurderMystery.Ai;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Show all players the card
/// </summary>
public class Intent_ShowAllPlayers : IIntentProcessor
{
    // sends a message to the game runner to show all the cards.
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
		Debug.LogError("Showing all Players");
        // // get the target's id from the card. 
        // var playerList = InGameRunner.Instance.PlayerList.Values;
        //
        // foreach (var player in playerList)
        // {
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
