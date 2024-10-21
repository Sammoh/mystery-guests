using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseFileTitle : MonoBehaviour
{
    [SerializeField]
    private Text _text;
    public void SetTitle(string text)
    {
        _text.text = text;
    }
}
