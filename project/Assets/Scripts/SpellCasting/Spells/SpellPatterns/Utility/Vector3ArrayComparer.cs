using System;
using UnityEngine;

public static class Vector3ArrayComparer
{
    public static float Compare(Vector3[] drawn, Vector3[] toMatch, float minDistance)
    {
        var totalDistance = 0f;

        var reducedArray = Reduce(drawn, toMatch, out var smallerArray);

        foreach (var element in reducedArray)
        {
            Debug.Log(element);
        }
        
        for (var i = 0; i < reducedArray.Length; i++)
        {
            // minDistance = float.MaxValue;
            
            for (int j = 0; j < smallerArray.Length; j++)
            {
                var distance = Vector3.Distance(reducedArray[i], smallerArray[j]);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            totalDistance += minDistance;
        }
        
        return 1f - (totalDistance / reducedArray.Length);

    }
    
    public static Vector3[] Reduce(Vector3[] array1, Vector3[] array2, out Vector3[] smallerArray)
    {
        if (array1.Length > array2.Length)
        {
            array1 = TrimArray(array1, array2.Length);
            smallerArray = array2;
        }
        else
        {
            array2 = TrimArray(array2, array1.Length);
            smallerArray = array1;
        }

        for (int i = 0; i < smallerArray.Length; i++)
        {
            array1[i] = (array1[i] + array2[i]) / 2;
        }

        return array1;
    }

    private static Vector3[] TrimArray(Vector3[] array, int newLength)
    {
        var newArray = new Vector3[newLength];
        Array.Copy(array, newArray, newLength);
        return newArray;
    }
}