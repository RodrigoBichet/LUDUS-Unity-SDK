using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LudusSDK
{
    internal static class LudusWebGlJsonDownload
    {
        public static bool TryDownload(
            string fileName,
            string json,
            out string errorMessage
        )
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                LudusDownloadJson(
                    fileName,
                    json
                );
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage =
                    "Não foi possível solicitar o download do JSON: " +
                    exception.Message;
                return false;
            }
#else
            errorMessage =
                "O download normal de arquivo é disponibilizado somente em builds WebGL. Use a cópia local de segurança neste ambiente.";
            return false;
#endif
        }

        public static string CreateFileName(
            LudusSdkConfig config,
            LudusSession session
        )
        {
            string gameName = SanitizeFileNamePart(
                config != null ? config.GetResolvedGameId() : "jogo"
            );
            string label = SanitizeFileNamePart(
                config != null ? config.downloadFileLabel : string.Empty
            );
            string dateTime = GetEndedAtForFileName(session);

            if (string.IsNullOrWhiteSpace(gameName))
            {
                gameName = "jogo";
            }

            return string.IsNullOrWhiteSpace(label)
                ? "ludus-" + gameName + "-" + dateTime + ".json"
                : "ludus-" + gameName + "-" + label + "-" + dateTime + ".json";
        }

        private static string GetEndedAtForFileName(LudusSession session)
        {
            if (
                session != null &&
                DateTimeOffset.TryParse(session.endedAt, out DateTimeOffset endedAt)
            )
            {
                return endedAt.ToLocalTime().ToString("yyyy-MM-dd_HH-mm-ss");
            }

            return DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        private static string SanitizeFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder();
            bool previousWasHyphen = false;

            foreach (char character in value.Trim())
            {
                if (char.IsLetterOrDigit(character))
                {
                    result.Append(char.ToLowerInvariant(character));
                    previousWasHyphen = false;
                }
                else if (!previousWasHyphen && result.Length > 0)
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

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void LudusDownloadJson(
            string fileName,
            string json
        );
#endif
    }
}
