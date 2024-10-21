using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Spell : ScriptableObject
{
    public string name;
    public SwipePattern pattern;

    private GameObject patternContainer;
    private List<LineRenderer> currentRenderers;
    public Spell(string name, SwipePattern pattern)
    {
        this.name = name;
        this.pattern = pattern;
    }

    public void DrawPattern(Transform transform)
    {
        patternContainer = Instantiate(new GameObject(), transform);
        
        for (int i = 0; i < pattern.lineRenderers.Length; i++)
        {
            currentRenderers.Add(Instantiate(pattern.lineRenderers[i], patternContainer.transform));
            currentRenderers[i].transform.position = transform.position; 
        }
    }

    public void DeletePattern()
    {
        Destroy(patternContainer);
    }
}