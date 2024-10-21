using System;
using System.Collections;
using LobbyRelaySample.ngo;
using Trigger;
using UnityEngine;
using Random = UnityEngine.Random;

public class Minigame_None : MinigameBase
{
    protected override Coroutine _minigameTimer_Co { get; set; }
    protected override MinigameType _minigameType => MinigameType.None;

    
    // replaces the start method
    public override void OnNetworkSpawn()
    {
        // when the network spawns the minigame, it should be sign up for player events.
    }

    // reset positions and events
    public override void StartMiniGame(ulong id, Action onMinigameFinish, MinigameInput input)
    {
        // check to make sure that the minigame only works for the player that requested it.

        base.StartMiniGame(id, onMinigameFinish, input);
        Debug.LogError("Starting Minigame None on the server.");
        
        input.OnInputTriggerEnter += Evt_OnTriggerEntered;
        input.OnInputTriggerExit += Evt_OnTriggerExited;
        input.OnInputClicked += Evt_OnInputClicked;
        
        // _minigameTimer_Co = StartCoroutine(OnAnimationFinished());
    }

    public override void Reset()
    {
        // selectedArObject.OnArObjectInteracted += Evt_OnArObjectInteracted;
        // selectedArObject.Start();
    }
    
    // reset and delete everything. 
    public override void CleanUp()
    {
        base.CleanUp();
        // Destroy(selectedArObject.gameObject);
    }
    
    //resolves events from arObject, fires the on complete to the listeners
    private IEnumerator OnAnimationFinished()
    {
        yield return new WaitForSeconds(2f);
        // OnInteractionComplete?.Invoke(InteractableObjectState.None);
        OnMinigameFinish?.Invoke();
    }
    
    private void Evt_OnTriggerEntered(Collider other)
    {
        Debug.LogError("Trigger Entered");
    }

    private void Evt_OnTriggerExited(Collider obj)
    {
        Debug.LogError("Trigger Exited");
    }
    
    private void Evt_OnInputClicked()
    {
        Debug.LogError("Input Clicked");
        OnMinigameFinish?.Invoke();
    }

}