using System;
using System.Collections;
using System.Collections.Generic;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// TODO Sammoh Remove the debug code and replace with Dependency Injection classes
public class Minigame_Search : MinigameBase
{
    [SerializeField] private Texture2D searchTexture;
    [SerializeField] private Image searchObject;
    [SerializeField] private RectTransform m_searchArea;
    [SerializeField] private RectTransform m_magnifyingGlass;
    [SerializeField] private Vector2 searchOffest;
    [SerializeField] private Vector2 acceptedOffset;

    // properties for the shader
    private static readonly int SearchPosition = Shader.PropertyToID("_Search_Position");
    private static readonly int FingerPrintPos = Shader.PropertyToID("_Fingerprint_Offset");
    private static readonly int ItemTexture = Shader.PropertyToID("_Main_Texture");
    private static readonly int FingerTiling = Shader.PropertyToID("_Fingerprint_Scale");

    //Indicator State
    private enum IndicatorState
    {
        Searching,
        Found,
        Accepted
    }

    // Indicator
    public Image foundImage;  // Reference to the Image component
    public Image acceptedImage;  // Reference to the Image component
    
    private bool isTimerRunning = false;
    private float timer = 0f;
    
    private Vector2 fingerPrintPos;
    private float fingerprintScale;
    
    
    [SerializeField] private float acceptedDistance = 0.8f;
    
    private Coroutine _timerCoroutine;
    
    List<SymbolObject> m_currentlyCollidingSymbols;

    [SerializeField] private bool isDebug;

    List<Vector2Int> FindPixels(Texture2D texture)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        Texture2D readableTexture = new Texture2D(texture.width, texture.height);
        Graphics.CopyTexture(texture, 0, 0, readableTexture, 0, 0);
        readableTexture.Apply();

        Color[] pixels = readableTexture.GetPixels(); // Get all pixel colors into an array
        int width = readableTexture.width;
        int height = readableTexture.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = pixels[y * width + x];

                if (Math.Abs(pixelColor.a) >= 0.95f) // Check if the red component is between 75% and 80%
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        return positions;
    }
    
    protected override Coroutine _minigameTimer_Co { get; set; }
    protected override MinigameType _minigameType { get; }

    private void Start()
    {
        if (!isDebug) return;
        
        var minigameInput = GameObject.Find("MinigameInput").GetComponent<MinigameInput>();
        StartMiniGame(0, () => Debug.LogError("Finished"), minigameInput);
        
        // set the texture to the search object
        searchObject.material.SetTexture(ItemTexture, searchTexture);
    }

    void Update()
    {
        m_magnifyingGlass.position = Input.mousePosition;
        
        // if not owner then skip, but if is debug then continue
        if (!IsHost && !isDebug) return;

        UpdateSearchPosition();
        
        var indicatorPosition = UpdateIndicatorLogic();

        if (isDebug)
        {
            SetPostition(indicatorPosition);
        }else
            SetPosition_ServerRpc(indicatorPosition);
    }

    private void UpdateSearchPosition()
    {
        var inputPosition = m_magnifyingGlass.position;

        var searchPos = new Vector2((inputPosition.x - m_searchArea.position.x) / m_searchArea.rect.width, (inputPosition.y - m_searchArea.position.y) / m_searchArea.rect.height);
        var invertPos = searchPos * -1;
        invertPos += searchOffest;

        searchObject.material.SetVector(SearchPosition, invertPos);
    }

    private Vector3 UpdateIndicatorLogic()
    {
        // Convert m_magnifyingGlass.position to searchArea local space
        Vector2 localMagnifyingPos = m_searchArea.InverseTransformPoint(m_magnifyingGlass.position);

        // Normalize local magnifying position to [0, 1] range
        Vector2 normalizedMagnifyingPos = new Vector2(
            (localMagnifyingPos.x + m_searchArea.rect.width / 2) / m_searchArea.rect.width,
            (localMagnifyingPos.y + m_searchArea.rect.height / 2) / m_searchArea.rect.height
        );

        // Scale it according to the fingerprint scale
        normalizedMagnifyingPos *= fingerprintScale;

        // Adjust the position by adding the searchOffset
        normalizedMagnifyingPos += acceptedOffset;
        searchPosition_gizmo = normalizedMagnifyingPos;
        return normalizedMagnifyingPos;
    }

    private Vector2 searchPosition_gizmo;
    
    [ServerRpc] // Leave (RequireOwnership = true) for these so that only the player whose cursor this is can make updates.
    private void SetPosition_ServerRpc(Vector3 position)
    {
        SetPostition(position);
    }
    
    private void SetPostition(Vector3 position)
    {
        var distance = Vector2.Distance(position, fingerPrintPos);
        if (distance < acceptedDistance)  // Threshold for detecting the fingerprint
        {
            Debug.LogError("Found the fingerprint");
            UpdateClientVisuals_ClientRpc(IndicatorState.Found);

            if (isTimerRunning) return;
            
            if (_timerCoroutine == null)
                _timerCoroutine = StartCoroutine(TimerCoroutine());
        }
        else
        {
            Debug.LogError("Not found the fingerprint");
            UpdateClientVisuals_ClientRpc(IndicatorState.Searching);

            // Stop and reset the timer if it's running
            if (!isTimerRunning) return;
            
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
            isTimerRunning = false;
        }
    }
    
    [ClientRpc]
    private void UpdateClientVisuals_ClientRpc(IndicatorState state)
    {
        // Sammoh todo - update the visuals on the client.
        switch (state)
        {
            case IndicatorState.Searching:
                foundImage.color = Color.clear;
                acceptedImage.color = Color.clear;
                break;
            case IndicatorState.Found:
                foundImage.color = Color.white;
                acceptedImage.color = Color.clear;
                break;
            case IndicatorState.Accepted:
                foundImage.color = Color.clear;
                acceptedImage.color = Color.white;
                break;
        }
        
        Debug.LogError($"Setting the indicator to {state}");
    }

    private void OnDrawGizmos()
    {
        // Draw a sphere at the search position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(searchPosition_gizmo, 0.1f);
    }

    private IEnumerator TimerCoroutine()
    {
        isTimerRunning = true;
        yield return new WaitForSeconds(2f);
        isTimerRunning = false;
    
        UpdateClientVisuals_ClientRpc(IndicatorState.Accepted);
        yield return new WaitForSeconds(1f);
        // Invoke the action here
        OnMinigameFinish?.Invoke();

        // foundImage.color = Color.clear;
        // acceptedImage.color = Color.white; // Show the accepted image
    }

    private void OnDisable()
    {
        searchObject.material.SetVector(SearchPosition, Vector3.zero);
        
        var fingerPrintScale = searchObject.material.GetFloat(FingerTiling);
        fingerPrintScale *= 0.5f;
        // subtract the scaled offset from the finger print position
        fingerPrintScale -= 0.5f;

        searchObject.material.SetVector(FingerPrintPos, Vector3.one * fingerPrintScale);
    }

    public override void StartMiniGame(ulong id, Action onMinigameFinish, MinigameInput input)
    {
        base.StartMiniGame(id, onMinigameFinish, input);
        
        input.OnInputTriggerEnter += Evt_OnTriggerEntered;
        input.OnInputTriggerExit += Evt_OnTriggerExited;

        var sampledPixels = FindPixels(searchTexture);
        // get a random pixel from the list
        Vector2 randomPixel = sampledPixels[UnityEngine.Random.Range(0, sampledPixels.Count)];
        // convert the pixel to a percentage of the searchObject
        randomPixel = new Vector2(randomPixel.x / searchTexture.width, randomPixel.y / searchTexture.height);
        fingerprintScale = searchObject.material.GetFloat(FingerTiling);
        fingerprintScale -= 0.5f;
        randomPixel *= fingerprintScale;

        // randomPixel offset of the percent of the search area
        randomPixel += searchOffest * fingerprintScale;
        fingerPrintPos = randomPixel;
        searchObject.material.SetVector(FingerPrintPos, randomPixel);
        
        searchObject.material.SetVector(SearchPosition, Vector3.zero);
        
        UpdateClientVisuals_ClientRpc(IndicatorState.Searching);
    }

    public override void Reset()
    {
    }
    
    private void Evt_OnTriggerEntered(Collider other)
    {
        Debug.LogError("Trigger Entered");
        
        if (!IsHost)
            return;
        
        SymbolObject symbol = other.GetComponent<SymbolObject>();
        if (symbol == null)
            return;
        if (!m_currentlyCollidingSymbols.Contains(symbol))
            m_currentlyCollidingSymbols.Add(symbol);
    }

    private void Evt_OnTriggerExited(Collider other)
    {
        Debug.LogError("Trigger Exited");
        
        if (!IsHost)
            return;
        
        SymbolObject symbol = other.GetComponent<SymbolObject>();
        if (symbol == null)
            return;
        if (m_currentlyCollidingSymbols.Contains(symbol))
            m_currentlyCollidingSymbols.Remove(symbol);
    }
}
