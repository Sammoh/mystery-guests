using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


public class Intent_TakeCard : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
		Debug.LogError("Take Card");

        // Needs to have a selected player.
        // if no player then a random player is selected.
        // Needs to have a card selected.
        // if no card is selected then a random card is selected.
        
        // NOTE: A random card is selected from the selected player's hand currently.

        var playerList = InGameRunner.Instance.PlayerList.Values;
        var selectedPlayer = intent.selectedCharacter;
        var selectedPlayerData = playerList.Select(player => player).FirstOrDefault(player => player.character == selectedPlayer);
        
        // takes a random card from the selected player and gives it to the caller.
        var playerHand = NewCardSelector.Instance.PlayerCards[selectedPlayerData.id];
        var randHandIndex = Random.Range(0, playerHand.Count);
        var randomCardFromHand = playerHand[randHandIndex];

        // take the card from the selected player.
        var removedCard = NewCardSelector.Instance.TakeCard(selectedPlayerData, randomCardFromHand); // give the card to the caller.
        var callerData = InGameRunner.Instance.PlayerList[caller];
        
        // Add the card to the caller's hand.
        NewCardSelector.Instance.AddCardsToPlayerHand(callerData, new List<BaseCard> {removedCard});
        
        // get the player's input panel and show the card to them.
        var playerObject = NetworkManager.Singleton.ConnectedClients[caller].PlayerObject.GetComponent<PlayerInputPanel>();
        playerObject.UpdateCards(selectedPlayerData.id);
        
        // finish the intent. 
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}