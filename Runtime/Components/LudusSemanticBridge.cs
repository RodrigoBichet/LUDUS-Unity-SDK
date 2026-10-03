using System;
using UnityEngine;

namespace LudusSDK
{
    /// <summary>
    /// Ponte visual entre eventos que já existem no jogo e a API semântica
    /// do LUDUS. O componente não decide se uma ação foi correta ou
    /// incorreta; essa decisão continua pertencendo ao jogo.
    /// </summary>
    [AddComponentMenu("LUDUS/Ponte semântica do jogo")]
    public sealed class LudusSemanticBridge : MonoBehaviour
    {
        [SerializeField]
        private bool semanticTrackingEnabled = true;

        [SerializeField]
        private bool showConsoleWarnings = true;

        [SerializeField]
        private string categoryName = "Atividade";

        [SerializeField]
        private string phaseId = "fase-1";

        [SerializeField]
        private string targetItem = "objetivo";

        [SerializeField]
        private string[] phaseOptions = Array.Empty<string>();

        [SerializeField]
        private string resultItem = "resultado";

        [SerializeField]
        private string expectedItem = "objetivo";

        [SerializeField]
        [Min(0)]
        private int completionStars;

        private int phaseCorrectCount;
        private int phaseWrongCount;
        private float phaseStartedAtSeconds;
        private bool hasPhaseTimer;
        private LudusSessionController sessionController;

        public bool SemanticTrackingEnabled => semanticTrackingEnabled;
        public int PhaseCorrectCount => phaseCorrectCount;
        public int PhaseWrongCount => phaseWrongCount;

        /// <summary>
        /// Permite que adaptadores e testes configurem a mesma ponte usada
        /// pelo fluxo visual do Inspector.
        /// </summary>
        public void Configure(
            string category,
            string phase,
            string target,
            string item,
            string expected,
            string[] options,
            int stars = 0
        )
        {
            categoryName = category;
            phaseId = phase;
            targetItem = target;
            resultItem = item;
            expectedItem = expected;
            phaseOptions = options ?? Array.Empty<string>();
            completionStars = Mathf.Max(0, stars);
        }

        public void SetSemanticTrackingEnabled(bool enabled)
        {
            semanticTrackingEnabled = enabled;
        }

        /// <summary>
        /// Método sem parâmetros para vinculação em UnityEvent.
        /// </summary>
        public void RegistrarCategoriaSelecionada()
        {
            ReportFailure(
                "registrar a categoria",
                TryRecordCategory(out string errorMessage),
                errorMessage
            );
        }

        public bool TryRecordCategory(out string errorMessage)
        {
            if (!TryEnsureEnabled(out errorMessage))
            {
                return false;
            }

            return LudusGameEvents.TryCategorySelected(
                categoryName,
                out errorMessage
            );
        }

        /// <summary>
        /// Registra o início e reinicia o cronômetro e os contadores locais
        /// da fase. Vincule este método ao evento que inicia a atividade.
        /// </summary>
        public void RegistrarInicioDeFase()
        {
            ReportFailure(
                "registrar o início da fase",
                TryRecordPhaseStarted(out string errorMessage),
                errorMessage
            );
        }

        public bool TryRecordPhaseStarted(out string errorMessage)
        {
            if (!TryEnsureEnabled(out errorMessage))
            {
                return false;
            }

            bool recorded = LudusGameEvents.TryPhaseStarted(
                phaseId,
                targetItem,
                phaseOptions,
                out errorMessage
            );

            if (recorded)
            {
                LudusSdk.TryGetSingleSessionController(
                    out sessionController,
                    out _
                );
                ResetPhaseProgress();
            }

            return recorded;
        }

        /// <summary>
        /// Registra somente a tentativa de arraste. O resultado final pode
        /// ser ligado ao mesmo UnityEvent com RegistrarAcerto ou
        /// RegistrarErro.
        /// </summary>
        public void RegistrarTentativaDeArrasteCorreta()
        {
            ReportFailure(
                "registrar a tentativa de arraste correta",
                TryRecordDragAttempt(true, out string errorMessage),
                errorMessage
            );
        }

        public void RegistrarTentativaDeArrasteIncorreta()
        {
            ReportFailure(
                "registrar a tentativa de arraste incorreta",
                TryRecordDragAttempt(false, out string errorMessage),
                errorMessage
            );
        }

        public bool TryRecordDragAttempt(
            bool correct,
            out string errorMessage
        )
        {
            return TryRecordDragAttempt(
                resultItem,
                targetItem,
                correct,
                out errorMessage
            );
        }

        public bool TryRecordDragAttempt(
            string draggedItem,
            string target,
            bool correct,
            out string errorMessage
        )
        {
            return TryRecordDragAttempt(
                draggedItem,
                target,
                string.Empty,
                System.Array.Empty<string>(),
                correct,
                out errorMessage
            );
        }

        public bool TryRecordDragAttempt(
            string draggedItem,
            string target,
            string expected,
            string[] options,
            bool correct,
            out string errorMessage
        )
        {
            if (!TryEnsureEnabled(out errorMessage))
            {
                return false;
            }

            return LudusGameEvents.TryDragAttempt(
                draggedItem,
                target,
                expected,
                options,
                correct,
                out errorMessage
            );
        }

        public void RegistrarAcerto()
        {
            ReportFailure(
                "registrar o acerto",
                TryRecordCorrect(out string errorMessage),
                errorMessage
            );
        }

        public bool TryRecordCorrect(out string errorMessage)
        {
            return TryRecordCorrect(resultItem, out errorMessage);
        }

        public bool TryRecordCorrect(
            string item,
            out string errorMessage
        )
        {
            if (!TryEnsureEnabled(out errorMessage))
            {
                return false;
            }

            bool recorded = LudusGameEvents.TryCorrectMatch(
                item,
                GetPhaseElapsedSeconds(),
                out errorMessage
            );

            if (recorded)
            {
                phaseCorrectCount++;
            }

            return recorded;
        }

        public void RegistrarErro()
        {
            ReportFailure(
                "registrar o erro",
                TryRecordWrong(out string errorMessage),
                errorMessage
            );
        }

        public bool TryRecordWrong(out string errorMessage)
        {
            return TryRecordWrong(
                resultItem,
                expectedItem,
                out errorMessage
            );
        }

        public bool TryRecordWrong(
            string attemptedItem,
            string expected,
            out string errorMessage
        )
        {
            if (!TryEnsureEnabled(out errorMessage))
            {
                return false;
            }

            bool recorded = LudusGameEvents.TryWrongMatch(
                attemptedItem,
                expected,
                out errorMessage
            );

            if (recorded)
            {
                phaseWrongCount++;
            }

            return recorded;
        }

        /// <summary>
        /// Encaminha um resultado de arraste descoberto por um adaptador.
        /// A avaliação continua sendo fornecida pela regra configurada no
        /// adaptador; a ponte apenas registra os eventos correspondentes.
        /// </summary>
        public bool TryRecordEvaluatedDrag(
            string draggedItem,
            string target,
            string expected,
            bool correct,
            bool includeAttemptEvent,
            bool includeOutcomeEvent,
            out string errorMessage
        )
        {
            return TryRecordEvaluatedDrag(
                draggedItem,
                target,
                expected,
                System.Array.Empty<string>(),
                correct,
                includeAttemptEvent,
                includeOutcomeEvent,
                out errorMessage
            );
        }

        public bool TryRecordEvaluatedDrag(
            string draggedItem,
            string target,
            string expected,
            string[] options,
            bool correct,
            bool includeAttemptEvent,
            bool includeOutcomeEvent,
            out string errorMessage
        )
        {
            if (!includeAttemptEvent && !includeOutcomeEvent)
            {
                errorMessage =
                    "Habilite ao menos um evento para o resultado do arraste.";
                return false;
            }

            if (
                includeAttemptEvent &&
                !TryRecordDragAttempt(
                    draggedItem,
                    target,
                    expected,
                    options,
                    correct,
                    out errorMessage
                )
            )
            {
                return false;
            }

            if (!includeOutcomeEvent)
            {
                errorMessage = string.Empty;
                return true;
            }

            return correct
                ? TryRecordCorrect(draggedItem, out errorMessage)
                : TryRecordWrong(
                    draggedItem,
                    expected,
                    out errorMessage
                );
        }

        /// <summary>
        /// Registra a conclusão usando os acertos, erros e tempo acumulados
        /// desde o último início de fase registrado por esta ponte.
        /// </summary>
        public void RegistrarConclusaoDeFase()
        {
            ReportFailure(
                "registrar a conclusão da fase",
                TryRecordPhaseCompleted(out string errorMessage),
                errorMessage
            );
        }

        public bool TryRecordPhaseCompleted(out string errorMessage)
        {
            if (!TryEnsureEnabled(out errorMessage))
            {
                return false;
            }

            if (
                sessionController != null &&
                sessionController.HasActiveSession
            )
            {
                return LudusGameEvents.TryPhaseCompleted(
                    sessionController,
                    phaseCorrectCount,
                    phaseWrongCount,
                    GetPhaseElapsedSeconds(),
                    completionStars,
                    out errorMessage
                );
            }

            return LudusGameEvents.TryPhaseCompleted(
                phaseCorrectCount,
                phaseWrongCount,
                GetPhaseElapsedSeconds(),
                completionStars,
                out errorMessage
            );
        }

        public void ReiniciarProgressoDaFase()
        {
            ResetPhaseProgress();
        }

        private void ResetPhaseProgress()
        {
            phaseCorrectCount = 0;
            phaseWrongCount = 0;
            phaseStartedAtSeconds = Time.realtimeSinceStartup;
            hasPhaseTimer = true;
        }

        private float GetPhaseElapsedSeconds()
        {
            if (!hasPhaseTimer)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                Time.realtimeSinceStartup - phaseStartedAtSeconds
            );
        }

        private bool TryEnsureEnabled(out string errorMessage)
        {
            if (!semanticTrackingEnabled)
            {
                errorMessage =
                    "A ponte semântica LUDUS está desabilitada neste objeto.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void ReportFailure(
            string action,
            bool succeeded,
            string errorMessage
        )
        {
            if (succeeded || !showConsoleWarnings)
            {
                return;
            }

            Debug.LogWarning(
                "[LUDUS] Não foi possível "
                    + action
                    + ": "
                    + errorMessage,
                this
            );
        }

        private void OnValidate()
        {
            completionStars = Mathf.Max(0, completionStars);
            phaseOptions ??= Array.Empty<string>();
        }
    }
}
