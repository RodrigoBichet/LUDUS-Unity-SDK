using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LudusSDK
{
    public static class LudusJsonSerializer
    {
        public static bool TrySerialize(
            LudusSession session,
            out string json,
            out string errorMessage
        )
        {
            json = string.Empty;

            if (!TryValidate(session, out errorMessage))
            {
                return false;
            }

            StringBuilder builder = new StringBuilder(4096);

            builder.Append('{');

WriteProperty(builder, "schemaVersion", session.schemaVersion, true);
WriteProperty(builder, "captureMode", session.captureMode, true);
WriteProperty(builder, "source", session.source, true);
WriteProperty(builder, "sourceVersion", session.sourceVersion, true);
WriteProperty(builder, "ingestionMethod", session.ingestionMethod, true);

WritePropertyName(builder, "capabilities", true);
WriteCapabilities(builder, session.capabilities);
builder.Append(',');

WriteProperty(builder, "sessionId", session.sessionId, true);
WriteProperty(builder, "studentId", session.studentId, true);
WriteProperty(builder, "playerId", session.playerId, true);
WriteProperty(builder, "gameId", session.gameId, true);
WriteProperty(builder, "gameVersion", session.gameVersion, true);
WriteProperty(builder, "platform", session.platform, true);
WriteProperty(builder, "startedAt", session.startedAt, true);

if (!string.IsNullOrWhiteSpace(session.endedAt))
{
    WriteProperty(builder, "endedAt", session.endedAt, true);
}

WriteProperty(builder, "durationMs", session.durationMs, true);

WritePropertyName(builder, "viewport", true);
WriteViewport(builder, session.viewport);
builder.Append(',');

WritePropertyName(builder, "metrics", true);
WriteMetrics(builder, session.metrics);
builder.Append(',');

WritePropertyName(builder, "clicks", true);
WriteClicks(builder, session.clicks);
builder.Append(',');

WritePropertyName(builder, "mousePath", true);
WriteMousePath(builder, session.mousePath);
builder.Append(',');

WritePropertyName(builder, "dragPath", true);
WriteDragPath(builder, session.dragPath);
builder.Append(',');

WritePropertyName(builder, "gameEvents", true);
WriteGameEvents(builder, session.gameEvents);
builder.Append(',');

WritePropertyName(builder, "screenshots", true);
WriteScreenshots(builder, session.screenshots);

builder.Append('}');

            json = builder.ToString();
            errorMessage = string.Empty;
            return true;
        }

        private static bool TryValidate(
            LudusSession session,
            out string errorMessage
        )
        {
            if (session == null)
            {
                errorMessage = "A sessão não pode ser nula.";
                return false;
            }

            if (
                session.schemaVersion != LudusSdkInfo.SchemaVersion ||
                session.captureMode != LudusSdkInfo.CaptureMode ||
                session.source != LudusSdkInfo.Source ||
                session.sourceVersion != LudusSdkInfo.SourceVersion ||
                session.ingestionMethod != LudusSdkInfo.DirectApiIngestionMethod
            )
            {
                errorMessage = "Os metadados do SDK não atendem ao contrato LUDUS.";
                return false;
            }

            if (
                string.IsNullOrWhiteSpace(session.sessionId) ||
                session.sessionId.Length > 128 ||
                !HasMongoObjectId(session.studentId) ||
                string.IsNullOrWhiteSpace(session.playerId) ||
                session.playerId.Length > 200 ||
                !HasValidGameId(session.gameId) ||
                string.IsNullOrWhiteSpace(session.gameVersion) ||
                session.gameVersion.Length > 50 ||
                string.IsNullOrWhiteSpace(session.platform) ||
                session.platform.Length > 50 ||
                !HasValidDateTime(session.startedAt) ||
                (!string.IsNullOrWhiteSpace(session.endedAt) &&
                    !HasValidDateTime(session.endedAt))
            )
            {
                errorMessage = "A sessão possui campos obrigatórios inválidos.";
                return false;
            }

            if (session.durationMs < 0)
            {
                errorMessage = "durationMs não pode ser negativo.";
                return false;
            }

            if (
                session.capabilities == null ||
                session.viewport == null ||
                session.metrics == null ||
                session.clicks == null ||
                session.mousePath == null ||
                session.dragPath == null ||
                session.gameEvents == null ||
                session.screenshots == null
            )
            {
                errorMessage = "A sessão possui estruturas de telemetria ausentes.";
                return false;
            }

            if (
                session.viewport.widthPx < 1 ||
                session.viewport.heightPx < 1 ||
                (session.viewport.coordinateUnit != "pixel" &&
                    session.viewport.coordinateUnit != "normalized") ||
                (session.viewport.coordinateOrigin != "top-left" &&
                    session.viewport.coordinateOrigin != "bottom-left")
            )
            {
                errorMessage = "viewport inválido.";
                return false;
            }

            if (
                !IsFinite(session.metrics.avgTimeBetweenActionsMs) ||
                session.metrics.avgTimeBetweenActionsMs < 0f ||
                !IsFinite(session.metrics.totalInactivityMs) ||
                session.metrics.totalInactivityMs < 0f
            )
            {
                errorMessage = "As métricas possuem valores inválidos.";
                return false;
            }

            if (
                session.capabilities.clicks == false &&
                session.clicks.Count > 0
            )
            {
                errorMessage = "Há cliques, mas a capacidade clicks está desativada.";
                return false;
            }

            if (
                session.capabilities.mousePath == false &&
                session.mousePath.Count > 0
            )
            {
                errorMessage =
                    "Há pontos de mouse, mas a capacidade mousePath está desativada.";
                return false;
            }

            if (
                session.capabilities.dragPath == false &&
                session.dragPath.Count > 0
            )
            {
                errorMessage =
                    "Há pontos de arraste, mas a capacidade dragPath está desativada.";
                return false;
            }

            if (
                session.capabilities.screenshots == false &&
                session.screenshots.Count > 0
            )
            {
                errorMessage =
                    "Há screenshots, mas a capacidade screenshots está desativada.";
                return false;
            }

            return
                TryValidateClicks(session, out errorMessage) &&
                TryValidateMousePath(session, out errorMessage) &&
                TryValidateDragPath(session, out errorMessage) &&
                TryValidateGameEvents(session, out errorMessage) &&
                TryValidateScreenshots(session, out errorMessage);
        }

        private static bool TryValidateClicks(
            LudusSession session,
            out string errorMessage
        )
        {
            foreach (LudusClick click in session.clicks)
            {
                if (
                    click == null ||
                    !HasValidPoint(
                        click.x,
                        click.y,
                        click.timestamp,
                        session
                    )
                )
                {
                    errorMessage = "clicks possui ponto inválido.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryValidateMousePath(
            LudusSession session,
            out string errorMessage
        )
        {
            foreach (LudusPathPoint point in session.mousePath)
            {
                if (
                    point == null ||
                    !HasValidPoint(point.x, point.y, point.t, session)
                )
                {
                    errorMessage = "mousePath possui ponto inválido.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryValidateDragPath(
            LudusSession session,
            out string errorMessage
        )
        {
            foreach (LudusDragPoint point in session.dragPath)
            {
                if (
                    point == null ||
                    !HasValidPoint(point.x, point.y, point.t, session) ||
                    (!string.IsNullOrWhiteSpace(point.state) &&
                        point.state != "start" &&
                        point.state != "move" &&
                        point.state != "end" &&
                        point.state != "unknown")
                )
                {
                    errorMessage = "dragPath possui ponto inválido.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryValidateGameEvents(
            LudusSession session,
            out string errorMessage
        )
        {
            foreach (LudusGameEvent gameEvent in session.gameEvents)
            {
                if (
                    gameEvent == null ||
                    string.IsNullOrWhiteSpace(gameEvent.eventType) ||
                    gameEvent.eventType.Length > 100 ||
                    gameEvent.timestamp < 0 ||
                    gameEvent.timestamp > session.durationMs ||
                    !IsValidJsonObject(gameEvent.payloadJson)
                )
                {
                    errorMessage = "gameEvents possui evento ou payload inválido.";
                    return false;
                }

                if (
                    gameEvent.eventType.StartsWith("Phase") &&
                    !session.capabilities.phaseEvents
                )
                {
                    errorMessage =
                        "Há evento de fase, mas a capacidade phaseEvents está desativada.";
                    return false;
                }

                if (
                    (gameEvent.eventType == "CorrectMatch" ||
                        gameEvent.eventType == "WrongMatch") &&
                    !session.capabilities.correctWrong
                )
                {
                    errorMessage =
                        "Há evento de acerto/erro, mas a capacidade correctWrong está desativada.";
                    return false;
                }

                if (
                    gameEvent.eventType.StartsWith("Category") &&
                    !session.capabilities.categoryEvents
                )
                {
                    errorMessage =
                        "Há evento de categoria, mas a capacidade categoryEvents está desativada.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryValidateScreenshots(
            LudusSession session,
            out string errorMessage
        )
        {
            foreach (LudusScreenshot screenshot in session.screenshots)
            {
                if (
                    screenshot == null ||
                    screenshot.faseIndex < 0 ||
                    screenshot.timestamp < 0 ||
                    screenshot.timestamp > session.durationMs ||
                    string.IsNullOrWhiteSpace(screenshot.caminho) ||
                    screenshot.caminho.Length > 2048
                )
                {
                    errorMessage = "screenshots possui item inválido.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool HasValidPoint(
            float x,
            float y,
            int timestamp,
            LudusSession session
        )
        {
            if (
                !IsFinite(x) ||
                !IsFinite(y) ||
                timestamp < 0 ||
                timestamp > session.durationMs
            )
            {
                return false;
            }

            if (session.viewport.coordinateUnit == "normalized")
            {
                return x >= 0f && x <= 1f && y >= 0f && y <= 1f;
            }

            return
                x >= 0f &&
                x <= session.viewport.widthPx &&
                y >= 0f &&
                y <= session.viewport.heightPx;
        }

        private static bool HasMongoObjectId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 24)
            {
                return false;
            }

            foreach (char character in value)
            {
                bool isDigit = character >= '0' && character <= '9';
                bool isLowercaseHex = character >= 'a' && character <= 'f';
                bool isUppercaseHex = character >= 'A' && character <= 'F';

                if (!isDigit && !isLowercaseHex && !isUppercaseHex)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasValidGameId(string value)
        {
            if (
                string.IsNullOrWhiteSpace(value) ||
                value.Length > 100 ||
                !IsLowercaseLetterOrDigit(value[0])
            )
            {
                return false;
            }

            foreach (char character in value)
            {
                if (
                    !IsLowercaseLetterOrDigit(character) &&
                    character != '-'
                )
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLowercaseLetterOrDigit(char character)
        {
            return
                (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');
        }

        private static bool HasValidDateTime(string value)
        {
            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out _
            );
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void WriteCapabilities(
            StringBuilder builder,
            LudusCapabilities capabilities
        )
        {
            builder.Append('{');
            WriteProperty(builder, "clicks", capabilities.clicks, true);
            WriteProperty(builder, "mousePath", capabilities.mousePath, true);
            WriteProperty(builder, "dragPath", capabilities.dragPath, true);
            WriteProperty(builder, "screenshots", capabilities.screenshots, true);
            WriteProperty(builder, "inactivity", capabilities.inactivity, true);
            WriteProperty(builder, "focusEvents", capabilities.focusEvents, true);
            WriteProperty(builder, "phaseEvents", capabilities.phaseEvents, true);
            WriteProperty(builder, "correctWrong", capabilities.correctWrong, true);
            WriteProperty(builder, "categoryEvents", capabilities.categoryEvents, true);
            WriteProperty(builder, "customEvents", capabilities.customEvents, false);
            builder.Append('}');
        }

        private static void WriteViewport(
            StringBuilder builder,
            LudusViewport viewport
        )
        {
            builder.Append('{');
            WriteProperty(builder, "widthPx", viewport.widthPx, true);
            WriteProperty(builder, "heightPx", viewport.heightPx, true);
            WriteProperty(
                builder,
                "coordinateUnit",
                viewport.coordinateUnit,
                true
            );
            WriteProperty(
                builder,
                "coordinateOrigin",
                viewport.coordinateOrigin,
                false
            );
            builder.Append('}');
        }

        private static void WriteMetrics(
            StringBuilder builder,
            LudusSessionMetrics metrics
        )
        {
            builder.Append('{');
            WriteProperty(builder, "totalClicks", metrics.totalClicks, true);
            WriteProperty(builder, "totalCorrect", metrics.totalCorrect, true);
            WriteProperty(builder, "totalWrong", metrics.totalWrong, true);
            WriteProperty(builder, "firstActionMs", metrics.firstActionMs, true);
            WriteProperty(
                builder,
                "avgTimeBetweenActionsMs",
                metrics.avgTimeBetweenActionsMs,
                true
            );
            WriteProperty(builder, "inactivityCount", metrics.inactivityCount, true);
            WriteProperty(
                builder,
                "totalInactivityMs",
                metrics.totalInactivityMs,
                false
            );
            builder.Append('}');
        }

        private static void WriteClicks(
            StringBuilder builder,
            List<LudusClick> clicks
        )
        {
            builder.Append('[');

            for (int index = 0; index < clicks.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                LudusClick click = clicks[index];

                builder.Append('{');

                if (!string.IsNullOrWhiteSpace(click.element))
                {
                    WriteProperty(builder, "element", click.element, true);
                }

                WriteProperty(builder, "x", click.x, true);
                WriteProperty(builder, "y", click.y, true);
                WriteProperty(builder, "timestamp", click.timestamp, false);

                builder.Append('}');
            }

            builder.Append(']');
        }

        private static void WriteMousePath(
            StringBuilder builder,
            List<LudusPathPoint> mousePath
        )
        {
            builder.Append('[');

            for (int index = 0; index < mousePath.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                LudusPathPoint point = mousePath[index];

                builder.Append('{');
                WriteProperty(builder, "x", point.x, true);
                WriteProperty(builder, "y", point.y, true);
                WriteProperty(builder, "t", point.t, false);
                builder.Append('}');
            }

            builder.Append(']');
        }

        private static void WriteDragPath(
            StringBuilder builder,
            List<LudusDragPoint> dragPath
        )
        {
            builder.Append('[');

            for (int index = 0; index < dragPath.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                LudusDragPoint point = dragPath[index];

                builder.Append('{');

                if (!string.IsNullOrWhiteSpace(point.element))
                {
                    WriteProperty(builder, "element", point.element, true);
                }

                WriteProperty(builder, "x", point.x, true);
                WriteProperty(builder, "y", point.y, true);
                WriteProperty(builder, "t", point.t, true);

                if (!string.IsNullOrWhiteSpace(point.state))
                {
                    WriteProperty(builder, "state", point.state, false);
                }
                else
                {
                    WriteProperty(builder, "state", "unknown", false);
                }

                builder.Append('}');
            }

            builder.Append(']');
        }

        private static void WriteGameEvents(
            StringBuilder builder,
            List<LudusGameEvent> gameEvents
        )
        {
            builder.Append('[');

            for (int index = 0; index < gameEvents.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                LudusGameEvent gameEvent = gameEvents[index];

                builder.Append('{');
                WriteProperty(builder, "eventType", gameEvent.eventType, true);
                WriteProperty(builder, "timestamp", gameEvent.timestamp, true);
                WritePropertyName(builder, "payload", false);
                builder.Append(gameEvent.payloadJson.Trim());
                builder.Append('}');
            }

            builder.Append(']');
        }

        private static void WriteScreenshots(
            StringBuilder builder,
            List<LudusScreenshot> screenshots
        )
        {
            builder.Append('[');

            for (int index = 0; index < screenshots.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                LudusScreenshot screenshot = screenshots[index];

                builder.Append('{');

                if (screenshot.faseIndex >= 0)
                {
                    WriteProperty(builder, "faseIndex", screenshot.faseIndex, true);
                }

                if (!string.IsNullOrWhiteSpace(screenshot.phaseId))
                {
                    WriteProperty(builder, "phaseId", screenshot.phaseId, true);
                }

                WriteProperty(builder, "timestamp", screenshot.timestamp, true);
                WriteProperty(builder, "caminho", screenshot.caminho, false);

                builder.Append('}');
            }

            builder.Append(']');
        }

        private static void WritePropertyName(
            StringBuilder builder,
            string name,
            bool appendComma
        )
        {
            WriteString(builder, name);
            builder.Append(':');
        }

        private static void WriteProperty(
            StringBuilder builder,
            string name,
            string value,
            bool appendComma
        )
        {
            WriteString(builder, name);
            builder.Append(':');
            WriteString(builder, value);

            if (appendComma)
            {
                builder.Append(',');
            }
        }

        private static void WriteProperty(
            StringBuilder builder,
            string name,
            bool value,
            bool appendComma
        )
        {
            WriteString(builder, name);
            builder.Append(':');
            builder.Append(value ? "true" : "false");

            if (appendComma)
            {
                builder.Append(',');
            }
        }

        private static void WriteProperty(
            StringBuilder builder,
            string name,
            int value,
            bool appendComma
        )
        {
            WriteString(builder, name);
            builder.Append(':');
            builder.Append(value.ToString(CultureInfo.InvariantCulture));

            if (appendComma)
            {
                builder.Append(',');
            }
        }

        private static void WriteProperty(
            StringBuilder builder,
            string name,
            float value,
            bool appendComma
        )
        {
            WriteString(builder, name);
            builder.Append(':');
            builder.Append(value.ToString("R", CultureInfo.InvariantCulture));

            if (appendComma)
            {
                builder.Append(',');
            }
        }

        private static void WriteString(StringBuilder builder, string value)
        {
            builder.Append('"');

            foreach (char character in value ?? string.Empty)
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
                        if (character < ' ')
                        {
                            builder.Append("\\u");
                            builder.Append(
                                ((int)character).ToString(
                                    "x4",
                                    CultureInfo.InvariantCulture
                                )
                            );
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            builder.Append('"');
        }

        private static bool IsValidJsonObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            int index = 0;
            SkipWhitespace(json, ref index);

            if (!TryReadObject(json, ref index))
            {
                return false;
            }

            SkipWhitespace(json, ref index);
            return index == json.Length;
        }

        private static bool TryReadObject(string json, ref int index)
        {
            if (!TryConsume(json, ref index, '{'))
            {
                return false;
            }

            SkipWhitespace(json, ref index);

            if (TryConsume(json, ref index, '}'))
            {
                return true;
            }

            while (true)
            {
                if (!TryReadString(json, ref index))
                {
                    return false;
                }

                SkipWhitespace(json, ref index);

                if (!TryConsume(json, ref index, ':'))
                {
                    return false;
                }

                SkipWhitespace(json, ref index);

                if (!TryReadValue(json, ref index))
                {
                    return false;
                }

                SkipWhitespace(json, ref index);

                if (TryConsume(json, ref index, '}'))
                {
                    return true;
                }

                if (!TryConsume(json, ref index, ','))
                {
                    return false;
                }

                SkipWhitespace(json, ref index);
            }
        }

        private static bool TryReadArray(string json, ref int index)
        {
            if (!TryConsume(json, ref index, '['))
            {
                return false;
            }

            SkipWhitespace(json, ref index);

            if (TryConsume(json, ref index, ']'))
            {
                return true;
            }

            while (true)
            {
                if (!TryReadValue(json, ref index))
                {
                    return false;
                }

                SkipWhitespace(json, ref index);

                if (TryConsume(json, ref index, ']'))
                {
                    return true;
                }

                if (!TryConsume(json, ref index, ','))
                {
                    return false;
                }

                SkipWhitespace(json, ref index);
            }
        }

        private static bool TryReadValue(string json, ref int index)
        {
            if (index >= json.Length)
            {
                return false;
            }

            char current = json[index];

            if (current == '{')
            {
                return TryReadObject(json, ref index);
            }

            if (current == '[')
            {
                return TryReadArray(json, ref index);
            }

            if (current == '"')
            {
                return TryReadString(json, ref index);
            }

            if (current == 't')
            {
                return TryReadLiteral(json, ref index, "true");
            }

            if (current == 'f')
            {
                return TryReadLiteral(json, ref index, "false");
            }

            if (current == 'n')
            {
                return TryReadLiteral(json, ref index, "null");
            }

            return TryReadNumber(json, ref index);
        }

        private static bool TryReadString(string json, ref int index)
        {
            if (!TryConsume(json, ref index, '"'))
            {
                return false;
            }

            while (index < json.Length)
            {
                char current = json[index++];

                if (current == '"')
                {
                    return true;
                }

                if (current == '\\')
                {
                    if (index >= json.Length)
                    {
                        return false;
                    }

                    char escapeCharacter = json[index++];

                    if (escapeCharacter == 'u')
                    {
                        for (int count = 0; count < 4; count++)
                        {
                            if (
                                index >= json.Length ||
                                !IsHexadecimal(json[index++])
                            )
                            {
                                return false;
                            }
                        }
                    }
                    else if (
                        escapeCharacter != '"' &&
                        escapeCharacter != '\\' &&
                        escapeCharacter != '/' &&
                        escapeCharacter != 'b' &&
                        escapeCharacter != 'f' &&
                        escapeCharacter != 'n' &&
                        escapeCharacter != 'r' &&
                        escapeCharacter != 't'
                    )
                    {
                        return false;
                    }
                }
                else if (current < ' ')
                {
                    return false;
                }
            }

            return false;
        }

        private static bool TryReadLiteral(
            string json,
            ref int index,
            string literal
        )
        {
            if (index + literal.Length > json.Length)
            {
                return false;
            }

            for (int count = 0; count < literal.Length; count++)
            {
                if (json[index + count] != literal[count])
                {
                    return false;
                }
            }

            index += literal.Length;
            return true;
        }

        private static bool TryReadNumber(string json, ref int index)
        {
            int start = index;

            if (index < json.Length && json[index] == '-')
            {
                index++;
            }

            if (index >= json.Length)
            {
                return false;
            }

            if (json[index] == '0')
            {
                index++;
            }
            else if (json[index] >= '1' && json[index] <= '9')
            {
                while (
                    index < json.Length &&
                    json[index] >= '0' &&
                    json[index] <= '9'
                )
                {
                    index++;
                }
            }
            else
            {
                return false;
            }

            if (index < json.Length && json[index] == '.')
            {
                index++;

                int decimalStart = index;

                while (
                    index < json.Length &&
                    json[index] >= '0' &&
                    json[index] <= '9'
                )
                {
                    index++;
                }

                if (decimalStart == index)
                {
                    return false;
                }
            }

            if (
                index < json.Length &&
                (json[index] == 'e' || json[index] == 'E')
            )
            {
                index++;

                if (
                    index < json.Length &&
                    (json[index] == '+' || json[index] == '-')
                )
                {
                    index++;
                }

                int exponentStart = index;

                while (
                    index < json.Length &&
                    json[index] >= '0' &&
                    json[index] <= '9'
                )
                {
                    index++;
                }

                if (exponentStart == index)
                {
                    return false;
                }
            }

            return index > start;
        }

        private static bool TryConsume(
            string json,
            ref int index,
            char expected
        )
        {
            if (index >= json.Length || json[index] != expected)
            {
                return false;
            }

            index++;
            return true;
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (
                index < json.Length &&
                (json[index] == ' ' ||
                    json[index] == '\n' ||
                    json[index] == '\r' ||
                    json[index] == '\t')
            )
            {
                index++;
            }
        }

        private static bool IsHexadecimal(char character)
        {
            return
                (character >= '0' && character <= '9') ||
                (character >= 'a' && character <= 'f') ||
                (character >= 'A' && character <= 'F');
        }
    }
}
