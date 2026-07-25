using System;
using System.Diagnostics;

namespace LudusSDK
{
    public sealed class LudusSessionLifecycle
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private LudusSession activeSession;
        private LudusCaptureContext activeCaptureContext;
        private string activeContextInstanceId;

        public bool HasActiveSession => activeSession != null;

        public bool HasActiveCaptureContext =>
            activeCaptureContext != null &&
            !string.IsNullOrWhiteSpace(activeContextInstanceId);

        public LudusSession LastCompletedSession { get; private set; }

        public bool TryStartSession(
            LudusSdkConfig config,
            LudusParticipant participant,
            LudusViewport viewport,
            out string errorMessage
        )
        {
            if (HasActiveSession)
            {
                errorMessage = "Já existe uma sessão ativa.";
                return false;
            }

            try
            {
                activeSession = LudusSession.Create(
                    config,
                    participant,
                    viewport
                );

                activeCaptureContext = null;
                activeContextInstanceId = string.Empty;
                stopwatch.Restart();

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                activeSession = null;
                errorMessage = exception.Message;
                return false;
            }
        }

        public bool TryBeginCaptureContext(
            LudusCaptureContext context,
            out string errorMessage
        )
        {
            if (!HasActiveSession)
            {
                errorMessage =
                    "Não existe sessão ativa para iniciar um contexto de captura.";
                return false;
            }

            if (!activeSession.capabilities.customEvents)
            {
                errorMessage =
                    "A capacidade customEvents deve estar habilitada para usar contextos de captura.";
                return false;
            }

            if (context == null)
            {
                errorMessage = "O contexto de captura não pode ser nulo.";
                return false;
            }

            if (!context.TryValidate(out errorMessage))
            {
                return false;
            }

            if (HasActiveCaptureContext)
            {
                EndActiveCaptureContext(GetElapsedMilliseconds());
            }

            activeCaptureContext = context;
            activeContextInstanceId = Guid.NewGuid().ToString("N");

            activeSession.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = "CaptureContextStarted",
                    timestamp = GetElapsedMilliseconds(),
                    payloadJson = context.CreateStartedPayload(
                        activeContextInstanceId
                    ),
                }
            );

            errorMessage = string.Empty;
            return true;
        }

        public bool TryEndCaptureContext(out string errorMessage)
        {
            if (!HasActiveSession)
            {
                errorMessage =
                    "Não existe sessão ativa para encerrar um contexto de captura.";
                return false;
            }

            if (!HasActiveCaptureContext)
            {
                errorMessage = "Não existe contexto de captura ativo.";
                return false;
            }

            EndActiveCaptureContext(GetElapsedMilliseconds());

            errorMessage = string.Empty;
            return true;
        }

        public bool TryEndAndSerialize(
            out string json,
            out string errorMessage
        )
        {
            json = string.Empty;

            if (!HasActiveSession)
            {
                errorMessage = "Não existe sessão ativa para encerrar.";
                return false;
            }

            int durationMs = GetElapsedMilliseconds();

            if (HasActiveCaptureContext)
            {
                EndActiveCaptureContext(durationMs);
            }

            activeSession.End(durationMs);
            stopwatch.Stop();

            LastCompletedSession = activeSession;
            activeSession = null;

            return LudusJsonSerializer.TrySerialize(
                LastCompletedSession,
                out json,
                out errorMessage
            );
        }

        private void EndActiveCaptureContext(int timestamp)
        {
            activeSession.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = "CaptureContextEnded",
                    timestamp = timestamp,
                    payloadJson = activeCaptureContext.CreateEndedPayload(
                        activeContextInstanceId
                    ),
                }
            );

            activeCaptureContext = null;
            activeContextInstanceId = string.Empty;
        }

        private int GetElapsedMilliseconds()
        {
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            return elapsedMilliseconds > int.MaxValue
                ? int.MaxValue
                : (int)elapsedMilliseconds;
        }
    }
}
