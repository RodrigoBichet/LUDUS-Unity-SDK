using UnityEngine;

namespace LudusSDK
{
    public enum LudusThresholdComparison
    {
        AtLeast,
        AtMost,
        Equal,
    }

    public enum LudusThresholdAction
    {
        RecordCorrect,
        CompletePhase,
        RecordCorrectAndCompletePhase,
    }

    /// <summary>
    /// Recebe atualizações de valor por UnityEvent e registra o resultado
    /// configurado quando a meta é alcançada. Não lê variáveis internas do
    /// jogo por reflection.
    /// </summary>
    [AddComponentMenu("LUDUS/Adaptadores/Meta por valor ou pontuação")]
    [RequireComponent(typeof(LudusSemanticBridge))]
    public sealed class LudusProgressThresholdAdapter : MonoBehaviour
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private float currentValue;

        [SerializeField]
        private float targetValue = 1f;

        [SerializeField]
        private LudusThresholdComparison comparison =
            LudusThresholdComparison.AtLeast;

        [SerializeField]
        [Min(0f)]
        private float equalityTolerance = 0.001f;

        [SerializeField]
        private LudusThresholdAction action =
            LudusThresholdAction.CompletePhase;

        [SerializeField]
        private string resultDisplayName = "Meta atingida";

        [SerializeField]
        private bool recordOnlyOnce = true;

        [SerializeField]
        private bool showConsoleWarnings = true;

        [SerializeField]
        private LudusSemanticBridge semanticBridge;

        private bool resultAlreadyRecorded;

        public float CurrentValue => currentValue;
        public bool ResultAlreadyRecorded => resultAlreadyRecorded;
        public LudusThresholdAction Action => action;

        private void Reset()
        {
            semanticBridge = GetComponent<LudusSemanticBridge>();
        }

        private void Awake()
        {
            TryResolveBridge();
        }

        public void Configure(
            float target,
            LudusThresholdComparison thresholdComparison,
            LudusThresholdAction thresholdAction,
            string resultName,
            bool onlyOnce = true
        )
        {
            targetValue = target;
            comparison = thresholdComparison;
            action = thresholdAction;
            resultDisplayName = resultName;
            recordOnlyOnce = onlyOnce;
            TryResolveBridge();
        }

        public void AdicionarUm()
        {
            AddAndReport(1f);
        }

        public void SubtrairUm()
        {
            AddAndReport(-1f);
        }

        public void Adicionar(float amount)
        {
            AddAndReport(amount);
        }

        public void DefinirValor(float value)
        {
            EvaluateAndReport(value);
        }

        public void AvaliarAgora()
        {
            EvaluateAndReport(currentValue);
        }

        public void ReiniciarMeta()
        {
            resultAlreadyRecorded = false;
        }

        public void ReiniciarValorEMeta()
        {
            currentValue = 0f;
            resultAlreadyRecorded = false;
        }

        public bool TrySetValueAndEvaluate(
            float value,
            out bool thresholdReached,
            out string errorMessage
        )
        {
            thresholdReached = false;

            if (!trackingEnabled)
            {
                errorMessage =
                    "O adaptador de meta por valor está desabilitado.";
                return false;
            }

            if (!IsFinite(value) || !IsFinite(targetValue))
            {
                errorMessage =
                    "O valor atual e a meta devem ser números finitos.";
                return false;
            }

            if (!TryResolveBridge())
            {
                errorMessage =
                    "Não foi possível localizar a ponte semântica LUDUS.";
                return false;
            }

            currentValue = value;
            thresholdReached = HasReachedThreshold();

            if (
                !thresholdReached ||
                (recordOnlyOnce && resultAlreadyRecorded)
            )
            {
                errorMessage = string.Empty;
                return true;
            }

            if (!TryRecordConfiguredAction(out errorMessage))
            {
                return false;
            }

            resultAlreadyRecorded = true;
            return true;
        }

        private void AddAndReport(float amount)
        {
            if (!IsFinite(amount))
            {
                ReportFailure(
                    false,
                    "O incremento deve ser um número finito."
                );
                return;
            }

            EvaluateAndReport(currentValue + amount);
        }

        private void EvaluateAndReport(float value)
        {
            bool evaluated = TrySetValueAndEvaluate(
                value,
                out _,
                out string errorMessage
            );
            ReportFailure(evaluated, errorMessage);
        }

        private bool TryRecordConfiguredAction(out string errorMessage)
        {
            string displayName = string.IsNullOrWhiteSpace(resultDisplayName)
                ? "Meta atingida"
                : resultDisplayName.Trim();

            if (
                action == LudusThresholdAction.RecordCorrect ||
                action ==
                    LudusThresholdAction.RecordCorrectAndCompletePhase
            )
            {
                if (
                    !semanticBridge.TryRecordCorrect(
                        displayName,
                        out errorMessage
                    )
                )
                {
                    return false;
                }
            }

            if (
                action == LudusThresholdAction.CompletePhase ||
                action ==
                    LudusThresholdAction.RecordCorrectAndCompletePhase
            )
            {
                return semanticBridge.TryRecordPhaseCompleted(
                    out errorMessage
                );
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool HasReachedThreshold()
        {
            switch (comparison)
            {
                case LudusThresholdComparison.AtMost:
                    return currentValue <= targetValue;
                case LudusThresholdComparison.Equal:
                    return Mathf.Abs(currentValue - targetValue) <=
                        equalityTolerance;
                default:
                    return currentValue >= targetValue;
            }
        }

        private bool TryResolveBridge()
        {
            if (semanticBridge != null)
            {
                return true;
            }

            semanticBridge = GetComponent<LudusSemanticBridge>();
            return semanticBridge != null;
        }

        private void ReportFailure(bool succeeded, string errorMessage)
        {
            if (succeeded || !showConsoleWarnings)
            {
                return;
            }

            Debug.LogWarning(
                "[LUDUS] A meta por valor não foi registrada: "
                    + errorMessage,
                this
            );
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void OnValidate()
        {
            equalityTolerance = Mathf.Max(0f, equalityTolerance);
        }
    }
}
