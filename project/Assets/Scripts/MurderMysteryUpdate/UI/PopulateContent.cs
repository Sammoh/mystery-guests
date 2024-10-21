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
using Button = UnityEngine.UI.Button;

namespace MurderMystery
{
    public class PopulateContent : MonoBehaviour
    {
        [SerializeField] private CardIcon cardIcon;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        private int columns = 3;
        
        // refactor
        private RectTransform contentView;
        [SerializeField] private Button closeButton;

        private UnityEvent<IPlayerData> m_retrievePlayer = new UnityEvent<IPlayerData>();
        private List<int> playerCharacters = new List<int>();
        
        private Coroutine _populateItems;
        private ulong m_localId;

        public static PopulateContent Instance { get; private set; }
        public Action OnClose { get; set; }
        private Action OnCancel { get; set; }
        
        private CanvasGroup _canvasGroup;
        
        public void SetPopulateContentInstance(PopulateContent populateContent)
        {
            Instance = populateContent;
            
            _canvasGroup = GetComponent<CanvasGroup>();
            EnableUI(false);

            m_localId = NetworkManager.Singleton.LocalClientId;

            m_retrievePlayer.AddListener(OnPlayerReceived);
            InGameRunner.Instance.PopulatePlayerCharacters(m_retrievePlayer);
            
            contentView = contentParent.GetComponent<RectTransform>();
            
            closeButton.onClick.AddListener(OnClosePopup);
        }

        private void OnClosePopup()
        {                
            Debug.LogError("Canceled Selection");
            OnCancel?.Invoke();
            CleanUp();
        }

        // when a player is received, add it to the list. 
        // bypass the local player
        private void OnPlayerReceived(IPlayerData data)
        {
            if (data.id == m_localId) return;

            playerCharacters.Add(data.character);
        }

        // this is used as a general selection of all possible types of cards. 
        public void PopulateCards(CardTypes cardType, Action<BaseCard> onComplete, Action onFail, bool includePlayer = false)
        {
            NewCardSelector.Instance.GetCardListFromServer(cardType, (cardList) =>
            {
                DestroyIcons();
                EnablePopup(cardList.ToList(), onComplete, onFail);
            }, includePlayer);
        }

        // used as a specific selection of cards from a player's hand, case file, drawn cards, etc.
        public void PopulateCardArray<T>(BaseCard[] playerHand, Action<BaseCard> onComplete, Action onFail) where T : BaseCard
        {
            List<T> cardList = new List<T>();

            if (cardList.Count > 0)
            {
                foreach (var card in playerHand)
                {
                    if (card is T)
                    {
                        cardList.Add(card as T);
                    }
                }
                EnablePopup(cardList, onComplete, onFail);

            }else
                onComplete?.Invoke(cardList[0]);
        }

        private void EnablePopup<T>(List<T> cardList, Action<BaseCard>onComplete, Action onFail) where T : BaseCard
        {
            OnCancel = onFail;

            // set the constraint count to the number of columns
            gridLayoutGroup.constraintCount = columns;

            // and the size of the grid layout group to be the same as the content view
            // var viewParent = transform.GetComponent<RectTransform>();
            // var viewParentSize = viewParent.rect.size;

            var contentViewRect = contentView.rect;
            // contentViewRect.size = Vector2.one * viewParentSize.y;

            var sizeDelta = contentViewRect.size;
            var minSide = Mathf.Min(sizeDelta.x, sizeDelta.y);
            var contentScale = minSide / columns;
            gridLayoutGroup.cellSize = new Vector2(contentScale, contentScale);

            // contentView.sizeDelta =
            //     new Vector2(sizeDelta.x, contentScale * Mathf.CeilToInt(cardList.Count / (float)columns));

            foreach (var card in cardList)
            {
                var cardIconInstance = Instantiate(cardIcon, contentParent);
                cardIconInstance.InitIcon(card, card =>
                {
                    onComplete?.Invoke(card);
                    CleanUp();
                });

                var iconRect = cardIconInstance.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(contentScale, contentScale);
                
            }
            
            EnableUI(true);
        }

        private void EnableUI(bool isActive)
        {
            _canvasGroup.alpha = isActive ? 1 : 0;
            _canvasGroup.interactable = isActive;
            _canvasGroup.blocksRaycasts = isActive;
        }
        private void AdjustCardIconSize(CardIcon cardIconInstance)
        {
            // Get the size of the predefined rect
            var predefinedSize = contentView.rect.size;

            // Calculate the minimum value between width and height to maintain a square ratio
            var squareSize = Mathf.Min(predefinedSize.x, predefinedSize.y);

            // Apply the calculated square size to the CardIcon RectTransform
            var iconRect = cardIconInstance.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(squareSize, squareSize);
        }

        private void CleanUp()
        {
            EnableUI(false);
            OnClose?.Invoke();
        }

        private void DestroyIcons()
        {
            var icons = contentParent.GetComponentsInChildren<CardIcon>();
            foreach (var icon in icons)
            {
                Destroy(icon.gameObject);
            }
        }
        
        public void HandleSuggestion(Action<BaseCard[]> onComplete = null, Action onFail = null)
        {
            // need to tell the player to request the cards from the server
            
            _populateItems = StartCoroutine(HandleSuggestionCoroutine(cards =>
            {
                // Debug.LogError("Action passed, cleanup");
                onComplete?.Invoke(cards);
                CleanUp();
                
                if (_populateItems != null)
                {
                    StopCoroutine(_populateItems);
                    _populateItems = null;
                }
                
            }, () =>
            {
                if (_populateItems != null)
                {
                    StopCoroutine(_populateItems);
                    _populateItems = null;
                    
                    onFail?.Invoke();
                }
            }));
        }

        private IEnumerator HandleSuggestionCoroutine(Action<BaseCard[]> onComplete, Action onFail)
        {
            // long running process...
            var suggestion = new List<BaseCard>();

            PopulateCards(CardTypes.Character, card =>suggestion.Add(card), onFail, true);
            yield return new WaitUntil(() => suggestion.Count == 1);
            // Debug.LogError($"Made suggestion {suggestion[0].Name}");

            PopulateCards(CardTypes.Weapon, card => suggestion.Add(card), onFail);
            yield return new WaitUntil(() => suggestion.Count == 2);
            // Debug.LogError($"Made suggestion {suggestion[1].Name}");

            PopulateCards(CardTypes.Motive, card => suggestion.Add(card), onFail);
            yield return new WaitUntil(() => suggestion.Count == 3);
            // Debug.LogError($"Made suggestion {suggestion[2].Name}");
            
            onComplete?.Invoke(suggestion.ToArray());
            suggestion.Clear();
            CleanUp();
        }
    }
}
