using System.Collections.Generic;
using UnityEngine;

public class LineRendererColorUpdater : MonoBehaviour
{
    public LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        
        // color keys 
        var startColor = new GradientColorKey(Color.red, 0.0f);
        var midColor = new GradientColorKey(Color.green, 0.5f);
        var endColor = new GradientColorKey(Color.blue, 1.0f);

        // alpha keys
        var startAlpha = new GradientAlphaKey(alpha, 0.0f);
        var endAlpha = new GradientAlphaKey(alpha, 1.0f);

        var colorKeyList = new List<GradientColorKey>();
        
        colorKeyList.Add(startColor);
        colorKeyList.Add(midColor);
        colorKeyList.Add(endColor);

        GradientAlphaKey[] alphaKeys = { startAlpha, endAlpha };
        
        gradient.SetKeys(colorKeyList.ToArray(), alphaKeys);
        lr.colorGradient = gradient;
    }

    public void OnPointFound(int index)
    {
        
    }
    
}