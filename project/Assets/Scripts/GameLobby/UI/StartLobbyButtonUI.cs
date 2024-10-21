using LobbyRelaySample.UI;
using UnityEngine;

namespace LobbyRelaySample
{
    /// <summary>
    /// Main menu start button.
    /// </summary>
    public class StartLobbyButtonUI : UIPanelBase
    {
        [SerializeField]
        private CanvasGroup m_joinPanel;
        
        public void ToCreateMenu()
        {
            // get the development environment.
            

            m_joinPanel.alpha = 0;
            // this should also make sure that the join panel is not visible.
            // Manager.UIChangeMenuState(GameState.JoinMenu);
            Manager.CreateLobby("m_ServerName", Debug.isDebugBuild);
            
        }
        public void ToJoinMenu()
        {
            m_joinPanel.alpha = 1;
            // This should go straight to the join panel, but with only the insert code panel visible.
            Manager.UIChangeMenuState(GameState.JoinMenu);
        }
        
    }
}
