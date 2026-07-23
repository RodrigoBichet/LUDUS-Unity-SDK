using System;
using UnityEngine;

namespace LudusSDK
{
    [CreateAssetMenu(
        fileName = "LudusSdkConfig",
        menuName = "LUDUS/Configuração do SDK",
        order = 1
    )]
    public sealed class LudusSdkConfig : ScriptableObject
    {
        [Header("Identificação do jogo")]

        [Tooltip("Identificador estável do jogo. Exemplo: historietas-divertidas.")]
        public string gameId = "";

        [Tooltip("Versão do jogo que está enviando a sessão.")]
        public string gameVersion = "";

        [Header("Transporte")]

        [Tooltip("URL base da API. Deixe vazia quando o jogo apenas exportar JSON.")]
        public string apiBaseUrl = "";

        [Tooltip("Envia automaticamente a sessão quando ela for encerrada.")]
        public bool sendOnSessionEnd = true;

        [Header("Fallback offline")]

        [Tooltip("Mantém sessões pendentes localmente quando o envio falhar.")]
        public bool enableLocalFallback = true;

        [Tooltip("Pasta relativa ao persistentDataPath usada pelo fallback.")]
        public string fallbackFolderName = "ludus_offline";

        [Header("Coleta")]

        [Tooltip("Define quais capacidades esta integração disponibiliza.")]
        public LudusCapabilities capabilities = new LudusCapabilities();

        [Tooltip("Tempo sem interação para caracterizar inatividade, em segundos.")]
        [Min(0f)]
        public float inactivityThresholdSeconds = 10f;

        [Header("Desenvolvimento")]

        [Tooltip("Ativa mensagens detalhadas no Console da Unity.")]
        public bool debugMode = true;

        public bool TryValidateForSession(out string errorMessage)
        {
            if (!HasValidGameId(gameId))
            {
                errorMessage =
                    "gameId deve usar letras minúsculas, números e hífens, com até 100 caracteres.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(gameVersion))
            {
                errorMessage = "gameVersion é obrigatório.";
                return false;
            }

            if (capabilities == null)
            {
                errorMessage = "capabilities é obrigatório.";
                return false;
            }

            if (capabilities.inactivity && inactivityThresholdSeconds <= 0f)
            {
                errorMessage =
                    "inactivityThresholdSeconds deve ser maior que zero quando a inatividade estiver habilitada.";
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(apiBaseUrl) &&
                !HasValidApiBaseUrl(apiBaseUrl)
            )
            {
                errorMessage =
                    "apiBaseUrl deve ser uma URL HTTP ou HTTPS válida.";
                return false;
            }

            errorMessage = string.Empty;
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
                bool isHyphen = character == '-';

                if (!IsLowercaseLetterOrDigit(character) && !isHyphen)
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

        private static bool HasValidApiBaseUrl(string value)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri apiUri))
            {
                return false;
            }

            return
                apiUri.Scheme == Uri.UriSchemeHttp ||
                apiUri.Scheme == Uri.UriSchemeHttps;
        }
    }
}