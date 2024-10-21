using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MurderMystery
{
    /// <summary>
    /// This is the player case file. It is used to show the player's case file.
    /// A case file tells the player what cards they have and what cards they need to win.
    /// </summary>
    public class PlayerCaseFile : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private CaseFileTitle caseFileTitle;
        [SerializeField] private CaseFileItem caseFileItem;

        [SerializeField] private Button closeButton;

        [SerializeField] private Transform scrollView;
        [SerializeField] private Transform contentPanel;

        private Dictionary<ulong, BaseCard> playerCharacters = new Dictionary<ulong, BaseCard>();
        private ulong m_localId;

        private Action _onButtonClose;

        private List<CaseFileItem> _caseFileItems = new List<CaseFileItem>();

        private IPlayerData localPlayerData;

        public void InitializeCheckList(Action onButtonClose)
        {
            m_localId = NetworkManager.Singleton.LocalClientId;

            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;

            closeButton.onClick.AddListener(() => EnableChecklist(false));
            
            _onButtonClose = onButtonClose;

            StartCoroutine(PopulateItems());

            // var playerInputPanel = NetworkManager.Singleton.ConnectedClients[m_localId].PlayerObject.GetComponent<PlayerInputPanel>();
            // playerInputPanel.OnCardReceived +=  OnCardValidated;
        }

        // Sammoh - todo scale the content panel to the size of the items

        // when a player is received, add it to the list. 
        // bypass the local player
        private void OnPlayerReceived(IPlayerData data)
        {
            if (data.id == m_localId)
            {
                localPlayerData = data;
            }
            NewCardSelector.Instance.GetPlayerCharacter(data.id, (id, characterCard) =>
            {
                playerCharacters.Add(data.id, characterCard);
            });
        }

        public void EnableChecklist(bool enable)
        {
            _canvasGroup.alpha = enable ? 1 : 0;
            _canvasGroup.interactable = enable;
            _canvasGroup.blocksRaycasts = enable;
            
            if (!enable)
                _onButtonClose?.Invoke();
        }

        private void OnCardValidated(CardObject obj)
        {
            if (obj is ActionCard) return;

            Debug.LogError("Card Added To Case File");

            var selectedFile = _caseFileItems.Find(caseFileItem => caseFileItem.Card == obj.CardData);
            selectedFile.SetState(CaseFileItem.PanelGameState.Validated);
        }

        private IEnumerator PopulateItems()
        {
            
            InGameRunner.Instance.GetPlayerData(OnPlayerReceived);
            
            // Debug.LogError("Populating Items");
            yield return new WaitUntil(() => playerCharacters.Count == InGameRunner.Instance.PlayerCount.Value);

            SpawnTitle("Suspects");
            SpawnItems(CardTypes.Character);
            yield return new WaitForSeconds(0.2f);
            SpawnTitle("Motives");
            SpawnItems(CardTypes.Motive);
            yield return new WaitForSeconds(0.2f);
            SpawnTitle("Clues");
            SpawnItems(CardTypes.Weapon);



            // validate local player;
            yield return new WaitForSeconds(0.4f);

            var playerRole = Role.Default;
            var playerCharacter = new BaseCard();
            
            NewCardSelector.Instance.GetPlayerCharacter(m_localId, (id, card) =>
            {
                playerCharacter = card;
            });

            NewCardSelector.Instance.GetPlayerRole(m_localId, (role) =>
            {
                playerRole = role.role;
            });
            
            // validate the character card
            if (playerRole != Role.Killer)
                ValidateCaseFile(playerCharacter);
            
            
            // validate hand
            NewCardSelector.Instance.GetPlayerCards(m_localId, (playerHand) =>
            {
                foreach (var card in playerHand)
                {
                    ValidateCaseFile(card);
                }
            });
        }

        // set the stat of a casefile object to validated
        public void ValidateCaseFile(BaseCard card)
        {
            var matchedFile = _caseFileItems.Find(caseFileItem => caseFileItem.Card.Name == card.Name);
            matchedFile?.SetState(CaseFileItem.PanelGameState.Validated);
        }

        private void SpawnTitle(string title)
        {
            var titleObject = Instantiate(caseFileTitle, contentPanel);
            titleObject.SetTitle(title);
        }

        private Action<CardObject> _onCardCollected;

        private void SpawnItems(CardTypes type)
        {
            NewCardSelector.Instance.GetCardsFromServer(type, card =>
            {
                // taking out the local player's card from the checklist.
                if (card.CardId == playerCharacters[localPlayerData.id].CardId) return;
                
                var cardIconInstance = Instantiate(caseFileItem, contentPanel);
                cardIconInstance.SetTitle(card, OnClicked);
                _caseFileItems.Add(cardIconInstance);
            });
        }


        private void OnClicked(CaseFileItem caseFile)
        {
            caseFile.ToggleState();
        }
    }
}