using System;

namespace LudusSDK
{
    /// <summary>
    /// Mantém em memória a credencial temporária obtida por nome+código.
    /// A credencial nunca é serializada em cena, asset, PlayerPrefs ou JSON.
    /// </summary>
    public static class LudusCollectionContext
    {
        private static LudusCollectionConnection current;

        public static bool IsConnected =>
            current != null && current.ExpiresAtUtc > DateTime.UtcNow;

        public static bool TryConfigure(
            string collectionId,
            string participantRef,
            string displayName,
            string token,
            string expiresAt,
            out string errorMessage
        )
        {
            if (
                string.IsNullOrWhiteSpace(collectionId) ||
                string.IsNullOrWhiteSpace(participantRef) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(token)
            )
            {
                errorMessage =
                    "A resposta de pareamento está incompleta.";
                return false;
            }

            if (
                !DateTime.TryParse(
                    expiresAt,
                    null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out DateTime expiresAtUtc
                ) ||
                expiresAtUtc <= DateTime.UtcNow
            )
            {
                errorMessage =
                    "A credencial da coleta está expirada ou possui validade inválida.";
                return false;
            }

            current = new LudusCollectionConnection(
                collectionId.Trim(),
                participantRef.Trim(),
                displayName.Trim(),
                token.Trim(),
                expiresAtUtc.ToUniversalTime()
            );
            errorMessage = string.Empty;
            return true;
        }

        public static bool TryGet(
            out LudusCollectionConnection connection,
            out string errorMessage
        )
        {
            if (current == null)
            {
                connection = null;
                errorMessage = "Nenhuma coleta foi conectada neste jogo.";
                return false;
            }

            if (current.ExpiresAtUtc <= DateTime.UtcNow)
            {
                current = null;
                connection = null;
                errorMessage =
                    "A conexão com a coleta expirou. Informe novamente nome e código.";
                return false;
            }

            connection = current;
            errorMessage = string.Empty;
            return true;
        }

        public static void Clear()
        {
            current = null;
        }
    }

    public sealed class LudusCollectionConnection
    {
        public string CollectionId { get; }
        public string ParticipantRef { get; }
        public string DisplayName { get; }
        public string Token { get; }
        public DateTime ExpiresAtUtc { get; }

        internal LudusCollectionConnection(
            string collectionId,
            string participantRef,
            string displayName,
            string token,
            DateTime expiresAtUtc
        )
        {
            CollectionId = collectionId;
            ParticipantRef = participantRef;
            DisplayName = displayName;
            Token = token;
            ExpiresAtUtc = expiresAtUtc;
        }
    }
}
