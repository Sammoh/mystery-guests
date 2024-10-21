using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using MurderMystery;
using UnityEngine;

[CreateAssetMenu(menuName = "IntentProcessorDatabase")]
public class IntentProcessorDatabase : ScriptableObject
{
    public List<IntentProcessorEntry> Entries;

    // Method to get a processor based on the intent type
    public IIntentProcessor GetProcessor(IntentType type)
    {
        var entry = Entries.FirstOrDefault(e => e.IntentType == type);
        return entry != null ? entry.GetProcessor() : null;
    }
}

