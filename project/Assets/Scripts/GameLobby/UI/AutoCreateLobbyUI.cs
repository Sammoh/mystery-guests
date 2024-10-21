using LobbyRelaySample.UI;

namespace LobbyRelaySample
{
    /// <summary>
    /// Main menu start button.
    /// </summary>
    public class AutoCreateLobbyUI : UIPanelBase
    {
        public void ToLobbyMenu()
        {
            Manager.UIChangeMenuState(GameState.JoinMenu);
        }
    }
}
