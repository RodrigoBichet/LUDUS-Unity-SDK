using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusSessionController : MonoBehaviour
    {
        private const int MaxScreenshotEncodedBytes = 2 * 1024 * 1024;

        [Header("Configuração")]

        [InspectorName("Configuração do jogo (asset)")]
        [Tooltip("Crie este asset em Project > Create > LUDUS > Configuração do SDK e arraste-o aqui.")]
        public LudusSdkConfig config;

        [InspectorName("Manter ativo ao trocar de cena")]
        [Tooltip("Mantém este controlador ativo ao trocar de cena.")]
        public bool persistAcrossScenes;

        private readonly LudusSessionLifecycle lifecycle =
            new LudusSessionLifecycle();

        private readonly HashSet<string> automaticScreenshotContexts =
            new HashSet<string>();

        private readonly HashSet<string> preparedAutomaticScreenshotContexts =
            new HashSet<string>();

        private int pendingScreenshotCaptures;

        public bool HasActiveSession => lifecycle.HasActiveSession;

        public bool HasActiveCaptureContext =>
            lifecycle.HasActiveCaptureContext;

        public bool HasPendingScreenshotCapture =>
            pendingScreenshotCaptures > 0;

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

            bool started = lifecycle.TryStartSession(
                config,
                new LudusParticipant(studentId, playerId),
                viewport,
                out errorMessage
            );

            if (started)
            {
                automaticScreenshotContexts.Clear();
                preparedAutomaticScreenshotContexts.Clear();
                pendingScreenshotCaptures = 0;
            }

            return started;
        }

        public bool TryBeginCaptureContext(
            string displayName,
            string contextKind,
            string observationPurpose,
            out string errorMessage
        )
        {
            return TryBeginCaptureContext(
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
            string previousContextInstanceId =
                lifecycle.ActiveCaptureContextInstanceId;
            bool started = lifecycle.TryBeginCaptureContext(
                context,
                out errorMessage
            );

            if (!string.IsNullOrWhiteSpace(previousContextInstanceId))
            {
                preparedAutomaticScreenshotContexts.Remove(
                    previousContextInstanceId
                );
            }

            PrepareAutomaticScreenshotIfNeeded(started, context);
            return started;
        }

        public bool TryEndCaptureContext(out string errorMessage)
        {
            string contextInstanceId =
                lifecycle.ActiveCaptureContextInstanceId;
            bool ended = lifecycle.TryEndCaptureContext(out errorMessage);

            if (ended)
            {
                preparedAutomaticScreenshotContexts.Remove(
                    contextInstanceId
                );
            }

            return ended;
        }

        public bool TryEndCaptureContext(
            LudusCaptureContext expectedContext,
            out string errorMessage
        )
        {
            string contextInstanceId =
                lifecycle.ActiveCaptureContextInstanceId;
            bool ended = lifecycle.TryEndCaptureContext(
                expectedContext,
                out errorMessage
            );

            if (ended)
            {
                preparedAutomaticScreenshotContexts.Remove(
                    contextInstanceId
                );
            }

            return ended;
        }

        public bool TryRecordClick(
            Vector2 position,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordClick(
                position.x,
                position.y,
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
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
            bool recorded = lifecycle.TryRecordDragPoint(
                position.x,
                position.y,
                state,
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
        }

        public bool TryRecordTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    displayName,
                    interactionKind,
                    action
                ),
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
        }

        public bool TryRecordTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            Vector2 position,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    displayName,
                    interactionKind,
                    action,
                    position
                ),
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
        }

        internal bool TryRecordGameEvent(
            string eventType,
            string payloadJson,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordGameEvent(
                eventType,
                payloadJson,
                out errorMessage
            );

            bool representsInteraction =
                !string.Equals(
                    eventType,
                    "CategorySelected",
                    StringComparison.Ordinal
                ) &&
                !string.Equals(
                    eventType,
                    "PhaseStarted",
                    StringComparison.Ordinal
                );

            SchedulePreparedAutomaticScreenshot(
                recorded && representsInteraction
            );
            return recorded;
        }

        public bool TryRecordTextInputCompletion(
            string displayName,
            int characterCount,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    displayName,
                    "text-input",
                    "completed",
                    characterCount
                ),
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
        }

        public bool TryRecordTextInputCompletion(
            string displayName,
            int characterCount,
            Vector2 position,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    displayName,
                    "text-input",
                    "completed",
                    characterCount,
                    position
                ),
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
        }

        public bool TryRecordTrackedDrag(
            string displayName,
            Vector2 startPosition,
            Vector2 endPosition,
            int durationMs,
            out string errorMessage
        )
        {
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    displayName,
                    startPosition,
                    endPosition,
                    durationMs
                ),
                out errorMessage
            );

            SchedulePreparedAutomaticScreenshot(recorded);
            return recorded;
        }

        public bool TryCaptureScreenshot(out string errorMessage)
        {
            return TryScheduleScreenshotCapture(
                lifecycle.ActiveCaptureContextInstanceId,
                false,
                out errorMessage
            );
        }

        public bool TryEndAndSerialize(
            out string json,
            out string errorMessage
        )
        {
            if (HasPendingScreenshotCapture)
            {
                json = string.Empty;
                errorMessage =
                    "Aguarde a captura visual terminar antes de encerrar a sessão.";
                return false;
            }

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

        private void PrepareAutomaticScreenshotIfNeeded(
            bool contextStarted,
            LudusCaptureContext context
        )
        {
            if (
                !contextStarted ||
                context == null ||
                !context.captureVisualReference ||
                config == null ||
                config.capabilities == null ||
                !config.capabilities.screenshots ||
                !config.captureScreenshotOnContextStart
            )
            {
                return;
            }

            string contextInstanceId =
                lifecycle.ActiveCaptureContextInstanceId;

            if (
                string.IsNullOrWhiteSpace(contextInstanceId) ||
                automaticScreenshotContexts.Contains(contextInstanceId) ||
                lifecycle.HasRetainedAutomaticScreenshot(
                    context.visualReferenceKey
                )
            )
            {
                return;
            }

            preparedAutomaticScreenshotContexts.Add(contextInstanceId);
        }

        private void SchedulePreparedAutomaticScreenshot(
            bool meaningfulActivityRecorded
        )
        {
            if (!meaningfulActivityRecorded)
            {
                return;
            }

            string contextInstanceId =
                lifecycle.ActiveCaptureContextInstanceId;

            if (
                string.IsNullOrWhiteSpace(contextInstanceId) ||
                !preparedAutomaticScreenshotContexts.Contains(
                    contextInstanceId
                ) ||
                automaticScreenshotContexts.Contains(contextInstanceId)
            )
            {
                return;
            }

            if (TryScheduleScreenshotCapture(
                contextInstanceId,
                true,
                out string errorMessage
            ))
            {
                preparedAutomaticScreenshotContexts.Remove(
                    contextInstanceId
                );
                automaticScreenshotContexts.Add(contextInstanceId);
                return;
            }

            if (config != null && config.debugMode)
            {
                Debug.LogWarning(
                    "[LUDUS] Captura visual automática não iniciada: " +
                    errorMessage,
                    this
                );
            }
        }

        private bool TryScheduleScreenshotCapture(
            string contextInstanceId,
            bool isAutomatic,
            out string errorMessage
        )
        {
            if (!HasActiveSession || !HasActiveCaptureContext)
            {
                errorMessage =
                    "Inicie uma sessão e um contexto de captura antes de registrar a tela.";
                return false;
            }

            if (
                config == null ||
                config.capabilities == null ||
                !config.capabilities.screenshots
            )
            {
                errorMessage = "A capacidade screenshots está desativada.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(contextInstanceId))
            {
                errorMessage = "O contexto da captura visual é inválido.";
                return false;
            }

            if (
                !isAutomatic &&
                lifecycle.ScreenshotCount + pendingScreenshotCaptures >=
                config.maxScreenshotsPerSession
            )
            {
                errorMessage =
                    "O limite configurado de capturas visuais da sessão foi atingido.";
                return false;
            }

            pendingScreenshotCaptures++;
            StartCoroutine(CaptureScreenshotAtEndOfFrame(
                contextInstanceId,
                isAutomatic
            ));

            errorMessage = string.Empty;
            return true;
        }

        private IEnumerator CaptureScreenshotAtEndOfFrame(
            string contextInstanceId,
            bool isAutomatic
        )
        {
            Texture2D sourceTexture = null;
            Texture2D resizedTexture = null;

            try
            {
                yield return new WaitForEndOfFrame();

                sourceTexture = ScreenCapture.CaptureScreenshotAsTexture();

                if (sourceTexture == null)
                {
                    LogScreenshotFailure(
                        "A Unity não retornou a imagem da tela.",
                        isAutomatic
                    );
                    yield break;
                }

                Texture2D encodingTexture = sourceTexture;
                int maxDimension = Mathf.Max(
                    1,
                    config.screenshotMaxDimensionPx
                );

                if (
                    sourceTexture.width > maxDimension ||
                    sourceTexture.height > maxDimension
                )
                {
                    resizedTexture = ResizeScreenshot(
                        sourceTexture,
                        maxDimension
                    );
                    encodingTexture = resizedTexture;
                }

                byte[] jpegBytes = encodingTexture.EncodeToJPG(
                    config.screenshotJpegQuality
                );

                if (
                    jpegBytes == null ||
                    jpegBytes.Length == 0 ||
                    jpegBytes.Length > MaxScreenshotEncodedBytes
                )
                {
                    LogScreenshotFailure(
                        "A imagem compactada ficou vazia ou maior que 2 MB.",
                        isAutomatic
                    );
                    yield break;
                }

                string screenshotBase64 = Convert.ToBase64String(jpegBytes);
                bool recorded;
                string errorMessage;

                if (isAutomatic)
                {
                    recorded = lifecycle.TryRecordAutomaticScreenshotCandidate(
                        contextInstanceId,
                        encodingTexture.width,
                        encodingTexture.height,
                        screenshotBase64,
                        out errorMessage
                    );
                }
                else
                {
                    recorded = lifecycle.TryRecordScreenshot(
                        contextInstanceId,
                        encodingTexture.width,
                        encodingTexture.height,
                        screenshotBase64,
                        out errorMessage
                    );
                }

                if (!recorded)
                {
                    LogScreenshotFailure(errorMessage, isAutomatic);
                }
            }
            finally
            {
                if (resizedTexture != null)
                {
                    Destroy(resizedTexture);
                }

                if (sourceTexture != null)
                {
                    Destroy(sourceTexture);
                }

                pendingScreenshotCaptures = Mathf.Max(
                    0,
                    pendingScreenshotCaptures - 1
                );
            }
        }

        private static Texture2D ResizeScreenshot(
            Texture2D sourceTexture,
            int maxDimension
        )
        {
            float scale = Mathf.Min(
                (float)maxDimension / sourceTexture.width,
                (float)maxDimension / sourceTexture.height
            );
            int targetWidth = Mathf.Max(
                1,
                Mathf.RoundToInt(sourceTexture.width * scale)
            );
            int targetHeight = Mathf.Max(
                1,
                Mathf.RoundToInt(sourceTexture.height * scale)
            );

            RenderTexture renderTexture = RenderTexture.GetTemporary(
                targetWidth,
                targetHeight,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default
            );
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                Graphics.Blit(sourceTexture, renderTexture);
                RenderTexture.active = renderTexture;

                Texture2D resizedTexture = new Texture2D(
                    targetWidth,
                    targetHeight,
                    TextureFormat.RGB24,
                    false
                );
                resizedTexture.ReadPixels(
                    new Rect(0, 0, targetWidth, targetHeight),
                    0,
                    0
                );
                resizedTexture.Apply(false, false);
                return resizedTexture;
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private void LogScreenshotFailure(
            string errorMessage,
            bool isAutomatic
        )
        {
            if (config != null && config.debugMode)
            {
                Debug.LogWarning(
                    "[LUDUS] Captura visual " +
                    (isAutomatic ? "automática" : "manual") +
                    " não registrada: " + errorMessage,
                    this
                );
            }
        }
    }
}
