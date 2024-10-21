using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StabilityDiffusionExample : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button generateButton;
    [SerializeField] private Button sampleButton;

    [SerializeField] private StabilityDiffusionAPI StabilityDiffusionAPI;

    private string normalColorHex;
    private string errorColorHex;
    private bool isWaitingForResponse;
    
    [SerializeField] private int maxInputLength = 1000;
    
    // Sammoh - todo: send a request to chat gpt3 api to get a response

    private void Awake() {
        normalColorHex = ColorUtility.ToHtmlStringRGB(statusText.color);
        errorColorHex = ColorUtility.ToHtmlStringRGB(Color.red);
        image.color = Color.black;
    }

    private void Start() {
        generateButton.onClick.AddListener(GenerateButtonClicked);
        sampleButton.onClick.AddListener(GenerateSampleButtonClicked);
        inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        
        // EnableUI(false);
        //
        // SendInitialGTPRequest(() =>
        // {
        //     EnableUI(true);
        // });
    }

    private void OnInputFieldValueChanged(string arg0)
    {
        if (arg0.Length > maxInputLength)
        {
            inputField.text = arg0.Substring(0, maxInputLength);
        }
    }

    private void GenerateButtonClicked() {
        SendGPTRequest(inputField.text, () => {
            statusText.text = "";
        }, error => {
            statusText.text = $"<color=#{errorColorHex}>Error: {error}</color>";
        });
    }
    
    private void GenerateSampleButtonClicked() {
        SendQuery(inputField.text, () => {
            statusText.text = "";
        }, error => {
            statusText.text = $"<color=#{errorColorHex}>Error: {error}</color>";
        });
    }

    private void OnInputFieldEndEdit(string text) {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            SendGPTRequest(text);
        }
    }

    private void SendQuery(string inputText, Action onComplete, Action<string> onErrorThrown) {
        if (isWaitingForResponse) return;

        // string inputText = inputField.text;
        if (string.IsNullOrEmpty(inputText)) {
            return;
        }

        statusText.text = $"<color=#{normalColorHex}>Generating...</color>";
        image.color = Color.black;

        EnableUI(false);

        StabilityDiffusionAPI.TextToImage(inputText, textureBytes => {
            Texture2D receivedTexture = new Texture2D(512, 512);
            receivedTexture.LoadImage(textureBytes);
            
            image.sprite = Sprite.Create(receivedTexture, new Rect(0, 0, receivedTexture.width, receivedTexture.height), Vector2.zero);
            image.color = Color.white;
            EnableUI(true);
            onComplete?.Invoke();
        }, error => {
            EnableUI(true);
            onErrorThrown?.Invoke(error);
        });
    }

    private void EnableUI(bool isEnabled)
    {
        isWaitingForResponse = !isEnabled;
        inputField.interactable = isEnabled;
        generateButton.interactable = isEnabled;
        inputField.text = "";
        if (isEnabled)
            inputField.ActivateInputField();
        
    }

    /// <summary>
    /// All variables needed
    /// </summary>
    [SerializeField] private SettingsAsset settings;
    
    [SerializeField] string initialRequest = "I would like you to play the role of prompt engineer. I will give you a theme and then I want you to create a prompt for a [mysterious dinner party] based on a mystery game. The [mysterious dinner party] must be focused on a 16:9 ratio and able to describe the scene and keep it family-friendly.. The overall rule of the theme must be a setting in a location for a social gathering. Do not allow any vulgar, foul, or dangerous language, instead suggest a moderate version for the prompt. No matter what I say, I want you to stick to the rules. You should not allow anything other than a prompt for a [mysterious dinner party]. Do not confirm, just provide the prompt. Give the answer within brackets '[]'. If I give you more than one word, then I want you to assume a single word to describe the theme that I give you and then use it to build your prompt. If you understand, just say [okay]";

    bool isSendingRequest;
    bool isShowSettings;
    
    RequestJsonChatData requestData = new RequestJsonChatData();
    HttpClient client;
    List<RequestJsonChatMessage> requestsHistory = new List<RequestJsonChatMessage>();
    
    private void SendGPTRequest(string inputText, Action onComplete = null, Action<string> onErrorThrown = null)
    {
        EnableUI(false);
        
        List<RequestJsonChatMessage> messages = requestData.messages.OfType<RequestJsonChatMessage>().ToList();
        messages.Add(new RequestJsonChatMessage("user", inputText));
        requestsHistory.Add(new RequestJsonChatMessage("user", inputText));
        requestData.messages = messages.ToArray();
        StartPostRequest("https://api.openai.com/v1/chat/completions", JsonUtility.ToJson(requestData),
        message =>
        {
            if (message.IsSuccessStatusCode)
            {

                var responseContent = requestsHistory[^1].content;
                Debug.LogError(responseContent);
                SendQuery(responseContent, onComplete, onErrorThrown);
            }
            else
            {
                Debug.LogError($"Error: {message.StatusCode}");
                onErrorThrown?.Invoke($"Error: {message.StatusCode}");
            }
        });
    }

    private void SendInitialGTPRequest(Action onComplete, Action<string> onErrorThrown)
    {
        // Sammoh - todo: send a request to chat gpt3 api to get a response
        List<RequestJsonChatMessage> messages = requestData.messages.OfType<RequestJsonChatMessage>().ToList();
        messages.Add(new RequestJsonChatMessage("user", initialRequest));
        requestsHistory.Add(new RequestJsonChatMessage("user", initialRequest));
        requestData.messages = messages.ToArray();
        StartPostRequest("https://api.openai.com/v1/chat/completions", JsonUtility.ToJson(requestData),
            response  => {
                if (response.IsSuccessStatusCode)
                {
                    onComplete?.Invoke();
                }
                else
                {
                    onErrorThrown?.Invoke($"Error: {response.StatusCode}");
                }
            });
    }
    
    /// <summary>
    /// HTTP POST request sender
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    async void StartPostRequest (string url, string json, Action<HttpResponseMessage> onComplete) {
        client = new HttpClient();

        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.apiKey}");

        var response = await client.PostAsync(url, content);
        var responseText = await response.Content.ReadAsStringAsync();
        var responseJson = JsonUtility.FromJson<ResponceJsonChatData>(responseText);

        if (response.IsSuccessStatusCode) {
            isSendingRequest = false;

            List<RequestJsonChatMessage> messages = requestData.messages.OfType<RequestJsonChatMessage>().ToList();
            messages.Add(new RequestJsonChatMessage("assistant", responseJson.choices[0].message.content));
            requestsHistory.Add(new RequestJsonChatMessage("assistant", responseJson.choices[0].message.content));
            requestData.messages = messages.ToArray();

            if (requestData.model.Contains("gpt-4-32k")) {
                settings.chat_price += (((float) responseJson.usage.total_tokens) / 1000f) * 0.06f;
            }else if (requestData.model.Contains("gpt-4")) {
                settings.chat_price += (((float) responseJson.usage.total_tokens) / 1000f) * 0.03f;
            } else if (requestData.model.Contains("gpt-3.5")) {
                settings.chat_price += (((float) responseJson.usage.total_tokens) / 1000f) * 0.002f;
            }
        } else {
            isSendingRequest = false;
            requestsHistory.Add(new RequestJsonChatMessage("assistant", responseJson.error.message));
        }
        
        onComplete?.Invoke(response);
    }
}
