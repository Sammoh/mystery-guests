using System;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using UnityEngine;

public class Intent_SelectSelf : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        Debug.LogError($"SelectSelf");
        // var playerCharacter = InGameRunner.Instance.PlayerList[caller].character;
        // var playerCard = NewCardSelector.Instance.GetCard<BaseCard>(playerCharacter);
        // intent.SelectCard<BaseCard>(playerCard);
		intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}