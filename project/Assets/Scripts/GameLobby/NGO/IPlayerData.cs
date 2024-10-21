using Unity.Netcode;
using MurderMystery;

public class IPlayerData : INetworkSerializable
{
    public string name;
    public ulong id;
    public int score;
    public int character;
    // public int[] handArray;
    
    public IPlayerData() { } // A default constructor is explicitly required for serialization.


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref id);
        // serializer.SerializeValue(ref index);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref character);

        // // Length
        // int length = 3;
        // if (!serializer.IsReader)
        // {
        //     length = handArray.Length;
        // }
        //
        // serializer.SerializeValue(ref length);
        //
        // // Array
        // if (serializer.IsReader)
        // {
        //     handArray = new int[length];
        // }
        //
        // for (int n = 0; n < length; ++n)
        // {
        //     serializer.SerializeValue(ref handArray[n]);
        // }
    }
}