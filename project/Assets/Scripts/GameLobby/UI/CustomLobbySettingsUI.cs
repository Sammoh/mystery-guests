using CardGame;
using LobbyRelaySample;
using TMPro;
using UnityEngine;

public class CustomLobbySettingsUI : MonoBehaviour
{
    // this class is will be refactored so that there can be custom settings.
    // for now, it's just a color picker.
    
    /*
     * Select ai, count and fill
     * Set user to role
     */

    public bool m_UseLocalLobby;
    
    // add ai count. 
    // autofill with ai
    // the the host role.
    

    private LocalLobby m_lobby;
    
    [SerializeField]
    private CanvasGroup[] m_aiCanvasGroup;
    [SerializeField]
    private CanvasGroup m_hostCanvasGroup;
    [SerializeField]
    private TMP_Dropdown m_aiDropdown;
    [SerializeField]
    private TMP_Dropdown m_hostRoleDropdown;
    [SerializeField]
    private TMP_InputField m_locationInputField;
    
    private int maxInputLength = 30;
    
    #region old code
    
    static readonly Color s_orangeColor = new Color(0.83f, 0.36f, 0);
    static readonly Color s_greenColor = new Color(0, 0.61f, 0.45f);
    static readonly Color s_blueColor = new Color(0.0f, 0.44f, 0.69f);
    static readonly Color[] s_colorsOrdered = new Color[]
        { new Color(0.9f, 0.9f, 0.9f, 0.7f), s_orangeColor, s_greenColor, s_blueColor };
    
    #endregion
    
    void Start()
    {
        if (m_UseLocalLobby)
            SetLobby(GameManager.Instance.LocalLobby);
        
        foreach (var aiCanvas in m_aiCanvasGroup)
        {
            aiCanvas.alpha = 0;
        }

        // get all of the Role enum values.
        // set the dropdown to those values.
        var roles = System.Enum.GetValues(typeof(Role));
        foreach (var role in roles)
        {
            m_hostRoleDropdown.options.Add(new TMP_Dropdown.OptionData(role.ToString()));
        }

        for (var i = 0; i < GameSettings.MaxPlayers - 1; i++)
        {
            m_aiDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }

        SetHostRole(0);
        
        m_locationInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        m_locationInputField.onEndEdit.AddListener(OnInputFieldEndEdit);

    }

    private void OnInputFieldValueChanged(string arg0)
    {
        if (arg0.Length > maxInputLength)
        {
            m_locationInputField.text = arg0.Substring(0, maxInputLength);
        }
    }
    
    private void OnInputFieldEndEdit(string text) 
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.LogError($"Set location to: {text}");
            GameManager.Instance.SetLocalLobbyLocation(text);
        }
    }

    private void SetLobby(LocalLobby lobby)
    {
        ChangeColors(lobby.LocalLobbyColor.Value);
        lobby.LocalLobbyColor.OnChanged += ChangeColors;
    }
    
    // only needs to be called in the panel.
    public void ToggleAi(bool toggle)
    {
        if (toggle)
        {
            m_aiCanvasGroup[0].alpha = 1;
            GameManager.Instance.SetLocalLobbyAiCount(-1);
        }
        else
        {
            GameManager.Instance.SetLocalLobbyAiCount(0);
            foreach (var aiCanvas in m_aiCanvasGroup)
            {
                aiCanvas.alpha = 0;
            }
        }
    }
    
    public void ToggleAiCount(bool toggle)
    {
        var connectedPlayers = GameManager.Instance.LocalLobby.PlayerCount;
        var aiCount = toggle ? GameSettings.MaxPlayers - connectedPlayers : 0;
        Debug.Log($"Setting ai count to {aiCount}");
        GameManager.Instance.SetLocalLobbyAiCount(aiCount);
    }
    
    public void SetAiCount(int aiCount)
    {
        GameManager.Instance.SetLocalLobbyAiCount(aiCount);
        Debug.Log($"Setting ai count to {aiCount}");
    }
    
    public void ToggleHostOverride(bool toggle)
    {
        m_hostCanvasGroup.alpha = !toggle? 1 : 0;
        var hostOverride = toggle ? -1 : 0;
        GameManager.Instance.SetLocalLobbyHostRole(hostOverride);

        if (hostOverride >= 0)
        {
            Debug.LogError($"Setting ai count to {(Role)hostOverride}");
        }
    }
    
    public void SetHostRole(int role)
    {
        GameManager.Instance.SetLocalLobbyHostRole(role - 1);
    }

    void ChangeColors(LobbyColor lobbyColor)
    {
        // Color color = s_colorsOrdered[(int)lobbyColor];
        // foreach (Graphic graphic in m_toRecolor)
        //     graphic.color = new Color(color.r, color.g, color.b, graphic.color.a);
    }
}
