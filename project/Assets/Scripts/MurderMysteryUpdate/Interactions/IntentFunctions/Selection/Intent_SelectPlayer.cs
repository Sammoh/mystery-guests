using System;
using CardGame.GameData.Cards;
using MurderMystery;
using Unity.Netcode;
using UnityEngine;

public class Intent_SelectPlayer : IIntentProcessor
{
    // private readonly IIntentHandler _handler;
    //
    // public Intent_SelectPlayer(IIntentHandler handler)
    // {
    //     _handler = handler;
    // }

    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        Debug.Log("Selecting player...");

        var playerPanel = NetworkManager.Singleton.ConnectedClients[caller].PlayerObject.GetComponent<PlayerInputPanel>();
        playerPanel.EnablePopupPanel(caller, CardTypes.Character, selectedCard =>
        {
            Debug.Log($"Selected Character: {selectedCard.Name}");
            intent.selectedCharacter = selectedCard.CardId;
            intent.hasPassed = true;
            onComplete?.Invoke(intent);
        }, onFail);
    }
}