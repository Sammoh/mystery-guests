#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            string[] sceneNames = GetSceneNames();
            int selected = 0;
            for (int i = 0; i < sceneNames.Length; ++i)
            {
                if (sceneNames[i] == property.stringValue)
                {
                    selected = i;
                    break;
                }
            }

            selected = EditorGUI.Popup(position, label.text, selected, sceneNames);
            property.stringValue = sceneNames[selected];
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use [Scene] with string.");
        }
    }

    private string[] GetSceneNames()
    {
        string[] scenes = new string[SceneManager.sceneCountInBuildSettings];
        for (int i = 0; i < scenes.Length; ++i)
        {
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
        }
        return scenes;
    }
}

#endif
