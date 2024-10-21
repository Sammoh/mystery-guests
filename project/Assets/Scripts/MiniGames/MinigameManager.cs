using System;
using System.Collections.Generic;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;

// this service is meant to be the central location for all game specific information. 
// this will help resolve reward systems.
// panels like home and ar should provide the mechanics of interacting with the options available.
// Minigames that are generated should be referred to and resolved by this system. 

public enum MinigameType
{
    None = 0,
    Search = 1,
    Fetch = 2,
    Golf = 3,
}

public interface IMinigameManager
{
    void StartMinigame(ulong id, MinigameType type, Action<bool> onComplete);
    void AddInteractionListener(ulong id, MinigameInput input);
}

public class MinigameManager : NetworkBehaviour, IMinigameManager
{
    private Dictionary<ulong, Action<bool>> OnMinigameComplete = new Dictionary<ulong, Action<bool>>(); // last thing that happens for interaction. 
    // public InteractablePrefab CurrentInteractable;
    
    private Cooldown _minigameTimer = new Cooldown();
    
    private static MinigameManager _instance;
    public static MinigameManager Instance => _instance;
    
    private Dictionary<ulong, MinigameInput> _minigameInput = new ();
    
    private void Awake()
    {
        _instance = this;
    }

    protected void Cleanup()
    {
        _minigameTimer.Interrupt(false);
    }

    [SerializeField]
    private MinigameBase[] m_minigames;
    
    public void AddInteractionListener(ulong id, MinigameInput input)
    {
        _minigameInput.Add(id, input);
    }
    
    // This should be done on the server.
    // This will then spawn the minigame on the client.
    public void StartMinigame(ulong id, MinigameType type, Action<bool> onComplete)
    {
        OnMinigameComplete.Add(id, onComplete);
        
        var minigame = Instantiate(m_minigames[(int)type]);
        minigame.NetworkObject.SpawnWithOwnership(id);
        minigame.name += $"_{id}";
        
        minigame.StartMiniGame(id, () =>
        {
            Debug.LogError("Minigame complete");
            OnMinigameComplete[id]?.Invoke(true);
            OnMinigameComplete.Remove(id);
            
            minigame.CleanUp();
        }, _minigameInput[id]);
    }
}

