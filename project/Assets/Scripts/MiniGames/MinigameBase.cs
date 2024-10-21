using System;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is the base class for minigames.
/// It should be spawned by the server for any client that may request it.
/// No client should be able to spawn a minigame on their own.
/// Tasks and objective should be set by the server.
/// The client should only be able to send the server the results of their input.
/// The server should be able to validate the results of the input and then send the results to the client.
/// </summary>
public abstract class MinigameBase : NetworkBehaviour
{
    // this is the base class for minigames.
    // should handle initiation
    // event callbacks from interactions

    public ulong localId;

    private protected MinigameInput _minigameInput;

    protected virtual Coroutine _minigameTimer_Co { get; set; }

    // this was used as a hack so that the minigame could be started from the reticle.
    public MinigameType GameType => _minigameType;
    protected abstract MinigameType _minigameType { get; }

    // the minigame task was used so that the ui outside of the minigame could be updated.
    // eg. the task is to collect a clue, the minigame is to find the clue.
    public Action OnMinigameStart;
    public Action OnMinigameFinish;

    // debug options
    [SerializeField] protected DebugStatus autoBeatGame;

    public enum DebugStatus
    {
        none,
        autoWinGame,
        autoFailGame
    }

    public virtual void StartMiniGame(ulong id, Action onMinigameFinish, MinigameInput minigameInput)
    {
        // turn off the canvas if the host isn't performing the game.
        // if (id != NetworkManager.Singleton.LocalClientId)
        // {
        //     TryGetComponent<Canvas>(out var canvas);
        //     if (canvas != null)
        //     {
        //         canvas.enabled = false;
        //     }
        // }

        // utilities
        localId = id;
        _minigameInput = minigameInput;
        OnMinigameFinish = onMinigameFinish;

        _minigameInput.OnMinigameBegan(id, this);

        // check the game events to see if the minigame needs to be canceled.
        var gameRunner = InGameRunner.Instance;
        if (gameRunner != null)
            gameRunner.onRoundEnding += Evt_OnRoundEnding;
    }

    protected virtual void Evt_OnRoundEnding()
    {
        OnMinigameFinish?.Invoke();
    }

    public abstract void Reset();

    public virtual void CleanUp()
    {
        // clear events
        InGameRunner.Instance.onRoundEnding -= Evt_OnRoundEnding;
        OnMinigameFinish = null;
        Destroy(gameObject);
    }
}