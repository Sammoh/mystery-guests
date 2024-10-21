using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System;
using UnityEditor;

public class StabilityDiffusionAPI : MonoBehaviour
{
    private int m_ImageWidth = 512;
    private int m_ImageHeight = 512;
    private string url = "https://api.stability.ai/v1/generation/stable-diffusion-xl-1024-v1-0/text-to-image";

    // the output path should be in resources folder
    private  string OUTPUT_PATH = Application.dataPath + "/Resources/Output/";
    
    private string apiKey = "sk-pab3Rg6oND74lRUFVGK12fLKpCMZOHxIF20Hqjx4Ion5MkhJ";
    public void TextToImage(string input, Action<byte[]> onSuccess, Action<string> onError)
    {
        StartCoroutine(PostRequest(input, onSuccess, onError));
    }

    private IEnumerator PostRequest(string input, Action<byte[]> onSuccess, Action<string> onError)
    {
        string bodyJson = new ImageRequest
        {
            steps = 40,
            width = m_ImageWidth,
            height = m_ImageHeight,
            seed = 0,
            cfg_scale = 5,
            samples = 1,
            style_preset = "fantasy-art",
            text_prompts = new []
            {
                new TextPrompt
                {
                    text = input,
                    weight = 1
                },
                new TextPrompt
                {
                    text = "blurry, bad",
                    weight = -1
                }
            }
        }.ToJson();

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("accept", "application/json");
        request.SetRequestHeader("content-type", "application/json");
        request.SetRequestHeader("authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            onError?.Invoke(request.error);
        }
        else
        {
            var response = request.downloadHandler.text;
            // Handling the response and saving the image
            var jsonData = JsonUtility.FromJson<ResponseData>(response);

            if (!Directory.Exists(OUTPUT_PATH))
            {
                Directory.CreateDirectory(OUTPUT_PATH);
            }

            foreach (var artifact in jsonData.artifacts)
            {
                byte[] imageBytes = Convert.FromBase64String(artifact.base64);
                File.WriteAllBytes(OUTPUT_PATH + $"/txt2img_{artifact.seed}.png", imageBytes);
                
                // // Convert the image bytes to Texture2D
                // Texture2D texture = new Texture2D(m_ImageWidth, m_ImageHeight);
                // texture.LoadImage(imageBytes);
                //
                // byte[] textureBytes = texture.EncodeToPNG();
                // networkedTextureBytes.Value = textureBytes;

                // Invoke the onSuccess callback with the texture
                onSuccess?.Invoke(imageBytes);
            }
        }
    }

    [Serializable]
    public class ResponseData
    {
        public Artifact[] artifacts;
    }

    [Serializable]
    public class Artifact
    {
        public string seed;
        public string base64;
    }

    [Serializable]
    public class ImageRequest
    {
        public int steps;
        public int width;
        public int height;
        public int seed;
        public int cfg_scale;
        public int samples;
        public string style_preset;
        public TextPrompt[] text_prompts;

        public ImageRequest(int steps, int width, int height, int seed, int cfgScale, int samples,
            string stylePreset, TextPrompt[] textPrompts)
        {
            this.steps = steps;
            this.width = width;
            this.height = height;
            this.seed = seed;
            this.cfg_scale = cfgScale;
            this.samples = samples;
            this.style_preset = stylePreset;
            this.text_prompts = textPrompts;
        }

        public ImageRequest()
        {
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ImageRequest FromJson(string json)
        {
            return JsonUtility.FromJson<ImageRequest>(json);
        }

        public static ImageRequest Default()
        {
            return new ImageRequest(40, 1024, 1024, 0, 5, 1, "comic-book",
                new []
                {
                    new TextPrompt() { text = "A lavish Art Deco ballroom", weight = 1 },
                    new TextPrompt() { text = "blurry, bad", weight = -1 }
                });
        }

    }
    
    [Serializable]
    public class TextPrompt
    {
        public string text;
        public int weight;
    }
}