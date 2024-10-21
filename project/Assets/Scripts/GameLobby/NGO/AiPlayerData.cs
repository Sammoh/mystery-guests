using System.Collections.Generic;
using CardGame.GameData.Cards;
using MurderMystery;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// An example of a custom type serialized for use in RPC calls. This represents the state of a player as far as NGO is concerned,
    /// with relevant fields copied in or modified directly.
    /// </summary>
    public class AiPlayerData : IPlayerData
    {
        public List<int> KnownCards { get; set; }

        // public AiPlayerData() { } // A default constructor is explicitly required for serialization.
        //
        public AiPlayerData(string name, ulong id,int index = 0, int score = 0, int character = 0, Role role = Role.Innocent, int[] handArray = null)
        {
            this.name = name; 
            this.id = id; 
            this.score = score;
            this.character = character;
            // this.handArray = new int [3];
        
        }

        public void SetPanelState(ulong playerID, IntentType intentInstruction)
        {
            throw new System.NotImplementedException();
        }
    }
}
