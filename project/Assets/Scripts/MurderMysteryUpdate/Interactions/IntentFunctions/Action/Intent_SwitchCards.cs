using System;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using MurderMystery;
using MurderMystery.Ai;
using Unity.Netcode;

/// <summary>
/// select a player and one of your cards, then the other player will react with SelectCard
/// </summary>
public class Intent_SwitchCards : IIntentProcessor
{
    public void ProcessIntent(ulong caller, CardIntent intent, Action<CardIntent> onComplete = null, Action onFail = null)
    {
        // Implementation for processing SwitchCardsIntent

        // Selected card is the character, the index is it's card index.
        // tell the other player to make the switch.

        var gameRunner = InGameRunner.Instance;
        var m_cardSelector = NewCardSelector.Instance;
        
        var selectedCharacterIndex = intent.selectedCharacter;
        var selectedCardIndex = intent.selectedCardIndex;

        var callerName = gameRunner.PlayerList[caller].name;
        IPlayerData playerData = null;
            
        // get the player data from the selected player card
        // foreach (var player in gameRunner.PlayerList.Values.Where(player => player.character == selectedCharacterIndex))
        //     playerData = player;

        // if the player is an ai, then we can just select the card for them
        if (playerData is AiPlayerData)
        {
            AiManager.Instance.SelectCardReaction(caller, playerData.id, intent);
        }
        else
        {
            // // relays the information to the client.
            // var playerPanel = NetworkManager.Singleton.ConnectedClients[playerData.id].PlayerObject.GetComponent<PlayerInputPanel>();
            // playerPanel.SelectCardReaction(caller, playerData.id); 
            //     
            // var playerCharacter = m_cardSelector.GetCard<BaseCard>(selectedCharacterIndex);
            // var characterName = playerCharacter.Name;
            //     
            // var selectedCard = m_cardSelector.GetCard<BaseCard>(selectedCardIndex);
            // var cardName = selectedCard.Name;
            //
            // // this would be the callback for the player to select a card to switch with
            // // the player would select a card and then the server would be expecting a reaction from the player
            // // _reactionQueue.Add(playerData.id, intent);
            //
            // var msg = $"switching {callerName}'s card, {cardName} with {characterName}";
            // playerPanel.PrintMessage(msg, false, playerData.id);
        }
        
        intent.hasPassed = true;
        onComplete?.Invoke(intent);
    }
}

