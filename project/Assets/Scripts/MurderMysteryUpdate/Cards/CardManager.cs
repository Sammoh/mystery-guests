using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using UnityEngine;

namespace MurderMystery
{
    public class CardManager : MonoBehaviour
    {
        [SerializeField] private Transform m_cardParent;
        [SerializeField] private CardObject m_cardobjectInstance;
        private List<CardObject> CardObjects = new List<CardObject>();
        private List<BaseCard> m_currentCards = new List<BaseCard>();

        public void DisplayCards(BaseCard[] cards)
        {
            var cardNames = String.Join(", ", cards.Select(card => card.Name));
            Debug.LogError($"Displaying cards: {cardNames}");

            m_currentCards.Clear();
            
            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                // SAMMOH TODO - DO NOT MAKE MORE
                // var cardObject = Instantiate(m_cardobjectInstance, m_cardParent, false);
                var cardObject = CardObjects[i];
                cardObject.IsUsed = false;
                cardObject.gameObject.SetActive(true);

                // scale cards to fit the parent.
                cardObject.GetComponent<RectTransform>().sizeDelta =
                    m_cardParent.GetComponent<RectTransform>().sizeDelta;
                
                cardObject.RenderCard(card);
                // cardObject.gameObject.SetActive(true);
                
                m_currentCards.Add(card);
                // show a tooltip when the card is hovered over for a certain amount of time.

                
                // need to make sure that the card can have a way to be cancelled.
                // m_currentCards.Add(card);
                // CardObjects.Add(cardObject);
            }
        }

        public void RenderCardObjects(BaseCard card, int position, Action<int> Button_SelectCard)
        {
            var cardObject = Instantiate(m_cardobjectInstance, m_cardParent, false);
            cardObject.GetComponent<RectTransform>().sizeDelta = m_cardParent.GetComponent<RectTransform>().sizeDelta;
            cardObject.SetButton(position, Button_SelectCard);
            cardObject.RenderCard(card);
            CardObjects.Add(cardObject);
        }

        // Additional card-related methods...
    }
}