using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewDownload : MonoBehaviour
{
    string url = "https://api.sportsdata.io/v3/nhl/scores/json/AreAnyGamesInProgress?key=807c4875565d4f9dafbdcdc01c465db7";

    IEnumerator Start()
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            GetComponent<Image>().sprite = Sprite.Create(www.texture, new Rect(0.0f, 0.0f, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
