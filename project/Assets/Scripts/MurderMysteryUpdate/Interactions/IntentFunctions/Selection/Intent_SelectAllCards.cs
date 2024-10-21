using System;
using CardGame.GameData.Cards;
using MurderMystery;

/// <summary>
/// Used to select any card in the player's hand.
/// </summary>
internal class Intent_SelectAllCards : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        NewCardSelector.Instance.GetPlayerCards(caller, (handArray) =>
        {
            var handCards = new BaseCard[handArray.Length];
            for (var i = 0; i < handCards.Length; i++)
            {
                handCards[i] = NewCardSelector.Instance.GetCard<BaseCard>(handArray[i]);
            }

            PopulateContent.Instance.PopulateCardArray<BaseCard>(handCards, selectedCharacterCard =>
            {
                CardIntent currentIntent = new CardIntent();
                currentIntent.SelectCard<BaseCard>(selectedCharacterCard);
                onComplete?.Invoke(currentIntent);
            }, () =>
            {
                onComplete?.Invoke(intent);
            }); 
        });
    }
}
