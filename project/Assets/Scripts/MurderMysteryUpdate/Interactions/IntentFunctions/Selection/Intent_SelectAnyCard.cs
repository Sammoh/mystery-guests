using System;
using CardGame.GameData.Cards;
using MurderMystery;
using UnityEngine;

/// <summary>
/// Used to select any card in the player's hand.
/// </summary>
internal class Intent_SelectAnyCard : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        Debug.Log("SelectAnyCard");
        
        NewCardSelector.Instance.GetPlayerCards(caller, ( handArray) =>
        {

            var msg = "Cards: ";

            foreach (var index in handArray)
            {
                var name = NewCardSelector.Instance.GetCard<BaseCard>(index).Name;
                msg += $"{name}, ";
            }
            
            Debug.LogError(msg);
            
            var handCards = new BaseCard[handArray.Length];
            for (var i = 0; i < handCards.Length; i++)
            {
                handCards[i] = NewCardSelector.Instance.GetCard<BaseCard>(handArray[i]);
            }

            PopulateContent.Instance.PopulateCardArray<BaseCard>(handCards, selectedCharacterCard =>
            {
                intent.SelectCard<BaseCard>(selectedCharacterCard);
                onComplete?.Invoke(intent);
            },
            () =>
            {
                Debug.LogError($"Failed to select a card.");
            }); 
        });
    }
}
