using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaShooter.Upgrades
{
    public class UpgradeSelectionUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private TMP_Text[] choiceTitleTexts;
        [SerializeField] private TMP_Text[] choiceDescriptionTexts;

        private UpgradeSelectionController controller;
        private UpgradeData[] currentChoices;

        private void Awake()
        {
            AutoWireIfNeeded();

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int choiceIndex = i;
                choiceButtons[i].onClick.AddListener(() => SelectChoice(choiceIndex));
            }
        }

        public void Initialize(UpgradeSelectionController newController)
        {
            controller = newController;
        }

        public void ShowChoices(int completedWave, UpgradeData[] choices)
        {
            currentChoices = choices;
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = $"Wave {completedWave} cleared - choose upgrade";
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                UpgradeData choice = choices != null && i < choices.Length ? choices[i] : null;
                bool hasChoice = choice != null;

                choiceButtons[i].gameObject.SetActive(hasChoice);

                if (!hasChoice)
                {
                    continue;
                }

                TMP_Text titleReference = choiceTitleTexts != null && i < choiceTitleTexts.Length ? choiceTitleTexts[i] : null;
                TMP_Text descriptionReference = choiceDescriptionTexts != null && i < choiceDescriptionTexts.Length ? choiceDescriptionTexts[i] : null;

                if (titleReference != null)
                {
                    if (descriptionReference != null)
                    {
                        titleReference.text = choice.Title;
                    }
                    else
                    {
                        titleReference.text = $"{choice.Title}\n<size=65%>{choice.Description}</size>";
                    }
                }

                if (descriptionReference != null)
                {
                    descriptionReference.text = choice.Description;
                }
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void AutoWireIfNeeded()
        {
            if (choiceButtons == null || choiceButtons.Length == 0 || HasMissingButtonReference())
            {
                choiceButtons = GetComponentsInChildren<Button>(includeInactive: true);
            }

            if (choiceButtons == null)
            {
                return;
            }

            if (choiceTitleTexts == null || choiceTitleTexts.Length == 0 || HasMissingTextReference(choiceTitleTexts))
            {
                choiceTitleTexts = new TMP_Text[choiceButtons.Length];
            }

            if (choiceDescriptionTexts == null || choiceDescriptionTexts.Length == 0 || HasMissingTextReference(choiceDescriptionTexts))
            {
                choiceDescriptionTexts = new TMP_Text[choiceButtons.Length];
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                TMP_Text[] texts = choiceButtons[i].GetComponentsInChildren<TMP_Text>(includeInactive: true);

                if ((choiceTitleTexts[i] == null) && texts.Length > 0)
                {
                    choiceTitleTexts[i] = texts[0];
                }

                if ((choiceDescriptionTexts[i] == null) && texts.Length > 1)
                {
                    choiceDescriptionTexts[i] = texts[1];
                }
            }
        }

        private bool HasMissingButtonReference()
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMissingTextReference(TMP_Text[] textReferences)
        {
            for (int i = 0; i < textReferences.Length; i++)
            {
                if (textReferences[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        private void SelectChoice(int index)
        {
            if (controller == null || currentChoices == null || index < 0 || index >= currentChoices.Length)
            {
                return;
            }

            controller.SelectUpgrade(currentChoices[index]);
        }
    }
}
