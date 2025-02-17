using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyRelaySample.UI
{
    /// <summary>
    /// When inside a lobby, this will show information about a player, whether local or remote.
    /// </summary>
    public class InLobbyUserUI : UIPanelBase
    {
        [SerializeField]
        TMP_Text m_DisplayNameText;

        [SerializeField]
        TMP_Text m_StatusText;

        [SerializeField]
        Image m_HostIcon;

        [SerializeField]
        Image m_EmoteImage;

        [SerializeField]
        Sprite[] m_EmoteIcons;

        public bool IsAssigned => UserId != null;
        public string UserId { get; set; }
        [SerializeField] LocalPlayer m_LocalPlayer;

        public void SetUser(LocalPlayer localPlayer)
        {
            transform.parent.gameObject.SetActive(true);
            Show();
            
            m_LocalPlayer = localPlayer;
            
            UserId = m_LocalPlayer.ID.Value;
            SetIsHost(m_LocalPlayer.IsHost.Value);
            SetEmote(m_LocalPlayer.Emote.Value);
            SetUserStatus(m_LocalPlayer.UserStatus.Value);
            SetDisplayName(m_LocalPlayer.DisplayName.Value);
            SubscribeToPlayerUpdates();
        }

        void SubscribeToPlayerUpdates()
        {
            m_LocalPlayer.DisplayName.OnChanged += SetDisplayName;
            m_LocalPlayer.UserStatus.OnChanged += SetUserStatus;
            m_LocalPlayer.Emote.OnChanged += SetEmote;
            m_LocalPlayer.IsHost.OnChanged += SetIsHost;
        }

        void UnsubscribeToPlayerUpdates()
        {
            if (m_LocalPlayer == null)
                return;
            if (m_LocalPlayer.DisplayName?.OnChanged != null)
                m_LocalPlayer.DisplayName.OnChanged -= SetDisplayName;
            if (m_LocalPlayer.UserStatus?.OnChanged != null)
                m_LocalPlayer.UserStatus.OnChanged -= SetUserStatus;
            if (m_LocalPlayer.Emote?.OnChanged != null)
                m_LocalPlayer.Emote.OnChanged -= SetEmote;
            if (m_LocalPlayer.IsHost?.OnChanged != null)
                m_LocalPlayer.IsHost.OnChanged -= SetIsHost;
        }

        public void ResetUI()
        {
            if (m_LocalPlayer == null)
                return;
            transform.parent.gameObject.SetActive(false);
            UserId = null;
            SetEmote(EmoteType.None);
            SetUserStatus(PlayerStatus.Lobby);
            Hide();
            UnsubscribeToPlayerUpdates();
            m_LocalPlayer = null;
        }

        void SetDisplayName(string displayName)
        {
            m_DisplayNameText.SetText(displayName);
        }

        void SetUserStatus(PlayerStatus statusText)
        {
            Debug.LogError("Status: " + statusText);
            m_StatusText.SetText(SetStatusFancy(statusText));
        }

        void SetEmote(EmoteType emote)
        {
            m_EmoteImage.sprite = EmoteIcon(emote);
        }

        void SetIsHost(bool isHost)
        {
            m_HostIcon.enabled = isHost;
        }

        /// <summary>
        /// EmoteType to Icon Sprite
        /// m_EmoteIcon[0] = Smile
        /// m_EmoteIcon[1] = Frown
        /// m_EmoteIcon[2] = UnAmused
        /// m_EmoteIcon[3] = Tongue
        /// </summary>
        Sprite EmoteIcon(EmoteType type)
        {
            switch (type)
            {
                case EmoteType.None:
                    m_EmoteImage.color = Color.clear;
                    return null;
                case EmoteType.Smile:
                    m_EmoteImage.color = Color.white;
                    return m_EmoteIcons[0];
                case EmoteType.Frown:
                    m_EmoteImage.color = Color.white;
                    return m_EmoteIcons[1];
                case EmoteType.Unamused:
                    m_EmoteImage.color = Color.white;
                    return m_EmoteIcons[2];
                case EmoteType.Tongue:
                    m_EmoteImage.color = Color.white;
                    return m_EmoteIcons[3];
                default:
                    return null;
            }
        }

        string SetStatusFancy(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.Lobby:
                    return "<color=#56B4E9>In Lobby</color>"; // Light Blue
                case PlayerStatus.Ready:
                    return "<color=#009E73>Ready</color>"; // Light Mint
                case PlayerStatus.Connecting:
                    return "<color=#F0E442>Connecting...</color>"; // Bright Yellow
                case PlayerStatus.InGame:
                    return "<color=#005500>In Game</color>"; // Green
                default:
                    return "";
            }
        }
    }
}