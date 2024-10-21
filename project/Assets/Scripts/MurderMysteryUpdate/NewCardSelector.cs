using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardGame;
using CardGame.GameData.Cards;
using LobbyRelaySample;
using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using CardGame.GameData.Decks;
using CardGame.Loaders;
using CardGame.Textures;

namespace MurderMystery
{
    
    /// <summary>
    /// This class acts as a dealer.
    /// </summary>
    public class NewCardSelector : NetworkBehaviour
    {
        #region Card Data
        
        private Deck _deck;

        private List<BaseCard> m_allCards = new();
        private List<BaseCard> drawPile = new ();

        private List<MMCharacterCard> characterList = new ();
        private List<MMMotiveCard> motiveList = new ();
        private List<MMWeaponCard> weaponList = new ();
        private List<MMActionCard> actionList = new ();

        [SerializeField] private BlankCard m_blankCard = new();

        public List<BlankCard> GetBlankCards(int amount = 3)
        {
            var blankHand = new List<BlankCard>();

            for (int i = 0; i < amount; i++)
            {
                var newBlank = Instantiate(m_blankCard);
                newBlank.SetId(i);
                blankHand.Add(newBlank);
            }

            return blankHand;
        }
        
        #endregion
        
        #region Player Data
        
        private ulong m_LocalId;
        // Player Hand
        public Dictionary<ulong, List<BaseCard>> PlayerCards => m_playerCards;
        private Dictionary<ulong, List<BaseCard>> m_playerCards = new ();
        private Dictionary<ulong, List<BaseCard>> m_playerDiscardedCards = new ();
        
        // Player Roles
        private List<RoleData> roleData = new ();
        private Dictionary<ulong, RoleData> m_PlayerRoleData = new ();
        public Dictionary<ulong, RoleData> RoleDataList => m_PlayerRoleData;
        
        // Character Cards
        private Dictionary<ulong, BaseCard> m_playerCharacters = new ();
        public Dictionary<ulong, BaseCard> PlayerCharacters => m_playerCharacters;
        
        // Internal 
        private int playerHandSize = 3;
        private int m_KillerIndex = -1;
        private List<BaseCard> m_solutionCards = new ();

        private List<Role> availableRoles = new ();

        // Actions
        private Action<BaseCard> m_onGetCardCallback;
        private Action<BaseCard> m_onGetCardsCallback;
        private Action<BaseCard[]> m_onGetCardArrayCallback;
        private Action<BaseCard[]> _onGetCards;
        private Action<ulong, BaseCard> _onGetCharacter;
        private Action<RoleData> _onGetRole;

        private Role GetRandomRole()
        {
            if (availableRoles.Count == 0)
            {
                // All enums have been used, reset the list
                return Role.Innocent;
            }

            // Make sure first enum is always selected on first round
            var randomIndex = Random.Range(0, availableRoles.Count);
            var randomEnum = availableRoles[randomIndex];

            ((IList)availableRoles).RemoveAt(randomIndex);

            return randomEnum;
        }

        private RoleData defaultRole = null;
        
        #endregion


        private string [] FillDeck (Deck deck, int deckSize) {
            Debug.LogFormat("[Player] FillDeck ()=> DeckId {0}, DeckSize {1}", deck.Id, deckSize);
            
            var cards = new List<string>();

            int required = deckSize - deck.Cards.Length;

            Debug.LogFormat("[Player] FillDeck ()=> Required cards to generate {0}", required);

            if (required < 0) {
                cards.AddRange(deck.Cards.Take(deckSize));
            } else {
                cards.AddRange(deck.Cards);

                for (int i = 0; i < required; i++) {
                    var tAsset = PlayableCards.Current.Select();
                    if (tAsset == null) {
                        Debug.LogError("[Player] Card came null from card randomizer.");
                    } else {
                        cards.Add(tAsset.name);
                    }
                }
            }

            Debug.LogFormat("[Player] GetDeck=> {0}", cards.Count);

            List<string> deckData = new List<string>();
            // load cards.
            for (int i = 0, length = cards.Count; i < length; i++) {
                var tAsset = PlayableCards.Current.Find(cards[i]);

                if (tAsset == null) {
                    Debug.LogError("[Player] Card came null from card randomizer.");
                } else {
                    deckData.Add(tAsset.text);
                }
            }

            return deckData.ToArray();
        }
        
        // from UIDeckSelector
        private void SetDeck (string deckId) {
            var deck = PlayableDecks.Current.Find(deckId);
            if (deck == null) {
                Debug.LogErrorFormat ("[UIDeckButton] Deck Id {0} is not found.", deckId);
                return;
            }

            _deck = deck;
        }

        // this is needed Game content
        [SerializeField] private Pool gameCardsData;

        // navigate game settings with this class
        [SerializeField] private GameSettings settings = default;
        [Tooltip ("Game will create effects, cards etc. to use in the game. This is the parent.")]
        [SerializeField] private Transform gameObjectsHolder;
        
        /// <summary>
        /// Path to roles.
        /// </summary>
        private static string AssetsPath {
            get {
                var path = string.Format("{0}/EasyCardGame/Resources", Application.dataPath);
                if (!Directory.Exists (path)) {
        
                    Debug.LogErrorFormat ("[EasyCardEditor] Resources is not found at {0}, you may want to change this path from EasyCardEditor.cs", path);
                }
                return path;
            }
        }

        private void InitCardDeck()
        {
            SetDeck(settings.DefaultDeck);

            int length = _deck.Cards.Length;
            var playableCards = new string[length];
            for (int i=0; i<length; i++) {
                playableCards[i] = PlayableCards.Current.Find(_deck.Cards[i]).text;
            }

            CreateDeck(settings.CardsPerRound, 0, playableCards, () => {
                Debug.LogError("[UIDeckButton] Deck drawed.");
                Debug.LogError(_deck.Cards.Length);
            });
        }
        
        /// <summary>
        /// Create deck.
        /// </summary>
        /// <param name="deckSize"></param>
        /// <param name="index"></param>
        /// <param name="cards"></param>
        /// <param name="onCompleted"></param>
        private void CreateDeck (int deckSize, int index, string[] cards, Action onCompleted) {
            // SAMMOH TODO: The index is who is playing the game.
            // This should be the player index.
            
            // this is how to cast the deck. 
            /*
            CreateDeck(gameSettings.CardsPerRound, 0, userDeck, () => { 
                isCompleted[0] = true;
                checkCompletion();
            });
            */
            
            Debug.Log("[Game] Create Deck => " + deckSize);

            if (cards == null) {
                Debug.LogError("[Game] Given cards are null");
                return;
            }

            if (deckSize < cards.Length) {
                cards = cards.ToList().Take(deckSize).ToArray();
                Debug.LogFormat("[Game] Cards length {0} is longer than {1}, cutting", cards.Length, deckSize);
            }

            // SAMMOH TODO this is used to pass out the cards to each of the users. 
            for (int i = 0, length = cards.Length; i < length; i++) {
                // card pool has 1 object pooler. So GetRandom is okay.
                var card = gameCardsData.GetRandom<Card>();

                card.SetCardData(cards[i]);

                // Sammoh todo: Add this back in.
                // card.OnToGraveyard = ResurrectAtGraveyard;

                card.UserId = index;

                // Sammoh TODO: Decks would be the layout normally. 
                // this should be added to player hands instead.
                //Decks[index].Add(card);
            }

            // start fancy mode.
            // Decks[index].Refresh(onCompleted, false, true)?;
            
            // onCompleted?.Invoke();
        }
        
        private void PrepareDrawPile()
        {
            // Start filtering the data into separate entities. 
            for (var i = 0; i < _deck.Cards.Length; i++)
            {
                var card = _deck.Cards[i];
                var cardObject = PlayableCards.Current.Find(card);
                var baseCard = JsonUtility.FromJson<BaseCard>(cardObject.text);
                var cardType = baseCard.CardInteractionType;
                

                // m_allCards.Add(baseCard);


                // Debug.LogError($"Adding <b>{card}</b> to {cardType} list");

                switch (cardType)
                {
                    case CardInteractionTypes.Action:
                        var actionCard = JsonUtility.FromJson<MMActionCard>(cardObject.text);
                        actionList.Add(actionCard);
                        break;
                    case CardInteractionTypes.Clue:
                        var clueCard = JsonUtility.FromJson<MMClueCard>(cardObject.text);
                        var clueType = clueCard.ClueCardType;

                        switch (clueType)
                        {
                            case ClueCardType.none:
                                break;
                            case ClueCardType.character:
                                var character = JsonUtility.FromJson<MMCharacterCard>(cardObject.text);
                                characterList.Add(character);
                                break;
                            case ClueCardType.motive:
                                motiveList.Add(JsonUtility.FromJson<MMMotiveCard>(cardObject.text));
                                break;
                            case ClueCardType.weapon:
                                weaponList.Add(JsonUtility.FromJson<MMWeaponCard>(cardObject.text));
                                break;
                        }
                        break;
                }
            }
            
            // randomize the character cards.
            characterList = characterList.OrderBy(x => Random.value).ToList();
            // remove the amount of cards that are not needed.
            var playerCount = InGameRunner.Instance.PlayerList.Count;
            characterList = characterList.Take(playerCount).ToList();
            
            m_allCards.AddRange(characterList);
            m_allCards.AddRange(motiveList);
            m_allCards.AddRange(weaponList);
            m_allCards.AddRange(actionList);
            
            // add all characters that are not the killer.
            drawPile.AddRange(characterList);
            drawPile.AddRange(motiveList);
            drawPile.AddRange(weaponList);
            drawPile.AddRange(actionList);

            for (var index = 0; index < m_allCards.Count; index++)
            {
                var card = m_allCards[index];
                card.CardId = index;
            }

            // randomize the draw pile.
            drawPile = drawPile.OrderBy(x => Random.value).ToList();
        }

        private void InitializePlayerCharacters()
        {
            // var path = "/Roles/";
            // var filePath = path.Replace(".json", "");
            // var targetFile = Resources.LoadAll<TextAsset>(filePath);
            // var roleFiles = Directory.GetFiles(AssetsPath + "/Roles/", "*.json");
            //        var jsonFiles = Resources.LoadAll<TextAsset>("*.json");
            
            var path = "Roles";
            // var filePath = path.Replace(".json", "");
            var roleFiles = Resources.LoadAll<TextAsset>(path);
            
            foreach (TextAsset file in roleFiles)
            {
                // string json = File.ReadAllText(file.text);
                RoleData role = JsonUtility.FromJson<RoleData>(file.text);
                roleData.Add(role);
            }
            
            // set the default role.
            defaultRole = roleData.Find(x => x.role == Role.Innocent);
            
            // randomize the players
            var allPlayers = InGameRunner.Instance.PlayerList;
            var randomized = allPlayers.OrderBy(x => Random.value).ToList();

            var charactersIndex = 0;
            // Assign the roles to the players.
            foreach (var player in randomized)
            {
                // assign player characters.
                var character = characterList[charactersIndex];
                // Debug.LogError($"Adding {player.Value.id}: {player.Value.name}");
                m_playerCharacters.Add(player.Value.id, character);
                
                // get the index of the character.
                var characterIndex = m_allCards.IndexOf(character);
                
                // assign the character to the player.
                player.Value.character = characterIndex;
                
                // assign the roles to characters strategically.
                var sortedRoleData = GetSortedRoleData();
                // Debug.LogError($"Giving {player.Value.name} the role of {sortedRoleData.role}");
                sortedRoleData.playerId = player.Value.id;
                m_PlayerRoleData.Add(player.Value.id, sortedRoleData);
                
                // assign player to discard
                m_playerDiscardedCards.Add(player.Value.id, new List<BaseCard>());

                
                charactersIndex++;
            }
            
            // override when needed. 
            if (!(GameManager.Instance.LocalLobby.LocalLobbyHostRole.Value > -1)) return;

            var newRole = (Role)GameManager.Instance.LocalLobby.LocalLobbyHostRole.Value;

            if (newRole == m_PlayerRoleData[m_LocalId].role) return;

            // switch other  role with host role.
            var hostRole = m_PlayerRoleData[m_LocalId];
            var tradeRole = m_PlayerRoleData.FirstOrDefault(x => x.Value.role == newRole);
            
            // get hte key of the player we want to switch.
            var killerRollId = m_PlayerRoleData.FirstOrDefault(x => x.Value.role == newRole).Key;
            
            // this is the player with the role we want to switch.
            m_PlayerRoleData[killerRollId] = hostRole;
            m_PlayerRoleData[m_LocalId] = tradeRole.Value;
                
            Debug.LogError($"Assigning {allPlayers[m_LocalId].name} the role of {newRole}");
        }
        
        private RoleData GetSortedRoleData()
        {
            if (roleData.Count == 0)
                return defaultRole;

            var rolesSorted = roleData.OrderBy(x => x.rate).ToList();
            var sorted = Pop(rolesSorted);

            roleData.Remove(sorted);

            return sorted;
        }

        private static T Pop<T>(List<T> list)
        {
            var lastIndex = list.Count - 1;
            var lastItem = list[lastIndex];
            list.RemoveAt(lastIndex);
            return lastItem;
        }
        
        public bool ConfirmCardCorrect(ulong playerId, BaseCard selectedCardIndex)
        {
            // check the player's hand to see if the card is correct.
            var isInHand = m_playerCards[playerId].Any(card => card == selectedCardIndex);
            return isInHand;
        }
        public bool ConfirmSuggestion(BaseCard[] selectedCards)
        {
            // check the solution to see if the accusation is correct.
            for (var i = 0; i < selectedCards.Length; i++)
            {
                if (selectedCards[i].CardId != m_solutionCards[i].CardId)
                {
                    return false;
                }
            }

            return true;
        }
        
        #region Instance

        public static NewCardSelector Instance
        {
            get
            {
                if (s_Instance!) return s_Instance;
                return s_Instance = FindObjectOfType<NewCardSelector>();
            }
        }
        static NewCardSelector s_Instance;

        #endregion

        #region Lifecycle
        
        public override void OnNetworkSpawn()
        {
            InGameRunner.Instance.onRoundBeginning += OnRoundBegan;
            InGameRunner.Instance.onRoundRestart += OnRoundRestart;

            m_LocalId = NetworkManager.Singleton.LocalClientId;

            gameCardsData.LoadSingleObject<Card>("GameCard", settings.GameCardPoolSize, gameObjectsHolder);
            TextureCollectionReader.ReadAll();
            Debug.Log("[Game] Loaded");
        }

        private void OnRoundBegan()
        {
            var displayStats =
                GameManager.Instance.LocalLobby.LocalLobbyColor.Value is LobbyColor.Orange or LobbyColor.Blue;
            if (!displayStats) return;

            // The information is only displayed to the host.
            RevealSolution();
            RevealDrawPile();
        }

        private void OnRoundRestart()
        {
            // Debug.LogError("Restarting the solution and draw pile.");
            m_solutionCards.Clear();
        }

        #endregion

        #region Initailize Game
        
        public void InitializeDeck(Action onComplete)
        {
            Debug.Log("Initialize Deck");
            
            InitCardDeck();
            
            PrepareDrawPile();

            InitializePlayerCharacters();

            CreateSolution();
            
            // drawing cards
            foreach (var player in InGameRunner.Instance.PlayerList)
            {
                m_playerCards.Add(player.Key, new List<BaseCard>());
                DrawCards(player.Value, 3);
            }
            
            onComplete?.Invoke();
        }

        private void CreateSolution()
        {
            // get the killers role data
            // extract the ulong id from the role data
            
            
            var killer = m_PlayerRoleData.FirstOrDefault(x => x.Value.role == Role.Killer);
            var killerCharacter = m_playerCharacters[killer.Key];
            var randomMotive = motiveList[Random.Range(0, motiveList.Count)];
            var randomWeapon = weaponList[Random.Range(0, weaponList.Count)];
            
            m_solutionCards.Add(killerCharacter);
            m_solutionCards.Add(randomWeapon);
            m_solutionCards.Add(randomMotive);
            
            Debug.LogError($"Solution Cards: {killerCharacter.Name}, {randomWeapon.Name},  {randomMotive.Name}");
            
            // remove all solution cards from the draw pile.
            drawPile.RemoveAll(x=> m_solutionCards.Contains(x));
        }
        
        #endregion


        public void DrawCards(IPlayerData player, int drawAmount)
        {
            // We've already ensured there are enough cards, so no need to check again.
            var cardsToDraw = drawPile.Take(drawAmount).ToList();
            drawPile.RemoveRange(0, drawAmount);
            
            // Add current cards 
            m_playerCards[player.id].AddRange(cardsToDraw);

            // if (player.id != 0) return;
            //
            // // debug all the drawn cards.
            // foreach (var card in cardsToDraw)
            // {
            //     Debug.LogError($"{player.name} drew {card.Name}");
            // }
        }
        
        public void DiscardCard(ulong playerId, BaseCard usedCard)
        {
            m_playerDiscardedCards[playerId].Add(usedCard);
            m_playerCards[playerId].RemoveAll(card => card.CardId == usedCard.CardId);
            Debug.LogError($"Player Current Cards: {String.Join(", ", m_playerCards[playerId].Select(c => c.Name))}");
        }
        
        public void AddCardsToPlayerHand(IPlayerData player, List<BaseCard> cards)
        {
            var tempCards = m_playerCards[player.id].ToList();
            
            // print all the tempCards
            for (var i = 0; i < cards.Count; i++)
            {
                if (i > tempCards.Count)
                    Debug.LogError("Player hand is full.");
                
                tempCards.Add(cards[i]);
            }

            m_playerCards[player.id] = cards;
        }
        
        // Remove card from player hand.
        public void RemoveCard(IPlayerData player, BaseCard card)
        {
            // get the index of the card.
            // var cardIndex = m_allCards.IndexOf(card);
            TakeCard(player, card);
        }
        
        // Remove card from player hand and return the index of the card.
        public BaseCard TakeCard(IPlayerData player, BaseCard card)
        {
            var playerHand = m_playerCards[player.id];
            playerHand = playerHand.Where(x => x != card).ToList();
            
            m_playerCards[player.id] = playerHand;
            
            return card;
        }
        
        #region Debug printouts

        // todo take these out and make more lean. 
        public Text m_debugText;
        public Text m_drawPileText;
        
        private void RevealSolution()
        {
            Debug.Log("Need to ask the server for the solution.");

            // InGameRunner.Instance.GetPlayerData(m_LocalId, (player, characterCard) =>
            // {
            //     var msg = "";
            //
            //     // msg += $"Player Name: {player.name} \n";
            //
            //     // todo the player doesn't have the cards?
            //     // var characterName = m_CardData.AvailableCards[player.character].name;
            //     // var characterName = m_CardData.AvailableCards[player.character].name;
            //
            //     // msg += $"Character: {characterName} \n";
            //     //
            //     // msg += player.isKiller ? "You are the killer \n" : "You are not the killer \n";
            //     // msg += $"Hand:\n";
            //
            //     // Sammoh - todo fix the hand array.
            //     // var hand = player.handArray;
            //     // for (var i = 0; i < hand.Length; i++)
            //     // {
            //     //     var index = hand[i];
            //     //     var name = m_CardData.AvailableCards[index].name;
            //     //     msg += $"{index + 1}-{name}";
            //     //     msg += i <= hand.Length - 1 ? ", " : "\n";
            //     // }
            //
            //     m_debugText.text += msg;
            //     m_debugText.text += SolutionString();
            // });

        }
        private void RevealDrawPile()
        {
            var msg = $"Draw Pile:{drawPile.Count} \n";
            for (var i = 0; i < drawPile.Count; i++)
            {
                var index = m_allCards.IndexOf(drawPile[i]);
                var name = m_allCards[index].Name;
                msg += $"{index + 1}-{name}";
                msg += i <= drawPile.Count - 1 ? ", " : "\n";
            }

            m_drawPileText.text += msg;
        }
        string SolutionString()
        {
            // get the solution cards.

            var Character = m_solutionCards[0];
            var Weapon =m_solutionCards[1];
            var Motive =m_solutionCards[2];

            var msg =
                $"\nSolution:\nCharacter: {Character.Name} \nWeapon: {Weapon.Name}\nMotive: {Motive.Name}\n";
            return msg;
        }
        
        #endregion

        #region Get Card Info

        #region Get base card from server
        
        
        public void GetCardFromServer(int index, Action<BaseCard> onGet)
        {
            m_onGetCardCallback = onGet;
            GetCard_ServerRpc(index);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetCard_ServerRpc(int index)
        {
            var card = m_allCards[index];
            m_onGetCardCallback?.Invoke(card);
            m_onGetCardCallback = null;
        }
        
        public T GetCard<T>(BaseCard card) where T : BaseCard
        {
            // find a matching card
            var index = GetCardIndex(card);
            // return card as T from index
            return m_allCards[index] as T;
        }
        #endregion

        #region Get card Index From Server
        
        public BaseCard GetCard(int index)
        {
            var card = m_allCards[index];
            return card;
        }
        
        // gets it from the available cards
        private int GetCardIndex<T>(T card) where T : BaseCard
        {
            var foundCard = m_allCards.Find(c => c.CardId == card.CardId);
            return foundCard?.CardId ?? -1;
        }
        
        #endregion
        
        #region Get Cards from Server

        public void GetCardsFromServer(CardTypes type, Action<BaseCard> onComplete)
        {
            m_onGetCardsCallback = onComplete;
            GetCards_ServerRpc(m_LocalId, type);
        }
        
        public void GetCardListFromServer(CardTypes type, Action<BaseCard[]> onComplete, bool includePlayer = false)
        {
            m_onGetCardArrayCallback = onComplete;
            GetCardList_ServerRpc(m_LocalId, type, includePlayer);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetCardList_ServerRpc(ulong id, CardTypes type, bool includePlayer = false)
        {
            switch (type)
            {
                case CardTypes.Character:
                    var characterCards = GetCards<MMCharacterCard>();
                    // remove the caller from the list
                    if (!includePlayer)
                        characterCards = characterCards.Where(c => c != m_playerCharacters[id]).ToArray();
                    
                    GetCardList_ClientRpc(id, characterCards);
                    break;
                case CardTypes.Motive:
                    var motiveCards = GetCards<MMMotiveCard>();
                    GetCardList_ClientRpc(id, motiveCards);
                    break;
                case CardTypes.Weapon:
                    var weaponCards = GetCards<MMWeaponCard>();
                    GetCardList_ClientRpc(id, weaponCards);
                    break;
                case CardTypes.Action:
                    break;
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetCards_ServerRpc(ulong id, CardTypes type)
        {
            switch (type)
            {
                case CardTypes.Character:
                    var characterCards = GetCards<MMCharacterCard>(); 
                    GetCards_ClientRpc(id, characterCards);
                    break;
                case CardTypes.Motive:
                    var motiveCards = GetCards<MMMotiveCard>();
                    GetCards_ClientRpc(id, motiveCards);
                    break;
                case CardTypes.Weapon:
                    var weaponCards = GetCards<MMWeaponCard>();
                    GetCards_ClientRpc(id, weaponCards);
                    break;
                case CardTypes.Action:
                    break;
            }
        }
        
        [ClientRpc]
        private void GetCards_ClientRpc(ulong id, BaseCard[] cards)
        {
            if (id != NetworkManager.Singleton.LocalClientId) return;

            foreach (var card in cards)
            {
                m_onGetCardsCallback?.Invoke(card);
            }
            m_onGetCardsCallback = null;
        }
        
        [ClientRpc]
        private void GetCardList_ClientRpc(ulong id, BaseCard[] cards)
        {
            if (id != NetworkManager.Singleton.LocalClientId) return;
            
            m_onGetCardArrayCallback?.Invoke(cards);
            m_onGetCardArrayCallback = null;
        }


        public T[] GetCards<T>() where T : BaseCard
        {
            var cards = m_allCards.FindAll(c => c is T);

            // remove the cards are not in the player list
            if (typeof(T) == typeof(CharacterCard))
            {
                var playerCharacters = m_playerCharacters.Values.ToList();
                
                // remove the cards are not in the player list
                for (int i = cards.Count - 1; i >= 0; i--)
                {
                    if (cards[i] == playerCharacters[i])
                    {
                        cards.RemoveAt(i);
                    }
                }
            }
            
            
            return cards.Cast<T>().ToArray();
        }
        
        #endregion

        #endregion

        #region Get Player Cards

        // need to pass on the client id to the server to get the cards. 
        public void GetPlayerCards(ulong id, Action<BaseCard[]> onComplete)
        {
            // Debug.LogError("Getting Player Cards for " + id + "");
            _onGetCards = onComplete;
            GetPlayerCardsServerRpc(id);
        }

        [ServerRpc(RequireOwnership = false)]
        private void GetPlayerCardsServerRpc(ulong id)
        {
            if (m_playerCards[id] == null)
            {
                Debug.LogError("Player Cards are null");
                return;
            }

            var playerCards = m_playerCards[id];
            
            // Debug.LogError($"playerCards: {playerCards.Count}");
            
            GetPlayerCardsClientRpc(id, playerCards.ToArray());
        }
        
        [ClientRpc]
        private void GetPlayerCardsClientRpc(ulong id, BaseCard[] cards)
        {
            if (id != m_LocalId) return;
            
            _onGetCards?.Invoke(cards);
            _onGetCards = null;
        }
        
        public void UpdatePlayerCards()
        {
            // each player should have signed up to discard cards.
            // request cards
            // get the cards from each player
            // m_playerDiscardedCards

            var playerList = InGameRunner.Instance.PlayerList;
            
            foreach (var discardedCards in m_playerDiscardedCards)
            {
                var playerData = playerList [discardedCards.Key];
                var discardedCardNames = String.Join(", ", discardedCards.Value.Select(c => c.Name));
                var formattedNames = discardedCards.Value.Count > 1 ?  discardedCardNames : "nothing";
                Debug.LogError($"{playerData.name} has discarded {formattedNames}");

            }

            foreach (var player in playerList)
            {
                DrawCards(player.Value, m_playerDiscardedCards[player.Key].Count);
                m_playerDiscardedCards[player.Key].Clear();
            }

        }
        
        #endregion

        #region Get Role
        
        public void GetPlayerRole(ulong id, Action<RoleData> onComplete)
        {
            _onGetRole = onComplete;
            GetPlayerRoleServerRpc(id);
        }
        
        [ServerRpc (RequireOwnership = false)]
        private void GetPlayerRoleServerRpc(ulong id)
        {
            if (m_PlayerRoleData.Count == 1)
            {
                Debug.LogError("There is only one player");
                return;
            }
            
            var playerRole = m_PlayerRoleData[id];
            GetPlayerRoleClientRpc(id,playerRole);
        }
        
        [ClientRpc]
        private void GetPlayerRoleClientRpc(ulong id, RoleData role)
        {
            if (id != m_LocalId) return;

            _onGetRole?.Invoke(role);
            _onGetRole = null;
        }
        
        #endregion

        #region Get Player Characters

        public void GetPlayerCharacter(ulong id, Action<ulong, BaseCard> onComplete)
        {
            _onGetCharacter = onComplete;
            GetPlayerCharacters_ServerRpc(id);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetPlayerCharacters_ServerRpc(ulong id)
        {
            var playerCharacter = m_playerCharacters[id];
            GetPlayerCharacters_ClientRpc(id, playerCharacter);
        }
        
        [ClientRpc]
        private void GetPlayerCharacters_ClientRpc(ulong id, BaseCard card)
        {
            if (id != m_LocalId) return;

            _onGetCharacter?.Invoke(id, card);
            _onGetCharacter = null;
        }

        #endregion

        #region Debug Content
        
        public void InitDebugDrawPile()
        {
            // list all of the cards in a string
            var msg = $"Draw Pile:{drawPile.Count} \n";
            for (var i = 0; i < drawPile.Count; i++)
            {
                var index = m_allCards.IndexOf(drawPile[i]);
                var name = m_allCards[index].Name;
                msg += $"{index + 1}-{name}";
                msg += i <= drawPile.Count - 1 ? ", " : "\n";
            }
            
            GetDebugDrawpile_ClientRpc(msg);
        }

        [ClientRpc]
        private void GetDebugDrawpile_ClientRpc(string msg)
        {
            Debug.LogError(msg);
            debugCards = msg;
        }
        
        public string debugCards = "";

        #endregion

        #region Handle Suggestions
        
        private Dictionary<ulong, Suggestion> suggestions = new Dictionary<ulong, Suggestion>();
        // private List<int> solution = new List<int>();
        public int SuggestionCount => suggestions.Count;

        public bool AddSuggestion(ulong caller, BaseCard[] cards)
        {
            var suggestion = new Suggestion(cards[0].CardId, cards[1].CardId, cards[2].CardId);

            if (suggestions.ContainsKey(caller))
            {
                Debug.LogError("Already made a suggestion.");
                return true;
            };
            
            suggestions.Add(caller, suggestion);

            return false;
        }

        public SuggestionResults CheckSolution()
        {
            if (suggestions.Count < 1)
            {
                return null;
            }
            
            var winnerID = 0UL;
            var isKiller = false;
            var selectedPlayer = "";
            var bypassGroup = false;
            
            var solution = new List<int>
            {
                m_solutionCards[0].CardId,
                m_solutionCards[1].CardId,
                m_solutionCards[2].CardId
            };

            // determine if any of the suggestions match the solution
            foreach (var suggestion in suggestions)
            {
                var entry1Match = suggestion.Value.Entry1 == solution[0];
                var entry2Match = suggestion.Value.Entry2 == solution[1];
                var entry3Match = suggestion.Value.Entry3 == solution[2];

                if (entry1Match && entry2Match && entry3Match)
                {
                    winnerID = suggestions.FirstOrDefault(s => s.Value.Entry1 == solution[0] && s.Value.Entry2 == solution[1] && s.Value.Entry3 == solution[2]).Key;
                    isKiller = m_PlayerRoleData[winnerID].role == Role.Killer;
                    bypassGroup = true;
                }
            }
            
            // As a backup, if no one has the solution, then we need to determine if the most common entry is the solution
            
            // get the most common entry count
            var entryCount1 = suggestions.Select(s => s.Value.Entry1).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Count();
            var entryCount2 = suggestions.Select(s => s.Value.Entry2).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Count();
            var entryCount3 = suggestions.Select(s => s.Value.Entry3).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Count();
            
            // get the most common entry
            var entry1 = suggestions.Select(s => s.Value.Entry1).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
            var entry2 = suggestions.Select(s => s.Value.Entry2).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
            var entry3 = suggestions.Select(s => s.Value.Entry3).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;

            // get the most common value from each entry in the dictionary
            List<int> commonValues = new List<int>
            {
                entry1,
                entry2,
                entry3
            };
            
            Debug.LogError($"Common Values: {commonValues[0]}, {commonValues[1]}, {commonValues[2]}");
            Debug.LogError($"Solution: {solution[0]}, {solution[1]}, {solution[2]}");

            var wasCommonSuggestions = commonValues.SequenceEqual(solution);
            var answer = wasCommonSuggestions ? "Correct!" : "Incorrect!";
            Debug.LogError(answer);
            
            var characterName = GetCard(commonValues[0]).Name;
            var motiveName = GetCard(commonValues[1]).Name;
            var weaponName = GetCard(commonValues[2]).Name;

            var wasGroup = wasCommonSuggestions &! bypassGroup;
            
            // TODO: get the winner id, and if it's the killer, then show the killer end screen, otherwise show the winner end screen
            var suggestionResults = new SuggestionResults(wasCommonSuggestions, characterName, motiveName, weaponName, new []{entryCount1, entryCount2, entryCount3}, winnerID, isKiller, wasGroup, selectedPlayer);
            
            return suggestionResults;
        }

        public void ClearSuggestions()
        {
            suggestions.Clear();
        }
        
        #endregion
        
    }
    
    public class Suggestion
    {
        public int Entry1 { get; set; }
        public int Entry2 { get; set; }
        public int Entry3 { get; set; }

        public Suggestion(int entry1, int entry2, int entry3)
        {
            Entry1 = entry1;
            Entry2 = entry2;
            Entry3 = entry3;
        }
    }

    public class SuggestionResults : INetworkSerializable
    {
        public bool IsCorrect;
        public string CharacterName;
        public string WeaponName;
        public string MotiveName;
        public int[] Count;
        public ulong WinnerId;
        public bool IsKiller;
        public bool WasGroup;
        public string SelectedPlayer;
        
        public SuggestionResults()
        {
            
        }

        public SuggestionResults(bool isCorrect,
            string character,
            string motive,
            string weapon,
            int[] count,
            ulong winnerId,
            bool isKiller,
            bool wasGroup,
            string selectedPlayer)
        {
            IsCorrect = isCorrect;
            CharacterName = character;
            MotiveName = motive;
            WeaponName = weapon;
            Count = count;
            WinnerId = winnerId;
            IsKiller = isKiller;
            WasGroup = wasGroup;
            SelectedPlayer = selectedPlayer;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref IsCorrect);
            serializer.SerializeValue(ref CharacterName);
            serializer.SerializeValue(ref MotiveName);
            serializer.SerializeValue(ref WeaponName);
            serializer.SerializeValue(ref Count);
            serializer.SerializeValue(ref WinnerId);
            serializer.SerializeValue(ref IsKiller);
            serializer.SerializeValue(ref WasGroup);
            serializer.SerializeValue(ref SelectedPlayer);

            int  countLength = 0;

            if (!serializer.IsReader)
            {
                countLength = Count.Length;
            }
            
            serializer.SerializeValue(ref countLength);
            
            if (serializer.IsReader)
            {
                Count = new int[countLength];
            }

            for (int i = 0; i < countLength; i++)
            {
                serializer.SerializeValue(ref Count[i]);
            }
        }
    }

}