using System;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using UnityEngine;

public class Intent_GiveCard : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
		Debug.LogError("Giving Card");
        // get the target's id from the card.
        // var selectedPlayer = intent.selectedCharacter;
        // var selectedCard = intent.selectedCardIndex;
        // // find the target player from the list of players.
        // var playerList = InGameRunner.Instance.PlayerList.Values;
        // var selectedPlayerData = playerList.Select(player => player).FirstOrDefault(player => player.character == selectedPlayer);
        // // take card from caller
        // var callerData = InGameRunner.Instance.PlayerList[caller];
        // var removedCardIndex = NewCardSelector.Instance.TakeCard(callerData, selectedCard);
        // var removedCard = NewCardSelector.Instance.GetCard<BaseCard>(removedCardIndex);
        // // give card to target
        //
        // // NewCardSelector.Instance.GiveCard(selectedPlayerData, removedCard);

        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}
