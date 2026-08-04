using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace LudusSDK
{
    public enum LudusSceneCaptureMode
    {
        [InspectorName("Todas as cenas")]
        AllScenes,

        [InspectorName("Somente cenas selecionadas")]
        SelectedScenes,
    }

    [CreateAssetMenu(
        fileName = "LudusSdkConfig",
        menuName = "LUDUS/Configuração do SDK",
        order = 1
    )]
    public sealed class LudusSdkConfig : ScriptableObject
    {
        [Header("Identificação do jogo")]

        [Tooltip("Identificador técnico legado. O Inspector usa Nome do jogo.")]
        public string gameId = "";

        [Tooltip("Nome simples apresentado pelo integrador. O SDK gera o identificador técnico.")]
        public string gameName = "";

        [Tooltip("Versão do jogo que está enviando a sessão.")]
        public string gameVersion = "1.0.0";

        [Header("Transporte")]

        [InspectorName("URL base do servidor (opcional)")]
        [Tooltip("URL base da API. Deixe vazia quando o jogo apenas exportar JSON.")]
        public string apiBaseUrl = "";

        [InspectorName("Enviar automaticamente ao encerrar")]
        [Tooltip("Envia automaticamente a sessão quando ela for encerrada.")]
        public bool sendOnSessionEnd = true;

        [Tooltip("Mantém uma cópia local mesmo quando o envio for bem-sucedido.")]
        public bool saveLocalCopyOnSessionEnd;

        [Header("Importação manual")]

        [InspectorName("Baixar arquivo JSON ao encerrar (WebGL)")]
        [Tooltip("Pede ao navegador para baixar um arquivo JSON normal, pronto para importação manual no LUDUS Acompanha. Não substitui a cópia local de segurança.")]
        public bool downloadJsonOnSessionEnd;

        [Header("Fallback offline")]

        [InspectorName("Guardar cópia local se necessário")]
        [Tooltip("Mantém sessões pendentes localmente quando o envio falhar.")]
        public bool enableLocalFallback = true;

        [InspectorName("Nome da pasta local")]
        [Tooltip("Pasta relativa ao persistentDataPath usada pelo fallback.")]
        public string fallbackFolderName = "ludus_offline";

        [Header("Cenas acompanhadas")]

        [InspectorName("Onde capturar automaticamente")]
        [Tooltip("Define se a coleta automática acompanha todas as cenas ou somente as cenas escolhidas.")]
        public LudusSceneCaptureMode sceneCaptureMode =
            LudusSceneCaptureMode.AllScenes;

        [InspectorName("Cenas selecionadas")]
        [Tooltip("Nomes das cenas que terão captura automática quando o modo selecionado estiver ativo.")]
        public List<string> selectedSceneNames = new List<string>();

        [Header("Coleta")]

        [InspectorName("Tipos de dados a coletar")]
        [Tooltip("Define quais capacidades esta integração disponibiliza.")]
        public LudusCapabilities capabilities = new LudusCapabilities();

        [InspectorName("Tempo para considerar inatividade (segundos)")]
        [Tooltip("Tempo sem interação para caracterizar inatividade, em segundos.")]
        [Min(0f)]
        public float inactivityThresholdSeconds = 10f;

        [Header("Desenvolvimento")]

        [InspectorName("Exibir mensagens detalhadas no Console")]
        [Tooltip("Ativa mensagens detalhadas no Console da Unity.")]
        public bool debugMode = true;

        public bool TryValidateForSession(out string errorMessage)
        {
            if (!HasValidGameId(GetResolvedGameId()))
            {
                errorMessage =
                    "Informe um nome de jogo que gere um identificador válido de até 100 caracteres.";
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

            if (
                enableLocalFallback &&
                !HasValidFallbackFolderName(fallbackFolderName)
            )
            {
                errorMessage =
                    "fallbackFolderName deve usar letras minúsculas, números, hífens ou sublinhados, com até 100 caracteres.";
                return false;
            }

            if (
                sceneCaptureMode == LudusSceneCaptureMode.SelectedScenes &&
                (selectedSceneNames == null || selectedSceneNames.Count == 0)
            )
            {
                errorMessage =
                    "Selecione ao menos uma cena para usar a captura somente em cenas selecionadas.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public bool ShouldCaptureScene(string sceneName)
        {
            if (sceneCaptureMode == LudusSceneCaptureMode.AllScenes)
            {
                return true;
            }

            if (
                string.IsNullOrWhiteSpace(sceneName) ||
                selectedSceneNames == null
            )
            {
                return false;
            }

            foreach (string selectedSceneName in selectedSceneNames)
            {
                if (string.Equals(
                    selectedSceneName,
                    sceneName,
                    StringComparison.Ordinal
                ))
                {
                    return true;
                }
            }

            return false;
        }

        public string GetResolvedGameId()
        {
            if (!string.IsNullOrWhiteSpace(gameName))
            {
                return CreateGameIdFromName(gameName);
            }

            return gameId?.Trim() ?? string.Empty;
        }

        public string GetResolvedGameVersion()
        {
            return string.IsNullOrWhiteSpace(gameVersion)
                ? "1.0.0"
                : gameVersion.Trim();
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

        private static string CreateGameIdFromName(string value)
        {
            StringBuilder result = new StringBuilder();
            bool previousWasHyphen = false;
            string normalized = value.Trim().Normalize(
                NormalizationForm.FormD
            );

            foreach (char character in normalized)
            {
                if (
                    CharUnicodeInfo.GetUnicodeCategory(character) ==
                    UnicodeCategory.NonSpacingMark
                )
                {
                    continue;
                }

                bool isLetterOrDigit =
                    (character >= 'a' && character <= 'z') ||
                    (character >= 'A' && character <= 'Z') ||
                    (character >= '0' && character <= '9');

                if (isLetterOrDigit)
                {
                    result.Append(char.ToLowerInvariant(character));
                    previousWasHyphen = false;
                }
                else if (result.Length > 0 && !previousWasHyphen)
                {
                    result.Append('-');
                    previousWasHyphen = true;
                }
            }

            if (result.Length > 0 && result[result.Length - 1] == '-')
            {
                result.Length--;
            }

            return result.ToString();
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

        private static bool HasValidFallbackFolderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 100)
            {
                return false;
            }

            foreach (char character in value)
            {
                bool allowed =
                    IsLowercaseLetterOrDigit(character) ||
                    character == '-' ||
                    character == '_';

                if (!allowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
