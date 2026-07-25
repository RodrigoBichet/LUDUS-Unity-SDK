using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace LudusSDK
{
    public static class LudusOfflineSessionStore
    {
        public static bool TrySave(
            LudusSdkConfig config,
            string sessionId,
            string json,
            out string filePath,
            out string errorMessage
        )
        {
            filePath = string.Empty;

            if (config == null)
            {
                errorMessage = "LudusSdkConfig não foi configurado.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                errorMessage = "sessionId é obrigatório para o fallback local.";
                return false;
            }

            if (!HasSafeFolderName(config.fallbackFolderName))
            {
                errorMessage = "fallbackFolderName contém caracteres inválidos.";
                return false;
            }

            if (!HasSafeFileName(sessionId))
            {
                errorMessage = "sessionId contém caracteres inválidos para o fallback local.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                errorMessage = "O JSON da sessão não pode estar vazio.";
                return false;
            }

            try
            {
                string folderPath = Path.Combine(
                    Application.persistentDataPath,
                    config.fallbackFolderName
                );

                Directory.CreateDirectory(folderPath);

                filePath = Path.Combine(folderPath, sessionId + ".json");
                File.WriteAllText(filePath, json, new UTF8Encoding(false));

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                filePath = string.Empty;
                errorMessage = exception.Message;
                return false;
            }
        }

        public static bool TryDelete(
            LudusSdkConfig config,
            string sessionId,
            out string errorMessage
        )
        {
            if (
                config == null ||
                !HasSafeFolderName(config.fallbackFolderName) ||
                !HasSafeFileName(sessionId)
            )
            {
                errorMessage = "Não foi possível identificar o fallback local.";
                return false;
            }

            try
            {
                string filePath = Path.Combine(
                    Application.persistentDataPath,
                    config.fallbackFolderName,
                    sessionId + ".json"
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        private static bool HasSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 100)
            {
                return false;
            }

            foreach (char character in value)
            {
                bool allowed =
                    (character >= 'a' && character <= 'z') ||
                    (character >= 'A' && character <= 'Z') ||
                    (character >= '0' && character <= '9') ||
                    character == '-';

                if (!allowed)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSafeFolderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 100)
            {
                return false;
            }

            foreach (char character in value)
            {
                bool allowed =
                    (character >= 'a' && character <= 'z') ||
                    (character >= '0' && character <= '9') ||
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
