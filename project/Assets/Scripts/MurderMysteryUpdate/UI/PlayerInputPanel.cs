using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using CardGame.Textures;
using LobbyRelaySample;
using LobbyRelaySample.ngo;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace MurderMystery
{
    /// <summary>
    /// This is the player input panel. It is used to show the player's cards and allow them to use their cards.
    /// </summary>
    public class PlayerInputPanel : NetworkBehaviour
    {
        [SerializeField] private CanvasGroup m_canvasGroup;
        [SerializeField] private Transform m_cardParent;
        [SerializeField] private CardObject m_cardobjectInstance;
        [Header("Buttons")] 
        [SerializeField] private Button m_useAbilitybutton;
        [SerializeField] private Button m_suggestButton;
        [SerializeField] private Button m_clipboardButton;
        [SerializeField] private Toggle m_suspectViewButton;

        [SerializeField] private Text infoText;
        // [SerializeField] private Text m_playerInfo;
        [SerializeField] private GameObject isKilledPanel;

        // [FormerlySerializedAs("m_populateContentInstance")] [SerializeField]
        // private PopulateContent m_populateContent;
        
        [SerializeField]
        private PopUpUI m_popUpUI;

        [SerializeField]
        private PlayerCaseFile m_PlayerCaseFile;

        [SerializeField] 
        private PopulateContent m_PopulateContent;

        private ulong m_localId;
        // private List<BaseCard> m_currentCards = new ();
        private Coroutine _currentCoroutine;

        [SerializeField] private GameObject rowNetworkedPrefab = default;
        [SerializeField] private PlayerMiniPanel m_playerMiniPanelPrefab = default;
        [SerializeField] private List<PlayerMiniPanel> m_playerMiniPanels = new List<PlayerMiniPanel>();
        
        [SerializeField] private Transform rowParent;
        [SerializeField] private CanvasGroup m_suspectCanvasGroup;
        [SerializeField] private Toggle m_suspectToggleButton;
        
        [SerializeField] private Image m_playerImage;

        public List<GameObject> _rows = new List<GameObject>();

        [SerializeField] private int columnCount = 5;

        private List<CardObject> m_cardObjects = new List<CardObject>();

        private bool _abilityUsed = false;
        private bool _suggestionUsed = false;
        
        [SerializeField]
        private TMP_Text m_debugText;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                gameObject.SetActive(false);
                return;
            }
            
            isKilledPanel.SetActive(false);

            m_localId = NetworkManager.Singleton.LocalClientId;
            
            m_suspectViewButton.onValueChanged.AddListener(SetSuspectVisibility);
            m_useAbilitybutton.onClick.AddListener(Button_EnableAbilityPanel);
            m_suggestButton.onClick.AddListener(Button_EnableSuggestPanel);
            m_clipboardButton.onClick.AddListener(Button_EnableClipboardPanel);

            m_useAbilitybutton.gameObject.SetActive(false);
            m_suggestButton.gameObject.SetActive(false);
            m_clipboardButton.gameObject.SetActive(false);
            
            m_suspectToggleButton.gameObject.SetActive(false);
            m_suspectCanvasGroup.alpha = 0;
        }

        public void InitializeGameState()
        {
            InGameRunner.Instance.onGameBeginning += OnGameBegan;
            InGameRunner.Instance.onRoundBeginning += OnRoundBegan;
            InGameRunner.Instance.onRoundEnding += OnRoundFinished;
            InGameRunner.Instance.onRoundRestart += OnRestartRound;
        }

        private void OnGameBegan()
        {
            InGameRunner.Instance.onGameBeginning -= OnGameBegan;
            
            // instantiate the card objects for the player.
            var playerHandSize = 3;
            for (int i = 0; i < playerHandSize; i++)
            {
                var cardObject = Instantiate(m_cardobjectInstance, m_cardParent, false);
                cardObject.gameObject.SetActive(false);
                m_cardObjects.Add(cardObject);
                cardObject.SetButton(i, Button_SelectCard);
                cardObject.OnCardInterested += o =>
                {
                    m_tooltip.Show(o);
                };
            }
        }

        private void OnRoundBegan()
        {
            if (!IsOwner) return;
            
            // Debug.LogError("Beginning the round");
            
            m_canvasGroup.alpha = 1;

            InGameRunner.Instance.onRoundBeginning -= OnRoundBegan;
            m_PopulateContent.SetPopulateContentInstance(m_PopulateContent);

            m_useAbilitybutton.gameObject.SetActive(true);
            m_suggestButton.gameObject.SetActive(true);
            m_clipboardButton.gameObject.SetActive(true);
            m_suspectToggleButton.gameObject.SetActive(true);
            
            // server should have all the cards, so request them.
            NewCardSelector.Instance.GetPlayerCards(m_localId, DisplayCards);

            NewCardSelector.Instance.GetPlayerCharacter(m_localId, (id, playerCharacter) =>
            {
                // Get the preview data from the avatar
                if (!TextureCollectionReader.Readers["Avatars"].Textures.ContainsKey(playerCharacter.Avatar)) {
                    Debug.LogErrorFormat ("Target texture " + playerCharacter.Avatar + " is not found on avatars pool.", this);
                } else {
                    var texture = TextureCollectionReader.Readers["Avatars"].Textures[playerCharacter.Avatar].GetPreview();
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    m_playerImage.sprite = sprite;
                }
            });

            // Sammoh TODO Add this back in
            m_PlayerCaseFile.InitializeCheckList(() => SetButtonsActive(true));

            SpawnMiniPanels();
            
            // sign up cards to populate content 
            foreach (var cardObject in m_cardObjects)
            {
                PopulateContent.Instance.OnClose += () =>
                {
                    cardObject.IsUsed = false;
                };   
            }
            
            // debugging the card selection.
            m_debugText.text =  NewCardSelector.Instance.debugCards;
        }
        
        private void OnRoundFinished()
        {
            PrintMessage("Round Finished");
            m_canvasGroup.alpha = 0;
        }

        private void OnRestartRound()
        {
            if (!IsOwner) return;
            
            Debug.LogError("Sammoh - OnRestartRound");
            
            // request the cards again.
            UpdatePlayerCards_ClientRpc(m_localId);
            
            m_canvasGroup.alpha = 1;
            
            m_useAbilitybutton.gameObject.SetActive(true);
            m_suggestButton.gameObject.SetActive(true);
            m_clipboardButton.gameObject.SetActive(true);
            m_suspectToggleButton.gameObject.SetActive(true);

        }

        #region Player Buttons

        Action<ulong, CardIntent> _onCardReaction;

        public void Button_SelectCard(int cardPosition)
        {
            // get the card from the card position.
            // var card = m_currentCards[cardPosition];
            // var cardObject = m_cardObjects.Find(cardIndex => cardIndex.CardData == card);
            var cardObject = m_cardObjects[cardPosition];
            cardObject.EnableObjects(false);
            
            InGameRunner.Instance.OnPlayerInput(m_localId, cardObject.CardData, OnCardFinished, OnCardCancelled);
        }
        
        private void OnCardFinished(BaseCard card)
        {
            Debug.LogError($"The card {card.Name} was used");
            var cardObject = m_cardObjects.Find(cardIndex => cardIndex.CardData.CardId == card.CardId);
            cardObject.IsUsed = true;
            cardObject.EnableObjects(true);
        }

        private void OnCardCancelled()
        {
            Debug.LogError("The move was cancelled");
            SetButtonsActive(true);
        }

        public void Button_EnableSuggestPanel()
        {
            SetButtonsActive(false);
            
           OnHandleSuggestion(m_localId, () =>
            {
                _suggestionUsed = true;
                SetButtonsActive(true);
            });
        }
        
        private Action OnSuggestionMade;

        private void OnHandleSuggestion(ulong playerId, Action onComplete)
        {
            OnSuggestionMade += onComplete;
            OnHandleSuggestion_ServerRpc(playerId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void OnHandleSuggestion_ServerRpc(ulong caller)
        {
            IntentProcessing.Instance.ProcessSuggestion(caller, SuggestionCallback_ClientRpc, SuggestionCancelled_ClientRpc);
        }

        [ClientRpc]
        void SuggestionCallback_ClientRpc(ulong id)
        {
            if (id != m_localId) return;

            OnSuggestionMade?.Invoke();
            OnSuggestionMade = null;
        }
        
        [ClientRpc]
        private void SuggestionCancelled_ClientRpc(ulong id)
        {
            if (id != m_localId) return;

            _suggestionUsed = false;
            SetButtonsActive(true);
            OnSuggestionMade = null;
        }

        private void Button_EnableAbilityPanel()
        {
            SetButtonsActive(false);
            InGameRunner.Instance.UseAbility(m_localId, () =>
            {
                m_useAbilitybutton.enabled = false;
                _abilityUsed = true;
                SetButtonsActive(true);
            }, () =>
            {
                SetButtonsActive(true);
            });

        }

        // this will open a list with interactable check marks. 
        public void Button_EnableClipboardPanel()
        {
            SetButtonsActive(false);
            m_PlayerCaseFile.EnableChecklist(true);
        }

        #endregion

        #region Netcode


        [SerializeField] private SimpleTooltip m_tooltip;

        public void UpdateCards(ulong id)
        {
            UpdatePlayerCards_ClientRpc(id);
        }

        [ClientRpc]
        private void UpdatePlayerCards_ClientRpc(ulong id)
        {
            if(m_localId != id) return;
            Debug.LogError("Updating cards");
            NewCardSelector.Instance.GetPlayerCards(id, DisplayCards);
        }
        
        private void DisplayCards(BaseCard[] cards)
        {
            // SAMMOH todo clean this up. NEVER DESTROY.
            // var cardNames = String.Join(", ", cards.Select(card => card.Name));
            // Debug.LogError($"Displaying cards: {cardNames}");
            
            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                var cardObject = m_cardObjects[i];
                
                cardObject.IsUsed = false;
                cardObject.gameObject.SetActive(true);

                // scale cards to fit the parent.
                cardObject.GetComponent<RectTransform>().sizeDelta =
                    m_cardParent.GetComponent<RectTransform>().sizeDelta;
                
                cardObject.RenderCard(card);
            }
            
            SetButtonsActive(true);
        }
        
        [ClientRpc]
        void PrintGlobalMessage_ClientRpc(ulong id, string msg)
        {
            SetText(msg);
        }

        [ClientRpc]
        void PrintLocalMessage_ClientRpc(ulong id, string msg)
        {
            if (id != m_localId) return;

            infoText.text = msg;
            Debug.LogError(msg);
        }

        #endregion

        private void SpawnMiniPanels()
        {
            // spawn rows 
            var playerCount = InGameRunner.Instance.PlayerCount.Value - 1;

            // create new rows.  
            for (var i = 0; i < playerCount; i++)
            {
                if (i % columnCount != 0) continue;
                var newRow = Instantiate(rowNetworkedPrefab);
                newRow.name = $"Row {i}";
                _rows.Add(newRow);
                newRow.transform.SetParent(rowParent, false);
            }
            
            InGameRunner.Instance.GetPlayerData((player) =>
            {
                if (player.id == m_localId) return;
                
                // spawn mini panels that are not mine.
                var row = _rows[(int)player.id % _rows.Count];
                var miniPanel = Instantiate(m_playerMiniPanelPrefab, row.transform, false);
                miniPanel.name = player.name;
                m_playerMiniPanels.Add(miniPanel);
                
                NewCardSelector.Instance.GetPlayerCharacter(player.id, (id, card) =>
                {
                    miniPanel.Initialize(player, card);
                });
            });
        }
        
        public void SetSuspectVisibility(bool isVisible)
        {
            if (m_suspectCanvasGroup == null) return;
            m_suspectCanvasGroup.alpha = isVisible ? 1 : 0;
        }

        private void SetText(string msg)
        {
            infoText.text = msg;
            Debug.LogError(msg);
        }

        private Dictionary<ulong, Action<BaseCard>> _onPopupComplete = new Dictionary<ulong, Action<BaseCard>>();
        private Dictionary<ulong, Action> _onPopupCanceled = new Dictionary<ulong, Action>();

        Coroutine _onPopupCompleteCoroutine;
        Coroutine _onPopupCanceledCoroutine;
        
        public void EnablePopupPanel(ulong id, CardTypes type, Action<BaseCard> onComplete, Action onCanceled = null)
        {
            _onPopupComplete.Add(id, onComplete);
            _onPopupCanceled.Add(id, onCanceled);
            EnablePopupPanel_ClientRpc(id, type);
        }
        
        [ClientRpc]
        private void EnablePopupPanel_ClientRpc(ulong id, CardTypes cardType)
        {
            if (id != m_localId) return;
            
            PopulateContent.Instance.PopulateCards(cardType, selectedCard =>
            {
                OnPopupSelection_ServerRpc(id, selectedCard);
            },
            () =>
            {
                OnPopupCanceled_ServerRpc(id);
            });
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void OnPopupSelection_ServerRpc(ulong id, BaseCard card = null)
        {
            _onPopupComplete[id]?.Invoke(card);
            _onPopupComplete.Remove(id);
            _onPopupCanceled.Remove(id);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void OnPopupCanceled_ServerRpc(ulong id)
        {
            if (!_onPopupCanceled.ContainsKey(id))
            {
                Debug.LogError("No cancel action found.");
                return;
            }
            
            _onPopupCanceled[id]?.Invoke();
            _onPopupComplete.Remove(id);
            _onPopupCanceled.Remove(id);
        }
        
        public void PrintMessage(string msg, bool isGlobal = true, ulong receiver = 0)
        {
            if (isGlobal)
                PrintGlobalMessage_ClientRpc(m_localId, msg);
            else
                PrintLocalMessage_ClientRpc(receiver, msg);
        }

        public void ShowCard(ulong caller, BaseCard card)
        {
            Debug.LogError($"Showing card: {card.Name}");
            ShowCard_ClientRpc(caller, card);
        }

        [ClientRpc]
        private void ShowCard_ClientRpc(ulong caller, BaseCard cardIndex)
        {
            m_popUpUI.ShowPopup($"You have been shown the card: {cardIndex.Name}");
            m_PlayerCaseFile.ValidateCaseFile(cardIndex);
        }

        public void UpdatePlayerStatus(ulong targetId, IntentType intent)
        {
            UpdatePlayerStatus_ClientRpc(targetId, intent);
        }
        
        [ClientRpc]
        private void UpdatePlayerStatus_ClientRpc(ulong targetId, IntentType intent)
        {
            // everyone get the update.
            m_playerMiniPanels.Find(panel => panel.PlayerData.id == targetId)?.SetPanelState(intent);
        }

        public void SetPanelState(ulong targetId, IntentType intent)
        {
            SetPanelState_ClientRpc(targetId, intent);
        }

        [ClientRpc]
        private void SetPanelState_ClientRpc(ulong targetId, IntentType intent)
        {
            if (targetId != m_localId) return;

            switch (intent)
            {
                case IntentType.KillPlayer:
                    isKilledPanel.SetActive(true);
                    break;
            }
        }

        private void SetButtonsActive(bool isActive)
        {
            // convert all of these into single use  buttons.
            m_useAbilitybutton.interactable = isActive && !_abilityUsed;
            m_suggestButton.interactable = isActive && !_suggestionUsed;
            m_clipboardButton.interactable = isActive;
            
            // these will be on their own cards.
            // Sammoh todo get the button from an instanced card rather than the card data itself.
            
            foreach (var cardObject in m_cardObjects)
            {
                cardObject.EnableObjects(isActive);
            }
        }
    }
}