using System;

namespace LudusSDK
{
    /// <summary>
    /// Mantém a identidade do participante recebida pelo jogo ou pela URL do WebGL.
    /// Não armazena credenciais e não conhece regras específicas de nenhum jogo.
    /// </summary>
    public static class LudusParticipantContext
    {
        public const string StudentIdQueryParameter = "ludusStudentId";
        public const string PlayerIdQueryParameter = "ludusPlayerId";

        private static LudusParticipant configuredParticipant;

        public static bool TryConfigure(
            string studentId,
            string playerId,
            out string errorMessage
        )
        {
            LudusParticipant participant = new LudusParticipant(
                studentId,
                playerId
            );

            if (!participant.TryValidate(out errorMessage))
            {
                return false;
            }

            configuredParticipant = participant;
            errorMessage = string.Empty;
            return true;
        }

        public static void Clear()
        {
            configuredParticipant = null;
        }

        public static bool TryResolve(
            string absoluteUrl,
            out LudusParticipant participant,
            out string errorMessage
        )
        {
            if (configuredParticipant != null)
            {
                participant = new LudusParticipant(
                    configuredParticipant.studentId,
                    configuredParticipant.playerId
                );
                errorMessage = string.Empty;
                return true;
            }

            return TryResolveFromUrl(
                absoluteUrl,
                out participant,
                out errorMessage
            );
        }

        public static bool TryResolveFromUrl(
            string absoluteUrl,
            out LudusParticipant participant,
            out string errorMessage
        )
        {
            participant = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(absoluteUrl))
            {
                return false;
            }

            if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out Uri uri))
            {
                errorMessage =
                    "A URL de inicialização do jogo não pôde ser interpretada.";
                return false;
            }

            string studentId;
            string playerId;
            try
            {
                studentId = GetQueryValue(
                    uri.Query,
                    StudentIdQueryParameter
                );
                playerId = GetQueryValue(
                    uri.Query,
                    PlayerIdQueryParameter
                );
            }
            catch (UriFormatException)
            {
                errorMessage =
                    "A identificação recebida pela URL possui codificação inválida.";
                return false;
            }

            bool hasStudentId = !string.IsNullOrWhiteSpace(studentId);
            bool hasPlayerId = !string.IsNullOrWhiteSpace(playerId);
            if (!hasStudentId && !hasPlayerId)
            {
                return false;
            }

            if (!hasStudentId || !hasPlayerId)
            {
                errorMessage =
                    "A identificação recebida pela URL está incompleta. " +
                    "Informe ludusStudentId e ludusPlayerId.";
                return false;
            }

            LudusParticipant resolved = new LudusParticipant(
                studentId,
                playerId
            );
            if (!resolved.TryValidate(out errorMessage))
            {
                return false;
            }

            participant = resolved;
            errorMessage = string.Empty;
            return true;
        }

        private static string GetQueryValue(string query, string key)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            string normalizedQuery = query.TrimStart('?');
            foreach (string pair in normalizedQuery.Split('&'))
            {
                if (string.IsNullOrWhiteSpace(pair))
                {
                    continue;
                }

                string[] parts = pair.Split(new[] { '=' }, 2);
                string candidateKey = Decode(parts[0]);
                if (!string.Equals(
                    candidateKey,
                    key,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    continue;
                }

                return parts.Length == 2 ? Decode(parts[1]) : string.Empty;
            }

            return string.Empty;
        }

        private static string Decode(string value)
        {
            return Uri.UnescapeDataString(
                (value ?? string.Empty).Replace("+", " ")
            );
        }
    }
}
