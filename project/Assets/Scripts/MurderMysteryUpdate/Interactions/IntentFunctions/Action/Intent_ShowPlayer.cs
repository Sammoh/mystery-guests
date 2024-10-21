using System;
using LobbyRelaySample.ngo;
using MurderMystery;
using MurderMystery.Ai;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// sends the card to selected player. 
/// </summary>
internal class Intent_ShowPlayer : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // var playerList = InGameRunner.Instance.PlayerList.Values;
        var playerCharacters = NewCardSelector.Instance.PlayerCharacters;

        foreach (var character in playerCharacters)
        {
            if (character.Value.CardId != intent.selectedCharacter) continue;

            var targetId = character.Key;
            var targetPlayerData = InGameRunner.Instance.PlayerList[targetId];
            var targetCard = NewCardSelector.Instance.GetCard(intent.selectedCardIndex);

            if (targetPlayerData is PlayerData)
            {
                var playerObject = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject
                    .GetComponent<PlayerInputPanel>();
                
                Debug.LogError($"Showing {targetCard.Name} to {targetPlayerData.name}");
                
                playerObject?.ShowCard(caller, targetCard);
            }
            else if (targetPlayerData is AiPlayerData aiPlayerData)
            {
                Debug.LogError($"Showing {targetCard.Name} to {aiPlayerData.name}");

                AiManager.Instance.ShowCard(aiPlayerData, intent.selectedCardIndex);
            }

        }        
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
    
}
