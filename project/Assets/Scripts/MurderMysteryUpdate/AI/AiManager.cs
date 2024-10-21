using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace MurderMystery.Ai
{
    public class AiManager : MonoBehaviour
    {
        // public Dictionary<ulong, AiPlayerData> PlayerData => m_playerData;
        private Dictionary<ulong, AiPlayerData> m_playerData;
        ulong m_localId;

        // Sammoh - todo Add a running list of what all of the ai players know. 
        // they should have a checklist. 
        // they should know where each item in their checklist came from. 
        // they should have a running count about who they suspect a player to be the killer.


        public static AiManager Instance
        {
            get
            {
                if (s_Instance!) return s_Instance;
                return s_Instance = FindObjectOfType<AiManager>();
            }
        }

        static AiManager s_Instance;

        Action<AiPlayerData> m_onGetCurrentCallback;
        UnityEvent<AiPlayerData> m_onEachPlayerCallback;


        /// <summary>
        /// Retrieve the data for all players in order from 1st to last place, calling onEachPlayer for each.
        /// </summary>
        public void GetAllPlayerData(UnityEvent<AiPlayerData> onEachPlayer)
        {
            m_onEachPlayerCallback = onEachPlayer;
            GetAllPlayerData();
        }

        void GetAllPlayerData()
        {
            var sortedData = m_playerData.Select(kvp => kvp.Value).OrderBy(data => data.id);
            // GetAllPlayerData_ClientRpc(sortedData.ToArray());

            int rank = 1;
            foreach (var data in sortedData)
            {
                m_onEachPlayerCallback.Invoke(data);
                rank++;
            }

            m_onEachPlayerCallback = null;
        }

        public void Initialize()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
            m_playerData = new Dictionary<ulong, AiPlayerData>();
            var aiCount = InGameRunner.Instance.AiCount;
            Debug.Log($"Adding {aiCount} ai players.");
            // todo - add ai players to the list of players.

            var playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            for (var i = 0; i < aiCount; i++)
            {
                var name = $"AI {i}";
                var runningCount = playerCount + i;
                var id = (ulong)runningCount;
                AddAiPlayer(id, name, runningCount);
            }

            InGameRunner.Instance.onRoundBeginning += OnRoundStart;
        }

        private void OnRoundStart()
        {
            StartCoroutine(StartAiTimer());
        }

        private void AddAiPlayer(ulong id, string name, int index = -1)
        {
            
            var playerData = new AiPlayerData(name, id, index)
            {
            };
            m_playerData.Add(id, playerData);
        }

        // Start a random timer for the ai manager to make a move.
        private IEnumerator StartAiTimer()
        {
            var randomTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(randomTime);

            // MakeAiMove();
            var randomIndex = Random.Range(0, 3);
            var randomAiData = GetRandomAiData();
            // var randomAiCardFromHand = randomAiData.handArray[randomIndex];
            // var randomAiCardFromHandData = NewCardSelector.Instance.GetCard<BaseCard>(randomAiCardFromHand);


            // selecting the first player in the list.
            // var randomPlayer = InGameRunner.Instance.PlayerList[0];
            // var randPlayerCharacterCard = NewCardSelector.Instance.GetCard<MMCharacterCard>(randomPlayer.character);

            // var newIntent = new CardIntent();
            // newIntent.selectedCharacter = randomPlayer.character;
            // newIntent.selectedCardIndex = randomAiCardFromHand;
            // newIntent.instruction = randomAiCardFromHandData.Instructions;
            //
            // randomAiCardFromHandData.NewIntent(newIntent);
            //
            // Debug.LogError(
            //     $"{randomAiData.name} sending {randomAiCardFromHandData.name} to {randPlayerCharacterCard.name}");
            // InGameRunner.Instance.OnPlayerInput(randomAiData.id, randomAiCardFromHand,
            //     randomAiCardFromHandData.CardIntent);
        }

        AiPlayerData GetRandomAiData()
        {
            var randomIndex = UnityEngine.Random.Range(0, m_playerData.Count);
            var randomPlayer = m_playerData.ElementAt(randomIndex);
            return randomPlayer.Value;
        }

        // Sammoh - taking out card reaction for now. 
        public void SelectCardReaction(ulong caller, ulong playerDataID, CardIntent intent)
        {
            // var originalCallerData = m_playerData[caller];
            // var randomCardFromHand = originalCallerData.handArray[Random.Range(0, 3)];
            // // mutate the intent.
            // intent.selectedCharacter = originalCallerData.character;
            // intent.selectedCardIndex = randomCardFromHand;
            // InGameRunner.Instance.OnPlayerInput(playerDataID, randomCardFromHand, intent);
        }

        public void UpdateHand(ulong playerID, int[] playerHandArray)
        {
            // var playerData = m_playerData[playerID];
            // playerData.handArray = playerHandArray;
        }

        public AiPlayerData GetAiPlayerData(ulong playerID)
        {
            return m_playerData[playerID];
        }

        public void ShowCard(AiPlayerData aiPlayerData, int intentSelectedCardIndex)
        {
            // add known cards to the ai player data.
            Debug.LogError("add selected cards to the ai player data.");
            //aiPlayerData.KnownCards.Add(intentSelectedCardIndex);
        }
    }

}


