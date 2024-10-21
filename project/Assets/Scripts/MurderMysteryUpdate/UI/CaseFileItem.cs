using System;
using System.Collections;
using System.Collections.Generic;
using CardGame.GameData.Cards;
using UnityEngine;
using UnityEngine.UI;

public class CaseFileItem : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private PanelGameState PanelState { get { return (PanelGameState) _animator.GetInteger("NumPanelState"); } set { _animator.SetInteger("NumPanelState", (int) value); } }

    public enum PanelGameState
    {
        None = 0, // blank
        Toggled_0 = 1, // toggled on
        Toggled_x = 2, // toggled on
        Validated = 3, // received
    }

    [SerializeField]
    private Button toggleButton;
    [SerializeField]
    private Text _text;

    // private int _index;
    // public int Index => _index;
    
    private BaseCard _card;
    public BaseCard Card => _card;

    private void Start()
    {
        PanelState = PanelGameState.None;
        toggleButton.onClick.AddListener(() =>
        {
            _onClicked?.Invoke(this);
        });
    }

    private Action<CaseFileItem> _onClicked;
    public void SetTitle(BaseCard card, Action<CaseFileItem> onClicked)
    {
        _onClicked += onClicked;
        _text.text = card.Name;
        _card = card;
    }
    
    public void SetState(PanelGameState state)
    {
        PanelState = state;

        if (state == PanelGameState.Validated)
        {
            toggleButton.interactable = false;
        }
    }
    
    private int toggledState = 0;

    
    public void ToggleState()
    {
        toggledState++;
        if (toggledState > 2) toggledState = 0;
        
        PanelState = (PanelGameState)toggledState;
    }
}
