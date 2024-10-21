using System;
using MurderMystery;

public interface IIntentProcessor
{
    void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null);
}
