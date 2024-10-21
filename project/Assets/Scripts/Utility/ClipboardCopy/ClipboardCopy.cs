using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClipboardCopy : MonoBehaviour
{
    public string inputField;
    [SerializeField] private Button copyButton;
    
    [SerializeField] private Animator animator;

    void Start()
    {
        copyButton.onClick.AddListener(CopyToClipboard);
    }

    private void CopyToClipboard()
    {
        animator.SetTrigger("copied");
        GUIUtility.systemCopyBuffer = inputField;
    }
}
