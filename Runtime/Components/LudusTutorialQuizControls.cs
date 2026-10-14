using System;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Controla somente o miniquiz apresentado pelo laboratório tutorial.
    /// Não avalia regras do jogo consumidor nem registra telemetria.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialQuizControls : MonoBehaviour
    {
        [SerializeField]
        private Button correctButton;

        [SerializeField]
        private Button incorrectButton;

        [SerializeField]
        private Button correctImageButton;

        [SerializeField]
        private Button incorrectImageButton;

        [SerializeField]
        private Text feedbackLabel;

        [SerializeField]
        [Range(0, 2)]
        private int currentStage;

        private bool listenersRegistered;

        public bool IsCompleted => currentStage >= 2;

        public string PendingMessage => currentStage == 0
            ? "Responda quanto é 1 + 1."
            : currentStage == 1
                ? "Agora clique na imagem que representa duas formas."
                : "Quiz concluído. Você já pode avançar.";

        public event Action StateChanged;

        public void Configure(
            Button correct,
            Button incorrect,
            Button correctImage,
            Button incorrectImage,
            Text feedback
        )
        {
            RemoveListeners();
            correctButton = correct;
            incorrectButton = incorrect;
            correctImageButton = correctImage;
            incorrectImageButton = incorrectImage;
            feedbackLabel = feedback;
            currentStage = 0;
            if (Application.isPlaying && isActiveAndEnabled)
            {
                RegisterListeners();
            }

            ApplyState();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                RegisterListeners();
            }

            ApplyState();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void RegisterListeners()
        {
            if (listenersRegistered)
            {
                return;
            }

            correctButton?.onClick.AddListener(HandleCorrectAnswer);
            incorrectButton?.onClick.AddListener(HandleIncorrectAnswer);
            correctImageButton?.onClick.AddListener(HandleCorrectImageAnswer);
            incorrectImageButton?.onClick.AddListener(HandleIncorrectImageAnswer);
            listenersRegistered = true;
        }

        private void RemoveListeners()
        {
            if (!listenersRegistered)
            {
                return;
            }

            correctButton?.onClick.RemoveListener(HandleCorrectAnswer);
            incorrectButton?.onClick.RemoveListener(HandleIncorrectAnswer);
            correctImageButton?.onClick.RemoveListener(HandleCorrectImageAnswer);
            incorrectImageButton?.onClick.RemoveListener(HandleIncorrectImageAnswer);
            listenersRegistered = false;
        }

        private void HandleCorrectAnswer()
        {
            if (currentStage != 0)
            {
                return;
            }

            currentStage = 1;
            SetFeedback(
                "Correto: 1 + 1 = 2. Agora conclua o desafio visual.",
                true
            );
            ApplyState();
            StateChanged?.Invoke();
        }

        private void HandleIncorrectAnswer()
        {
            if (currentStage != 0)
            {
                return;
            }

            SetFeedback("Ainda não. Tente somar uma unidade a outra.", false);
            StateChanged?.Invoke();
        }

        private void HandleCorrectImageAnswer()
        {
            if (currentStage != 1)
            {
                return;
            }

            currentStage = 2;
            SetFeedback(
                "Você concluiu as alternativas textual e visual.",
                true
            );
            ApplyState();
            StateChanged?.Invoke();
        }

        private void HandleIncorrectImageAnswer()
        {
            if (currentStage != 1)
            {
                return;
            }

            SetFeedback(
                "Essa opção mostra apenas uma forma. Escolha a imagem com duas formas.",
                false
            );
            StateChanged?.Invoke();
        }

        private void ApplyState()
        {
            if (correctButton != null)
            {
                correctButton.interactable = currentStage == 0;
            }

            if (incorrectButton != null)
            {
                incorrectButton.interactable = currentStage == 0;
            }

            if (correctImageButton != null)
            {
                correctImageButton.interactable = currentStage == 1;
            }

            if (incorrectImageButton != null)
            {
                incorrectImageButton.interactable = currentStage == 1;
            }

            if (feedbackLabel != null && string.IsNullOrWhiteSpace(feedbackLabel.text))
            {
                SetFeedback("Escolha a resposta para 1 + 1.", false);
            }
        }

        private void SetFeedback(string message, bool success)
        {
            if (feedbackLabel == null)
            {
                return;
            }

            feedbackLabel.text = message;
            feedbackLabel.color = success
                ? new Color(0.52f, 0.86f, 0.76f)
                : new Color(0.92f, 0.72f, 0.38f);
        }
    }
}
