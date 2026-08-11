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
        private const int MaxClicks = 10000;
        private const int MaxMousePathPoints = 50000;
        private const int MaxDragPathPoints = 50000;
        private const int MaxGameEvents = 20000;

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
            return TryEndCaptureContext(null, out errorMessage);
        }

        public bool TryEndCaptureContext(
            LudusCaptureContext expectedContext,
            out string errorMessage
        )
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

            if (
                expectedContext != null &&
                !ReferenceEquals(activeCaptureContext, expectedContext)
            )
            {
                errorMessage =
                    "O contexto informado não é mais o contexto ativo.";
                return false;
            }

            EndActiveCaptureContext(GetElapsedMilliseconds());

            errorMessage = string.Empty;
            return true;
        }

public bool TryRecordClick(
    float x,
    float y,
    out string errorMessage
)
{
    if (!HasActiveSession)
    {
        errorMessage =
            "Não existe sessão ativa para registrar um clique.";
        return false;
    }

    if (
        !TryValidateRawCapture(
            activeSession.capabilities.clicks,
            "clicks",
            x,
            y,
            out errorMessage
        )
    )
    {
        return false;
    }

    if (activeSession.clicks.Count >= MaxClicks)
    {
        errorMessage = "O limite de cliques da sessão foi atingido.";
        return false;
    }

    int timestamp = GetElapsedMilliseconds();

    activeSession.clicks.Add(
        new LudusClick
        {
            x = x,
            y = y,
            timestamp = timestamp,
        }
    );

    activeSession.metrics.totalClicks++;
    RegisterFirstAction(timestamp);

    errorMessage = string.Empty;
    return true;
}

public bool TryRecordMousePoint(
    float x,
    float y,
    out string errorMessage
)
{
    if (!HasActiveSession)
    {
        errorMessage =
            "Não existe sessão ativa para registrar um ponto do mouse.";
        return false;
    }

    if (
        !TryValidateRawCapture(
            activeSession.capabilities.mousePath,
            "mousePath",
            x,
            y,
            out errorMessage
        )
    )
    {
        return false;
    }

    if (activeSession.mousePath.Count >= MaxMousePathPoints)
    {
        errorMessage =
            "O limite de pontos de trajetória do mouse foi atingido.";
        return false;
    }

    int timestamp = GetElapsedMilliseconds();

    activeSession.mousePath.Add(
        new LudusPathPoint
        {
            x = x,
            y = y,
            t = timestamp,
        }
    );

    RegisterFirstAction(timestamp);

    errorMessage = string.Empty;
    return true;
}

public bool TryRecordDragPoint(
    float x,
    float y,
    string state,
    out string errorMessage
)
{
    if (!HasActiveSession)
    {
        errorMessage =
            "Não existe sessão ativa para registrar um ponto de arraste.";
        return false;
    }

    if (
        !TryValidateRawCapture(
            activeSession.capabilities.dragPath,
            "dragPath",
            x,
            y,
            out errorMessage
        )
    )
    {
        return false;
    }

    if (state != "start" && state != "move" && state != "end")
    {
        errorMessage = "O estado do ponto de arraste é inválido.";
        return false;
    }

    if (activeSession.dragPath.Count >= MaxDragPathPoints)
    {
        errorMessage =
            "O limite de pontos de trajetória de arraste foi atingido.";
        return false;
    }

    int timestamp = GetElapsedMilliseconds();

    activeSession.dragPath.Add(
        new LudusDragPoint
        {
            x = x,
            y = y,
            t = timestamp,
            state = state,
        }
    );

    RegisterFirstAction(timestamp);

    errorMessage = string.Empty;
    return true;
}

        public bool TryRecordTrackedInteraction(
            LudusTrackedInteraction interaction,
            out string errorMessage
        )
        {
            if (!HasActiveSession)
            {
                errorMessage =
                    "Não existe sessão ativa para registrar uma interação acompanhada.";
                return false;
            }

            if (!HasActiveCaptureContext)
            {
                errorMessage =
                    "Não existe contexto de captura ativo para registrar uma interação acompanhada.";
                return false;
            }

            if (!activeSession.capabilities.customEvents)
            {
                errorMessage =
                    "A capacidade customEvents deve estar habilitada para registrar interações acompanhadas.";
                return false;
            }

            if (interaction == null)
            {
                errorMessage =
                    "A interação acompanhada não pode ser nula.";
                return false;
            }

            if (!interaction.TryValidate(out errorMessage))
            {
                return false;
            }

            if (
                interaction.HasPosition &&
                !HasValidPoint(interaction.PositionX, interaction.PositionY)
            )
            {
                errorMessage =
                    "A interação acompanhada possui coordenadas inválidas para o viewport.";
                return false;
            }

            if (
                interaction.HasDragSummary &&
                (
                    !HasValidPoint(
                        interaction.DragStartX,
                        interaction.DragStartY
                    ) ||
                    !HasValidPoint(
                        interaction.DragEndX,
                        interaction.DragEndY
                    )
                )
            )
            {
                errorMessage =
                    "O arraste acompanhado possui coordenadas inválidas para o viewport.";
                return false;
            }

            if (activeSession.gameEvents.Count >= MaxGameEvents)
            {
                errorMessage =
                    "O limite de eventos da sessão foi atingido.";
                return false;
            }

            int timestamp = GetElapsedMilliseconds();

            activeSession.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = "TrackedInteraction",
                    timestamp = timestamp,
                    payloadJson = interaction.CreatePayload(
                        activeContextInstanceId
                    ),
                }
            );

            RegisterFirstAction(timestamp);
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

private bool TryValidateRawCapture(
    bool capabilityEnabled,
    string capabilityName,
    float x,
    float y,
    out string errorMessage
)
{
    if (!HasActiveCaptureContext)
    {
        errorMessage =
            "Não existe contexto de captura ativo para registrar interação.";
        return false;
    }

    if (!capabilityEnabled)
    {
        errorMessage =
            $"A capacidade {capabilityName} está desativada.";
        return false;
    }

    if (!HasValidPoint(x, y))
    {
        errorMessage =
            "A interação possui coordenadas inválidas para o viewport.";
        return false;
    }

    errorMessage = string.Empty;
    return true;
}

private bool HasValidPoint(float x, float y)
{
    if (
        float.IsNaN(x) ||
        float.IsInfinity(x) ||
        float.IsNaN(y) ||
        float.IsInfinity(y)
    )
    {
        return false;
    }

    if (activeSession.viewport.coordinateUnit == "normalized")
    {
        return x >= 0f && x <= 1f && y >= 0f && y <= 1f;
    }

    return
        x >= 0f &&
        x <= activeSession.viewport.widthPx &&
        y >= 0f &&
        y <= activeSession.viewport.heightPx;
}

private void RegisterFirstAction(int timestamp)
{
    if (activeSession.metrics.firstActionMs < 0)
    {
        activeSession.metrics.firstActionMs = timestamp;
    }
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
