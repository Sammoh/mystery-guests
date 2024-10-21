using System.Collections;
using System.Collections.Generic;
using CardGame.GameData.Cards;
using MurderMystery;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntentProcessorEntry
{
    public IntentType IntentType;
    public UnityEvent IntentProcessor;

    // Ensure that the assigned MonoBehaviour actually implements IIntentProcessor
    public IIntentProcessor GetProcessor()
    {
        return IntentProcessor as IIntentProcessor;
    }
}
