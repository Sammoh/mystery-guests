using System;
using CardGame.GameData.Cards;
using MurderMystery;
using Unity.Netcode;

    public enum Role
    {
        Default = -1,
        Innocent = 0, // doesn't do too much..
        Killer = 1, // Use ability to kill.
        Detective = 2, // Use ability to verify a suggestion.
        Medic = 3, // Use ability to protect a player from kill.
        Sheriff = 4, // Use ability to add a player to suspect list.
        Spy = 5, // Use ability to see a card.
    }

    [Serializable]
    public class RoleData : INetworkSerializable
    {
        public ulong playerId;
        public string name;
        public string imagePath;
        public float rate;
        public Role role;
        public IntentType[] instructions;
        public int selectedCard;

        public RoleData()
        {
            
        }
        
        public RoleData(string name, string imagePath, float rate, Role role, IntentType[] instructions, int selectedCard)
        {
            this.name = name;
            this.imagePath = imagePath;
            this.rate = rate;
            this.role = role;
            this.instructions = instructions;
            this.selectedCard = selectedCard;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref imagePath);
            // serializer.SerializeValue(ref index);
            serializer.SerializeValue(ref rate);
            serializer.SerializeValue(ref role);        
            serializer.SerializeValue(ref instructions);
            serializer.SerializeValue(ref selectedCard);
            
            // // Length
            int length = 3;
            if (!serializer.IsReader)
            {
                length = instructions.Length;
            }
            
            serializer.SerializeValue(ref length);
            
            // Array
            if (serializer.IsReader)
            {
                instructions = new IntentType[length];
            }
            
            for (int n = 0; n < length; ++n)
            {
                serializer.SerializeValue(ref instructions[n]);
            }
        }
    }
