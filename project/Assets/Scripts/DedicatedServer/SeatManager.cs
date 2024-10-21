using System;
using System.Collections.Generic;
using LobbyRelaySample;
using Matchplay.Shared;
using MurderMystery;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Matchplay.Server
{
    /// <summary>
    /// Server spawns and manages the player positions.
    /// </summary>
    public class SeatManager : NetworkBehaviour
    {
        bool isGameStarted = false;
        List<Matchplayer> m_CurrentSeats = new List<Matchplayer>();

        public override void OnNetworkSpawn()
        {
            if (!IsServer || ApplicationData.IsServerUnitTest) //Ignore for server unit test
                return;
            
            Debug.LogError("Starting Seat Manager");

            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerSpawned += JoinSeat_Server;
            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerDespawned += LeaveSeat_Server;
            
            // GameManager.Instance.StartGame();
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer || ApplicationData.IsServerUnitTest || ServerSingleton.Instance == null)
                return;

            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerSpawned -= JoinSeat_Server;
            ServerSingleton.Instance.Manager.NetworkServer.OnServerPlayerDespawned -= LeaveSeat_Server;
        }

        void JoinSeat_Server(Matchplayer player)
        {
            m_CurrentSeats.Add(player);
            Debug.LogError($"{player.PlayerName} sat at the table. {m_CurrentSeats.Count} sat at the table.");
            player.transform.SetParent(transform);
            
            // var playerObject = NetworkManager.Singleton.ConnectedClients[m_localUserData.id].PlayerObject
            //     .GetComponent<PlayerInputPanel>();
            //
            // playerObject.InitPlayerPanel();
        }
        

        void LeaveSeat_Server(Matchplayer player)
        {
            m_CurrentSeats.Remove(player);
        }
    }
}