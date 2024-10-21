using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// This cursor object will follow the owning player's mouse cursor and be visible to the other players.
    /// The host will use this object's movement for detecting collision with symbol objects.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MinigameInput : NetworkBehaviour
    {
        [SerializeField]
        SpriteRenderer m_renderer = default;
        [SerializeField]
        ParticleSystem m_onClickParticles = default;
        [SerializeField]
        TMPro.TMP_Text m_nameOutput = default;
        Camera m_mainCamera;
        NetworkVariable<Vector3> m_position = new NetworkVariable<Vector3>(Vector3.zero);
        ulong m_localId;

        // If the local player cursor spawns before this cursor's owner, the owner's data won't be available yet. This is used to retrieve the data later.
        Action<ulong, Action<PlayerData>> m_retrieveName;

        // The host is responsible for determining if a player has successfully selected a symbol object, since collisions should be handled serverside.
        // List<SymbolObject> m_currentlyCollidingSymbols; 
        
        private MinigameBase _currentMinigame;

        [SerializeField]
        private bool _isActive;
        
        public Action<Collider> OnInputTriggerEnter;
        public Action<Collider> OnInputTriggerExit;
        public Action OnInputClicked;

        /// <summary>
        /// This cursor is spawned in dynamically but needs references to some scene objects. Pushing full object references over RPC calls
        /// is an option if we create custom data for each and ensure they're all spawned on the host correctly, but it's simpler to do
        /// some one-time retrieval here instead.
        /// This also sets up the visuals to make remote player cursors appear distinct from the local player's cursor.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            m_retrieveName = NetworkedDataStore.Instance.GetPlayerData;
            m_mainCamera = GameObject.Find("InGameCamera").GetComponent<Camera>();
            
            // note this doesn't work on the client.
            // I do not care about the game, just the minigames.
            // MinigameManager.Instance.OnMiniGameStart[m_localId] += OnMinigameBegan;
            
            // The host is responsible for determining if a player has successfully selected a symbol object, since collisions should be handled serverside.
            // if (IsHost)
            //     m_currentlyCollidingSymbols = new List<SymbolObject>();
            
            m_localId = NetworkManager.Singleton.LocalClientId;
            
            // InGameRunner.Instance.onGameBeginning += OnGameBegan;
            // MinigameManager.Instance.AssignMinigameInput(m_localId, this);


            // Other players' cursors should be less prominent than the local player's cursor.
            if (OwnerClientId != m_localId)
            {
                // will not need to see other player's cursors.
                // m_renderer.transform.localScale *= 0.75f;
                // m_renderer.color = new Color(1, 1, 1, 0.5f);
                // var trails = m_onClickParticles.trails;
                // trails.colorOverLifetime = new ParticleSystem.MinMaxGradient(Color.grey);
            }
            else
            {
                m_renderer.enabled =
                    false; // The local player should see their cursor instead of the simulated cursor object, since the object will appear laggy.
            }
        }

        [ClientRpc]
        private void SetName_ClientRpc(PlayerData data)
        {
            if (!IsOwner)
                m_nameOutput.text = data.name;
        }

        // It'd be better to have a separate input handler, but we don't need the mouse input anywhere else, so keep it simple.
        private bool IsSelectInputHit()
        {
            return Input.GetMouseButtonDown(0);
        }

        // Sammoh todo Make sure that the inpt is only sent when the minigame is active.
        // make sure inout us sent from the client to the server and then to the minigame. 
        public void Update()
        {
            if (!_isActive) return;

            transform.position = m_position.Value;
            
            // Sammoh todo check to make sure that this is the host.
            if (m_mainCamera == null || !IsOwner)
                return;

            // set the player's input locally.
            Vector3 targetPos = (Vector2)m_mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, -m_mainCamera.transform.position.z));
            
            SetPosition_ServerRpc(targetPos); // Client can't set a network variable value.
            
            if (IsSelectInputHit())
                SendInput_ServerRpc(m_localId);
        }

        [ServerRpc] // Leave (RequireOwnership = true) for these so that only the player whose cursor this is can make updates.
        private void SetPosition_ServerRpc(Vector3 position)
        {
            m_position.Value = position;
        }

        [ServerRpc]
        private void SendInput_ServerRpc(ulong id)
        {
            // Sammoh taking out the input for the cursor. 
            // if (m_currentlyCollidingSymbols.Count > 0)
            // {
            //     SymbolObject symbol = m_currentlyCollidingSymbols[0];
            //     MinigameManager.Instance.OnPlayerInput(id, symbol);
            //     // InGameRunner.Instance.OnPlayerInput(id, symbol);
            // }
            
            OnInputClicked?.Invoke();

            OnInputVisuals_ClientRpc();
        }

        // Should send the input to the minigame for any logic that needs to be handled.
        [ClientRpc]
        private void OnInputVisuals_ClientRpc()
        {
            // this should only be played on the client that asked.
            if (!IsOwner)
                return;
            
            m_onClickParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            m_onClickParticles.Play();
        }
        
        /// <summary>
        /// The host is responsible for determining if a player has successfully selected a symbol object
        /// collisions should be handled serverside.
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerEnter(Collider other)
        {
            if (!IsHost)
                return;

            // the host should have a list of all the symbols that are currently colliding with the cursor.
            // other.TryGetComponent<SymbolObject>(out var symbol);
            
            OnInputTriggerEnter?.Invoke(other);
            
            // if (symbol == null)
            //     return;
            //
            // if (!m_currentlyCollidingSymbols.Contains(symbol))
            //     m_currentlyCollidingSymbols.Add(symbol);
        }

        public void OnTriggerExit(Collider other)
        {
            if (!IsHost)
                return;
            
            OnInputTriggerExit?.Invoke(other);

            
            // other.TryGetComponent<SymbolObject>(out var symbol);
            //
            // if (symbol == null)
            //     return;
            // if (m_currentlyCollidingSymbols.Contains(symbol))
            //     m_currentlyCollidingSymbols.Remove(symbol);
        }

        public  void OnMinigameBegan(ulong id, MinigameBase minigame)
        {
            // m_retrieveName.Invoke(OwnerClientId, SetName_ClientRpc);
            // _currentMinigame.OnMinigameStart -= OnMinigameBegan;
            
            // send the minigame to the client.
            EnableMinigameInput_ClientRpc(id, minigame);
            
            if (!IsHost) return;
            
            _currentMinigame = minigame;
            // _currentMinigame.OnMinigameStart += OnMinigameBegan;
            // _currentMinigame.OnMinigameStart += OnMinigameFinished;
        }
        
        private void OnMinigameFinished()
        {
            _isActive = false;
            _currentMinigame.OnMinigameFinish -= OnMinigameFinished;
        }

        // public void AddMinigame(MinigameBase minigame)
        // {
        //     // send the minigame to the client.
        //     SetMinigame_ClientRpc(minigame);
        //     
        //     if (!IsHost) return;
        //     
        //     _currentMinigame = minigame;
        //     _currentMinigame.OnMinigameStart += OnMinigameBegan;
        //     _currentMinigame.OnMinigameStart += OnMinigameFinished;
        // }
        
        [ClientRpc]
        private void EnableMinigameInput_ClientRpc(ulong id, bool isActive)
        {
            if (id != m_localId)
                return;

            _isActive = isActive;
            Debug.LogError($"Setting minigame input to {isActive}");
        }
    }
}