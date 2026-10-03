using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    public enum LudusConfiguredOutcome
    {
        Correct,
        Incorrect,
    }

    /// <summary>
    /// Conecta uma alternativa de UI ao resultado semântico LUDUS sem
    /// remover ou substituir os listeners que já pertencem ao jogo.
    /// </summary>
    [AddComponentMenu("LUDUS/Adaptadores/Resultado de botão")]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(LudusSemanticBridge))]
    public sealed class LudusButtonOutcomeAdapter : MonoBehaviour
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private LudusConfiguredOutcome configuredOutcome =
            LudusConfiguredOutcome.Correct;

        [SerializeField]
        private string answerDisplayName = string.Empty;

        [SerializeField]
        private string expectedAnswerDisplayName = "Resposta correta";

        [SerializeField]
        private bool showConsoleWarnings = true;

        [SerializeField]
        private Button observedButton;

        [SerializeField]
        private LudusSemanticBridge semanticBridge;

        private bool subscribed;

        public LudusConfiguredOutcome ConfiguredOutcome => configuredOutcome;

        private void Reset()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            TrySubscribe(out _);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            LudusConfiguredOutcome outcome,
            string answerName,
            string expectedAnswerName
        )
        {
            configuredOutcome = outcome;
            answerDisplayName = answerName;
            expectedAnswerDisplayName = expectedAnswerName;
            TrySubscribe(out _);
        }

        public bool TryRecordConfiguredOutcome(out string errorMessage)
        {
            if (!trackingEnabled)
            {
                errorMessage =
                    "O adaptador de resultado do botão está desabilitado.";
                return false;
            }

            if (!ResolveDependencies())
            {
                errorMessage =
                    "O botão ou a ponte semântica LUDUS não foi localizado.";
                return false;
            }

            string answerName = string.IsNullOrWhiteSpace(answerDisplayName)
                ? gameObject.name
                : answerDisplayName.Trim();
            string expectedName = string.IsNullOrWhiteSpace(
                expectedAnswerDisplayName
            )
                ? "Resposta correta"
                : expectedAnswerDisplayName.Trim();

            return configuredOutcome == LudusConfiguredOutcome.Correct
                ? semanticBridge.TryRecordCorrect(
                    answerName,
                    out errorMessage
                )
                : semanticBridge.TryRecordWrong(
                    answerName,
                    expectedName,
                    out errorMessage
                );
        }

        private void HandleButtonClick()
        {
            bool recorded = TryRecordConfiguredOutcome(
                out string errorMessage
            );

            if (!recorded && showConsoleWarnings)
            {
                Debug.LogWarning(
                    "[LUDUS] O resultado do botão não foi registrado: "
                        + errorMessage,
                    this
                );
            }
        }

        private bool TrySubscribe(out string errorMessage)
        {
            if (!ResolveDependencies())
            {
                errorMessage =
                    "O botão ou a ponte semântica LUDUS não foi localizado.";
                return false;
            }

            if (!subscribed)
            {
                observedButton.onClick.AddListener(HandleButtonClick);
                subscribed = true;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || observedButton == null)
            {
                subscribed = false;
                return;
            }

            observedButton.onClick.RemoveListener(HandleButtonClick);
            subscribed = false;
        }

        private bool ResolveDependencies()
        {
            if (observedButton == null)
            {
                observedButton = GetComponent<Button>();
            }

            if (semanticBridge == null)
            {
                semanticBridge = GetComponent<LudusSemanticBridge>();
            }

            return observedButton != null && semanticBridge != null;
        }
    }
}
