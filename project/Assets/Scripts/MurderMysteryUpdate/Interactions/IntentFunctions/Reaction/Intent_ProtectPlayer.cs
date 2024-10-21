using System;
using System.Linq;
using LobbyRelaySample.ngo;
using MurderMystery;
using Unity.Netcode;

internal class Intent_ProtectPlayer : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // var m_cardSelector = NewCardSelector.Instance;
        // // caller Data
        // var playerData = InGameRunner.Instance.PlayerList[caller];
        // var playerPanel = NetworkManager.Singleton.ConnectedClients[caller].PlayerObject
        //     .GetComponent<PlayerInputPanel>();
        // playerPanel.SelectCardReaction(caller, playerData.id);
        
        var playerList = InGameRunner.Instance.PlayerList.Values;
        var selectedPlayer = intent.selectedCharacter;
        var selectedPlayerData = playerList.Select(player => player).FirstOrDefault(player => player.character == selectedPlayer);
        var selectedCharacterCard = NewCardSelector.Instance.PlayerCharacters[selectedPlayerData.id];
        
        NewCardSelector.Instance.RoleDataList[caller].selectedCard = selectedCharacterCard.CardId;

        onComplete?.Invoke(intent);
    }
}
