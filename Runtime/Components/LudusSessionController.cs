using UnityEngine;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusSessionController : MonoBehaviour
    {
        [Header("Configuração")]

        [Tooltip("Asset com a configuração reutilizável do SDK.")]
        public LudusSdkConfig config;

        [Tooltip("Mantém este controlador ativo ao trocar de cena.")]
        public bool persistAcrossScenes;

        private readonly LudusSessionLifecycle lifecycle =
            new LudusSessionLifecycle();

        public bool HasActiveSession => lifecycle.HasActiveSession;

        public LudusSession LastCompletedSession =>
            lifecycle.LastCompletedSession;

        private void Awake()
        {
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void Configure(LudusSdkConfig sdkConfig)
        {
            config = sdkConfig;
        }

        public bool TryStartSession(
            string studentId,
            string playerId,
            out string errorMessage
        )
        {
            if (config == null)
            {
                errorMessage = "LudusSdkConfig não foi configurado.";
                return false;
            }

            LudusViewport viewport = new LudusViewport(
                Mathf.Max(1, Screen.width),
                Mathf.Max(1, Screen.height),
                "pixel",
                "bottom-left"
            );

            return lifecycle.TryStartSession(
                config,
                new LudusParticipant(studentId, playerId),
                viewport,
                out errorMessage
            );
        }

        public bool TryBeginCaptureContext(
            string displayName,
            string contextKind,
            string observationPurpose,
            out string errorMessage
        )
        {
            return lifecycle.TryBeginCaptureContext(
                new LudusCaptureContext(
                    displayName,
                    contextKind,
                    observationPurpose
                ),
                out errorMessage
            );
        }

        public bool TryEndCaptureContext(out string errorMessage)
        {
            return lifecycle.TryEndCaptureContext(out errorMessage);
        }

        public bool TryRecordClick(
            Vector2 position,
            out string errorMessage
        )
        {
            return lifecycle.TryRecordClick(
                position.x,
                position.y,
                out errorMessage
            );
        }

        public bool TryRecordMousePoint(
            Vector2 position,
            out string errorMessage
        )
        {
            return lifecycle.TryRecordMousePoint(
                position.x,
                position.y,
                out errorMessage
            );
        }

        public bool TryEndAndSerialize(
            out string json,
            out string errorMessage
        )
        {
            return lifecycle.TryEndAndSerialize(out json, out errorMessage);
        }
    }
}
