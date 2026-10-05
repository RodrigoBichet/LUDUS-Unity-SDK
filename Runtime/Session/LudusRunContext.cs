using System;
using UnityEngine;

namespace LudusSDK
{
    /// <summary>
    /// Identifica uma execução contínua do jogo. Várias sessões ou tentativas
    /// podem compartilhar o mesmo runId sem perder seus sessionId próprios.
    /// </summary>
    public static class LudusRunContext
    {
        private static string currentRunId;
        private static int attemptCounter;

        public static string CurrentRunId => EnsureRun();

        public static string BeginNewRun()
        {
            currentRunId = Guid.NewGuid().ToString("N");
            attemptCounter = 0;
            LudusRunArchive.Reset(currentRunId);
            return currentRunId;
        }

        internal static int NextAttemptNumber()
        {
            EnsureRun();
            attemptCounter++;
            return attemptCounter;
        }

        private static string EnsureRun()
        {
            if (string.IsNullOrWhiteSpace(currentRunId))
            {
                BeginNewRun();
            }

            return currentRunId;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetWhenPlayerStarts()
        {
            currentRunId = string.Empty;
            attemptCounter = 0;
            LudusRunArchive.Reset(string.Empty);
        }
    }
}
