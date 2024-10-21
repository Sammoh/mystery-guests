using System;
using UnityEngine;

public class IPostRound_Protect : IPostRoundIntent
{
    public void ProcessIntent(ulong caller, RoleData data)
    {
        Debug.Log("Processing protect Player Intent");

    }
}
