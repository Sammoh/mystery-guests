using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class IntEvent : UnityEvent<int> { }

[RequireComponent (typeof(Button))]
public class IntButton : MonoBehaviour
{
    public IntEvent onClick;

    public int value;

    private void Awake ()
    {
        GetComponent<Button>().onClick.AddListener(() => onClick.Invoke(value));
    }
}