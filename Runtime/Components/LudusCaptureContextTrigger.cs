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
        [InspectorName("Objeto controlador LUDUS SDK")]
        [Tooltip("Arraste aqui o GameObject que possui o componente LudusSessionController.")]
        public LudusSessionController sessionController;

        [InspectorName("Iniciar ao ativar este objeto")]
        [Tooltip(
            "Com este objeto ativo, o SDK inicia a captura neste recorte. " +
            "Use em um Canvas, painel ou objeto-raiz da atividade."
        )]
        public bool beginWhenEnabled = true;

        [InspectorName("Encerrar ao desativar este objeto")]
        [Tooltip("Ao desativar este objeto, encerra o recorte iniciado por ele.")]
        public bool endWhenDisabled = true;

        [Header("Informações para o acompanhamento")]
        [InspectorName("Título exibido no acompanhamento")]
        [Tooltip("Título apresentado ao professor no dashboard.")]
        public string titleForDashboard;

        [InspectorName("Tipo deste recorte")]
        [Tooltip("Tipo geral do recorte, sem depender de um jogo específico.")]
        public LudusCaptureContextKind contextKind =
            LudusCaptureContextKind.Canvas;

        [InspectorName("Objetivo deste recorte (opcional)")]
        [TextArea(2, 4)]
        [Tooltip("Objetivo observacional ou pedagógico opcional deste recorte.")]
        public string observationPurpose;

        private LudusCaptureContext ownedContext;
        private bool automaticStartAttempted;
        private bool startedByThisTrigger;

        private void Awake()
        {
            TryResolveSessionController();
        }

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

            if (!TryResolveSessionController())
            {
                errorMessage =
                    "Não foi possível localizar um LudusSessionController ativo.";
                return false;
            }

            ownedContext = new LudusCaptureContext(
                GetDisplayName(),
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

            if (!TryResolveSessionController())
            {
                errorMessage =
                    "Não foi possível localizar um LudusSessionController ativo.";
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

        private bool TryResolveSessionController()
        {
            if (sessionController != null)
            {
                return true;
            }

            sessionController =
                FindFirstObjectByType<LudusSessionController>();

            return sessionController != null;
        }

        private string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(titleForDashboard)
                ? gameObject.name
                : titleForDashboard;
        }
    }
}
