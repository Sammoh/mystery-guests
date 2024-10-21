using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharacter : INetworkSerializable
{
    public ulong id;    
    public int character;
    public bool isKiller;
    public int[] handArray;
        
    // public PlayerCharacter(){}

    public PlayerCharacter(ulong id, int character = 0, bool isKiller = false, int[] handArray = null)
    {
        this.id = id; 
        this.character = character;
        this.isKiller = isKiller;
        this.handArray = handArray;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref character);
        serializer.SerializeValue(ref isKiller);
        serializer.SerializeValue(ref handArray);

        // Length
        int length = 0;
        if (!serializer.IsReader)
        {
            length = handArray.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            handArray = new int[length];
        }

        for (int n = 0; n < length; ++n)
        {
            serializer.SerializeValue(ref handArray[n]);
        }
    }

    public bool Equals(PlayerCharacter other)
    {
        return id.Equals(other.id) && character.Equals(other.character) && isKiller.Equals(other.isKiller) && handArray.Equals(other.handArray);

        
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return id == other.id && character == other.character && isKiller == other.isKiller && Equals(handArray, other.handArray);
    }

    // public override bool Equals(object obj)
    // {
    //     if (ReferenceEquals(null, obj)) return false;
    //     if (ReferenceEquals(this, obj)) return true;
    //     if (obj.GetType() != this.GetType()) return false;
    //     return Equals((PlayerCharacter)obj);
    // }
}
