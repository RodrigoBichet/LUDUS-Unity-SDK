using System;
using System.Collections.Generic;
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
        private const int MaxScreenshots = 20;
        private const int MaxScreenshotBase64Length = 2800000;

        private sealed class ContextActivityState
        {
            public string instanceId;
            public string visualReferenceKey;
            public bool captureVisualReference;
            public int startedAt;
            public int clickCount;
            public int dragCount;
            public int trackedInteractionCount;
            public int durationMs;
            public int order;
            public bool ended;
            public bool aggregateRegistered;
            public bool candidateEvaluated;
            public LudusScreenshot screenshotCandidate;
        }

        private sealed class ContextActivityAggregate
        {
            public int clickCount;
            public int dragCount;
            public int trackedInteractionCount;
            public int durationMs;
            public int firstOrder;

            public int InteractionCount =>
                clickCount + dragCount + trackedInteractionCount;
        }

        private readonly Dictionary<string, ContextActivityState>
            contextActivityByInstanceId =
                new Dictionary<string, ContextActivityState>();

        private readonly Dictionary<string, ContextActivityAggregate>
            contextActivityByVisualKey =
                new Dictionary<string, ContextActivityAggregate>();

        private readonly Dictionary<string, LudusScreenshot>
            retainedAutomaticScreenshots =
                new Dictionary<string, LudusScreenshot>();

        private ContextActivityState activeContextActivity;
        private int maxAutomaticScreenshots = 4;
        private int nextContextOrder;

        public bool HasActiveSession => activeSession != null;

        public bool HasActiveCaptureContext =>
            activeCaptureContext != null &&
            !string.IsNullOrWhiteSpace(activeContextInstanceId);

        public LudusSession LastCompletedSession { get; private set; }

        public string ActiveCaptureContextInstanceId =>
            HasActiveCaptureContext ? activeContextInstanceId : string.Empty;

        public int ScreenshotCount =>
            (activeSession?.screenshots?.Count ?? 0) +
            retainedAutomaticScreenshots.Count;

        internal bool HasRetainedAutomaticScreenshot(
            string visualReferenceKey
        )
        {
            return
                !string.IsNullOrWhiteSpace(visualReferenceKey) &&
                retainedAutomaticScreenshots.ContainsKey(visualReferenceKey);
        }

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
                activeContextActivity = null;
                contextActivityByInstanceId.Clear();
                contextActivityByVisualKey.Clear();
                retainedAutomaticScreenshots.Clear();
                maxAutomaticScreenshots = Math.Min(
                    MaxScreenshots,
                    Math.Max(1, config.maxScreenshotsPerSession)
                );
                nextContextOrder = 0;
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
            int startedAt = GetElapsedMilliseconds();

            activeContextActivity = new ContextActivityState
            {
                instanceId = activeContextInstanceId,
                visualReferenceKey = context.visualReferenceKey,
                captureVisualReference = context.captureVisualReference,
                startedAt = startedAt,
                order = nextContextOrder++,
            };
            contextActivityByInstanceId[activeContextInstanceId] =
                activeContextActivity;

            activeSession.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = "CaptureContextStarted",
                    timestamp = startedAt,
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
    if (activeContextActivity != null)
    {
        activeContextActivity.clickCount++;
    }
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

    if (state == "start" && activeContextActivity != null)
    {
        activeContextActivity.dragCount++;
    }

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

            if (activeContextActivity != null)
            {
                activeContextActivity.trackedInteractionCount++;
            }

            RegisterFirstAction(timestamp);
            errorMessage = string.Empty;
            return true;
        }

        internal bool TryRecordGameEvent(
            string eventType,
            string payloadJson,
            out string errorMessage
        )
        {
            if (!HasActiveSession)
            {
                errorMessage =
                    "Não existe sessão ativa para registrar um evento do jogo.";
                return false;
            }

            string normalizedEventType = eventType?.Trim() ?? string.Empty;
            string normalizedPayload = string.IsNullOrWhiteSpace(payloadJson)
                ? "{}"
                : payloadJson.Trim();

            if (
                normalizedEventType.Length == 0 ||
                normalizedEventType.Length > 100
            )
            {
                errorMessage =
                    "O tipo do evento deve possuir entre 1 e 100 caracteres.";
                return false;
            }

            if (
                normalizedPayload.Length > 100000 ||
                !LudusJsonSerializer.IsValidJsonObject(normalizedPayload)
            )
            {
                errorMessage =
                    "O payload do evento deve ser um objeto JSON válido de até 100000 caracteres.";
                return false;
            }

            bool isPhaseEvent = normalizedEventType.StartsWith(
                "Phase",
                StringComparison.Ordinal
            );
            bool isCorrectWrongEvent =
                normalizedEventType == "CorrectMatch" ||
                normalizedEventType == "WrongMatch";
            bool isCategoryEvent = normalizedEventType.StartsWith(
                "Category",
                StringComparison.Ordinal
            );

            if (isPhaseEvent && !activeSession.capabilities.phaseEvents)
            {
                errorMessage =
                    "A capacidade phaseEvents deve estar habilitada para registrar eventos de fase.";
                return false;
            }

            if (
                isCorrectWrongEvent &&
                !activeSession.capabilities.correctWrong
            )
            {
                errorMessage =
                    "A capacidade correctWrong deve estar habilitada para registrar acertos e erros.";
                return false;
            }

            if (
                isCategoryEvent &&
                !activeSession.capabilities.categoryEvents
            )
            {
                errorMessage =
                    "A capacidade categoryEvents deve estar habilitada para registrar categorias.";
                return false;
            }

            if (
                !isPhaseEvent &&
                !isCorrectWrongEvent &&
                !isCategoryEvent &&
                !activeSession.capabilities.customEvents
            )
            {
                errorMessage =
                    "A capacidade customEvents deve estar habilitada para registrar este evento do jogo.";
                return false;
            }

            if (activeSession.gameEvents.Count >= MaxGameEvents)
            {
                errorMessage = "O limite de eventos da sessão foi atingido.";
                return false;
            }

            int timestamp = GetElapsedMilliseconds();

            activeSession.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = normalizedEventType,
                    timestamp = timestamp,
                    payloadJson = normalizedPayload,
                }
            );

            if (normalizedEventType == "CorrectMatch")
            {
                activeSession.metrics.totalCorrect++;
            }
            else if (normalizedEventType == "WrongMatch")
            {
                activeSession.metrics.totalWrong++;
            }

            RegisterFirstAction(timestamp);
            errorMessage = string.Empty;
            return true;
        }

        public bool TryRecordScreenshot(
            string contextInstanceId,
            int widthPx,
            int heightPx,
            string screenshotBase64,
            out string errorMessage
        )
        {
            if (!TryValidateScreenshotData(
                contextInstanceId,
                widthPx,
                heightPx,
                screenshotBase64,
                out errorMessage
            ))
            {
                return false;
            }

            if (activeSession.screenshots.Count >= MaxScreenshots)
            {
                errorMessage =
                    "O limite absoluto de capturas visuais da sessão foi atingido.";
                return false;
            }

            activeSession.screenshots.Add(
                new LudusScreenshot
                {
                    contextInstanceId = contextInstanceId,
                    timestamp = GetElapsedMilliseconds(),
                    widthPx = widthPx,
                    heightPx = heightPx,
                    screenshotBase64 = screenshotBase64,
                }
            );

            errorMessage = string.Empty;
            return true;
        }

        public bool TryRecordAutomaticScreenshotCandidate(
            string contextInstanceId,
            int widthPx,
            int heightPx,
            string screenshotBase64,
            out string errorMessage
        )
        {
            if (!TryValidateScreenshotData(
                contextInstanceId,
                widthPx,
                heightPx,
                screenshotBase64,
                out errorMessage
            ))
            {
                return false;
            }

            if (
                !contextActivityByInstanceId.TryGetValue(
                    contextInstanceId,
                    out ContextActivityState activityState
                ) ||
                !activityState.captureVisualReference
            )
            {
                errorMessage =
                    "O contexto não foi marcado para captura visual automática.";
                return false;
            }

            if (activityState.screenshotCandidate != null)
            {
                errorMessage =
                    "Este recorte já possui uma imagem candidata nesta ativação.";
                return false;
            }

            activityState.screenshotCandidate = new LudusScreenshot
            {
                contextInstanceId = contextInstanceId,
                timestamp = GetElapsedMilliseconds(),
                widthPx = widthPx,
                heightPx = heightPx,
                screenshotBase64 = screenshotBase64,
            };

            TryRetainAutomaticScreenshot(activityState);
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

            AppendRetainedAutomaticScreenshots();

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

        private bool TryValidateScreenshotData(
            string contextInstanceId,
            int widthPx,
            int heightPx,
            string screenshotBase64,
            out string errorMessage
        )
        {
            if (!HasActiveSession)
            {
                errorMessage =
                    "Não existe sessão ativa para registrar uma captura visual.";
                return false;
            }

            if (!activeSession.capabilities.screenshots)
            {
                errorMessage = "A capacidade screenshots está desativada.";
                return false;
            }

            if (
                string.IsNullOrWhiteSpace(contextInstanceId) ||
                contextInstanceId.Length > 128 ||
                widthPx < 1 ||
                widthPx > 8192 ||
                heightPx < 1 ||
                heightPx > 8192 ||
                string.IsNullOrWhiteSpace(screenshotBase64) ||
                screenshotBase64.Length > MaxScreenshotBase64Length
            )
            {
                errorMessage = "A captura visual possui dados inválidos.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void RegisterContextAggregate(
            ContextActivityState activityState
        )
        {
            if (
                activityState == null ||
                activityState.aggregateRegistered ||
                !activityState.captureVisualReference
            )
            {
                return;
            }

            if (
                !contextActivityByVisualKey.TryGetValue(
                    activityState.visualReferenceKey,
                    out ContextActivityAggregate aggregate
                )
            )
            {
                aggregate = new ContextActivityAggregate
                {
                    firstOrder = activityState.order,
                };
                contextActivityByVisualKey[activityState.visualReferenceKey] =
                    aggregate;
            }

            aggregate.clickCount += activityState.clickCount;
            aggregate.dragCount += activityState.dragCount;
            aggregate.trackedInteractionCount +=
                activityState.trackedInteractionCount;
            aggregate.durationMs += activityState.durationMs;
            aggregate.firstOrder = Math.Min(
                aggregate.firstOrder,
                activityState.order
            );
            activityState.aggregateRegistered = true;
        }

        private void TryRetainAutomaticScreenshot(
            ContextActivityState activityState
        )
        {
            if (
                activityState == null ||
                !activityState.ended ||
                activityState.screenshotCandidate == null ||
                activityState.candidateEvaluated
            )
            {
                return;
            }

            RegisterContextAggregate(activityState);
            activityState.candidateEvaluated = true;

            if (
                retainedAutomaticScreenshots.ContainsKey(
                    activityState.visualReferenceKey
                )
            )
            {
                activityState.screenshotCandidate = null;
                return;
            }

            if (
                retainedAutomaticScreenshots.Count < maxAutomaticScreenshots
            )
            {
                retainedAutomaticScreenshots[
                    activityState.visualReferenceKey
                ] = activityState.screenshotCandidate;
                activityState.screenshotCandidate = null;
                return;
            }

            string leastRelevantKey = FindLeastRelevantVisualKey();

            if (
                string.IsNullOrEmpty(leastRelevantKey) ||
                CompareVisualRelevance(
                    activityState.visualReferenceKey,
                    leastRelevantKey
                ) <= 0
            )
            {
                activityState.screenshotCandidate = null;
                return;
            }

            retainedAutomaticScreenshots.Remove(leastRelevantKey);
            retainedAutomaticScreenshots[
                activityState.visualReferenceKey
            ] = activityState.screenshotCandidate;
            activityState.screenshotCandidate = null;
        }

        private string FindLeastRelevantVisualKey()
        {
            string leastRelevantKey = null;

            foreach (string key in retainedAutomaticScreenshots.Keys)
            {
                if (
                    leastRelevantKey == null ||
                    CompareVisualRelevance(key, leastRelevantKey) < 0
                )
                {
                    leastRelevantKey = key;
                }
            }

            return leastRelevantKey;
        }

        private int CompareVisualRelevance(string leftKey, string rightKey)
        {
            ContextActivityAggregate left =
                contextActivityByVisualKey[leftKey];
            ContextActivityAggregate right =
                contextActivityByVisualKey[rightKey];
            int interactionComparison = left.InteractionCount.CompareTo(
                right.InteractionCount
            );

            if (interactionComparison != 0)
            {
                return interactionComparison;
            }

            int durationComparison = left.durationMs.CompareTo(
                right.durationMs
            );

            if (durationComparison != 0)
            {
                return durationComparison;
            }

            // Em empate completo, o recorte visto primeiro é preservado.
            return right.firstOrder.CompareTo(left.firstOrder);
        }

        private void AppendRetainedAutomaticScreenshots()
        {
            int availableSlots = Math.Min(
                maxAutomaticScreenshots - activeSession.screenshots.Count,
                MaxScreenshots - activeSession.screenshots.Count
            );

            if (availableSlots <= 0)
            {
                return;
            }

            List<string> orderedKeys = new List<string>(
                retainedAutomaticScreenshots.Keys
            );
            orderedKeys.Sort(
                (left, right) => -CompareVisualRelevance(left, right)
            );

            foreach (string key in orderedKeys)
            {
                if (availableSlots <= 0)
                {
                    break;
                }

                activeSession.screenshots.Add(
                    retainedAutomaticScreenshots[key]
                );
                availableSlots--;
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

            if (activeContextActivity != null)
            {
                activeContextActivity.durationMs = Math.Max(
                    0,
                    timestamp - activeContextActivity.startedAt
                );
                activeContextActivity.ended = true;
                RegisterContextAggregate(activeContextActivity);
                TryRetainAutomaticScreenshot(activeContextActivity);
            }

            activeCaptureContext = null;
            activeContextInstanceId = string.Empty;
            activeContextActivity = null;
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
