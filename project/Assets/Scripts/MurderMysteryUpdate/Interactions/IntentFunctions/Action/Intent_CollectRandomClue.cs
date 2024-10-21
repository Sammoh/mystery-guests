using System;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using UnityEngine;

/// <summary>
/// select a player to collect a random or selected card from the other player's hand.
/// </summary>
public class Intent_CollectRandomClue : IIntentProcessor
{
	public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        var selectedIndex = -1;

        // // get the target's id from the card. 
        // var playersList = InGameRunner.Instance.PlayerList.Values;
        // var cardSelector = NewCardSelector.Instance;
        //
        // System.Random rand = new System.Random();
        // foreach (var player in playersList)
        // {
        //     if (player.character != intent.selectedCharacter) continue;
        //
        //     var playerHand = NewCardSelector.Instance.PlayerCards[player.id];
        //     var randomizedArr = playerHand.OrderBy(x => rand.Next());
        //
        //     foreach (var cardIndex in randomizedArr)
        //     {
        //         var card = cardSelector.GetCard<BaseCard>(cardIndex);
        //         switch (intent.instruction)
        //         {
        //             case IntentType.CollectCharacter:
        //                 if (card is CharacterCard)
        //                 {
        //                     selectedIndex = cardIndex;
        //                     return;
        //                 }
        //
        //                 break;
        //             case IntentType.CollectMotive:
        //                 if (card is MotiveClueCard)
        //                 {
        //                     selectedIndex = cardIndex;
        //                     return;
        //                 }
        //
        //                 break;
        //             case IntentType.CollectWeapon:
        //                 if (card is WeaponClueCard)
        //                 {
        //                     selectedIndex = cardIndex;
        //                     return;
        //                 }
        //
        //                 break;
        //             case IntentType.CollectRandomClue:
        //             {
        //                 selectedIndex = cardIndex;
        //                 return;
        //             }
        //                 break;
        //         }
        //     }
        //
        //     Debug.LogError("No card found to collect");
        //
        //     if (selectedIndex == -1)
        //     {
        //         Debug.LogError($"No {intent.instruction} found for {player.name}");
        //
        //     }
        //     else
        //     {
        //         // delete this when done 
        //         var selectedCard = cardSelector.GetCard<BaseCard>(selectedIndex);
        //         Debug.LogError($"Selected card is {selectedCard.Name}");
        //     }
        // }
		
		Debug.LogError("Collecting Random Clue");

        intent.hasPassed = true;
        onComplete?.Invoke(intent);
	}
}
