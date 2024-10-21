using System;
using System.Collections;
using CardGame.GameData.Cards;
using CardGame.Textures;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MurderMystery
{ 
    [RequireComponent(typeof(NetworkObject))]
    public class CardObject : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] 
        internal Animator m_animator;

        private bool _isUsed;
        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                _isUsed = value;
                // m_animator.SetBool("IsUsed", value);
            }
        }

        // take this out of runtime
        [SerializeField]
        private Image m_renderer;
        [SerializeField]
        private TextMeshProUGUI m_title;
        [SerializeField]
        private TextMeshProUGUI m_titleType;
        [SerializeField]
        private TextMeshProUGUI m_description;
        [SerializeField]
        private Transform m_overlay;
        
        public BaseCard CardData;
        public Image Image
        {
            get => m_renderer;
            set => m_renderer = value;
        }
        public TextMeshProUGUI Title
        {
            get => m_title;
            set => m_title = value;
        }
        public TextMeshProUGUI Description
        {
            get => m_description;
            set => m_description = value;
        }

        int _cardIndex;
        bool isInteractable = true;
        bool isInterested = false;
        float _timer = 0;
        Cooldown _cooldown = new Cooldown();
        CardIntent _cardIntents;
        
        public Action<CardObject> OnCardInterested { get; set; }
        private Action<int> OnClicked;
        
        // when the pointer is down, we want to wait for a second, and then if the pointer is still down, we want to show the card tooltip
        private void Update()
        {
            if (_cooldown.IsCountingDown())
            {
                _cooldown.Update();
                // wait a second
                _timer = 0;
                // start the timer and set interested to true
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable)
                return;

            isInterested = true;
            _cooldown.StartCountdown(0.5f,() =>
            {
                if (isInterested)
                {
                    // show the card tooltip
                    OnCardInterested?.Invoke(this);
                }
            });
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInteractable)
                return;
            
            if (!_cooldown.IsCountingDown())
            {
                isInterested = false;
            }
            else
            {
                // Debug.LogError("Do card action");

                OnClicked?.Invoke(_cardIndex);
                _cooldown.Interrupt(false);
            }
                
        }
        
        public void SetButton(int index, Action<int> onClicked)
        {
            _cardIndex = index;
            OnClicked += onClicked;
        }

        // initializes the card object
        public void RenderCard(BaseCard card)
        {
            // Set the main texture.
            var texture  = TextureCollectionReader.Readers["Avatars"].Textures[card.Avatar].GetPreview();
            var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            m_renderer.sprite = sprite;
                
            // set the title
            m_title.text = card.Name;

            // set the description. 
            Description.text = card.Desc;
            CardData = card;
            
            // set the card type
            m_titleType.text = this switch
            {
                // get the card type from 
                CharacterCard => "Character",
                WeaponClueCard => "Weapon",
                MotiveClueCard => "Motive",
                ActionCard => "Action",
                _ => m_titleType.text
            };        
        }
        
        public void EnableObjects(bool b)
        {
            var visible = !_isUsed && b;

            // Debug.LogError($"Setting card {CardData.Name} to {visible}");
            
            m_overlay.gameObject.SetActive(!visible);
            isInteractable = visible;
            m_titleType.gameObject.SetActive(visible);
            m_title.gameObject.SetActive(visible);
            m_description.gameObject.SetActive(visible);
            m_renderer.gameObject.SetActive(visible);
            
            gameObject.SetActive(visible);
        }
    }
}

