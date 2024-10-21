using System;
using UnityEngine;

public class IPostRound_None : IPostRoundIntent
{
    public void ProcessIntent(ulong caller, RoleData data)
    {
        Debug.Log("Doing Nothing");

    }
}