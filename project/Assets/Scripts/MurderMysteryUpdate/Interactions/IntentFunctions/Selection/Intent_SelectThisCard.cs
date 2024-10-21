using System;
using System.Collections;
using System.Collections.Generic;
using MurderMystery;
using UnityEngine;

public class Intent_SelectThisCard : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        var cardName = NewCardSelector.Instance.GetCard(intent.cardId).Name;
        Debug.LogError($"Selecting card: {cardName}");
        intent.selectedCardIndex = intent.cardId;
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}
