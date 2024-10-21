using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using UnityEngine;

public class Intent_SelectClue : IIntentProcessor
{
    // This is a tool used to select a specific clue from this player's hand. 
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // var selectedIndex = -1;

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
        //     
        // }      

		Debug.LogError("Selecting Clue...");
		intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}
