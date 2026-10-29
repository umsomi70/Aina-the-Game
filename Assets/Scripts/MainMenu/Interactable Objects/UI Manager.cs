using UnityEngine;
using TMPro; // Required for TextMeshProUGUI

namespace Gidigbo.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("TextMeshPro UI References")]
        [SerializeField] private TextMeshProUGUI lookPromptText;         // e.g., "Press E to Inspect"
        [SerializeField] private TextMeshProUGUI exitPromptText;         // e.g., "Press E to Exit"
        [SerializeField] private TextMeshProUGUI scrollPromptText;       // e.g., "Use A/D to cycle levels"
        [SerializeField] private TextMeshProUGUI levelIndicatorText;     // e.g., "Level 1: The Ring"

        private void Awake()
        {
            HideAll();
        }

        public void HideAll()
        {
            if (lookPromptText) lookPromptText.gameObject.SetActive(false);
            if (exitPromptText) exitPromptText.gameObject.SetActive(false);
            if (scrollPromptText) scrollPromptText.gameObject.SetActive(false);
            if (levelIndicatorText) levelIndicatorText.gameObject.SetActive(false);
        }

        public void ShowLookPrompt(string promptMessage)
        {
            if (lookPromptText != null)
            {
                lookPromptText.text = promptMessage;
                lookPromptText.gameObject.SetActive(true);
            }
        }

        public void HideLookPrompt()
        {
            if (lookPromptText) lookPromptText.gameObject.SetActive(false);
        }

        public void ShowInspectionUI(bool isMapSelector, string levelName = "")
        {
            HideLookPrompt();

            if (exitPromptText != null)
            {
                exitPromptText.text = "Press E / Right Click to Exit";
                exitPromptText.gameObject.SetActive(true);
            }

            if (isMapSelector)
            {
                if (scrollPromptText != null)
                {
                    scrollPromptText.text = "Use A / D to Cycle Levels";
                    scrollPromptText.gameObject.SetActive(true);
                }

                UpdateLevelIndicator(levelName);
            }
        }

        public void UpdateLevelIndicator(string levelName)
        {
            if (levelIndicatorText != null)
            {
                levelIndicatorText.text = levelName;
                levelIndicatorText.gameObject.SetActive(true);
            }
        }

        public void HideInspectionUI()
        {
            if (exitPromptText) exitPromptText.gameObject.SetActive(false);
            if (scrollPromptText) scrollPromptText.gameObject.SetActive(false);
            if (levelIndicatorText) levelIndicatorText.gameObject.SetActive(false);
        }
    }
}