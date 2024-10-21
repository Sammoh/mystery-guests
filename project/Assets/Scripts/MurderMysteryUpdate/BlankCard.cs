using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
// a blank card is used by the player to select a card from their hand
// it is then replaced by the selected card
    public class BlankCard : CardObject
    {
        [SerializeField] private int _cardId;
        public int CardId => _cardId;

        public void SetId(int index)
        {
            _cardId = index;
        }

        public void CastCard()
        {

        }
    }
}
