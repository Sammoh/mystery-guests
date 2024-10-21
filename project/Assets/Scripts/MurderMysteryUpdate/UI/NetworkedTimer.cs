using System;
using LobbyRelaySample;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedTimer : NetworkBehaviour
{
    [SerializeField]
    private Text m_timerText;

    readonly int debugTimer = 20;

    readonly int tenSeconds = 10;
    readonly int twentyMin = 20 * 60;
    readonly int thirtyMin = 30 * 60;
    readonly int oneHour = 60 * 60;
    
    Action m_onTimerFinished;
    
    private Cooldown m_cooldown;
    NetworkVariable<float> m_timer = new NetworkVariable<float>(0f);
    
    public bool IsRunning => m_cooldown.IsCountingDown();

    private void Awake()
    {
        m_cooldown = new Cooldown();
    }
    
    public override void OnNetworkSpawn()
    {
        // increments the timer GUI
        m_timer.OnValueChanged += (prevValue, newValue) =>
        {
            // timerText.text = "Time: " + newValue.ToString("0.00");
            var timerString = ConvertToClockFormat((int)newValue);
            m_timerText.text = timerString;
        };
    }

    private void Update()
    {
        if (!m_cooldown.IsCountingDown()) return;
        
        // reduce the timer by the time that has passed since the last frame.
        // convert the timer into a string.
        
        m_timer.Value = m_cooldown.SecondsRemaining;
        m_cooldown.Update();
    }

    public void StartTimer(Action onCompleted)
    {
        SetTimerCount(GameManager.Instance.LocalLobby.LocalLobbyTimer.Value);
        m_onTimerFinished = onCompleted;
    }

    private void SetTimerCount(int timeCounter)
    {
        m_timer.Value = timeCounter switch
        {
            (int)TimeCounterEnum.TenMin => tenSeconds,
            (int)TimeCounterEnum.TwentyMin => twentyMin,
            (int)TimeCounterEnum.ThirtyMin => thirtyMin,
            (int)TimeCounterEnum.OneHour => oneHour,
            _ => m_timer.Value
        };

        // var useDebug = Debug.isDebugBuild;
        // m_timer.Value = useDebug ? debugTimer : m_timer.Value;

        m_cooldown.StartCountdown(m_timer.Value, 
            () => m_onTimerFinished?.Invoke());
    }

    // convert a string into a clock format.
    private string ConvertToClockFormat(int time)
    {
        var minutes = time / 60;
        var seconds = time % 60;
        return $"{minutes}:{seconds:00}";
    }

    public void StopTimer()
    {
        m_cooldown.Interrupt(false);
        m_timerText.text = "";
    }
}
