using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyRelaySample.UI
{
    /// <summary>
    /// We want to illustrate filtering the lobby list by some arbitrary variable. This will allow the lobby host to choose a color for the lobby, and will display a lobby's current color.
    /// (Note that this isn't sent over Relay to other clients for realtime updates.)
    /// </summary>
    public class TimerLobbyUI : MonoBehaviour
    {
        public bool m_UseLocalLobby;
        LocalLobby m_lobby;

        void Start()
        {
            if (m_UseLocalLobby)
                SetLobby(GameManager.Instance.LocalLobby);
        }

        private void SetLobby(LocalLobby lobby)
        {
            SetTimerCount(lobby.LocalLobbyTimer.Value);
            lobby.LocalLobbyTimer.OnChanged += SetTimerCount;
        }

        public void SetTimerCount(int timeCounter)
        {
            var timeCounterEnum = (TimeCounterEnum)timeCounter;
            GameManager.Instance.SetLocalLobbyTimer(timeCounterEnum);
        }
    }
}