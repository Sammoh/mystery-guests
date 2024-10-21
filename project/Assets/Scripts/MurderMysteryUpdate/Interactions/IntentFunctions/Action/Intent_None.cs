using System;
using MurderMystery;
using UnityEngine;

public class Intent_None : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // Implementation for processing NoneIntent
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}
