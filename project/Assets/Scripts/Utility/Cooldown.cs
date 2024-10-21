using System;
using UnityEngine;
public class Cooldown
{
    public float SecondsRemaining { private set; get; }
    private Action _OnCompleteCallback;

    public Cooldown()
    {
        SecondsRemaining = -1f;
    }

    public bool IsCountingDown()
    {
        return SecondsRemaining >= 0;
    }

    public void StartCountdown(float duration, Action OnComplete)
    {
        if (IsCountingDown())
            Debug.LogError("Countdown is already running. Call Interrupt() if you want to restart it");

        SecondsRemaining = duration;
        _OnCompleteCallback = OnComplete;
    }

    public void Update()
    {
        if(SecondsRemaining < 0) return;

        SecondsRemaining -= Time.deltaTime;
        if(SecondsRemaining < 0)
        {
            _OnCompleteCallback();
        }
    }

    public void Interrupt(bool executeCallback)
    {
        SecondsRemaining = -1f;
        if (executeCallback)
        {
            _OnCompleteCallback();
        }
    }
}