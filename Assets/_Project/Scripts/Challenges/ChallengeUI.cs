using System;
using System.Collections.Generic;
using MathDungeon.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathDungeon.Challenges
{
    /// <summary>
    /// Presentation layer for a challenge (SRS 1.5): shows the prompt, collects the
    /// answer, and displays hints and explanations. It knows nothing about whether
    /// an answer is right - it only reports what the player submitted.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChallengeUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text topicLabel;
        [SerializeField] private TMP_Text promptLabel;

        [Header("Multiple choice")]
        [SerializeField] private GameObject optionsGroup;
        [SerializeField] private List<Button> optionButtons = new List<Button>();

        [Header("Typed answer")]
        [SerializeField] private GameObject inputGroup;
        [SerializeField] private TMP_InputField answerInput;
        [SerializeField] private Button submitButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private Button continueButton;
        [SerializeField] private Color hintColor = new Color(1f, 0.78f, 0.31f);
        [SerializeField] private Color explanationColor = new Color(0.55f, 0.9f, 0.55f);

        /// <summary>Raised with whatever the player answered; validation happens elsewhere.</summary>
        public event Action<string> AnswerSubmitted;

        /// <summary>Raised when the player dismisses the explanation after solving.</summary>
        public event Action ContinueRequested;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                int index = i;
                optionButtons[i].onClick.AddListener(() => SubmitOption(index));
            }

            if (submitButton != null)
            {
                submitButton.onClick.AddListener(SubmitTypedAnswer);
            }

            if (answerInput != null)
            {
                answerInput.onSubmit.AddListener(_ => SubmitTypedAnswer());
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() => ContinueRequested?.Invoke());
            }

            Hide();
        }

        /// <summary>Opens the panel for a challenge, laid out for its question type.</summary>
        public void Show(Challenge challenge)
        {
            if (challenge == null)
            {
                return;
            }

            bool isMultipleChoice = challenge.questionType == QuestionType.MultipleChoice;

            SetActive(panel, true);
            SetText(topicLabel, challenge.topic);
            SetText(promptLabel, challenge.promptText);
            SetText(feedbackLabel, string.Empty);
            SetActive(continueButton != null ? continueButton.gameObject : null, false);

            SetActive(optionsGroup, isMultipleChoice);
            SetActive(inputGroup, !isMultipleChoice);

            if (isMultipleChoice)
            {
                PopulateOptions(challenge.options);
            }
            else if (answerInput != null)
            {
                answerInput.text = string.Empty;
                answerInput.contentType = challenge.questionType == QuestionType.NumericalInput
                    ? TMP_InputField.ContentType.DecimalNumber
                    : TMP_InputField.ContentType.Standard;
                answerInput.ActivateInputField();
            }
        }

        /// <summary>Shows the conceptual hint after a wrong answer (FR7) and re-arms input.</summary>
        public void ShowHint(string hint)
        {
            SetText(feedbackLabel, hint, hintColor);
            SetInteractable(true);

            if (answerInput != null && inputGroup != null && inputGroup.activeSelf)
            {
                answerInput.text = string.Empty;
                answerInput.ActivateInputField();
            }
        }

        /// <summary>
        /// Shows the worked explanation after a correct answer and locks the inputs,
        /// leaving only Continue.
        /// </summary>
        public void ShowExplanation(string explanation)
        {
            SetText(feedbackLabel, explanation, explanationColor);
            SetInteractable(false);
            SetActive(continueButton != null ? continueButton.gameObject : null, true);
        }

        public void Hide()
        {
            SetActive(panel, false);
        }

        private void PopulateOptions(string[] options)
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                bool hasOption = options != null && i < options.Length;
                optionButtons[i].gameObject.SetActive(hasOption);
                optionButtons[i].interactable = true;

                if (hasOption)
                {
                    TMP_Text label = optionButtons[i].GetComponentInChildren<TMP_Text>();
                    if (label != null)
                    {
                        label.text = options[i];
                    }
                }
            }
        }

        private void SubmitOption(int index)
        {
            if (index < 0 || index >= optionButtons.Count)
            {
                return;
            }

            TMP_Text label = optionButtons[index].GetComponentInChildren<TMP_Text>();
            AnswerSubmitted?.Invoke(label != null ? label.text : string.Empty);
        }

        private void SubmitTypedAnswer()
        {
            if (answerInput == null)
            {
                return;
            }

            string answer = answerInput.text;
            if (string.IsNullOrWhiteSpace(answer))
            {
                return;
            }

            AnswerSubmitted?.Invoke(answer);
        }

        private void SetInteractable(bool interactable)
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                optionButtons[i].interactable = interactable;
            }

            if (submitButton != null)
            {
                submitButton.interactable = interactable;
            }

            if (answerInput != null)
            {
                answerInput.interactable = interactable;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetText(TMP_Text label, string value, Color color)
        {
            if (label != null)
            {
                label.text = value;
                label.color = color;
            }
        }
    }
}
