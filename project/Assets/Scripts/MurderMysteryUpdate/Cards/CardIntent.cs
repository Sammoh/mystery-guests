using System;
using CardGame.GameData.Cards;
using CardGame.Loaders;
using Unity.Netcode;

namespace MurderMystery
{
    /// <summary>
    /// A custom type serialized for use in RPC calls.
    /// This represents the action data that can be passed from player to server.
    /// </summary>
    public class CardIntent : INetworkSerializable
    {
        public ulong userId;
        public int cardId;
        public IntentType instruction;
        public IntentType reaction;
        public bool hasPassed;
        public int selectedCharacter;
        public int selectedCardIndex;

        public CardIntent()
        {
            this.hasPassed = false;
        } // A default constructor is explicitly required for serialization.

        public CardIntent(IntentType instruction, int selectedCharacter)
        {
            this.instruction = instruction;
            this.selectedCharacter = selectedCharacter;
        }

        public CardIntent(IntentType instruction, bool hasPassed, int selectedCharacter = -1,
            int selectedCardIndex = -1)
        {
            this.instruction = instruction;
            this.hasPassed = hasPassed;
            this.selectedCharacter = selectedCharacter;
            this.selectedCardIndex = selectedCardIndex;
        }

        public void SelectCard<T>(BaseCard card) where T : BaseCard
        {
            switch (card.CardType)
            {
                case CardTypes.Character:
                    selectedCharacter = card.CardId;
                    break;
                case CardTypes.Motive:
                    selectedCardIndex = card.CardId;
                    break;
                case CardTypes.Weapon:
                    selectedCardIndex = card.CardId;
                    break;
                case CardTypes.Action:
                    selectedCardIndex = card.CardId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            this.hasPassed = true;
        }

        public void MakeSelection(int index)
        {
            selectedCardIndex = index;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref userId);
            serializer.SerializeValue(ref cardId);
            serializer.SerializeValue(ref instruction);
            serializer.SerializeValue(ref hasPassed);
            serializer.SerializeValue(ref selectedCharacter);
            serializer.SerializeValue(ref selectedCardIndex);
        }
    }
}