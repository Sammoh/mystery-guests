using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGame.GameData.Cards;
using UnityEngine;

namespace MurderMystery
{
    public class IntentProcessing : MonoBehaviour
    {
        public static IntentProcessing Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
        
        readonly Dictionary<IntentType, IIntentProcessor> intentProcessors = new Dictionary<IntentType, IIntentProcessor>()
        {
            { IntentType.None, new Intent_None() },
            // Player Selection
            { IntentType.SelectCard, new Intent_SelectAnyCard() },
            { IntentType.SelectPlayer, new Intent_SelectPlayer()},
            // { IntentType.SelectRandomPlayer, new Intent_SelectPlayer()},
            { IntentType.SelectMotive, new Intent_SelectClue()},
            { IntentType.SelectWeapon, new Intent_SelectClue()},
            { IntentType.SelectCharacter, new Intent_SelectClue()},
            { IntentType.SelectRandomClue, new Intent_SelectRandomClue()},
            { IntentType.SelectAction, new Intent_SelectAction()},
            { IntentType.SelectAnyClue, new Intent_SelectAnyClue()},
            { IntentType.SelectThisCard, new Intent_SelectThisCard()},
            { IntentType.SelectSelf, new Intent_SelectSelf()},

            // Player Actions
            { IntentType.ShowPlayer, new Intent_ShowPlayer()},
            { IntentType.ShowAllPlayers, new Intent_ShowAllPlayers()},
            { IntentType.SwitchCards, new Intent_SwitchCards()},
            { IntentType.KillPlayer, new Intent_KillPlayer()},
            { IntentType.TakeCard, new Intent_TakeCard()},
            { IntentType.DrawCard, new Intent_DrawCard()},
            { IntentType.CollectCharacter, new Intent_CollectClue()},
            { IntentType.CollectMotive, new Intent_CollectClue()},
            { IntentType.CollectWeapon, new Intent_CollectClue()},
            { IntentType.CollectRandomClue, new Intent_CollectRandomClue()},
            { IntentType.GiveCard, new Intent_GiveCard()},
            { IntentType.MakeSuggestion, new Intent_MakeSuggestion()},

            // Reactions
            { IntentType.Falsify, new Intent_Falsify()},
            { IntentType.ProtectPlayer, new Intent_ProtectPlayer()},
        };
        
        // Dictionary<ulong, Coroutine> playerCardProcesses = new Dictionary<ulong, Coroutine>();

        private Coroutine currentCardProcess;

        
        // this is processed on the server. Need to the client to process the card intent
        public void ProcessCardIntent(ulong id, BaseCard selectedCard, Action<ulong, BaseCard> onComplete = null, Action<ulong> onFail = null)
        {
            if (currentCardProcess != null)
            {
                StopCoroutine(currentCardProcess);
                currentCardProcess = null;
            }
            currentCardProcess = StartCoroutine(Process_Co(id, selectedCard, onComplete, onFail));
            // playerCardProcesses.Add(id, StartCoroutine(Process_Co(id, selectedCard, onComplete, onFail)));

        }

        private IEnumerator Process_Co(ulong id, BaseCard selectedCard, Action<ulong, BaseCard> onComplete = null, Action<ulong> onFail = null)
        {
            var playerCards = NewCardSelector.Instance.PlayerCards[id].ToList();
            var matchedCard = playerCards.Find(card => card.CardId == selectedCard.CardId);
            
            var cardIntent = new CardIntent()
            {
                userId = id,
                cardId = selectedCard.CardId,
            };
            
            switch (matchedCard)
            {
                case null:
                    yield break;
                case MMActionCard actionCard:
                {
                    foreach (var instruction in actionCard.Instructions)
                    {
                        Debug.LogError($"Performing instruction: {instruction}");

                        // Look up the appropriate IIntentProcessor instance based on the IntentType
                        var intentProcessor = intentProcessors[instruction];
                        
                        cardIntent.instruction = instruction;
                        cardIntent.hasPassed = false;

                        // Call the ProcessIntent method on the IIntentProcessor instance, passing in the CardIntent
                        intentProcessor.ProcessIntent(id, cardIntent, intent =>
                        {
                            Debug.LogError($"The card intent has passed: {cardIntent.hasPassed}");
                        }, () => onFail?.Invoke(id));

                        yield return new WaitUntil(() => cardIntent.hasPassed);
                    }
                    
                    onComplete?.Invoke(id, selectedCard);

                    break;
                }
                case MMClueCard clueCard:
                {
                    foreach (var instruction in clueCard.Instructions)
                    {
                        // Debug.LogError($"Performing instruction: {instruction}");

                        // Look up the appropriate IIntentProcessor instance based on the IntentType
                        var intentProcessor = intentProcessors[instruction];
                    
                        cardIntent.instruction = instruction;
                        cardIntent.hasPassed = false;

                        // Call the ProcessIntent method on the IIntentProcessor instance, passing in the CardIntent
                        intentProcessor.ProcessIntent(id, cardIntent, selectedIntent =>
                        {
                            cardIntent = selectedIntent;
                            // Debug.LogError($"Selected intent: {selectedIntent.hasPassed}");
                        }, () => onFail?.Invoke(id));
                        yield return new WaitUntil(() => cardIntent.hasPassed);
                    }
                    
                    onComplete?.Invoke(id, selectedCard);

                    break;
                }
            }
        }
        
        Coroutine currentSuggestionProcess;
        public void ProcessSuggestion(ulong id, Action<ulong> onComplete = null, Action<ulong> onFailed = null)
        {
            if (currentSuggestionProcess != null)
            {
                StopCoroutine(currentSuggestionProcess);
                currentSuggestionProcess = null;
            }
            
            currentSuggestionProcess = StartCoroutine(ProcessSuggestion_Co(id, onComplete, onFailed));
            // playerCardProcesses.Add(id, StartCoroutine(ProcessSuggestion_Co(id, onComplete, onFailed)));
        }

        private IEnumerator ProcessSuggestion_Co(ulong id, Action<ulong> onComplete = null, Action<ulong> onFailed = null)
        {
            var cardIntent = new CardIntent()
            {
                userId = id,
                instruction = IntentType.MakeSuggestion,
                hasPassed = false,
            };
            
            // Look up the appropriate IIntentProcessor instance based on the IntentType
            var intentProcessor = intentProcessors[cardIntent.instruction];
            
            // Call the ProcessIntent method on the IIntentProcessor instance, passing in the CardIntent
            intentProcessor.ProcessIntent(id, cardIntent, intent =>
                {
                    // Debug.LogError($"The card intent has passed: {cardIntent.hasPassed}");
                    onComplete?.Invoke(id);
                },
                () =>
                {
                    // Debug.LogError($"Failed to complete");
                    onFailed?.Invoke(id);
                });

            yield return new WaitUntil(() => cardIntent.hasPassed);
        }

        private Coroutine currentAbilityProcess;
        public void ProcessAbility(ulong id, Action onComplete = null, Action onFail = null)
        {
            if (currentAbilityProcess != null)
            {
                StopCoroutine(currentAbilityProcess);
                currentAbilityProcess = null;
            }
            currentAbilityProcess = StartCoroutine(ProcessAbility_Co(id, onComplete, onFail));
            // playerCardProcesses.Add(id, currentAbility);
        }
        
        private IEnumerator ProcessAbility_Co(ulong id, Action onComplete = null, Action onFail = null)
        {
            var role = NewCardSelector.Instance.RoleDataList[id];
            // Debug.LogError($"Processing ability for {role.role.ToString()}");

            var cardIntent = new CardIntent()
            {
                userId = id,
            };
            
            foreach (var instruction in role.instructions)
            {
                // Debug.LogError($"Performing: {instruction}");

                cardIntent.instruction = instruction;
                cardIntent.hasPassed = false;
                
                // Look up the appropriate IIntentProcessor instance based on the IntentType
                var intentProcessor = intentProcessors[cardIntent.instruction];
            
                // Call the ProcessIntent method on the IIntentProcessor instance, passing in the CardIntent
                intentProcessor.ProcessIntent(id, cardIntent, intent =>
                    {
                        // Debug.LogError($"The card intent has passed: {cardIntent.hasPassed}");
                    },
                    () =>
                    {
                        // Debug.LogError($"Failed to complete");
                        onFail?.Invoke();
                    });

                yield return new WaitUntil(() => cardIntent.hasPassed);
            }
            
            onComplete?.Invoke();
        }
        
        // Post Round Processes

        private readonly Dictionary<IntentType, IPostRoundIntent> postIntentProcessors =
            new Dictionary<IntentType, IPostRoundIntent>()
            {
                { IntentType.None, new IPostRound_None()},
                { IntentType.KillPlayer, new IPostRound_Eliminate()},
                { IntentType.ProtectPlayer, new IPostRound_Protect()},
            };
        
        public void ProcessPostRound(RoleData roleData)
        {
            var id = roleData.playerId;
            var intent = roleData.instructions[^1];
            var intentProcessor = postIntentProcessors[intent];

            intentProcessor.ProcessIntent(id, roleData);
        }


    }
}
