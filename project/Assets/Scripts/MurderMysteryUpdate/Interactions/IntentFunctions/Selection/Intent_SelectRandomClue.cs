using System;
using MurderMystery;

public class Intent_SelectRandomClue : IIntentProcessor
{
    // This is a tool used to select a specific clue from this player's hand. 
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
    }
}
