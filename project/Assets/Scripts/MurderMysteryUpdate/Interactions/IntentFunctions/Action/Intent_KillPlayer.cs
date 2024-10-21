using System;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using Unity.Netcode;
using UnityEngine;

/// <summary>
///  KillPlayer = 5, // used by killer, select a player to kill.
/// </summary>
public class Intent_KillPlayer : IIntentProcessor
{
    
    // Sammoh Todo: This needs to run on the server.
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // Debug.LogError("Processing Kill Player Intent");
        
        // There must first be a selected player.
        var playerList = InGameRunner.Instance.PlayerList.Values;
        var selectedPlayer = intent.selectedCharacter;
        var selectedPlayerData = playerList.Select(player => player).FirstOrDefault(player => player.character == selectedPlayer);
        var selectedCharacterCard = NewCardSelector.Instance.PlayerCharacters[selectedPlayerData.id];
        // Sammoh TODO: Move this to the GameRunner.
        // This should be a function that is called at the end of the round.
        
        // Debug.LogError($"Trying to KILL Player: {selectedPlayer}");
        // store the information about the selected player in the player's role.
        NewCardSelector.Instance.RoleDataList[caller].selectedCard = selectedCharacterCard.CardId;

        // finish the intent. 
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }

}
