using System;
using System.Collections.Generic;
using System.Text;

namespace LudusSDK
{
    /// <summary>
    /// Mantém as sessões concluídas somente durante a execução atual e produz
    /// um pacote para exportação manual sem alterar cada sessão canônica.
    /// </summary>
    public static class LudusRunArchive
    {
        private sealed class ArchivedSession
        {
            public string sessionId;
            public string json;
        }

        private static readonly List<ArchivedSession> Sessions =
            new List<ArchivedSession>();
        private static string archivedRunId = string.Empty;

        public static int CompletedSessionCount => Sessions.Count;

        internal static void Reset(string runId)
        {
            Sessions.Clear();
            archivedRunId = runId ?? string.Empty;
        }

        internal static void RecordCompleted(
            LudusSession session,
            string json
        )
        {
            if (
                session == null ||
                string.IsNullOrWhiteSpace(session.sessionId) ||
                string.IsNullOrWhiteSpace(json)
            )
            {
                return;
            }

            EnsureRun(session.runId);
            int existingIndex = Sessions.FindIndex(
                item => string.Equals(
                    item.sessionId,
                    session.sessionId,
                    StringComparison.Ordinal
                )
            );
            ArchivedSession archived = new ArchivedSession
            {
                sessionId = session.sessionId,
                json = json,
            };

            if (existingIndex >= 0)
            {
                Sessions[existingIndex] = archived;
            }
            else
            {
                Sessions.Add(archived);
            }
        }

        public static bool TryCreateBundle(
            string activeSessionId,
            string activeSessionJson,
            out string bundleJson,
            out string errorMessage
        )
        {
            string runId = LudusRunContext.CurrentRunId;
            EnsureRun(runId);

            List<string> sessionJsons = new List<string>();
            foreach (ArchivedSession archived in Sessions)
            {
                if (
                    !string.IsNullOrWhiteSpace(activeSessionId) &&
                    string.Equals(
                        archived.sessionId,
                        activeSessionId,
                        StringComparison.Ordinal
                    )
                )
                {
                    continue;
                }

                sessionJsons.Add(archived.json);
            }

            if (!string.IsNullOrWhiteSpace(activeSessionJson))
            {
                sessionJsons.Add(activeSessionJson);
            }

            if (sessionJsons.Count == 0)
            {
                bundleJson = string.Empty;
                errorMessage =
                    "Ainda não há uma sessão iniciada ou concluída nesta execução.";
                return false;
            }

            StringBuilder builder = new StringBuilder(4096);
            builder.Append('{');
            WriteString(builder, "bundleVersion", "1.0.0");
            builder.Append(',');
            WriteString(builder, "type", "ludus-session-bundle");
            builder.Append(',');
            WriteString(builder, "runId", runId);
            builder.Append(',');
            WriteString(
                builder,
                "exportedAt",
                DateTimeOffset.UtcNow.ToString("O")
            );
            builder.Append(",\"sessions\":[");

            for (int index = 0; index < sessionJsons.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                builder.Append(sessionJsons[index]);
            }

            builder.Append("]}");
            bundleJson = builder.ToString();
            errorMessage = string.Empty;
            return true;
        }

        private static void EnsureRun(string runId)
        {
            string normalizedRunId = runId ?? string.Empty;
            if (
                !string.Equals(
                    archivedRunId,
                    normalizedRunId,
                    StringComparison.Ordinal
                )
            )
            {
                Reset(normalizedRunId);
            }
        }

        private static void WriteString(
            StringBuilder builder,
            string propertyName,
            string value
        )
        {
            builder.Append('"');
            AppendEscaped(builder, propertyName);
            builder.Append("\":\"");
            AppendEscaped(builder, value ?? string.Empty);
            builder.Append('"');
        }

        private static void AppendEscaped(StringBuilder builder, string value)
        {
            foreach (char character in value)
            {
                switch (character)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (character < 32)
                        {
                            builder.Append("\\u");
                            builder.Append(((int)character).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(character);
                        }
                        break;
                }
            }
        }
    }
}
