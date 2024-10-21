using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SwipePattern
{
    private List<Vector3> points = new List<Vector3>();
    public LineRenderer[] lineRenderers;
    public float threshold = 300;

    public List<Vector3> GetAllPoints()
    {
        if (points.Count != 0) return points;
        
        foreach (var line in lineRenderers)
        {
            var newPos = new Vector3[line.positionCount];
            line.GetPositions(newPos);

            foreach (var vector in newPos)
            {
                points.Add(vector);
            }
        }

        return points;
    }

    public float compareVector3(LineRenderer drawn)
    {
        var lineRenderer1 = drawn;
        var lineRenderer2 = lineRenderers[0];

        Vector3[] lineBuffer1  = new Vector3[lineRenderer1.positionCount];
        Vector3[] lineBuffer2  = new Vector3[lineRenderer2.positionCount];

        Array.Resize(ref lineBuffer1, lineRenderer1.positionCount);
        Array.Resize(ref lineBuffer2, lineRenderer2.positionCount);

        lineRenderer1.GetPositions(lineBuffer1);
        lineRenderer2.GetPositions(lineBuffer2);

        float diff = CompareLines.DifferenceBetweenLines(lineBuffer1, lineBuffer2);

        Debug.Log(diff < threshold ? "Pretty close!" : "Not that close...");
        
        // var compare = CompareLines.DifferenceBetweenLines(GetAllPoints().ToArray(), drawn, minDistance);
        Debug.LogError($"diff: {diff}");
        return diff;
    }

}