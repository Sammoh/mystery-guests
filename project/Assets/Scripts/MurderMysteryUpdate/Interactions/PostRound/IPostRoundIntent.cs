using System;
using MurderMystery;

public interface IPostRoundIntent
{
    void ProcessIntent(ulong caller, RoleData data);
}
