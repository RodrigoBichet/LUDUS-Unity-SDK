using System;
using System.Runtime.InteropServices;

namespace LudusSDK
{
    internal static class LudusWebGlJsonDownload
    {
        public static bool TryDownload(
            string sessionId,
            string json,
            out string errorMessage
        )
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                LudusDownloadJson(
                    "ludus-session-" + sessionId + ".json",
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

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void LudusDownloadJson(
            string fileName,
            string json
        );
#endif
    }
}
