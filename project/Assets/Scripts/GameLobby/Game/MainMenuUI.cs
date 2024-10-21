using System;
using System.Collections;
using System.Collections.Generic;
using Matchplay.Client;
using Matchplay.Networking;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.UIElements;

enum MainMenuPlayState
{
    Authenticating,
    AuthenticationError,
    Error,
    Ready,
    MatchMaking,
    Cancelling,
    Connecting,
    Connected
}

public class MainMenuUI : MonoBehaviour
{
    ClientGameManager gameManager;
    AuthState m_AuthState;
    bool m_LocalLaunchMode;
    string m_LocalIP;
    string m_LocalPort;
    string m_LocalName;

    Button m_ExitButton;
    Button m_RenameButton;
    Button m_MatchmakerButton;
    Button m_CancelButton;
    Button m_LocalButton;
    Button m_CompetetiveButton;
    Button m_PlayButton;

    DropdownField m_QueueDropDown;
    DropdownField m_ModeDropDown;
    DropdownField m_MapDropDown;
    
    VisualElement m_ButtonGroup;
    VisualElement m_IPPortGroup;
    VisualElement m_QueueGroup;
    VisualElement m_MapGroup;
    VisualElement m_ModeGroup;
    Label m_NameLabel;
    Label m_MessageLabel;
    //
    TextField m_IPField;
    TextField m_PortField;
    TextField m_RenameField;
    
    // Start is called before the first frame update
    async void Start()
    {
        if (ClientSingleton.Instance == null)
            return;
            
        // SetUpUI();
        SetUpInitialState();
        //Default mode is Matchmaker
        SetMatchmakerMode();

        m_AuthState = await AuthenticationWrapper.Authenticating();
            
        if (m_AuthState == AuthState.Authenticated)
            SetMenuState(MainMenuPlayState.Ready, "Authenticated!");
        else
        {
            SetMenuState(MainMenuPlayState.AuthenticationError,
                "Error Authenticating: Check the Console for more details.\n" +
                "(Did you remember to link the editor with the Unity cloud Project?)");
        }
    }

    private void SetUpInitialState()
    {
        gameManager = ClientSingleton.Instance.Manager;

        SetName(gameManager.User.Name);
        gameManager.User.onNameChanged += SetName;
        gameManager.NetworkClient.OnLocalConnection += OnConnectionChanged;
        gameManager.NetworkClient.OnLocalDisconnection += OnConnectionChanged;
        
        //Set the game manager casual gameMode defaults to whatever the UI starts with
        gameManager.SetGameMode(Enum.Parse<GameMode>(m_ModeDropDown.value));
        gameManager.SetGameMap(Enum.Parse<Map>(m_MapDropDown.value));
        gameManager.SetGameQueue(Enum.Parse<GameQueue>(m_QueueDropDown.value));
        gameManager.SetGamePassword("123456");
    }
    
    void SetMatchmakerMode()
    {
        m_LocalLaunchMode = false;
        if (m_AuthState == AuthState.Authenticated)
            m_ButtonGroup.contentContainer.SetEnabled(true);
        else
            m_ButtonGroup.contentContainer.SetEnabled(false);
        m_PlayButton.text = "Matchmake";
        m_ModeGroup.contentContainer.style.display = DisplayStyle.Flex;
        m_MapGroup.contentContainer.style.display = DisplayStyle.Flex;
        m_QueueGroup.contentContainer.style.display = DisplayStyle.Flex;
        m_IPPortGroup.contentContainer.style.display = DisplayStyle.None;
    }
    
    void SetName(string newName)
    {
        // m_NameLabel.text = newName;
    }
    
    void SetMenuState(MainMenuPlayState state, string message = "")
    {
        switch (state)
        {
            case MainMenuPlayState.Authenticating:
                //We can't click play until the auth is set up.
                m_ButtonGroup.SetEnabled(false);
                SetLabelMessage("Authenticating...", Color.white);
                break;
            case MainMenuPlayState.AuthenticationError:
                SetLabelMessage(message, new Color(1, .2f, .2f, 1));
                m_PlayButton.contentContainer.style.display = DisplayStyle.Flex;
                m_ButtonGroup.contentContainer.SetEnabled(false);
                m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                break;
            case MainMenuPlayState.Error:
                SetLabelMessage(message, new Color(1, .2f, .2f, 1));
                m_PlayButton.contentContainer.style.display = DisplayStyle.Flex;
                m_ButtonGroup.contentContainer.SetEnabled(true);
                m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                break;
            case MainMenuPlayState.Ready:
                m_PlayButton.contentContainer.style.display = DisplayStyle.Flex;
                m_ButtonGroup.contentContainer.SetEnabled(true);
                m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                SetLabelMessage(message, new Color(.2f, 1, .2f, 1));
                break;
            case MainMenuPlayState.MatchMaking:
                m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                m_CancelButton.contentContainer.style.display = DisplayStyle.Flex;
                SetLabelMessage("Matchmaking...", Color.white);
                break;
            case MainMenuPlayState.Connecting:
                m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                m_CancelButton.contentContainer.style.display = DisplayStyle.Flex;
                SetLabelMessage("Connecting...", Color.white);
                break;
            case MainMenuPlayState.Connected:
                m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                SetLabelMessage("Connected!", Color.white);
                break;
            case MainMenuPlayState.Cancelling:
                m_ButtonGroup.contentContainer.SetEnabled(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
    
    void SetLabelMessage(string message, Color messageColor)
    {
        m_MessageLabel.text = message;
        m_MessageLabel.style.color = messageColor;
    }

    
    void OnConnectionChanged(ConnectStatus status)
    {
        if (status == ConnectStatus.Success)
            SetMenuState(MainMenuPlayState.Connected);
        else if (status == ConnectStatus.UserRequestedDisconnect)
            SetMenuState(MainMenuPlayState.Ready, $"Successfully Disconnected!");
        else
            SetMenuState(MainMenuPlayState.Error, $"Connection Error: {status}");
    }
}
