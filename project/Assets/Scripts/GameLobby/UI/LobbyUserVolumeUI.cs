using UnityEngine;
using UnityEngine.UI;

namespace LobbyRelaySample.UI
{
    public class LobbyUserVolumeUI : MonoBehaviour
    {
        [SerializeField]
        UIPanelBase m_volumeSliderContainer;
        [SerializeField]
        UIPanelBase m_muteToggleContainer;
        [SerializeField]
        [Tooltip("This is shown for other players, to mute them.")]
        GameObject m_muteIcon;
        [SerializeField]
        [Tooltip("This is shown for the local player, to make it clearer that they are muting themselves.")]
        GameObject m_micMuteIcon;
        public bool IsLocalPlayer { private get; set; }

        [SerializeField]
        Slider m_volumeSlider;
        [SerializeField]
        Toggle m_muteToggle;
        
    }
}
