using System;
using System.Collections.Generic;
using UnityEngine;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusSession
    {
        public string schemaVersion = LudusSdkInfo.SchemaVersion;
        public string captureMode = LudusSdkInfo.CaptureMode;
        public string source = LudusSdkInfo.Source;
        public string sourceVersion = LudusSdkInfo.SourceVersion;
        public string ingestionMethod = LudusSdkInfo.DirectApiIngestionMethod;

        public LudusCapabilities capabilities;
        public string sessionId;
        public string studentId;
        public string playerId;
        public string gameId;
        public string gameVersion;
        public string platform;
        public string startedAt;
        public string endedAt;
        public int durationMs;
        public LudusViewport viewport;
        public LudusSessionMetrics metrics = new LudusSessionMetrics();

        public List<LudusClick> clicks = new List<LudusClick>();
        public List<LudusPathPoint> mousePath = new List<LudusPathPoint>();
        public List<LudusDragPoint> dragPath = new List<LudusDragPoint>();
        public List<LudusGameEvent> gameEvents = new List<LudusGameEvent>();
        public List<LudusScreenshot> screenshots = new List<LudusScreenshot>();

        private LudusSession() { }

        public static LudusSession Create(
            LudusSdkConfig config,
            LudusParticipant participant,
            LudusViewport viewport
        )
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (!config.TryValidateForSession(out string configError))
            {
                throw new ArgumentException(configError, nameof(config));
            }

            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (!participant.TryValidate(out string participantError))
            {
                throw new ArgumentException(participantError, nameof(participant));
            }

            if (viewport == null || viewport.widthPx < 1 || viewport.heightPx < 1)
            {
                throw new ArgumentException(
                    "viewport deve possuir largura e altura maiores que zero.",
                    nameof(viewport)
                );
            }

            return new LudusSession
            {
                capabilities = config.capabilities.Clone(),
                sessionId = Guid.NewGuid().ToString("N"),
                studentId = participant.studentId,
                playerId = participant.playerId,
                gameId = config.GetResolvedGameId(),
                gameVersion = config.GetResolvedGameVersion(),
                platform = Application.platform.ToString(),
                startedAt = DateTimeOffset.UtcNow.ToString("O"),
                viewport = viewport,
            };
        }

        public void End(int sessionDurationMs)
        {
            durationMs = Math.Max(0, sessionDurationMs);
            endedAt = DateTimeOffset.UtcNow.ToString("O");
        }
    }
}
