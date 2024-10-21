using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public  class MyTestScript : MonoBehaviour
{
    #region Temporary script file operations

    const string TempFilePath = "Assets/AICommandTemp.cs";

    bool TempFileExists => System.IO.File.Exists(TempFilePath);

    static void CreateScriptAsset(string code)
    {
        // UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
        method.Invoke(null, new object[]{TempFilePath, code});
    }

    #endregion

    #region Script generator

    private static string ReadInstructions()
    {
        var instructions = System.IO.File.ReadAllText("Assets/Editor/AICommand/Instructions.txt");
        return instructions;
    }
    
    static string WrapPrompt(string input)
      => ReadInstructions() +
         "The task is described as follows:\n" + input;

    void RunGenerator()
    {
        var code = AICommand.OpenAIUtil.InvokeChat(WrapPrompt(_prompt));
        Debug.Log("AI command script:" + code);
        CreateScriptAsset(code);
    }
    
    static void RunGenerator(string prompt)
    {
        var code =  AICommand.OpenAIUtil.InvokeChat(WrapPrompt(prompt));
        Debug.Log("AI command script:" + code);
        CreateScriptAsset(code);
    }

    #endregion

    #region Editor GUI

    string _prompt = "Create 100 cubes at random points.";

    const string ApiKeyErrorText =
      "API Key hasn't been set. Please check the project settings " +
      "(Edit > Project Settings > AI Command > API Key).";

    bool IsApiKeyOk
      => !string.IsNullOrEmpty( AICommand.AICommandSettings.instance.apiKey);

    [MenuItem("Window/AI Command")]
    // static void Init() => GetWindow<AICommandWindow>(true, "AI Command");

    void OnGUI()
    {
        if (IsApiKeyOk)
        {
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Run")) RunGenerator();
        }
        else
        {
            EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
        }
    }

    #endregion

    #region Script lifecycle

    void OnEnable()
      => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

    void OnDisable()
      => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

    void OnAfterAssemblyReload()
    {
        if (!TempFileExists) return;
        EditorApplication.ExecuteMenuItem("Edit/Do Task");
        AssetDatabase.DeleteAsset(TempFilePath);
    }

    #endregion

    #region Extended editor
// found at "C:\Users\samea\Documents\GitRepos\core\Automation"
    public static void RunAi()
    {
        // Get command line arguments
        var args = System.Environment.GetCommandLineArgs();

        // Find the index of the custom argument
        var customArgIndex = args.ToList().IndexOf("-customArg");

        // Check if the custom argument exists
        if (customArgIndex != -1 && customArgIndex < args.Length - 1)
        {
            var customArgValue = args[customArgIndex + 1];
            Debug.Log("Custom argument: " + customArgValue);

            // Your automation logic here, using the customArgValue string
            RunGenerator(customArgValue);
        }
        else
        {
            Debug.LogError("Custom argument not found");
        }
    }
    

    #endregion
}
