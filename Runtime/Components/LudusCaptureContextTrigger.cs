using UnityEngine;

namespace LudusSDK
{
    public enum LudusCaptureContextKind
    {
        Scene,
        Canvas,
        Activity,
        Other,
    }

    [DisallowMultipleComponent]
    public sealed class LudusCaptureContextTrigger : MonoBehaviour
    {
        [Header("Onde a captura começa")]
        [Tooltip("Controlador que mantém a sessão LUDUS ativa.")]
        public LudusSessionController sessionController;

        [Tooltip(
            "Com este objeto ativo, o SDK inicia a captura neste recorte. " +
            "Use em um Canvas, painel ou objeto-raiz da atividade."
        )]
        public bool beginWhenEnabled = true;

        [Tooltip("Ao desativar este objeto, encerra o recorte iniciado por ele.")]
        public bool endWhenDisabled = true;

        [Header("Informações para o acompanhamento")]
        [Tooltip("Título apresentado ao professor no dashboard.")]
        public string titleForDashboard;

        [Tooltip("Tipo geral do recorte, sem depender de um jogo específico.")]
        public LudusCaptureContextKind contextKind =
            LudusCaptureContextKind.Canvas;

        [TextArea(2, 4)]
        [Tooltip("Objetivo observacional ou pedagógico opcional deste recorte.")]
        public string observationPurpose;

        private LudusCaptureContext ownedContext;
        private bool automaticStartAttempted;
        private bool startedByThisTrigger;

        private void OnEnable()
        {
            automaticStartAttempted = false;
            startedByThisTrigger = false;
        }

        private void Update()
        {
            if (
                !beginWhenEnabled ||
                automaticStartAttempted ||
                sessionController == null ||
                !sessionController.HasActiveSession
            )
            {
                return;
            }

            automaticStartAttempted = true;
            TryBeginCapture(out _);
        }

        private void OnDisable()
        {
            if (endWhenDisabled && startedByThisTrigger)
            {
                TryEndCapture(out _);
            }

            startedByThisTrigger = false;
            ownedContext = null;
        }

        public bool TryBeginCapture(out string errorMessage)
        {
            if (startedByThisTrigger)
            {
                errorMessage = string.Empty;
                return true;
            }

            if (sessionController == null)
            {
                errorMessage = "O LudusSessionController não foi configurado.";
                return false;
            }

            ownedContext = new LudusCaptureContext(
                titleForDashboard,
                contextKind.ToString(),
                observationPurpose
            );

            bool started = sessionController.TryBeginCaptureContext(
                ownedContext,
                out errorMessage
            );

            startedByThisTrigger = started;
            return started;
        }

        public bool TryEndCapture(out string errorMessage)
        {
            if (!startedByThisTrigger || ownedContext == null)
            {
                errorMessage = "Este objeto não iniciou um contexto de captura.";
                return false;
            }

            if (sessionController == null)
            {
                errorMessage = "O LudusSessionController não foi configurado.";
                return false;
            }

            bool ended = sessionController.TryEndCaptureContext(
                ownedContext,
                out errorMessage
            );

            if (ended)
            {
                startedByThisTrigger = false;
                ownedContext = null;
            }

            return ended;
        }
    }
}
