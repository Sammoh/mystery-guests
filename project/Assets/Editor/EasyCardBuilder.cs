using UnityEditor;
using UnityEngine;

public class EasyCardBuilder
{
    [MenuItem("Edit/CreateCards")]
    private static void DoTask()
    {
        string cardName = "Action Card.json";
        string[] filePaths = AssetDatabase.FindAssets(cardName, new []{ "Assets/Resources" });

        if (filePaths.Length == 0)
        {
            Debug.LogError($"Couldn't find {cardName}");
            return;
        }

        string cardPath = AssetDatabase.GUIDToAssetPath(filePaths[0]);
        TextAsset card = AssetDatabase.LoadAssetAtPath<TextAsset>(cardPath);

        if (card == null)
        {
            Debug.LogError($"Couldn't load {cardPath}");
            return;
        }

        string directory = "Assets/Resources/ActionCards/";
        if (!AssetDatabase.IsValidFolder(directory))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "ActionCards");
        }

        for (int i = 0; i < 18; i++)
        {
            string newCardName = GetCardName(i);
            string newPath = $"{directory}{newCardName}.json";

            if (AssetDatabase.CopyAsset(cardPath, newPath))
            {
                Debug.Log($"Created {newPath}");
            }
            else
            {
                Debug.LogError($"Failed to create {newPath}");
            }
        }

        AssetDatabase.Refresh();
    }

    private static string GetCardName(int index)
    {
        switch (index)
        {
            case 0:
                return "Alibi";
            case 1:
                return "Blackmail";
            case 2:
                return "Bribery";
            case 3:
                return "Character Witness";
            case 4:
                return "Conspiracy Theory";
            case 5:
                return "Eye Witness";
            case 6:
                return "Forensic Evidence";
            case 7:
                return "Form Ally";
            case 8:
                return "Frame";
            case 9:
                return "Insider Information";
            case 10:
                return "Intercept";
            case 11:
                return "Mystery Box";
            case 12:
                return "Red Herring";
            case 13:
                return "Scape Goat";
            case 14:
                return "Sticky Fingers";
            case 15:
                return "Wager";
            case 16:
                return "Witness";
            default:
                return $"Card {index}";
        }
    }
}