using System.Threading.Tasks;
using LobbyRelaySample;
using Matchplay.Client;
using Matchplay.Server;
using UnityEngine;

namespace Matchplay.Shared
{
    public class ApplicationController : MonoBehaviour
    {
        //Manager instances to be instantiated.
        [SerializeField]
        ServerSingleton m_ServerPrefab;
        [SerializeField]
        ClientSingleton m_ClientPrefab;

        ApplicationData m_AppData;
        public static bool IsServer;
        async void Start()
        {
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
            
            //We use EditorApplicationController for Editor launching.
            if (Application.isEditor)
                return;
            
            Debug.LogError("DEDICATED_SERVER 0.1");

            //If this is a build and we are headless, we are a server
            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }

        public void OnParrelSyncStarted(bool isServer, string cloneName)
        {
#pragma warning disable 4014
            LaunchInMode(isServer, cloneName);
#pragma warning restore 4014
        }

        /// <summary>
        /// Main project launcher, launched in Start() for builds, and via the EditorApplicationController in-editor
        /// </summary>
        async Task LaunchInMode(bool isServer, string profileName = "default")
        {
            //init the command parser, get launch args
            m_AppData = new ApplicationData();
            IsServer = isServer;
            if (isServer)
            {
                Debug.LogError("STARTING_SERVER");

                var serverSingleton = Instantiate(m_ServerPrefab);
                await serverSingleton.CreateServer(); //run the init instead of relying on start.

                // SAMMOH SERVER Default Game Info
                var defaultGameInfo = new GameInfo
                {
                    gameMode = GameMode.Meditating,
                    map = Map.Default,
                    gameQueue = GameQueue.Casual,
                    gamePassword = "123456"
                };

                await serverSingleton.Manager.StartGameServerAsync(defaultGameInfo);
                
                // This is the server we should be able to spawn the network manager when finished. 
                GameManager.Instance.StartServerGame();
            }
            else
            {
                var clientSingleton = Instantiate(m_ClientPrefab);
                clientSingleton.CreateClient(profileName);

                //We want to load the main menu while the client is still initializing.
                clientSingleton.Manager.ToMainMenu();
            }
        }
    }
}