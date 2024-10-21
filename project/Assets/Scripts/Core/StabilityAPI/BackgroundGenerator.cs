using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using LobbyRelaySample;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundGenerator : MonoBehaviour
{

    [SerializeField] private StabilityDiffusionAPI StabilityDiffusionAPI;
    private NetworkedTexture networkedTexture = new NetworkedTexture();
    
    private bool isWaitingForResponse;

    public void GenerateBackground(Action<byte[]> onComplete, Action<string> onError) {

        SendInitialGTPRequest(() =>
        {
            var inputText = GameManager.Instance.LocalLobby.LocalLobbyLocationValue.Value;
            SendGPTRequest(inputText, responseContent => {
                SendQuery(responseContent, onComplete, onError);
            }, error => { onError?.Invoke(error); });
        }, error => { onError?.Invoke(error);});
    }

    private void SendQuery(string inputText, Action<byte[]> onComplete, Action<string> onErrorThrown) {
        
        // if (isWaitingForResponse) return;

        // string inputText = inputField.text;
        if (string.IsNullOrEmpty(inputText)) {
            return;
        }
        Debug.LogError($"Sending Image Query: {inputText}");
        StabilityDiffusionAPI.TextToImage(inputText, texture => {
            onComplete?.Invoke(texture);
        }, error => {
            onErrorThrown?.Invoke(error);
        });
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
    
    private void SendGPTRequest(string inputText, Action<string> onComplete = null, Action<string> onErrorThrown = null)
    {
        Debug.LogError($"SendGPTRequest: {inputText}");
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
                    onComplete?.Invoke(responseContent);
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
        Debug.LogError("SendInitialGTPRequest");
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
