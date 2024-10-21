using System;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using UnityEngine;


public class Intent_SelectRandomPlayer : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
		Debug.LogError("Selecting Random player...");

        // var playerList = InGameRunner.Instance.PlayerList.Values;
        //
        // // get player that is not the caller
        // var selectedPlayer = playerList.Select(player => player).FirstOrDefault(player => player.id != caller);
        // var selectedCharacterIndex = selectedPlayer.character;
        // var characterCard = NewCardSelector.Instance.GetCard<MMCharacterCard>(selectedCharacterIndex);
        //
        CardIntent currentIntent = new CardIntent();
        // currentIntent.SelectCard<BaseCard>(characterCard);
        onComplete?.Invoke(currentIntent);
    }
}
