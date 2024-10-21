using System;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using Unity.Netcode;
using UnityEngine;

public class IPostRound_Eliminate : IPostRoundIntent
{
    public void ProcessIntent(ulong caller, RoleData data)
    {
        var playerList = InGameRunner.Instance.PlayerList.Values;
        var playerMedicRole = NewCardSelector.Instance.RoleDataList.Values
            .FirstOrDefault(role => role.role == Role.Medic);
        var selectedVictimIndex = data.selectedCard;
        // get the id from 

        if (selectedVictimIndex == -1) return;

        if(playerMedicRole != null && playerMedicRole.selectedCard == data.selectedCard)return;
        
        foreach (var player in playerList)
        {
            if (player is PlayerData)
            {

                var playerObject = NetworkManager.Singleton.ConnectedClients[player.id].PlayerObject
                    .GetComponent<PlayerInputPanel>();
                
                
                var playerCharacterCard = NewCardSelector.Instance.PlayerCharacters[player.id];

                // if the player is the selected victim, then set the panel to the selected victim.
                if (playerCharacterCard.CardId == selectedVictimIndex)
                {
                    playerObject.SetPanelState(player.id, IntentType.KillPlayer);
                }
                else
                {
                    // get all of the other player's panels and disable the selected player's panel.
                    playerObject.UpdatePlayerStatus(player.id, IntentType.KillPlayer);

                }
            }
            else
            {
                Debug.LogError("Eliminating Ai Player");
            }
        }
    }
}
