using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Painel genérico de nome+código para conectar um WebGL a uma coleta.
    /// O desenvolvedor pode estilizar toda a hierarquia criada pelo assistente.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("LUDUS/Conexão/Painel de acesso à coleta")]
    [DisallowMultipleComponent]
    public sealed class LudusCollectionAccessPanel : MonoBehaviour
    {
        [Header("Controles")]
        [SerializeField] private Button openButton;
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private RectTransform modalCard;
        [SerializeField] private InputField participantNameInput;
        [SerializeField] private InputField collectionCodeInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text instructionText;
        [SerializeField] private Text openButtonText;
        [SerializeField] private Text connectButtonText;
        [SerializeField] private Text cancelButtonText;
        [SerializeField] private LudusRoundedGraphic connectionBadgeBackground;
        [SerializeField] private Text connectionBadgeText;

        [Header("Comportamento")]
        [SerializeField] private bool hideModalOnStart = true;
        [SerializeField] private bool hideOpenButtonAfterConnection;
        [SerializeField] private bool startSessionAfterConnection;
        [SerializeField] private bool finishSessionFromConnectedView;
        [SerializeField] private bool ensureEventSystemAcrossScenes;
        [SerializeField] private bool showOpenButtonOnlyInConnectionScene;

        private bool requestInProgress;
        private bool connectionJustCompleted;
        private string connectedStatusMessage = string.Empty;
        private EventSystem runtimeEventSystem;
        private string connectionScenePath = string.Empty;

        private static readonly Color BadgeDisconnectedColor =
            new Color32(205, 52, 69, 255);
        private static readonly Color BadgeConnectingColor =
            new Color32(217, 145, 32, 255);
        private static readonly Color BadgeConnectedColor =
            new Color32(37, 166, 105, 255);

        public bool IsRequestInProgress => requestInProgress;

        public void ConfigureAutomaticSessionStart(bool enabled)
        {
            startSessionAfterConnection = enabled;
        }

        public void ConfigureSimpleMode()
        {
            hideModalOnStart = false;
            hideOpenButtonAfterConnection = false;
            startSessionAfterConnection = true;
            finishSessionFromConnectedView = true;
            ensureEventSystemAcrossScenes = true;
            showOpenButtonOnlyInConnectionScene = true;
        }

        private void OnTransformParentChanged()
        {
            FitToVisualCanvas();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                FitToVisualCanvas();
                FitModalToAvailableArea();
            }
        }

        private void Reset()
        {
            FitToVisualCanvas();
        }

        private void FitToVisualCanvas()
        {
            RectTransform rect = transform as RectTransform;
            if (
                rect == null ||
                transform.parent == null ||
                GetComponentInParent<Canvas>() == null
            )
            {
                return;
            }

            bool alreadyFitted =
                rect.anchorMin == Vector2.zero &&
                rect.anchorMax == Vector2.one &&
                rect.pivot == new Vector2(0.5f, 0.5f) &&
                rect.anchoredPosition == Vector2.zero &&
                rect.sizeDelta == Vector2.zero &&
                rect.localPosition == Vector3.zero &&
                rect.localRotation == Quaternion.identity &&
                rect.localScale == Vector3.one;
            if (alreadyFitted)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            FitModalToAvailableArea();
        }

        private void OnRectTransformDimensionsChange()
        {
            FitModalToAvailableArea();
        }

        private void FitModalToAvailableArea()
        {
            RectTransform rootRect = transform as RectTransform;
            if (rootRect == null || modalRoot == null)
            {
                return;
            }

            if (modalCard == null)
            {
                modalCard = modalRoot.transform.Find("Conteúdo") as RectTransform;
            }
            if (modalCard == null)
            {
                return;
            }

            float availableWidth = Mathf.Max(0f, rootRect.rect.width - 64f);
            float availableHeight = Mathf.Max(0f, rootRect.rect.height - 64f);
            float cardWidth = Mathf.Abs(modalCard.rect.width);
            float cardHeight = Mathf.Abs(modalCard.rect.height);
            if (
                availableWidth <= 0f ||
                availableHeight <= 0f ||
                cardWidth <= 0f ||
                cardHeight <= 0f
            )
            {
                return;
            }

            float scale = Mathf.Clamp01(
                Mathf.Min(
                    availableWidth / cardWidth,
                    availableHeight / cardHeight
                )
            );
            Vector3 desiredScale = new Vector3(scale, scale, 1f);
            if (modalCard.localScale != desiredScale)
            {
                modalCard.localScale = desiredScale;
            }
        }

        public void Configure(
            Button open,
            GameObject modal,
            RectTransform card,
            InputField participantName,
            InputField collectionCode,
            Button connect,
            Button cancel,
            Text status,
            Text buttonLabel
        )
        {
            openButton = open;
            modalRoot = modal;
            modalCard = card;
            participantNameInput = participantName;
            collectionCodeInput = collectionCode;
            connectButton = connect;
            cancelButton = cancel;
            statusText = status;
            openButtonText = buttonLabel;
        }

        private void Awake()
        {
            FitToVisualCanvas();
            FitModalToAvailableArea();
            ResolveVisualReferences();
            EnsureStatusIndicator();
            if (!Application.isPlaying)
            {
                return;
            }

            connectionScenePath = SceneManager.GetActiveScene().path;

            RefreshConnectionVisualState();

            if (modalRoot != null)
            {
                modalRoot.SetActive(!hideModalOnStart);
            }
            UpdateAccessVisibility(SceneManager.GetActiveScene());
        }

        private void OnEnable()
        {
            FitToVisualCanvas();
            FitModalToAvailableArea();
            ResolveVisualReferences();
            if (!Application.isPlaying)
            {
                return;
            }

            openButton?.onClick.AddListener(Open);
            connectButton?.onClick.AddListener(Connect);
            cancelButton?.onClick.AddListener(Close);
            if (ensureEventSystemAcrossScenes)
            {
                SceneManager.sceneLoaded += HandleSceneLoaded;
                EnsureRuntimeEventSystem();
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            openButton?.onClick.RemoveListener(Open);
            connectButton?.onClick.RemoveListener(Connect);
            cancelButton?.onClick.RemoveListener(Close);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void Open()
        {
            if (modalRoot == null)
            {
                return;
            }

            modalRoot.SetActive(true);
            if (
                LudusCollectionContext.TryGet(
                    out LudusCollectionConnection connection,
                    out _
                )
            )
            {
                connectionJustCompleted = false;
                if (
                    finishSessionFromConnectedView &&
                    LudusSdk.TryGetSingleSessionController(
                        out LudusSessionController controller,
                        out _
                    ) &&
                    !controller.HasActiveSession
                )
                {
                    ApplyFinishedView();
                    return;
                }
                string message = string.IsNullOrWhiteSpace(
                    connectedStatusMessage
                )
                    ? "Conectado como " + connection.DisplayName + "."
                    : connectedStatusMessage;
                ApplyConnectedView(message);
                return;
            }

            ApplyDisconnectedView();
            participantNameInput?.ActivateInputField();
        }

        public void Close()
        {
            if (!requestInProgress && modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        public void Connect()
        {
            if (requestInProgress)
            {
                return;
            }

            if (LudusCollectionContext.TryGet(out _, out _))
            {
                HandleConnectedAction();
                return;
            }

            string participantName = participantNameInput == null
                ? string.Empty
                : participantNameInput.text.Trim();
            string code = collectionCodeInput == null
                ? string.Empty
                : collectionCodeInput.text.Trim();

            if (!HasEnoughNameParts(participantName))
            {
                ApplyErrorView(
                    "Informe nome e sobrenome, como \"João Silva\" ou \"João S\"."
                );
                return;
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                ApplyErrorView("Informe o código da coleta.");
                return;
            }
            if (
                !LudusSdk.TryGetSingleSessionController(
                    out LudusSessionController controller,
                    out string errorMessage
                ) ||
                controller.Config == null ||
                string.IsNullOrWhiteSpace(controller.Config.apiBaseUrl)
            )
            {
                ApplyErrorView(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "A URL da API LUDUS não está configurada."
                        : errorMessage
                );
                return;
            }

            StartCoroutine(
                Pair(
                    controller,
                    controller.Config.apiBaseUrl,
                    participantName,
                    code
                )
            );
        }

        private void HandleConnectedAction()
        {
            if (!finishSessionFromConnectedView)
            {
                Close();
                return;
            }

            if (connectionJustCompleted)
            {
                connectionJustCompleted = false;
                Close();
                return;
            }

            if (
                !LudusSdk.TryGetSingleSessionController(
                    out LudusSessionController controller,
                    out string controllerError
                )
            )
            {
                ApplyErrorView(controllerError);
                return;
            }

            if (controller.HasActiveSession)
            {
                if (!LudusSdk.TryEndSession(out _, out string endError))
                {
                    ApplyErrorView(endError);
                    return;
                }

                ApplyFinishedView();
                return;
            }

            if (!LudusCollectionContext.TryGet(
                out LudusCollectionConnection connection,
                out string connectionError
            ))
            {
                ApplyErrorView(connectionError);
                return;
            }
            if (!LudusSdk.TryStartSession(
                LudusSdk.StudentIdPendingManualImport,
                connection.DisplayName,
                out string startError
            ))
            {
                ApplyErrorView(startError);
                return;
            }

            connectionJustCompleted = true;
            string message = string.IsNullOrWhiteSpace(connectedStatusMessage)
                ? "Nova sessão iniciada como " + connection.DisplayName + "."
                : connectedStatusMessage;
            ApplyConnectedView(message);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureRuntimeEventSystem();
            UpdateAccessVisibility(scene);
        }

        private void UpdateAccessVisibility(Scene scene)
        {
            if (!showOpenButtonOnlyInConnectionScene || openButton == null)
            {
                return;
            }

            bool isConnectionScene = string.Equals(
                scene.path,
                connectionScenePath,
                StringComparison.Ordinal
            );
            openButton.gameObject.SetActive(isConnectionScene);
            if (!isConnectionScene && modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private static bool HasEnoughNameParts(string participantName)
        {
            return Regex.IsMatch(
                participantName ?? string.Empty,
                @"^\p{L}[\p{L}'’.-]*(?:\s+[\p{L}\p{N}][\p{L}\p{N}'’.-]*)+$"
            );
        }

        private void EnsureRuntimeEventSystem()
        {
            if (!Application.isPlaying || !ensureEventSystemAcrossScenes)
            {
                return;
            }

            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );
            LudusSessionController controller =
                GetComponentInParent<LudusSessionController>();
            if (runtimeEventSystem == null && controller != null)
            {
                foreach (EventSystem eventSystem in eventSystems)
                {
                    if (eventSystem.transform.IsChildOf(controller.transform))
                    {
                        runtimeEventSystem = eventSystem;
                        break;
                    }
                }
            }
            foreach (EventSystem eventSystem in eventSystems)
            {
                if (eventSystem != runtimeEventSystem)
                {
                    if (runtimeEventSystem != null)
                    {
                        Destroy(runtimeEventSystem.gameObject);
                        runtimeEventSystem = null;
                    }
                    return;
                }
            }

            if (runtimeEventSystem != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject(
                "LUDUS — EventSystem",
                typeof(EventSystem)
            );
            eventSystemObject.transform.SetParent(
                controller == null ? transform : controller.transform,
                false
            );

#if ENABLE_INPUT_SYSTEM
            Type inputSystemModuleType = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
            );
            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (eventSystemObject.GetComponent<BaseInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
            runtimeEventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        private IEnumerator Pair(
            LudusSessionController controller,
            string apiBaseUrl,
            string participantName,
            string code
        )
        {
            requestInProgress = true;
            SetInteractable(false);
            SetConnectionBadge("…", BadgeConnectingColor);
            SetButtonText(connectButtonText, "Conectando...");
            SetStatus("Conectando à coleta...", false);

            PairRequest payload = new PairRequest
            {
                participantName = participantName,
                code = code,
            };
            string json = JsonUtility.ToJson(payload);
            string url = apiBaseUrl.TrimEnd('/') + "/api/collections/pair";

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

                if (request.result != UnityWebRequest.Result.Success)
                {
                    requestInProgress = false;
                    ApplyErrorView(ReadErrorMessage(request));
                    SetInteractable(true);
                    yield break;
                }

                PairResponse response;
                try
                {
                    response = JsonUtility.FromJson<PairResponse>(
                        request.downloadHandler.text
                    );
                }
                catch (Exception)
                {
                    response = null;
                }

                string contextError = string.Empty;
                string participantError = string.Empty;
                bool responseIsComplete =
                    response != null &&
                    response.participante != null &&
                    response.coleta != null &&
                    response.credencial != null;
                bool contextConfigured = responseIsComplete &&
                    LudusCollectionContext.TryConfigure(
                        response.coleta.collectionId,
                        response.participante.participantRef,
                        response.participante.displayName,
                        response.credencial.token,
                        response.credencial.expiresAt,
                        out contextError
                    );
                bool participantConfigured = contextConfigured &&
                    LudusSdk.TrySetParticipant(
                        LudusSdk.StudentIdPendingManualImport,
                        response.participante.displayName,
                        out participantError
                    );

                if (!participantConfigured)
                {
                    LudusCollectionContext.Clear();
                    requestInProgress = false;
                    ApplyErrorView(
                        !string.IsNullOrWhiteSpace(contextError)
                            ? contextError
                            : !string.IsNullOrWhiteSpace(participantError)
                                ? participantError
                                : "A resposta do servidor não pôde ser aplicada."
                    );
                    SetInteractable(true);
                    yield break;
                }

                string displayName = response.participante.displayName;
                bool shouldStartSession =
                    startSessionAfterConnection ||
                    FindFirstObjectByType<
                        LudusTutorialScenarioNavigator
                    >() != null;
                if (
                    shouldStartSession &&
                    !controller.HasActiveSession &&
                    !LudusSdk.TryStartSession(
                        LudusSdk.StudentIdPendingManualImport,
                        displayName,
                        out string sessionError
                    )
                )
                {
                    LudusSdk.ClearParticipant();
                    requestInProgress = false;
                    ApplyErrorView(
                        "A coleta foi localizada, mas a sessão do tutorial " +
                        "não pôde ser iniciada: " + sessionError
                    );
                    SetInteractable(true);
                    yield break;
                }

                connectedStatusMessage =
                    "Conectado à coleta “" + response.coleta.title +
                    "” como " + displayName + ".";
                connectionJustCompleted = finishSessionFromConnectedView;
                ApplyConnectedView(connectedStatusMessage);
                if (
                    openButtonText != null &&
                    !string.Equals(
                        openButtonText.text,
                        "L",
                        StringComparison.Ordinal
                    )
                )
                {
                    openButtonText.text = "Conectado: " + displayName;
                }
                if (hideOpenButtonAfterConnection && openButton != null)
                {
                    openButton.gameObject.SetActive(false);
                }
            }

            requestInProgress = false;
            SetInteractable(true);
        }

        public void EnsureStatusIndicator()
        {
            ResolveVisualReferences();
            if (openButton == null || connectionBadgeText != null)
            {
                return;
            }

            GameObject badgeObject = new GameObject(
                "Estado da conexão",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(LudusRoundedGraphic)
            );
            badgeObject.transform.SetParent(openButton.transform, false);
            RectTransform badgeRect =
                badgeObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = Vector2.one;
            badgeRect.anchorMax = Vector2.one;
            badgeRect.pivot = Vector2.one;
            badgeRect.anchoredPosition = new Vector2(4f, 4f);
            badgeRect.sizeDelta = new Vector2(24f, 24f);

            connectionBadgeBackground =
                badgeObject.GetComponent<LudusRoundedGraphic>();
            connectionBadgeBackground.CornerRadius = 12f;
            connectionBadgeBackground.raycastTarget = false;

            GameObject symbolObject = new GameObject(
                "Símbolo",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text)
            );
            symbolObject.transform.SetParent(badgeObject.transform, false);
            RectTransform symbolRect =
                symbolObject.GetComponent<RectTransform>();
            symbolRect.anchorMin = Vector2.zero;
            symbolRect.anchorMax = Vector2.one;
            symbolRect.offsetMin = Vector2.zero;
            symbolRect.offsetMax = Vector2.zero;

            connectionBadgeText = symbolObject.GetComponent<Text>();
            connectionBadgeText.font = openButtonText != null
                ? openButtonText.font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            connectionBadgeText.fontSize = 16;
            connectionBadgeText.fontStyle = FontStyle.Bold;
            connectionBadgeText.alignment = TextAnchor.MiddleCenter;
            connectionBadgeText.color = Color.white;
            connectionBadgeText.raycastTarget = false;
            SetConnectionBadge("×", BadgeDisconnectedColor);
        }

        public void RefreshConnectionVisualState()
        {
            EnsureStatusIndicator();
            if (LudusCollectionContext.TryGet(out _, out _))
            {
                SetConnectionBadge("✓", BadgeConnectedColor);
                return;
            }

            SetConnectionBadge("×", BadgeDisconnectedColor);
        }

        private void ResolveVisualReferences()
        {
            if (instructionText == null && modalCard != null)
            {
                Transform instruction = modalCard.Find("Explicação");
                if (instruction != null)
                {
                    instructionText = instruction.GetComponent<Text>();
                }
            }
            if (connectButtonText == null && connectButton != null)
            {
                connectButtonText =
                    connectButton.GetComponentInChildren<Text>(true);
            }
            if (cancelButtonText == null && cancelButton != null)
            {
                cancelButtonText =
                    cancelButton.GetComponentInChildren<Text>(true);
            }
            if (connectionBadgeText != null || openButton == null)
            {
                return;
            }

            Transform badge = openButton.transform.Find("Estado da conexão");
            if (badge == null)
            {
                return;
            }

            connectionBadgeBackground =
                badge.GetComponent<LudusRoundedGraphic>();
            connectionBadgeText = badge.GetComponentInChildren<Text>(true);
        }

        private void ApplyDisconnectedView()
        {
            SetFormVisible(true);
            SetButtonText(connectButtonText, "Entrar");
            SetButtonText(cancelButtonText, "Voltar");
            SetStatus(string.Empty, false);
            SetConnectionBadge("×", BadgeDisconnectedColor);
        }

        private void ApplyErrorView(string message)
        {
            SetFormVisible(true);
            SetButtonText(connectButtonText, "Tentar novamente");
            SetStatus(message, true);
            SetConnectionBadge("×", BadgeDisconnectedColor);
        }

        private void ApplyConnectedView(string message)
        {
            SetFormVisible(false);
            SetButtonText(
                connectButtonText,
                finishSessionFromConnectedView
                    ? connectionJustCompleted
                        ? "Começar"
                        : "Finalizar e enviar"
                    : "Confirmar"
            );
            SetSimpleCloseActionVisible();
            SetStatus(message, false);
            SetConnectionBadge("✓", BadgeConnectedColor);
        }

        private void ApplyFinishedView()
        {
            SetFormVisible(false);
            SetButtonText(connectButtonText, "Iniciar nova sessão");
            SetSimpleCloseActionVisible();
            SetStatus(
                "Sessão finalizada. O envio ao Dashboard foi iniciado.",
                false
            );
            SetConnectionBadge("✓", BadgeConnectedColor);
        }

        private void SetSimpleCloseActionVisible()
        {
            if (!finishSessionFromConnectedView || cancelButton == null)
            {
                return;
            }

            cancelButton.gameObject.SetActive(true);
            SetButtonText(cancelButtonText, "Fechar");
        }

        private void SetFormVisible(bool visible)
        {
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(visible);
            }
            if (participantNameInput != null)
            {
                participantNameInput.gameObject.SetActive(visible);
            }
            if (collectionCodeInput != null)
            {
                collectionCodeInput.gameObject.SetActive(visible);
            }
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(visible);
            }
        }

        private void SetConnectionBadge(string symbol, Color color)
        {
            EnsureStatusIndicator();
            if (connectionBadgeBackground != null)
            {
                connectionBadgeBackground.color = color;
            }
            if (connectionBadgeText != null)
            {
                connectionBadgeText.text = symbol;
            }
        }

        private static void SetButtonText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private void SetInteractable(bool value)
        {
            if (connectButton != null)
            {
                connectButton.interactable = value;
            }
            if (cancelButton != null)
            {
                cancelButton.interactable = value;
            }
        }

        private void SetStatus(string message, bool isError)
        {
            if (statusText == null)
            {
                return;
            }

            statusText.text = message;
            statusText.color = isError
                ? new Color32(185, 40, 58, 255)
                : new Color32(55, 48, 163, 255);
        }

        private static string ReadErrorMessage(UnityWebRequest request)
        {
            string fallback = string.IsNullOrWhiteSpace(request.error)
                ? "Não foi possível conectar à coleta."
                : "Não foi possível conectar à coleta: " + request.error;

            try
            {
                ErrorResponse response = JsonUtility.FromJson<ErrorResponse>(
                    request.downloadHandler.text
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
        private sealed class PairRequest
        {
            public string participantName;
            public string code;
        }

        [Serializable]
        private sealed class PairResponse
        {
            public PairParticipant participante;
            public PairCollection coleta;
            public PairCredential credencial;
        }

        [Serializable]
        private sealed class PairParticipant
        {
            public string participantRef;
            public string displayName;
            public string resolutionStatus;
        }

        [Serializable]
        private sealed class PairCollection
        {
            public string collectionId;
            public string title;
        }

        [Serializable]
        private sealed class PairCredential
        {
            public string token;
            public string expiresAt;
        }

        [Serializable]
        private sealed class ErrorResponse
        {
            public string mensagem;
        }
    }
}
