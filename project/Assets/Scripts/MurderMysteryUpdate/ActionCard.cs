using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
    public class ActionCard : CardObject, IUseCard
    {
        public void UseCard(ulong playerId, CardObject data, Action<CardIntent> action = null)
        {
            Debug.LogError($"Player {playerId} used card {data.name}");
        }

        public CardIntent OnCardSelected()
        {
            // the intent is to show all players. 
            return null;
        }
    }
}