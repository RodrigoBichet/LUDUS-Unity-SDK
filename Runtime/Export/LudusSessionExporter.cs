using System.Collections;
using System.Collections.Generic;
using System;
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
        private bool checkpointRequestInProgress;
        private float nextCheckpointAt;
        private string checkpointSessionId = string.Empty;
        private string checkpointKey = string.Empty;
        private string lastCheckpointSignature = string.Empty;
        private readonly HashSet<string> checkpointedScreenshotKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private void OnEnable()
        {
            Subscribe();
            nextCheckpointAt = Time.unscaledTime + 1f;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (
                checkpointRequestInProgress ||
                sessionController == null ||
                sessionController.Config == null ||
                !sessionController.Config.sendOnSessionEnd ||
                !sessionController.Config.enableAutomaticCheckpoints ||
                !sessionController.HasActiveSession ||
                Time.unscaledTime < nextCheckpointAt
            )
            {
                return;
            }

            nextCheckpointAt = Time.unscaledTime + Mathf.Max(
                3f,
                sessionController.Config.checkpointIntervalSeconds
            );

            if (
                !sessionController.TrySerializeSnapshot(
                    out LudusSession snapshot,
                    out _,
                    out string errorMessage
                )
            )
            {
                Log(sessionController.Config, errorMessage);
                return;
            }

            EnsureCheckpointSessionState(snapshot.sessionId);
            string checkpointSignature = BuildCheckpointSignature(snapshot);
            if (string.Equals(
                checkpointSignature,
                lastCheckpointSignature,
                StringComparison.Ordinal
            ))
            {
                return;
            }

            snapshot.screenshots = GetPendingScreenshots(
                snapshot.screenshots
            );
            if (
                !LudusJsonSerializer.TrySerialize(
                    snapshot,
                    out string json,
                    out errorMessage
                )
            )
            {
                Log(sessionController.Config, errorMessage);
                return;
            }

            StartCoroutine(
                SendCheckpoint(snapshot, json, checkpointSignature)
            );
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
            LudusRunArchive.RecordCompleted(session, json);

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

            bool hasCollectionConnection = LudusCollectionContext.TryGet(
                out LudusCollectionConnection collectionConnection,
                out string collectionError
            );
            if (
                !hasCollectionConnection &&
                string.Equals(
                    session.studentId,
                    LudusSdk.StudentIdPendingManualImport,
                    StringComparison.Ordinal
                )
            )
            {
                Log(config, collectionError);
                SaveFallbackIfEnabled(config, session.sessionId, json);
                yield break;
            }

            string url = config.apiBaseUrl.TrimEnd('/') +
                (hasCollectionConnection
                    ? "/api/collections/sdk-sessions"
                    : "/api/sessions");

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
                request.SetRequestHeader(
                    "X-LUDUS-Checkpoint-Key",
                    GetCheckpointKey(session.sessionId)
                );
                if (hasCollectionConnection)
                {
                    request.SetRequestHeader(
                        "Authorization",
                        "Bearer " + collectionConnection.Token
                    );
                }

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
                    ClearCheckpointKey(session.sessionId);
                    yield break;
                }

                Log(
                    config,
                    "Envio falhou; usando fallback local. " +
                        ReadRequestError(request)
                );
                SaveFallbackIfEnabled(config, session.sessionId, json);
            }
        }

        private IEnumerator SendCheckpoint(
            LudusSession session,
            string json,
            string checkpointSignature
        )
        {
            LudusSdkConfig config = sessionController.Config;
            if (string.IsNullOrWhiteSpace(config.apiBaseUrl))
            {
                yield break;
            }

            bool hasCollectionConnection = LudusCollectionContext.TryGet(
                out LudusCollectionConnection collectionConnection,
                out string collectionError
            );
            if (
                !hasCollectionConnection &&
                string.Equals(
                    session.studentId,
                    LudusSdk.StudentIdPendingManualImport,
                    StringComparison.Ordinal
                )
            )
            {
                Log(config, collectionError);
                yield break;
            }

            checkpointRequestInProgress = true;
            string url = config.apiBaseUrl.TrimEnd('/') +
                (hasCollectionConnection
                    ? "/api/collections/sdk-sessions/checkpoint"
                    : "/api/sessions/checkpoint");

            using (UnityWebRequest request = new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPUT
            ))
            {
                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(json)
                );
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader(
                    "X-LUDUS-Checkpoint-Key",
                    GetCheckpointKey(session.sessionId)
                );
                if (hasCollectionConnection)
                {
                    request.SetRequestHeader(
                        "Authorization",
                        "Bearer " + collectionConnection.Token
                    );
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    lastCheckpointSignature = checkpointSignature;
                    foreach (LudusScreenshot screenshot in session.screenshots)
                    {
                        checkpointedScreenshotKeys.Add(
                            GetScreenshotKey(screenshot)
                        );
                    }
                    Log(
                        config,
                        "Progresso parcial salvo: " + session.sessionId
                    );
                }
                else
                {
                    Log(
                        config,
                        "Não foi possível atualizar o progresso parcial. " +
                            ReadRequestError(request)
                    );
                }
            }

            checkpointRequestInProgress = false;
        }

        private static string BuildCheckpointSignature(LudusSession session)
        {
            return string.Join(
                ":",
                session.sessionId,
                session.clicks.Count,
                session.mousePath.Count,
                session.dragPath.Count,
                session.gameEvents.Count,
                BuildScreenshotSignature(session.screenshots)
            );
        }

        private List<LudusScreenshot> GetPendingScreenshots(
            List<LudusScreenshot> screenshots
        )
        {
            List<LudusScreenshot> pending = new List<LudusScreenshot>();
            if (screenshots == null)
            {
                return pending;
            }

            foreach (LudusScreenshot screenshot in screenshots)
            {
                if (
                    screenshot != null &&
                    !checkpointedScreenshotKeys.Contains(
                        GetScreenshotKey(screenshot)
                    )
                )
                {
                    pending.Add(screenshot);
                }
            }

            return pending;
        }

        private static string BuildScreenshotSignature(
            List<LudusScreenshot> screenshots
        )
        {
            if (screenshots == null || screenshots.Count == 0)
            {
                return "0";
            }

            List<string> keys = new List<string>();
            foreach (LudusScreenshot screenshot in screenshots)
            {
                keys.Add(GetScreenshotKey(screenshot));
            }
            keys.Sort(StringComparer.Ordinal);
            return string.Join(",", keys);
        }

        private static string GetScreenshotKey(LudusScreenshot screenshot)
        {
            if (screenshot == null)
            {
                return "captura-ausente";
            }

            return string.Join(
                ":",
                screenshot.contextInstanceId ?? string.Empty,
                screenshot.timestamp,
                screenshot.widthPx,
                screenshot.heightPx
            );
        }

        private void EnsureCheckpointSessionState(string sessionId)
        {
            if (
                string.Equals(
                    checkpointSessionId,
                    sessionId,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            checkpointSessionId = sessionId;
            checkpointKey = string.Empty;
            lastCheckpointSignature = string.Empty;
            checkpointedScreenshotKeys.Clear();
        }

        private string GetCheckpointKey(string sessionId)
        {
            if (
                !string.Equals(
                    checkpointSessionId,
                    sessionId,
                    StringComparison.Ordinal
                ) ||
                string.IsNullOrWhiteSpace(checkpointKey)
            )
            {
                checkpointSessionId = sessionId;
                checkpointKey = Guid.NewGuid().ToString("N") +
                    Guid.NewGuid().ToString("N");
            }

            return checkpointKey;
        }

        private void ClearCheckpointKey(string sessionId)
        {
            if (
                !string.Equals(
                    checkpointSessionId,
                    sessionId,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            checkpointSessionId = string.Empty;
            checkpointKey = string.Empty;
            lastCheckpointSignature = string.Empty;
            checkpointedScreenshotKeys.Clear();
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

        private static string ReadRequestError(UnityWebRequest request)
        {
            string fallback = string.IsNullOrWhiteSpace(request.error)
                ? "O servidor recusou a solicitação."
                : request.error;

            try
            {
                ErrorResponse response = JsonUtility.FromJson<ErrorResponse>(
                    request.downloadHandler == null
                        ? string.Empty
                        : request.downloadHandler.text
                );
                return string.IsNullOrWhiteSpace(response?.mensagem)
                    ? fallback
                    : response.mensagem;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        [Serializable]
        private sealed class ErrorResponse
        {
            public string mensagem;
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
