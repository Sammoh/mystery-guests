using System;
using System.Collections;
using System.Collections.Generic;
using MurderMystery;
using TMPro;
using UnityEngine;
// using UnityEngine.UIElements;
using UnityEngine.UI;

public class SimpleTooltip : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField]
    private TextMeshProUGUI m_title;
    [SerializeField]
    private TextMeshProUGUI m_titleType;
    [SerializeField]
    private TextMeshProUGUI m_description;
    [SerializeField]
    private Image m_renderer;


    
    // public Image Image
    // {
    //     get => m_renderer;
    //     set => m_renderer = value;
    // }
    private void Start()
    {
        EnableCanvasGroup(false);
    }


    public void Show(CardObject cardObject)
    {
        m_renderer.sprite = cardObject.Image.sprite;
        m_title.text = cardObject.Title.text;
        m_description.text = cardObject.Description.text;
        
        m_titleType.text = cardObject switch
        {
            // get the card type from 
            CharacterCard => "Character",
            WeaponClueCard => "Weapon",
            MotiveClueCard => "Motive",
            ActionCard => "Action",
            _ => m_titleType.text
        };
        
        EnableCanvasGroup(true);
    }
    
    public void Button_Hide()
    {
        EnableCanvasGroup(false);
    }
    
    void EnableCanvasGroup(bool value) {
        _canvasGroup.alpha = value ? 1 : 0;
        _canvasGroup.blocksRaycasts = value;
        _canvasGroup.interactable = value;
    }
}
