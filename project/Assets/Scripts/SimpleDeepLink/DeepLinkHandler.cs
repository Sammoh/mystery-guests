using UnityEngine;
using UnityEngine.SceneManagement;

public class DeepLinkHandler : MonoBehaviour
{
    public static DeepLinkHandler Instance { get; private set; }
    public string deeplinkURL;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;                
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
 
    private void onDeepLinkActivated(string url)
    {
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;
    
        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // myapp://launch/Level2?data=TWPD
        string[] urlParts = url.Split('?');
        string sceneName = urlParts[1];
        string data = "";
        if (urlParts.Length > 2)
        {
            data = urlParts[2].Split('=')[1];
        }
    
        // Load the specified scene
        bool validScene = false;
        switch (sceneName)
        {
            case "Level1":
                validScene = true;
                break;
            case "Level2":
                validScene = true;
                break;
            default:
                validScene = false;
                break;
        }
        if (validScene)
        {
            SceneManager.LoadScene(sceneName);
            // Call the DoSomething method in MyOtherClass with the data parameter
            // MyOtherClass otherClass = new MyOtherClass();
            // otherClass.DoSomething(data);
            Debug.LogError("using this data from the deeplink: " + data);
        }
    }

}