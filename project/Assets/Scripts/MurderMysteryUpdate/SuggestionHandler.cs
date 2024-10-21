using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using MurderMystery;
using UnityEngine;

public class SuggestionHandler : MonoBehaviour
{
    private static SuggestionHandler Instance;
    private List<int> suggestion = new List<int>();
    private void Awake()
    {
        Instance = this;
    }
    

}
