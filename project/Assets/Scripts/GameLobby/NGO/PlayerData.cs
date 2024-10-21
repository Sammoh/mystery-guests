using MurderMystery;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// An example of a custom type serialized for use in RPC calls. This represents the state of a player as far as NGO is concerned,
    /// with relevant fields copied in or modified directly.
    /// </summary>
    public class PlayerData : IPlayerData
    {       
        public PlayerData() { } // A default constructor is explicitly required for serialization.

        public PlayerData(string name, ulong id,int index = 0, int score = 0, int character = 0, Role role = Role.Innocent, int[] handArray = null)
        {
            this.name = name; 
            this.id = id; 
            this.score = score;
            this.character = character;
            // this.handArray = new int [3];
        
        }
    }
}
