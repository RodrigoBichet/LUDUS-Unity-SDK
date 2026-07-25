using System;
using System.Diagnostics;

namespace LudusSDK
{
    public sealed class LudusSessionLifecycle
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private LudusSession activeSession;

        public bool HasActiveSession => activeSession != null;

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

            activeSession.End(GetElapsedMilliseconds());
            stopwatch.Stop();

            LastCompletedSession = activeSession;
            activeSession = null;

            return LudusJsonSerializer.TrySerialize(
                LastCompletedSession,
                out json,
                out errorMessage
            );
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