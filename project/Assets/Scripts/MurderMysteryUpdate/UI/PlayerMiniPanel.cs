using System;
using System.Collections.Generic;
using CardGame.GameData.Cards;
using CardGame.Textures;
using LobbyRelaySample;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MurderMystery
{

    public class PlayerMiniPanel : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        // private PanelGameState PanelState { get { return (PanelGameState) _animator.GetInteger("NumPanelState"); } set { _animator.SetInteger("NumPanelState", (int) value); } }
        private IntentType PanelState
        {
            get => (IntentType)_animator.GetInteger("NumPanelState");
            set => _animator.SetInteger("NumPanelState", (int)value);
        }


        [SerializeField] Button _panelButton;
        [SerializeField] private Text m_playerNameText;
        [SerializeField] private Image m_playerIcon;
        // [SerializeField] private Transform m_playerDeadIcon;

        private IPlayerData _playerData;
        public IPlayerData PlayerData => _playerData;

        public int PlayerCharacterIndex => _playerData.character;

        // [FormerlySerializedAs("gameState")] public PanelGameState panelGameState = PanelGameState.Innocent;

        public void Initialize(IPlayerData data, BaseCard characterCard)
        {
            _playerData = data;

            // set the player name
            var playerName = $"{data.name}";
            m_playerNameText.text = playerName;
            // set the player icon
            var texture  = TextureCollectionReader.Readers["Avatars"].Textures[characterCard.Avatar].GetPreview();
            var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            m_playerIcon.sprite = sprite;
            
            _panelButton.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            // tell the server that this player was selected.
            InGameRunner.Instance.PlayerSelected_ServerRpc(_playerData);
        }

        public void SetPanelState(IntentType intent)
        {
            PanelState = intent;
            
            Debug.LogError($"Setting {_playerData.name} State to {intent}");

            // switch (intent)
            // {
            //     case IntentType.ShowPlayer:
            //         break;
            //     case IntentType.SelectCard:
            //         break;
            //     case IntentType.ShowAllPlayers:
            //         break;
            //     case IntentType.SwitchCards:
            //         break;
            //     case IntentType.KillPlayer:
            //         m_playerDeadIcon.gameObject.SetActive(true);
            //         break;
            //     case IntentType.None:
            //     case IntentType.CollectCharacter:
            //     case IntentType.CollectMotive:
            //     case IntentType.CollectWeapon:
            //     case IntentType.MakeSuggestion:
            //         
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
            // }
        }

        private void OnPlayerSelected(PlayerData obj)
        {
            // check the game state.
        }
    }
}
