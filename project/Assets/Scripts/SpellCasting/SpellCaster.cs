using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SpellCaster : MonoBehaviour
{
    private List<SwipePoint> points = new List<SwipePoint>();
    private LineRenderer[] lines;
    
    [FormerlySerializedAs("DrawLine")] [SerializeField] private DrawLine drawLine;
    
    [SerializeField] private List<Spell> spellList = new List<Spell>();
    [SerializeField] private Spell currentSpell;
    private LineRenderer currentShape;

    
    [SerializeField] private List<Button> spellSelectButtons;
    [SerializeField] private SpellCanvasUI spellCanvas;
    [SerializeField] private GameObject patternArea;

    private void Awake()
    {
        drawLine.OnLineFinished += OnLineFinished;
        spellCanvas.OnAnimationFinished += OnAnimationFinished;

        if (spellSelectButtons.Count == 0) return;

        for (var i = 0; i < spellSelectButtons.Count; i++)
        {
            var button = spellSelectButtons[i];
            var text = button.GetComponentInChildren<Text>();
            
            var spellName = spellList[i].name;
            text.text = spellName;

            // button.SetDisabled(false);

        }
        
    }

    private void OnAnimationFinished()
    {
        for (int i = 0; i < spellSelectButtons.Count; i++)
        {
            // spellSelectButtons[i].SetDisabled(false);
        }    
    }
    private void OnLineFinished(List<Vector3> fingerPoses)
    {
        if (fingerPoses.Count == 0) return;

        CastSpell();
        
        // cleanup
        Destroy(currentShape.gameObject);
        points.Clear();    
    }

    // this is very dynamic. Could ask for any spell...
    // I like it, but this should be optional for 'quick casting'. 
    private void CastSpell()
    {
        var pattern = currentSpell.pattern;
        var isValid = pattern.compareVector3(drawLine.LineRenderer) < pattern.threshold;
        
        spellCanvas.SetState(isValid ? SpellCanvasUI.SpellState.Success : SpellCanvasUI.SpellState.Fail);
        Debug.LogError("Casting spell: " + currentSpell.name);
        
        drawLine.SetEnabled(false);
    }

    public void Button_SelectSpell(int index)
    {
        drawLine.SetEnabled(true);

        currentSpell = spellList[index];
        
        // move this all to the spell level. 
        // spell should be able to clean itself up. 
        var newPattern = spellList[index].pattern;
        currentShape = Instantiate(newPattern.lineRenderers[0], transform);
        currentShape.transform.position = transform.position;

        // turn off all unused buttons
        for (int i = 0; i < spellSelectButtons.Count; i++)
        {
            // spellSelectButtons[i].SetDisabled(true);
        }
    }
}
    