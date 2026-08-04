using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusSessionExporter : MonoBehaviour
    {
        [InspectorName("Objeto controlador LUDUS SDK")]
        [Tooltip("Arraste aqui o GameObject que possui o componente LudusSessionController.")]
        public LudusSessionController sessionController;

        private LudusSessionController subscribedController;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Unsubscribe();
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (sessionController != null && subscribedController == null)
            {
                sessionController.SessionSerialized += HandleSessionSerialized;
                subscribedController = sessionController;
            }
        }

        private void Unsubscribe()
        {
            if (subscribedController != null)
            {
                subscribedController.SessionSerialized -= HandleSessionSerialized;
                subscribedController = null;
            }
        }

        private void HandleSessionSerialized(LudusSession session, string json)
        {
            if (sessionController == null || sessionController.Config == null)
            {
                return;
            }

            LudusSdkConfig config = sessionController.Config;

            if (config.downloadJsonOnSessionEnd)
            {
                RequestWebGlDownload(config, session, json);
            }

            StartCoroutine(ExportSession(session, json));
        }

        private static void RequestWebGlDownload(
            LudusSdkConfig config,
            LudusSession session,
            string json
        )
        {
            if (LudusWebGlJsonDownload.TryDownload(
                LudusWebGlJsonDownload.CreateFileName(config, session),
                json,
                out string errorMessage
            ))
            {
                Log(
                    config,
                    "Download do arquivo JSON solicitado ao navegador."
                );
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.LogWarning("[LUDUS] " + errorMessage);
#else
            Log(config, errorMessage);
#endif
        }

        private IEnumerator ExportSession(LudusSession session, string json)
        {
            LudusSdkConfig config = sessionController.Config;

            if (config.saveLocalCopyOnSessionEnd)
            {
                SaveFallbackIfEnabled(config, session.sessionId, json);
            }

            if (!config.sendOnSessionEnd)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(config.apiBaseUrl))
            {
                Log(
                    config,
                    "Conexão com a plataforma não configurada; usando fallback local."
                );
                SaveFallbackIfEnabled(config, session.sessionId, json);
                yield break;
            }

            string url = config.apiBaseUrl.TrimEnd('/') + "/api/sessions";

            using (UnityWebRequest request = new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            ))
            {
                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(json)
                );
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (!config.saveLocalCopyOnSessionEnd)
                    {
                        LudusOfflineSessionStore.TryDelete(
                            config,
                            session.sessionId,
                            out _
                        );
                    }

                    Log(config, "Sessão enviada: " + session.sessionId);
                    yield break;
                }

                Log(
                    config,
                    "Envio falhou; usando fallback local. " + request.error
                );
                SaveFallbackIfEnabled(config, session.sessionId, json);
            }
        }

        private static void SaveFallbackIfEnabled(
            LudusSdkConfig config,
            string sessionId,
            string json
        )
        {
            if (!config.enableLocalFallback)
            {
                Log(config, "Envio não realizado e fallback local desativado.");
                return;
            }

            bool saved = LudusOfflineSessionStore.TrySave(
                config,
                sessionId,
                json,
                out string filePath,
                out string errorMessage
            );

            if (saved)
            {
                Log(config, "Sessão salva localmente: " + filePath);
                return;
            }

            Debug.LogError("[LUDUS] Falha ao salvar fallback local: " + errorMessage);
        }

        private static void Log(LudusSdkConfig config, string message)
        {
            if (config.debugMode)
            {
                Debug.Log("[LUDUS] " + message);
            }
        }
    }
}
