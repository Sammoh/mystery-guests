using System;
using UnityEngine;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// Handles any visual tasks for running the NGO minigame's intro and outro.
    /// </summary>
    public class IntroOutroRunner : MonoBehaviour
    {
        [SerializeField]
        InGameRunner m_inGameRunner;
        [SerializeField] Animator m_animator;
        Action m_onIntroComplete, m_onOutroComplete, m_onSuggestionComplete, m_onEliminationComplete;
        
        // game start / stop.
        private static readonly int Intro = Animator.StringToHash("DoIntro");
        private static readonly int Outro = Animator.StringToHash("DoOutro");
        private static readonly int NewRound = Animator.StringToHash("DoNewRound");
        
        // round end.
        // note, this could be either the killer or one of the innocents.
        private static readonly int PlayerCorrect = Animator.StringToHash("DoPlayerCorrect");
        private static readonly int GroupCorrect = Animator.StringToHash("DoGroupCorrect");
        private static readonly int DoElimination = Animator.StringToHash("DoElimination");

        // backup round end.
        private static readonly int MadeSuggestions = Animator.StringToHash("DoMakeSuggestions");
        private static readonly int NoSuggestions = Animator.StringToHash("DoNoSuggestions");
        private static readonly int NoElimination = Animator.StringToHash("NullElimination");

        #region Do Animations

        #region Game Start / Stop
        public void DoIntro(Action onIntroComplete)
        {
            m_onIntroComplete = onIntroComplete;
            m_animator.SetTrigger(Intro);
        }
        public void DoOutro(Action onOutroComplete)
        {
            m_onOutroComplete = onOutroComplete;
            m_animator.SetTrigger(Outro);
        }
        
        public void DoNewRound(Action onNewRoundComplete)
        {
            m_onIntroComplete = onNewRoundComplete;
            m_animator.SetTrigger(NewRound);
        }
        #endregion

        /// <summary>
        /// Show the results of the round.
        /// </summary>
        /// <param name="onOutroComplete"></param>
        public void DoPlayerCorrect(Action onOutroComplete)
        {
            m_onSuggestionComplete = onOutroComplete;
            m_animator.SetTrigger(PlayerCorrect);
        }
        
        /// <summary>
        /// Show the results of the round.
        /// </summary>
        /// <param name="onOutroComplete"></param>
        public void DoGroupCorrect(Action onOutroComplete)
        {
            m_onSuggestionComplete = onOutroComplete;
            m_animator.SetTrigger(GroupCorrect);
        }
        
        public void DoMakeSuggestions(Action onOutroComplete)
        {
            m_onSuggestionComplete = onOutroComplete;
            m_animator.SetTrigger(MadeSuggestions);
        }
        
        /// <summary>
        /// No Suggestions were made.
        /// </summary>
        /// <param name="onOutroComplete"></param>
        public void DoNoSuggestions(Action onOutroComplete)
        {
            m_onSuggestionComplete = onOutroComplete;
            m_animator.SetTrigger(NoSuggestions);
        }
        
        /// <summary>
        /// Reveal which player was eliminated.
        /// </summary>
        /// <param name="onEliminationComplete"></param>
        public void DoPlayerEliminated(Action onEliminationComplete)
        {
            m_onEliminationComplete = onEliminationComplete;
            m_animator.SetTrigger(DoElimination);
            
        }
        
        /// <summary>
        /// No player was selected for elimination.
        /// </summary>
        /// <param name="onComplete"></param>
        public void DoNoElimination(Action onComplete)
        {
            m_onEliminationComplete = onComplete;
            m_animator.SetTrigger(NoElimination);
            
        }
        
        /// <summary>
        /// Player was saved from elimination.
        /// </summary>
        /// <param name="onSavedComplete"></param>
        public void DoPlayerSaved(Action onSavedComplete)
        {
            m_onEliminationComplete = onSavedComplete;
            m_animator.SetTrigger(NoElimination);
            
        }
        
        #endregion

        #region Animation Events
        #region Game Start / Stop
        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnIntroComplete()
        {
            m_animator.ResetTrigger(Intro);
            m_onIntroComplete?.Invoke();
        }
        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnOutroComplete()
        {
            m_animator.ResetTrigger(Outro);
            m_onOutroComplete?.Invoke();
        }
        
        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnNewRoundComplete()
        {
            m_animator.ResetTrigger(NewRound);
            m_onIntroComplete?.Invoke();
        }
        #endregion
        
        // SAMMOH TODO: These are events that
        
        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnSuggestionComplete()
        {
            m_animator.ResetTrigger(GroupCorrect);
            m_onSuggestionComplete?.Invoke();
        }
        
        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnEliminationComplete()
        {
            m_animator.ResetTrigger(DoElimination);
            m_onEliminationComplete?.Invoke();
        }
        #endregion

    }
}
