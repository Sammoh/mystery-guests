using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame;
using CardGame.GameData.Cards;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MurderMystery;
using MurderMystery.Ai;
using TMPro;
using UnityEngine.Serialization;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// Once the NetworkManager has been spawned, we need something to manage the game state and setup other in-game objects
    /// that is itself a networked object, to track things like network connect events.
    /// </summary>
    public class InGameRunner : NetworkBehaviour
    {
        #region Previous Game Objects. 
        [SerializeField]
        private PlayerCursor m_playerCursorPrefab = default;
        [FormerlySerializedAs("m_playerInputPrefab")] [SerializeField]
        private MinigameInput m_minigameInputPrefab = default;
        [SerializeField]
        private SymbolContainer m_symbolContainerPrefab = default;
        private SymbolContainer m_symbolContainerInstance;
        [SerializeField]
        private SymbolObject m_symbolObjectPrefab = default;
        // [SerializeField]
        // private SequenceSelector m_sequenceSelector = default;
        [SerializeField]
        private Scorer m_scorer = default;
        [SerializeField]
        private SymbolKillVolume m_killVolume = default;
        #endregion
        
        #region New Game Objects

        // card selector
        [SerializeField]
        private NewCardSelector m_cardSelector = default;

        #endregion
        
        [SerializeField]
        private Image m_generatedImage = default;
        CanvasGroup m_generatedImageCanvasGroup;
        
        [SerializeField]
        private Text introRoleText = default;
        
        [SerializeField]
        private IntroOutroRunner m_introOutroRunner = default;
        [SerializeField]
        private NetworkedDataStore m_dataStore = default;

        public Action onGameBeginning, onRoundBeginning, onRoundEnding, onRoundRestart;
        Action m_onConnectionVerified, m_onGameEnd;
        private int m_expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        private bool? m_canSpawnInGameObjects;
        
        private Queue<Vector2> m_pendingSymbolPositions = new Queue<Vector2>();
        private float m_symbolSpawnTimer = 0.5f; // Initial time buffer to ensure connectivity before loading objects.
        private int m_remainingSymbolCount = 0; // Only used by the host.
        private float m_timeout = 10;
        private bool m_hasConnected = false;
        
        public NetworkVariable<int> PlayerCount = new ();

        [SerializeField]
        private NetworkedTimer m_timer;

        #region AI Components
        
        // fill out the ai with all the characters that are not in the player list.
        private int aiCount
        {
            get
            {
                var aiCountOverride = GameManager.Instance.LocalLobby.LocalLobbyAiCount.Value;

                // get the number of players and the number of characters
                var playerCount = GameManager.Instance.LocalLobby.PlayerCount;
                // subtract the number of players from the number of characters to get the number of fillers
                var aiFillCount = GameSettings.MaxPlayers - playerCount;
                // if the number of fillers is less than 0, set it to 0
                var count = aiCountOverride <= 0 ? aiCountOverride : aiFillCount;
                return count;
            }
            set => aiCount = value ;
        }
        public int AiCount => aiCount;
        private bool useAi
        {
            get
            {
                var aiCountOverride = GameManager.Instance.LocalLobby.LocalLobbyAiCount.Value != 0;
                return aiCountOverride;
            }
            set{}
        }
        
        #endregion

        private PlayerData
            m_localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.

        [SerializeField] private BackgroundGenerator bgGenerator;
        // Sammoh - TODO: byte array does not serialize across the network.
        // private NetworkVariable<byte[]> networkedTextureBytes = new NetworkVariable<byte[]>();
        // private NetworkedTexture m_networkedTexture2D = new();

        #region Instance
        public static InGameRunner Instance
        {
            get
            {
                if (s_Instance!) return s_Instance;
                return s_Instance = FindObjectOfType<InGameRunner>();
            }
        }
        static InGameRunner s_Instance;
        #endregion

        #region Lifecycles
        
        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser = null)
        {
            m_onConnectionVerified = onConnectionVerified;
            m_expectedPlayerCount = expectedPlayerCount;
            onGameBeginning = onGameBegin;
            m_onGameEnd = onGameEnd;
            m_canSpawnInGameObjects = null;
            
            // only the players will spawn user data.
            // Server will only react to commands.
            if (m_localUserData == null) return;
            m_localUserData = new PlayerData(localUser.DisplayName.Value, 0);
        }

        public override void OnNetworkSpawn()
        {
            InitializeNetworkedBackground();

            if (IsServer)
            {
                // the host will update the generated image
                // check to make sure the string has been set
                if (GameManager.Instance.LocalLobby.LocalLobbyLocationValue.Value != null)
                {
                    // make a request to make a new image.
                    bgGenerator.GenerateBackground(image =>
                    {
                        Debug.LogError("Generated background - sending bytes to clients");
                        // networkedTextureBytes.Value = image;

                    }, error =>
                    {
                        Debug.LogError($"Error generating background: {error}");
                    });
                    
                }else
                {
                    // Debug.LogError("Using default background");
                }
                
                FinishInitialize();
            }else
            {
                m_localUserData = new PlayerData(m_localUserData.name, NetworkManager.Singleton.LocalClientId);
                VerifyConnection_ServerRpc(m_localUserData.id);
            }
        }
        
        public override void OnNetworkDespawn()
        {

            // Sammoh - TODO make sure this is called when the game ends.
           // m_onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
        }

        private void FinishInitialize()
        {
            Debug.Log("Finished Initializing");
            Instance.onGameBeginning += OnGameBegan;
        }
        
        #endregion
        
        #region Verify Connection
        
        /// <summary>
        /// To verify the connection, invoke a server RPC call that then invokes a client RPC call. After this, the actual setup occurs.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnection_ServerRpc(ulong clientId)
        {
            VerifyConnection_ClientRpc(clientId);
            // While we could start pooling symbol objects now, incoming clients would be flooded with the Spawn calls.
            // This could lead to dropped packets such that the InGameRunner's Spawn call fails to occur, so we'll wait until all players join.
            // (Besides, we will need to display instructions, which has downtime during which symbol objects can be spawned.)
        }

        [ClientRpc]
        private void VerifyConnection_ClientRpc(ulong clientId)
        {
            if (clientId == m_localUserData.id)
                VerifyConnectionConfirm_ServerRpc(m_localUserData);
        }

        /// <summary>
        /// Once the connection is confirmed, spawn a player cursor and check if all players have connected.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnectionConfirm_ServerRpc(PlayerData clientData)
        {
            // Note that the client will not receive the cursor object reference, so the cursor must handle initializing itself.
            // var playerCursor = Instantiate(m_playerCursorPrefab);
            // playerCursor.NetworkObject.SpawnWithOwnership(clientData.id);
            // playerCursor.name += clientData.name;
            
            // spawn the input object for minigames.
            // Note that the client will not receive the cursor object reference, so the cursor must handle initializing itself.
            var playerInput = Instantiate(m_minigameInputPrefab);
            playerInput.NetworkObject.SpawnWithOwnership(clientData.id);
            playerInput.name += clientData.name;

            MinigameManager.Instance.AddInteractionListener(clientData.id, playerInput);

            m_dataStore.AddPlayer(clientData.id, clientData.name, out var playerData);
            playerList.Add(clientData.id, playerData);
            
            // Debug.LogError($"Added player: {clientData.name}");
            
            PlayerCount.Value++;

            // The game will begin at this point, or else there's a timeout for booting any unconnected players.
            bool areAllPlayersConnected = NetworkManager.Singleton.ConnectedClients.Count >= m_expectedPlayerCount;
            VerifyConnectionConfirm_ClientRpc(clientData.id, areAllPlayersConnected);
        }

        [ClientRpc]
        private void VerifyConnectionConfirm_ClientRpc(ulong clientId, bool canBeginGame)
        {
            if (clientId == m_localUserData.id)
            {
                m_onConnectionVerified?.Invoke();
                m_hasConnected = true;
            }

            if (canBeginGame && m_hasConnected)
            {
                m_timeout = -1;
                BeginGame();
            }
        }

        #endregion

        #region Game States

        /// <summary>
        /// The game will begin either when all players have connected successfully or after a timeout.
        /// </summary>
        void BeginGame()
        {
            StartCoroutine(WaitForAllPlayers());
        }

        IEnumerator WaitForAllPlayers()
        {
            // Sammoh TODO this is a hack. Validate all player info has been received.
            // check the validation. 
            yield return new WaitForSeconds(5);
            
            m_generatedImage.color = Color.white;
            m_canSpawnInGameObjects = true;
            GameManager.Instance.BeginGame();
            onGameBeginning?.Invoke();
        }
        
        private void OnGameBegan()
        {
            Debug.Log("Starting game");

            // if (IsHost)
            // {
            //     // tell all the players to start a loading screen.
            //     // send an api request to the server to make a prompt.
            //     // when the prompt is received, send another api request to get the image.
            //     // when the image is received, send a message to all the players to stop the loading screen.
            // }
            
            if (IsServer)
                InitializeData(() =>
                {
                    StartRound_ServerRpc();
                });
            
            Instance.onGameBeginning -= OnGameBegan;
        }

        void InitializeData(Action onComplete)
        {
            // m_getOtherPlayers.AddListener(OnClientReceived);
            // NetworkedDataStore.Instance.GetAllPlayerData(m_getOtherPlayers);
            //     
            if (useAi)
            {
                AiManager.Instance.Initialize();
                    
                // this seems redundant now. 
                m_retrieveAi.AddListener(OnAiReceived);
                AiManager.Instance.GetAllPlayerData(m_retrieveAi);
            }

            m_cardSelector.InitializeDeck(() =>
            {

                if (Debug.isDebugBuild)
                {
                    // looking for a cards that shouldn't be there. 
                    m_cardSelector.InitDebugDrawPile();

                }
                
                onComplete?.Invoke();
            });
            
        }

        private void SetIntroRoleText()
        {
            NewCardSelector.Instance.GetPlayerRole(m_localUserData.id, role =>
            {
                var msg = "";
                switch (role.role)
                {
                    case Role.Innocent:
                        msg += "An ";
                        break;
                    default:
                        msg += "The ";
                        break;
                        
                }
                
                var roleMessage = $"{msg} {role.role.ToString()}";
                introRoleText.text = roleMessage;
            });
        }

        #region Finish Round
        // Sammoh todo this needs to be timed. 
        // this is a network race condition. 
        private void FinishRound()
        {
            if (IsServer)
                StartCoroutine(FinishRound_ClientsFirst());
        }
        
        private IEnumerator FinishRound_ClientsFirst()
        {
            FinishRound_ClientRpc();
            yield return null;
            SendLocalFinishRoundSignal();
        }
        
        [ClientRpc]
        private void FinishRound_ClientRpc()
        {
            if (IsServer)
                return;
            SendLocalFinishRoundSignal();
            
            // m_introOutroRunner.DoOutro(() =>
            // {
            //     Debug.LogError("Game ended");
            //     m_onGameEnd?.Invoke();
            // });
        }
        
        // private void SendLocalFinishRoundSignal()
        // {
        //     onRoundEnding?.Invoke();
        // }

        

        #endregion
        
        private void SendLocalFinishRoundSignal()
        {
            onRoundEnding?.Invoke();

            if (IsServer)
            {
                if (m_timer.IsRunning)
                    m_timer.StopTimer();

                // Find out of the solution is correct.
                // var isCorrect = m_cardSelector.CheckSolution(out var results);
                var results = m_cardSelector.CheckSolution();
                
                // The roles ay have the ability to do something after the round ends.
                // There may be a role that effects the outcome of the round... (only once per game)

                // Debug.LogError("get the roles and sort them."); 
                var roleDataList = m_cardSelector.RoleDataList.Values.ToList();
                    
                // sort the list by the role
                // roleDataList.Sort((x, y) => x.role.CompareTo(y.role));
                
                // process the roles.
                foreach (var roleData in roleDataList)
                {
                    if (roleData.role == Role.Innocent) continue;

                    // Debug.LogError("Processing role: " + roleData.role.ToString());
                    IntentProcessing.Instance.ProcessPostRound(roleData);
                }
                
                // player cards have been used, so we need to update the player cards.
                m_cardSelector.UpdatePlayerCards();
                
                // Sammoh if the timer is running then suggestions have been made.
                // if timer not running then round has ended before all suggestions were made.
                if (m_timer.IsRunning)
                    m_timer.StopTimer();
                
                var killerRole = roleDataList.Find(x => x.role == Role.Killer);
                var medicRole = roleDataList.Find(x => x.role == Role.Medic);

                var skipElimination = false;
                var selectedPlayer = "";

                // Determine if killer made move. 
                if (killerRole.selectedCard > -1)
                {
                    // check to see if the killer and medic chose the same card.
                    if (medicRole.selectedCard == killerRole.selectedCard)
                    {
                        // the medic saved the killer.
                        var validate = medicRole.selectedCard > -1;
                        Debug.LogError($"The medic saved the killer: {validate}");
                        skipElimination = validate;
                    }
                    
                    Debug.LogError("The killer made a move.");
                    
                    // var killerRole = m_cardSelector.RoleDataList.FirstOrDefault(r => r.Value.role == Role.Killer);
                    var selectedCardIndex = killerRole.selectedCard;
                    var selectedPlayerId = m_cardSelector.PlayerCharacters.FirstOrDefault(c => c.Value.CardId == selectedCardIndex).Key;
                    selectedPlayer = playerList[selectedPlayerId].name;
                }
                else
                {
                    Debug.LogError("The killer did not make a move.");
                    skipElimination = true;
                }

                if (results == null)
                {
                    Debug.LogError("The solution was not determined");
                    EndRound_ClientRpc(null, true, skipElimination, selectedPlayer, "No winner");
                    return;
                }

                // find out if the game is over.
                if (results.IsCorrect)
                {
                    Debug.LogError("Do End Game Sequence");
                    EndRound_ClientRpc(results, false, true, selectedPlayer, selectedPlayer);
                }
                else
                {
                    Debug.LogError("Do End Round Sequence");                   
                    EndRound_ClientRpc(results, false, skipElimination, selectedPlayer, "No winner");
                }
            }
        }

        /*
        private void HandleElimination()
        {
            if (VictimSelected())
            {
                SendEliminationEvent();
            }
            else
            {
                Console.WriteLine("Everyone is safe.");
            }
            
        }

        private void HandleVotes()
        {
            if (HasVotes())
            {
                TriggerVoteEvent();
            }
            else
            {
                Console.WriteLine("There were no votes.");
            }
        }

        private void HandleSolutions()
        {
            if (SolutionSolved())
                return;

            if (Player1MadeSolution())
            {
                var groupSolutions = Player2Group.Where(p => p.HasMadeSolution).ToList();
        
                if (groupSolutions.Any())
                {
                    foreach (var player in groupSolutions)
                    {
                        DetermineCorrectSolution(Player1Solution, player.Solution);
                    }
                }
                else
                {
                    CheckSolution(Player1Solution);
                }
            }
        }
        
        private void DetermineCorrectSolution(Solution player1Solution, Solution player2Solution)
        {
            if (IsCorrect(player1Solution))
            {
                // Player 1 has the correct solution
            }
            else if (IsCorrect(player2Solution))
            {
                // One of the players in Player 2's group has the correct solution
            }
            else
            {
                // Neither Player 1 nor the current player in Player 2's group has the correct solution
            }
        }
        */

        // Starts the round for all players and spawns the player icons. 
        // this may just start the game
        [ClientRpc]
        private void StartRound_ClientRpc()
        {
            SetIntroRoleText();
            
            m_introOutroRunner.DoIntro(() =>
            {
                if (IsServer)
                    m_timer.StartTimer(FinishRound);
                
                m_generatedImageCanvasGroup.alpha = 1;
                // Debug.LogError($"StartRound for {m_localUserData.name}");
                onRoundBeginning?.Invoke();
            });
        }

        [ServerRpc(RequireOwnership = false)]
        void StartRound_ServerRpc()
        {
            StartRound_ClientRpc();
        }
        
        [ClientRpc]
        void RestartRound_ClientRpc()
        {
            Debug.LogError($"RestartRound for {m_localUserData.name}");
            m_introOutroRunner.DoNewRound(() =>
            {
                if (IsServer)
                    m_timer.StartTimer(FinishRound);
                
                m_generatedImageCanvasGroup.alpha = 1;
                onRoundRestart?.Invoke();
            });
        }
        
        [ServerRpc(RequireOwnership = false)]
        void RestartRound_ServerRpc()
        {
            // reset the game mechanics. 
            // give new cards to the players.
            // reset the timer.
            // reset the suggestions.
            // reset the player's ui.
            
                
            m_cardSelector.ClearSuggestions();
            
            RestartRound_ClientRpc();
        }
        
        [SerializeField] private TMP_Text resultsText;
        [SerializeField] private TMP_Text playerEliminatedText;
        [SerializeField] private TMP_Text winningPlayerText;
        
        [ClientRpc]
        private void EndRound_ClientRpc(SuggestionResults results, bool skipSuggestion, bool skipElimination, string selectedPlayer = "", string winningPlayer = "")
        {
            Debug.Log($"EndRound for {m_localUserData.name}");
            
            // Get  the votes from the server.
            
            // turn off the player's ui

            if (skipSuggestion)
            {
                Debug.LogError("No one made a suggestion, skipping suggestion results.");
                
                if (skipElimination)
                {
                    Debug.LogError("Reveal no suggestions determined");
                    m_introOutroRunner.DoNoSuggestions(() =>
                    {
                        Debug.LogError("Do No Elimination");
                        m_introOutroRunner.DoNoElimination(() =>
                        {
                            if (IsServer)
                            {
                                Debug.LogError("Starting new round.");
                                RestartRound_ServerRpc();
                            }
                        });
                    });
                }
                else
                {
                    playerEliminatedText.text = selectedPlayer;
                    Debug.LogError("Reveal no suggestions determined");

                    m_introOutroRunner.DoNoSuggestions(() =>
                    {
                        Debug.LogError($"Eliminating {selectedPlayer}");
                        playerEliminatedText.text = selectedPlayer;
                        
                        m_introOutroRunner.DoPlayerEliminated(() =>
                        {
                            Debug.LogError("Move to the final screen.");
                        });
                    });
                }

                return;
            }
            
            // print the results. 
            var resultsString = $"{results.CharacterName} : {results.Count[0]}\n {results.MotiveName} : {results.Count[1]}\n {results.WeaponName} : {results.Count[2]}";
            resultsText.text = resultsString;
            
            // show the results.
            if (results.IsCorrect)
            {
                if (results.WasGroup)
                {
                    // the group made the correct suggestion.
                    Debug.LogError("The group made the correct suggestion.");

                    
                    
                    m_introOutroRunner.DoGroupCorrect(() =>
                    {
                        Debug.LogError("Move to the final screen.");
                    });
                    return;
                }

                // not the group, therefore a player made the correct suggestion.
                Debug.LogError("A player made the correct suggestion.");
                
                // determine if the player is a killer or not.
                // get the player name. 
                GetPlayerData((data) =>
                {
                    if (data.id != results.WinnerId) return;

                    var role = "";
                    role += results.IsKiller ? "Killer" : "Innocent";

                    winningPlayerText.text = $"{data.name} made the correct suggestion\n{role}";
                    
                    // Debug.LogError($"{data.name} made the correct suggestion\n{role}");
                    
                    // the player made the correct suggestion.
                    m_introOutroRunner.DoPlayerCorrect(() =>
                    {
                        Debug.LogError("Move to the final screen.");
                    });
                });
                
            }
            else
            {
                Debug.LogError("The group made an  incorrect suggestion.");
                

                
                m_introOutroRunner.DoMakeSuggestions(() =>
                {
                    if (skipElimination)
                    {
                        Debug.LogError("Skipping elimination.");
                        m_introOutroRunner.DoNoElimination(() =>
                        {
                            if (IsServer)
                            {
                                Debug.LogError("Starting new round.");
                                RestartRound_ServerRpc();
                            }
                        });
                        return;
                    }
                    
                    playerEliminatedText.text = selectedPlayer;

                    Debug.LogError("Move to elimination screen.");
                    m_introOutroRunner.DoPlayerEliminated(() =>
                    {
                        if (IsServer)
                        {
                            Debug.LogError("Starting new round.");
                            RestartRound_ServerRpc();
                        }
                    });
                });
            }
        }

        #endregion
        
        void InitializeNetworkedBackground()
        {
            m_generatedImageCanvasGroup = m_generatedImage.GetComponent<CanvasGroup>();
            m_generatedImage.color = Color.black;
            
            // Sammoh todo this should recieve the updated bg image. 
            // networkedTextureBytes.OnValueChanged += (prevValue, textureBytes) =>
            // {
            //     Texture2D receivedTexture = new Texture2D(512, 512);
            //     receivedTexture.LoadImage(textureBytes);
            //     
            //     // var image = receivedTexture;
            //     var sprite = Sprite.Create(receivedTexture, new Rect(0, 0, receivedTexture.width, receivedTexture.height), Vector2.zero);
            //     m_generatedImage.sprite = sprite;
            //     m_generatedImage.color = Color.white;
            // };
            
            // m_networkedTexture2D.networkedTextureBytes.OnValueChanged += (prevValue, textureBytes) =>
            // {
            //     if (textureBytes != null)
            //     {
            //         Texture2D receivedTexture = new Texture2D(2, 2);
            //         receivedTexture.LoadImage(textureBytes);
            //         
            //         var sprite = Sprite.Create(receivedTexture, new Rect(0, 0, receivedTexture.width, receivedTexture.height), Vector2.zero);
            //         m_generatedImage.sprite = sprite;
            //         m_generatedImage.color = Color.white;
            //     }
            // };
        }
        
        public void Update()
        {
            CheckIfCanSpawnNewSymbol();
            if (m_timeout >= 0)
            {
                m_timeout -= Time.deltaTime;
                if (m_timeout < 0)
                    BeginGame();
            }

            void CheckIfCanSpawnNewSymbol()
            {
                if (!m_canSpawnInGameObjects.GetValueOrDefault() ||
                    m_remainingSymbolCount >= SequenceSelector.symbolCount || !IsServer)
                    return;
                if (m_pendingSymbolPositions.Count > 0)
                {
                    m_symbolSpawnTimer -= Time.deltaTime;
                    if (m_symbolSpawnTimer < 0)
                    {
                        m_symbolSpawnTimer = 0.02f; // Space out the object spawning a little to prevent a lag spike.
                        SpawnNewSymbol();
                        if (m_remainingSymbolCount >= SequenceSelector.symbolCount)
                            m_canSpawnInGameObjects = false;
                    }
                }
            }

            void SpawnNewSymbol()
            {
                int index = SequenceSelector.symbolCount - m_pendingSymbolPositions.Count;
                Vector3 pendingPos = m_pendingSymbolPositions.Dequeue();
                SymbolObject symbolObj = Instantiate(m_symbolObjectPrefab);
                symbolObj.NetworkObject.Spawn();
                symbolObj.name = "Symbol" + index;
                symbolObj.SetParentAndPosition_Server(m_symbolContainerInstance.NetworkObject, pendingPos);
                // symbolObj.SetSymbolIndex_Server(m_sequenceSelector.GetNextSymbol(index));
                m_remainingSymbolCount++;
            }
        }

        // /// <summary>
        // /// Called while on the host to determine if incoming input is a legal action.
        // /// If the player is a killer or
        // /// <param name="playerId">
        // /// The sender of the method..
        // /// </param>
        // /// <param name="cardIndex">
        // /// The card being used to validate with the server.
        // /// </param>
        // /// <param name="intent">
        // /// A set of parameters with information that the selected card uses.
        // /// @note this is not always the same as the intent of the card.
        // /// @note this can be overloaded with Abilities or reactions.
        // /// </param>
        // /// /// </summary>
        private Action<BaseCard> OnInputComplete;
        private Action OnInputCanceled;
        
        public void OnPlayerInput(ulong playerId, BaseCard card, Action<BaseCard> onComplete, Action onCancel)
        {
            OnInputComplete = onComplete;
            OnInputCanceled = onCancel;
            OnPlayerInput_ServerRpc(playerId, card);
        }

        
        
        [ServerRpc(RequireOwnership = false)]
        private void OnPlayerInput_ServerRpc(ulong playerId, BaseCard card)
        {
            Debug.LogError("Fix this");
            // if (!m_cardSelector.ConfirmCardCorrect(playerId, card)) return;
            
            IntentProcessing.Instance.ProcessCardIntent(playerId, card, (playerId, usedCard) =>
            {
                Debug.LogError("Player input complete, discarding card.");
                // tell the card selector which cards have been used for this player. 
                m_cardSelector.DiscardCard(playerId, usedCard);
                
                PlayerInputComplete_ClientRpc(playerId, usedCard);
            }, PlayerInputCanceled_ClientRpc);
        }
        
        [ClientRpc]
        private void PlayerInputComplete_ClientRpc(ulong playerId, BaseCard card)
        {
            if (playerId != m_localUserData.id) return;
            
            
            OnInputComplete?.Invoke(card);
            Debug.LogError("Player input complete");

        }
        
        [ClientRpc]
        private void PlayerInputCanceled_ClientRpc(ulong playerId)
        {
            if (playerId != m_localUserData.id) return;
            
            OnInputCanceled?.Invoke();
            Debug.LogError("Player input canceled");
        }
        
        private Action<BaseCard[]> OnSuggestionComplete;
        private Action OnCanceled;
        public void SuggestPlayer(ulong id, Action<BaseCard[]> onComplete, Action onCancel = null)
        {
            OnSuggestionComplete = onComplete;
            OnCanceled = onCancel;
            SuggestPlayer_ClientRpc(id);
        }

        [ClientRpc]
        private void SuggestPlayer_ClientRpc(ulong id)
        {
            
            if (id != m_localUserData.id) return;
            
            PopulateContent.Instance.
            HandleSuggestion(cards =>
            {
                // var msg = cards.Aggregate("Suggestion made: ", (current, card) => current + $"{card.Name}, ");
                // Debug.LogError(msg);
                SendSuggestion_ServerRpc(id, cards);
            }, () =>
            {
                CancelSuggestion_ServerRpc(id);
                // Debug.LogError("Suggestion canceled");
            });
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendSuggestion_ServerRpc(ulong id, BaseCard[] suggestion = null)
        {
            OnSuggestionComplete?.Invoke(suggestion);
            OnSuggestionComplete = null;
            OnCanceled = null;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CancelSuggestion_ServerRpc(ulong id)
        {
            OnCanceled?.Invoke();
            OnCanceled = null;
            OnSuggestionComplete = null;
        }
        
        
        [ClientRpc]
        public void HandleNetworkIntent_ClientRpc(ulong caller, CardIntent intent)
        {
            // get the player panel
            var playerPanel = NetworkManager.Singleton.ConnectedClients[caller].PlayerObject.GetComponent<PlayerInputPanel>();
            
            // playerPanel.EnablePopupPanel_ClientRpc(caller, intent);
        }

        private Dictionary<ulong, bool> m_usedAbilities = new Dictionary<ulong, bool>();

        Action OnAbilityUsed;
        Action OnAbilityCanceled;
        // Sammoh todo: Make sure ability only happens once per round.
        
        public void UseAbility(ulong playerId, Action onComplete, Action onCancel = null)
        {
            // sammoh todo: turn off the ability button.
            
            OnAbilityUsed += onComplete;
            OnAbilityCanceled += onCancel;
            OnHandleAbility_ServerRpc(playerId);
        }
        
        [ClientRpc]
        void AbilityCallback_ClientRpc()
        {
            OnAbilityUsed?.Invoke();
            OnAbilityUsed = null;
            OnAbilityCanceled = null;
        }
        
        [ClientRpc]
        void AbilityCanceled_ClientRpc()
        {
            OnAbilityCanceled?.Invoke();
            OnAbilityUsed = null;
            OnAbilityCanceled = null;
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnHandleAbility_ServerRpc(ulong caller)
        {
            // can use ability. 
            // add the ability to a list and wait for all of the abilities to be used.
            // otherwise the timer will end the round. 
            
            // all players, when the ability is used will wnd the rouhnd
            // this will allow the innocent people to win.            
            // Sammoh Todo: create an instance of the user ro validate
            if (m_usedAbilities.ContainsKey(caller))
            {
                Debug.LogError("Ability already used");
                return;
            };

            // Debug.LogError("Starting Ability");
            // add the ability to a list and wait for all of the abilities to be used.
            // otherwise the timer will end the round. 
            
            
            // wait until the round ends before using the ability.
            IntentProcessing.Instance.ProcessAbility(caller, () =>
            {
                m_usedAbilities.Add(caller, true);
                AbilityCallback_ClientRpc();
            }, AbilityCanceled_ClientRpc);
        }
        
        #region Notes

        // only one card can be used at a time.
        // if it's a weapon, motive, or character card, then the player can share the card with another player.
                
        // tell the client how to resolve the card.
        // eg. if it's a weapon card, then the player can accuse.
        // eg. swap the card with a new card from the draw pile (all players can mark off the card)
        // eg. select a player to swap cards with.
        // playerCard.ResolveCard(player);

        #endregion
        
        // private Dictionary<ulong, BaseCard[]> m_usedSuggestion = new Dictionary<ulong, BaseCard[]>();

        
        // Sammoh: Todo this needs to be moved to the server.
        public void PlayerInput_SuggestPlayer(ulong caller, BaseCard[] cards)
        {
            // Sammoh TODO: make suggestions are added to a pool to end the round.
            // Sammoh Todo: create an instance of the user to validate that they can use a suggestion.
            
            // if the player has already used a suggestion, then return.
            if (m_cardSelector.AddSuggestion(caller, cards)) return;
            
            // var msg = $"Sammoh: Suggestion: {cards[0].Name}, {cards[1].Name}, {cards[2].Name}";
            // Debug.LogError(msg);
            
            // tell all the players that a suggestion has been made.
            foreach (var player in playerList)
            {
                if (player.Value.id == caller) return;
                
                // the the server to let all users know that the suggestion has been made by the user. 
                var playerObject = NetworkManager.Singleton.ConnectedClients[player.Value.id].PlayerObject
                    .GetComponent<PlayerInputPanel>();
                playerObject.UpdatePlayerStatus(caller, IntentType.MakeSuggestion);
            }
            
            // all the other players will end the round for themselves.
            // stop the timer.
            // play an animation. 
            // show the results from the server.

            if (m_cardSelector.SuggestionCount != playerList.Count)
            {
                Debug.LogError("Waiting for all players to make a suggestion");
                return;
            };
            
            FinishRound();
            
            
            // end the round for all the players.
            

            // The server will handle the suggestion.
            
            // for each of the suggestion entry, figure out the results. 
            // the most common card in each position will be added to a final suggestion.
            // if there is a tie, then on card will be chosen at random.
            // all players will see the new results. 
            // if the suggestion is correct, then the game will end.
            // if the suggestion is incorrect, then the game will continue.
            
            
            // find out if any of the suggestions are correct.
            // if they are, then end the game.
            // if they are not, then continue the game.
            // foreach (var suggestion in m_usedSuggestion)
            // {
            //     var cardNames = "";
            //     var isCorrect = m_cardSelector.ConfirmSuggestion(suggestion.Value);
            //     var answer = isCorrect ? "Correct!" : "Incorrect!";
            //
            //     var playerName = playerList[caller].name;
            //
            //     foreach (var index in suggestion.Value)
            //     {
            //         var cardName = m_cardSelector.GetCard<BaseCard>(index);
            //         cardNames += cardName.Name + " ";
            //     }
            //
            //     // only send it to the players that are in connected
            //     foreach (var player in playerList)
            //     {
            //         if (player.Value is PlayerData)
            //         {
            //             var msg = $"{playerName} was {answer}, they guessed {cardNames}";
            //             // get the local player object
            //             var playerInputPanel = NetworkManager.Singleton.ConnectedClients[player.Key].PlayerObject.GetComponent<PlayerInputPanel>();
            //             playerInputPanel.PrintMessage(msg, false, player.Key);
            //         }
            //     }
            // }
            

        }

        /// <summary>
        /// The server determines when the game should end. Once it does, it needs to inform the clients to clean up their networked objects first,
        /// since disconnecting before that happens will prevent them from doing so (since they can't receive despawn events from the disconnected server).
        /// </summary>
        [ClientRpc]
        private void WaitForEndingSequence_ClientRpc()
        {
            m_scorer.OnGameEnd();
            m_introOutroRunner.DoOutro(EndGame);
        }

        #region EndGame
        
        private void EndGame()
        {
            if (IsServer)
                StartCoroutine(EndGame_ClientsFirst());
        }

        private IEnumerator EndGame_ClientsFirst()
        {
            EndGame_ClientRpc();
            yield return null;
            SendLocalEndGameSignal();
        }

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if (IsServer)
                return;
            SendLocalEndGameSignal();
            
            // m_introOutroRunner.DoOutro(() =>
            // {
            //     Debug.LogError("Game ended");
            //     m_onGameEnd?.Invoke();
            // });
        }

        private void SendLocalEndGameSignal()
        {
            m_onGameEnd();
        }
        
        #endregion

        #region Player Management

        private UnityEvent<AiPlayerData> m_retrieveAi = new ();
        private Dictionary<ulong, IPlayerData> playerList = new ();
        public Dictionary<ulong, IPlayerData> PlayerList => playerList;

        // Action<IPlayerData, BaseCard> m_onEachPlayerCallback;
        UnityEvent<IPlayerData> m_populatePlayerCallback;
        Action<IPlayerData> m_onGetCurrentCallback;
        
        // Sammoh this is going to be player specific callback.
        Dictionary<ulong, Action<IPlayerData, BaseCard>> m_onEachPlayerCallbacks = new ();

        // Sammoh TODO looking into the queue. 
        private Queue<Func<IPlayerData, BaseCard>> jobQueue = new ();
        private List<Action<IPlayerData, BaseCard>> subscribers = new ();
        
        private void OnAiReceived(AiPlayerData data)
        {
            if (PlayerCount.Value == AiCount + NetworkManager.Singleton.ConnectedClients.Count)
                return;

            PlayerCount.Value++;
            playerList.Add(data.id, data);
        }

        public void PopulatePlayerCharacters(UnityEvent<IPlayerData> onEachPlayer)
        {
            m_populatePlayerCallback = onEachPlayer;
            PopulatePlayerData_ServerRpc(m_localUserData.id);
        }
        
        [ServerRpc(RequireOwnership = false)]
        void PopulatePlayerData_ServerRpc(ulong callerId)
        {
            var sortedData = playerList.Select(kvp => kvp.Value).OrderByDescending(data => data.id);
            var sortedDataArray = sortedData.ToArray();
            
            PopulatePlayerData_ClientRpc(callerId, sortedDataArray.ToArray());
        }
        
        [ClientRpc]
        void PopulatePlayerData_ClientRpc(ulong callerId, IPlayerData[] sortedData)
        {
            if (callerId != m_localUserData.id)
                return;

            var rank = 1;
            foreach (var data in sortedData)
            {
                m_populatePlayerCallback?.Invoke(data);
                rank++;
            }
            m_populatePlayerCallback = null;
        }
        
        // public void GetAllPlayerData(Action<IPlayerData, BaseCard> onEachPlayer)
        // {
        //     // m_onEachPlayerCallback = onEachPlayer;
        //     m_onEachPlayerCallbacks.TryAdd(m_localUserData.id, onEachPlayer);
        //     GetAllPlayerData_ServerRpc(m_localUserData.id);
        // }
        
        [ServerRpc(RequireOwnership = false)]
        void GetAllPlayerData_ServerRpc(ulong callerId, bool returnCaller = false)
        {
            // Sammoh todo this is a hack to get the player data to the client.
            // loop through the player list and send the data to the client.

            var players = new List<IPlayerData>();
            var characters = new List<BaseCard>();

            foreach (var player in PlayerList.Values)
            {
                players.Add(player);
                var playerCharacter = NewCardSelector.Instance.PlayerCharacters[player.id];
                characters.Add(playerCharacter);
            }
            
            // // list all of the data and send it to the client.
            // for (var i = 0; i < playerData.Length; i++)
            // {
            //     var player = playerData[i];
            //     var card = playerCharacterCard[i];
            //     Debug.LogError($"Sending {player.name} with {card.Name}");
            // }

            GetAllPlayerData_ClientRpc(callerId, players.ToArray(), characters.ToArray());
            
            // clear the lists
            players.Clear();
            characters.Clear();
        }

        [ClientRpc]
        void GetAllPlayerData_ClientRpc(ulong callerId, IPlayerData[] playerData, BaseCard[] characterCard)
        {
            if (callerId != m_localUserData.id)
                return;

            for (var i = 0; i < playerData.Length; i++)
            {
                // m_onEachPlayerCallback?.Invoke(playerData[i], characterCard[i]);
                m_onEachPlayerCallbacks[callerId]?.Invoke(playerData[i], characterCard[i]);
                // remove the callback from the list.
                // m_onEachPlayerCallbacks.Remove(callerId);
            }

            // m_onEachPlayerCallback = null;
            // m_onEachPlayerCallbacks.Remove(callerId);
        }
        

        public void GetPlayerData(Action<IPlayerData> onGet)
        {
            m_onGetCurrentCallback = onGet;
            GetPlayerData_ServerRpc(m_localUserData.id);
        }
        
        [ServerRpc(RequireOwnership = false)]
        void GetPlayerData_ServerRpc(ulong id)
        {
            if (playerList.TryGetValue(id, out var playerData))
            {
                Debug.LogError($"Getting {playerData.name}");

                GetPlayerData_ClientRpc(id, playerData);
            }
            else
                GetPlayerData_ClientRpc(id, new PlayerData(null, 0));
        
        }
        [ClientRpc]
        private void GetPlayerData_ClientRpc(ulong callerId, IPlayerData data)
        {
            if (callerId != m_localUserData.id) return;
            m_onGetCurrentCallback?.Invoke(data);
            m_onGetCurrentCallback = null;
        }
        

        [ServerRpc(RequireOwnership = false)]
        public void PlayerSelected_ServerRpc(IPlayerData playerData)
        {
            Debug.LogError($"player selected {playerData.name}");

        }
        
        #endregion
        

        private Action<ulong, CardIntent> _onPlayerReaction;
        private Dictionary<ulong, CardIntent> _reactionQueue = new Dictionary<ulong, CardIntent>();
        
    }

}