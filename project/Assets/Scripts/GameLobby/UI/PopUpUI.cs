using System.Text;
using TMPro;
using UnityEngine;

namespace LobbyRelaySample
{
    /// <summary>
    /// Controls a pop-up message that lays over the rest of the UI, with a button to dismiss. Used for displaying player-facing error messages.
    /// </summary>
    public class PopUpUI : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup m_canvasGroup;
        [SerializeField]
        private TMP_InputField m_popupText;
        [SerializeField]
        private CanvasGroup m_buttonVisibility;
        private float m_buttonVisibilityTimeout = -1;
        private StringBuilder m_currentText = new StringBuilder();

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// If the pop-up is not currently visible, display it. If it is, append the incoming text to the existing pop-up.
        /// </summary>
        public void ShowPopup(string newText)
        {
            if (!gameObject.activeSelf)
            {   
                m_canvasGroup.alpha = 1;
                m_currentText.Clear();
                gameObject.SetActive(true);
            }
            m_canvasGroup.alpha = 1;
            m_currentText.AppendLine(newText);
            m_popupText.SetTextWithoutNotify(m_currentText.ToString());
            DisableButton();
        }
        
        public void ShowPopup()
        {
            if (!gameObject.activeSelf)
            {   
                m_canvasGroup.alpha = 1;
                m_currentText.Clear();
                gameObject.SetActive(true);
            }
            m_canvasGroup.alpha = 1;
            // m_currentText.AppendLine(newText);
            // m_popupText.SetTextWithoutNotify(m_currentText.ToString());
            DisableButton();
        }

        private void DisableButton()
        {
            m_buttonVisibilityTimeout = 0.5f; // Briefly prevent the popup from being dismissed, to ensure the player doesn't accidentally click past it without seeing it.
            m_buttonVisibility.alpha = 0.5f;
            m_buttonVisibility.interactable = false;
        }
        private void ReenableButton()
        {
            m_buttonVisibility.alpha = 1;
            m_buttonVisibility.interactable = true;
        }

        private void Update()
        {
            if (m_buttonVisibilityTimeout >= 0 && m_buttonVisibilityTimeout - Time.deltaTime < 0)
                ReenableButton();
            m_buttonVisibilityTimeout -= Time.deltaTime;
        }

        public void ClearPopup()
        {
            m_canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}
