using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LobbyRelaySample.lobby;
using LobbyRelaySample.ngo;
using Matchplay.Client;
using Matchplay.Networking;
using Matchplay.Shared;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
#if UNITY_EDITOR
using ParrelSync;
#endif

namespace LobbyRelaySample
{
    /// <summary>
    /// Current state of the local game.
    /// Set as a flag to allow for the Inspector to select multiple valid states for various UI features.
    /// </summary>
    [Flags]
    public enum GameState
    {
        Menu = 1,
        Lobby = 2,
        JoinMenu = 4,
    }

    /// <summary>
    /// Sets up and runs the entire sample.
    /// All the Data that is important gets updated in here, the GameManager in the mainScene has all the references
    /// needed to run the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public LocalLobby LocalLobby => m_LocalLobby;
        public Action<GameState> onGameStateChanged;
        public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

        public GameState LocalGameState { get; private set; }
        public LobbyManager LobbyManager { get; private set; }
        public MatchplayMatchmaker MatchmakingManager { get; private set; }
        [SerializeField]
        SetupInGame m_setupInGame;
        [SerializeField]
        Countdown m_countdown;

        LocalPlayer m_LocalUser;
        LocalLobby m_LocalLobby;

        LobbyColor m_lobbyColorFilter;

        static GameManager m_GameManagerInstance;

        public static GameManager Instance
        {
            get
            {
                if (m_GameManagerInstance != null)
                    return m_GameManagerInstance;
                m_GameManagerInstance = FindObjectOfType<GameManager>();
                return m_GameManagerInstance;
            }
        }

        public LocalPlayer LocalUser => m_LocalUser;

        /// <summary>Rather than a setter, this is usable in-editor. It won't accept an enum, however.</summary>
        public void SetLobbyColorFilter(int color)
        {
            m_lobbyColorFilter = (LobbyColor)color;
        }

        public async Task<LocalPlayer> AwaitLocalUserInitialization()
        {
            while (m_LocalUser == null)
                await Task.Delay(100);
            return m_LocalUser;
        }
        
        // bool m_LocalLaunchMode;
        // string m_LocalIP;
        // string m_LocalPort;
        // string m_LocalName;
        
        public async void CreateLobby(string name, bool isPrivate)
        {
            
            try
            {
                var lobby = await LobbyManager.CreateLobbyAsync(
                    name,
                    CardGame.GameSettings.MaxPlayers,
                    isPrivate, m_LocalUser);

                LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);
                await CreateLobby();
            }
            catch (Exception exception)
            {
                SetGameState(GameState.JoinMenu);
                Debug.LogError($"Error creating lobby : {exception} ");
            }
        }

        public async void JoinLobby(string lobbyID, string lobbyCode)
        {
            try
            {
                var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode,
                    m_LocalUser);

                LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);
                await JoinLobby();
            }
            catch (Exception exception)
            {
                SetGameState(GameState.JoinMenu);
                Debug.LogError($"Error joining lobby : {exception} ");
            }
        }

        public async void QueryLobbies()
        {
            LobbyList.QueryState.Value = LobbyQueryState.Fetching;
            var qr = await LobbyManager.RetrieveLobbyListAsync(m_lobbyColorFilter);
            if (qr == null)
            {
                return;
            }

            SetCurrentLobbies(LobbyConverters.QueryToLocalList(qr));
        }

        public async void QuickJoin()
        {
            var lobby = await LobbyManager.QuickJoinLobbyAsync(m_LocalUser, m_lobbyColorFilter);
            if (lobby != null)
            {
                LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);
                await JoinLobby();
            }
            else
            {
                SetGameState(GameState.JoinMenu);
            }
        }

        public void SetLocalUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LogHandlerSettings.Instance.SpawnErrorPopup(
                    "Empty Name not allowed."); // Lobby error type, then HTTP error type.
                return;
            }

            m_LocalUser.DisplayName.Value = name;
            SendLocalUserData();
        }

        public void SetLocalUserEmote(EmoteType emote)
        {
            m_LocalUser.Emote.Value = emote;
            SendLocalUserData();
        }

        public void SetLocalUserStatus(PlayerStatus status)
        {
            m_LocalUser.UserStatus.Value = status;
            SendLocalUserData();
        }

        public void SetLocalLobbyColor(LobbyColor color)
        {
            if (m_LocalLobby.PlayerCount < 1)
                return;
            m_LocalLobby.LocalLobbyColor.Value = color;
            SendLocalLobbyData();
        }

        public void SetLocalLobbyTimer(TimeCounterEnum time)
        {
            if (m_LocalLobby.PlayerCount < 1)
                return;
            m_LocalLobby.LocalLobbyTimer.Value = (int)time;
            SendLocalLobbyData();
        }
        
        public void SetLocalLobbyAiCount(int count)
        {
            if (m_LocalLobby.PlayerCount < 1)
                return;
            m_LocalLobby.LocalLobbyAiCount.Value = count;
            SendLocalLobbyData();
        }
        
        public void SetLocalLobbyHostRole(int role)
        {
            m_LocalLobby.LocalLobbyHostRole.Value = role;
            SendLocalLobbyData();
        }
        
        public void SetLocalLobbyLocation(string location)
        {
            m_LocalLobby.LocalLobbyLocationValue.Value = location;
            SendLocalLobbyData();
        }

        bool updatingLobby;

        async void SendLocalLobbyData()
        {
            await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(m_LocalLobby));
        }

        async void SendLocalUserData()
        {
            await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(m_LocalUser));
        }

        public void UIChangeMenuState(GameState state)
        {
            var isQuittingGame = LocalGameState == GameState.Lobby &&
                m_LocalLobby.LocalLobbyState.Value == LobbyState.InGame;

            if (isQuittingGame)
            {
                //If we were in-game, make sure we stop by the lobby first
                state = GameState.Lobby;
                ClientQuitGame();
            }
            SetGameState(state);
        }

        public void HostSetRelayCode(string code)
        {
            m_LocalLobby.RelayCode.Value = code;
            SendLocalLobbyData();
        }

        //Only Host needs to listen to this and change state.
        void OnPlayersReady(int readyCount)
        {
            Debug.LogError($"Ready Count: {readyCount}");
            
            if (readyCount == m_LocalLobby.PlayerCount &&
                m_LocalLobby.LocalLobbyState.Value != LobbyState.CountDown)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.CountDown;
                SendLocalLobbyData();
            }
            else if (m_LocalLobby.LocalLobbyState.Value == LobbyState.CountDown)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.Lobby;
                SendLocalLobbyData();
            }
        }

        void OnLobbyStateChanged(LobbyState state)
        {
            if (state == LobbyState.Lobby)
                CancelCountDown();
            if (state == LobbyState.CountDown)
                BeginCountDown();
        }

        void BeginCountDown()
        {
            Debug.Log("Beginning Countdown.");
            m_countdown.StartCountDown();
        }

        void CancelCountDown()
        {
            Debug.Log("Countdown Cancelled.");
            m_countdown.CancelCountDown();
        }

        private bool _useLocalServer;

        public void EnableLocalServer()
        {
            _useLocalServer = true;
        }

        public async void FinishedCountDown()
        {
            var clientManager = ClientSingleton.Instance.Manager;

            // The networked is starting 
            m_LocalUser.UserStatus.Value = PlayerStatus.InGame;
            m_LocalLobby.LocalLobbyState.Value = LobbyState.InGame;
            
            // TODO Add fail states. 
            // clientManager.NetworkClient.OnLocalConnection += OnConnectionChanged;
            // clientManager.NetworkClient.OnLocalDisconnection += OnConnectionChanged;
            
            var lobbyPassword = m_LocalLobby.LobbyCode.Value;
            // set the password for the matchmaking result.
            clientManager.SetGamePassword(lobbyPassword);

            if (_useLocalServer)
            {
                // Sammoh SERVER - this is where we start the game for the client and the host.
                // Server should have started by now.
                var m_LocalIP = ApplicationData.IP();
                var port = ApplicationData.Port();
                clientManager.BeginConnection(m_LocalIP, port);
                m_setupInGame.StartNetworkedGame(m_LocalLobby, m_LocalUser);
                return;
            }

            await clientManager.MatchmakeAsync(OnMatchMade);
        }
        
        void OnConnectionChanged(ConnectStatus status)
        {
            if (status == ConnectStatus.Success)
            {
                if (_useLocalServer)
                    m_setupInGame.StartNetworkedGame(m_LocalLobby, m_LocalUser);
                Debug.LogError("Connected!");
            }
            else if (status == ConnectStatus.UserRequestedDisconnect)
                Debug.LogError($"Successfully Disconnected!");
            else
                Debug.LogError($"Connection Error: {status}");
        }

        private void OnMatchMade(MatchmakerPollingResult obj)
        {
            switch (obj)
            {
                case MatchmakerPollingResult.Success:
                    m_setupInGame.StartNetworkedGame(m_LocalLobby, m_LocalUser);
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                    break;
                case MatchmakerPollingResult.TicketCancellationError:
                    break;
                case MatchmakerPollingResult.TicketRetrievalError:
                    break;
                case MatchmakerPollingResult.MatchAssignmentError:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }
        
        public void StartServerGame()
        {
            // the server has started up and needs to start a game without having to be a local player, should have lobby info though.
            m_setupInGame.StartServerGame();
        }

        public void BeginGame()
        {
            if (m_LocalUser.IsHost.Value)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.InGame;
                m_LocalLobby.Locked.Value = true;
                SendLocalLobbyData();
            }
        }

        public void ClientQuitGame()
        {
            EndGame();
            m_setupInGame?.OnGameEnd();
        }

        public void EndGame()
        {
            if (m_LocalUser.IsHost.Value)
            {
                m_LocalLobby.LocalLobbyState.Value = LobbyState.Lobby;
                m_LocalLobby.Locked.Value = false;
                SendLocalLobbyData();
            }

            SetLobbyView();
        }

        #region Setup

        async void Awake()
        {
            Application.wantsToQuit += OnWantToQuit;
            m_LocalUser = new LocalPlayer("", 0, false, "LocalPlayer");
            m_LocalLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
            LobbyManager = new LobbyManager();
            MatchmakingManager = new MatchplayMatchmaker();

            // Sammoh SERVER - check if we are a dedicated server
            // Move everything to another function

            // Sammoh - SERVER Maybe move this to somewhere else so that it doesn't authenticate on app start. 
            // await InitializeServices();
            AuthenticatePlayer();
            // StartVivoxLogin(); // take out vivox
        }

        async Task InitializeServices()
        {
            string serviceProfileName = "player";
#if UNITY_EDITOR
             serviceProfileName = $"{serviceProfileName}_{ClonesManager.GetCurrentProject().name}";
#endif
            await Auth.Authenticate(serviceProfileName);
        }

        void AuthenticatePlayer()
        {
            if (ApplicationController.IsServer) return;

            var localIdBootstrap = ClientSingleton.Instance.Manager.User;
            
            // var localId = AuthenticationService.Instance.PlayerId;
            // var randomName = NameGenerator.GetName(localId);

            m_LocalUser.ID.Value = localIdBootstrap.AuthId;
            m_LocalUser.DisplayName.Value = localIdBootstrap.Name;
        }

        #endregion

        void SetGameState(GameState state)
        {
            var isLeavingLobby = (state == GameState.Menu || state == GameState.JoinMenu) &&
                LocalGameState == GameState.Lobby;
            LocalGameState = state;

            Debug.Log($"Switching Game State to : {LocalGameState}");

            if (isLeavingLobby)
                LeaveLobby();
            onGameStateChanged.Invoke(LocalGameState);
        }

        #region Lobby

        void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = new Dictionary<string, LocalLobby>();
            foreach (var lobby in lobbies)
                newLobbyDict.Add(lobby.LobbyID.Value, lobby);

            LobbyList.CurrentLobbies = newLobbyDict;
            LobbyList.QueryState.Value = LobbyQueryState.Fetched;
        }

        async Task CreateLobby()
        {
            m_LocalUser.IsHost.Value = true;
            m_LocalLobby.onUserReadyChange = OnPlayersReady;
            try
            {
                await BindLobby();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Couldn't join Lobby: {exception}");
            }
        }

        async Task JoinLobby()
        {
            //Trigger UI Even when same value
            m_LocalUser.IsHost.ForceSet(false);
            await BindLobby();
        }

        async Task BindLobby()
        {
            await LobbyManager.BindLocalLobbyToRemote(m_LocalLobby.LobbyID.Value, m_LocalLobby);
            m_LocalLobby.LocalLobbyState.OnChanged += OnLobbyStateChanged;
            SetLobbyView();
            // StartVivoxJoin();
        }

        public void LeaveLobby()
        {
            m_LocalUser.ResetState();
#pragma warning disable 4014
            LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
            ResetLocalLobby();
            // m_VivoxSetup.LeaveLobbyChannel();
        }
        
        void SetLobbyView()
        {
            Debug.Log($"Setting Lobby user state {GameState.Lobby}");
            SetGameState(GameState.Lobby);
            SetLocalUserStatus(PlayerStatus.Lobby);
        }

        void ResetLocalLobby()
        {
            m_LocalLobby.ResetLobby();
            m_LocalLobby.RelayServer = null;
        }
        
        #endregion

        #region Teardown

        /// <summary>
        /// In builds, if we are in a lobby and try to send a Leave request on application quit, it won't go through if we're quitting on the same frame.
        /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
        /// </summary>
        IEnumerator LeaveBeforeQuit()
        {
            ForceLeaveAttempt();
            yield return null;
            Application.Quit();
        }

        bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(m_LocalLobby?.LobbyID.Value);
            StartCoroutine(LeaveBeforeQuit());
            return canQuit;
        }

        void OnDestroy()
        {
            ForceLeaveAttempt();
            LobbyManager.Dispose();
        }

        void ForceLeaveAttempt()
        {
            if (!string.IsNullOrEmpty(m_LocalLobby?.LobbyID.Value))
            {
#pragma warning disable 4014
                LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
                m_LocalLobby = null;
            }
        }

        #endregion
    }
}