using System;
using UnityEngine;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusSessionController : MonoBehaviour
    {
        [Header("Configuração")]

        [InspectorName("Configuração do jogo (asset)")]
        [Tooltip("Crie este asset em Project > Create > LUDUS > Configuração do SDK e arraste-o aqui.")]
        public LudusSdkConfig config;

        [InspectorName("Manter ativo ao trocar de cena")]
        [Tooltip("Mantém este controlador ativo ao trocar de cena.")]
        public bool persistAcrossScenes;

        private readonly LudusSessionLifecycle lifecycle =
            new LudusSessionLifecycle();

        public bool HasActiveSession => lifecycle.HasActiveSession;

        public bool HasActiveCaptureContext =>
            lifecycle.HasActiveCaptureContext;

        public LudusSession LastCompletedSession =>
            lifecycle.LastCompletedSession;

        public LudusSdkConfig Config => config;

        public event Action<LudusSession, string> SessionSerialized;

        private void Awake()
        {
            if (!persistAcrossScenes)
            {
                return;
            }

            LudusSessionController persistentController =
                FindOldestPersistentController();

            if (persistentController != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private LudusSessionController FindOldestPersistentController()
        {
            LudusSessionController[] controllers =
                FindObjectsByType<LudusSessionController>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );
            LudusSessionController oldest = this;

            foreach (LudusSessionController current in controllers)
            {
                if (
                    current.persistAcrossScenes &&
                    current.GetInstanceID() < oldest.GetInstanceID()
                )
                {
                    oldest = current;
                }
            }

            return oldest;
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

        public bool TryBeginCaptureContext(
            LudusCaptureContext context,
            out string errorMessage
        )
        {
            return lifecycle.TryBeginCaptureContext(context, out errorMessage);
        }

        public bool TryEndCaptureContext(out string errorMessage)
        {
            return lifecycle.TryEndCaptureContext(out errorMessage);
        }

        public bool TryEndCaptureContext(
            LudusCaptureContext expectedContext,
            out string errorMessage
        )
        {
            return lifecycle.TryEndCaptureContext(
                expectedContext,
                out errorMessage
            );
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

        public bool TryRecordDragPoint(
            Vector2 position,
            string state,
            out string errorMessage
        )
        {
            return lifecycle.TryRecordDragPoint(
                position.x,
                position.y,
                state,
                out errorMessage
            );
        }

        public bool TryRecordTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            out string errorMessage
        )
        {
            return lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    displayName,
                    interactionKind,
                    action
                ),
                out errorMessage
            );
        }

        public bool TryEndAndSerialize(
            out string json,
            out string errorMessage
        )
        {
            bool serialized = lifecycle.TryEndAndSerialize(
                out json,
                out errorMessage
            );

            if (serialized)
            {
                try
                {
                    SessionSerialized?.Invoke(LastCompletedSession, json);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }

            return serialized;
        }
    }
}
