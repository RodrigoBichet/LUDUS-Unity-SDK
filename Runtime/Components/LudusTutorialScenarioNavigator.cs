using System;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Alterna as páginas do laboratório criado exclusivamente pela cena
    /// tutorial. Não registra resultados nem deve ser usado no jogo real.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialScenarioNavigator : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] scenarioPages = Array.Empty<GameObject>();

        [SerializeField]
        private Button previousButton;

        [SerializeField]
        private Button nextButton;

        [SerializeField]
        private Text pageIndicator;

        [SerializeField]
        private GameObject completionPage;

        [SerializeField]
        private LudusTutorialScenarioGate[] scenarioGates =
            Array.Empty<LudusTutorialScenarioGate>();

        [SerializeField]
        private Text navigationHint;

        [SerializeField]
        private int generatedVersion;

        [SerializeField]
        [Min(0)]
        private int currentPageIndex;

        private bool listenersRegistered;
        private bool gatesSubscribed;
        private bool connectedSessionCompletionHandled;
        private string connectedCompletionStatus = string.Empty;
        private int lastButtonNavigationFrame = int.MinValue;

        public int CurrentPageIndex => currentPageIndex;
        public int PageCount => scenarioPages?.Length ?? 0;
        public int ScenarioCount => completionPage != null
            ? Mathf.Max(0, PageCount - 1)
            : PageCount;
        public int GeneratedVersion => generatedVersion;

        public void UpdateGeneratedVersion(int version)
        {
            generatedVersion = version;
        }

        public void Configure(
            GameObject[] pages,
            Button previous,
            Button next,
            Text indicator,
            GameObject completion = null,
            LudusTutorialScenarioGate[] gates = null,
            Text hint = null,
            int version = 0
        )
        {
            RemoveListeners();
            UnsubscribeGates();
            scenarioPages = pages ?? Array.Empty<GameObject>();
            previousButton = previous;
            nextButton = next;
            pageIndicator = indicator;
            completionPage = completion;
            scenarioGates = gates ?? Array.Empty<LudusTutorialScenarioGate>();
            navigationHint = hint;
            generatedVersion = version;
            currentPageIndex = 0;
            connectedSessionCompletionHandled = false;
            connectedCompletionStatus = string.Empty;

            if (Application.isPlaying && isActiveAndEnabled)
            {
                RegisterListeners();
                SubscribeGates();
            }

            ApplyCurrentPage();
        }

        private void OnEnable()
        {
            RegisterListeners();
            SubscribeGates();
            ApplyCurrentPage();
        }

        private void OnDisable()
        {
            RemoveListeners();
            UnsubscribeGates();
        }

        public void ShowPreviousPage()
        {
            ShowPage(currentPageIndex - 1);
        }

        public void ShowNextPage()
        {
            if (!IsCurrentScenarioCompleted())
            {
                ApplyCurrentPage();
                return;
            }

            ShowPage(currentPageIndex + 1);
        }

        public void ShowPage(int index)
        {
            if (PageCount == 0)
            {
                currentPageIndex = 0;
                ApplyCurrentPage();
                return;
            }

            currentPageIndex = Mathf.Clamp(index, 0, PageCount - 1);
            ApplyCurrentPage();
        }

        private void RegisterListeners()
        {
            if (listenersRegistered)
            {
                return;
            }

            if (previousButton != null)
            {
                previousButton.onClick.AddListener(
                    HandlePreviousButtonClick
                );
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(HandleNextButtonClick);
            }

            listenersRegistered = true;
        }

        private void RemoveListeners()
        {
            if (!listenersRegistered)
            {
                return;
            }

            if (previousButton != null)
            {
                previousButton.onClick.RemoveListener(
                    HandlePreviousButtonClick
                );
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(HandleNextButtonClick);
            }

            listenersRegistered = false;
        }

        private void HandlePreviousButtonClick()
        {
            NavigateOncePerFrame(-1);
        }

        private void HandleNextButtonClick()
        {
            NavigateOncePerFrame(1);
        }

        private void NavigateOncePerFrame(int direction)
        {
            int frame = Time.frameCount;
            if (lastButtonNavigationFrame == frame)
            {
                return;
            }

            lastButtonNavigationFrame = frame;
            if (direction > 0)
            {
                ShowNextPage();
            }
            else
            {
                ShowPreviousPage();
            }
        }

        private void ApplyCurrentPage()
        {
            for (int index = 0; index < PageCount; index++)
            {
                GameObject page = scenarioPages[index];
                if (page != null)
                {
                    page.SetActive(index == currentPageIndex);
                }
            }

            if (previousButton != null)
            {
                previousButton.interactable = currentPageIndex > 0;
            }

            if (nextButton != null)
            {
                nextButton.interactable =
                    PageCount > 0 &&
                    currentPageIndex < PageCount - 1 &&
                    IsCurrentScenarioCompleted();
            }

            if (pageIndicator != null)
            {
                if (PageCount == 0)
                {
                    pageIndicator.text = "Nenhum cenário disponível";
                }
                else if (
                    completionPage != null &&
                    scenarioPages[currentPageIndex] == completionPage
                )
                {
                    pageIndicator.text = "Tutorial concluído";
                }
                else
                {
                    pageIndicator.text =
                        $"Cenário {currentPageIndex + 1} de {ScenarioCount}";
                }
            }

            TryCompleteConnectedTutorialSession();
            UpdateNavigationHint();
        }

        private void TryCompleteConnectedTutorialSession()
        {
            bool isCompletionPage =
                completionPage != null &&
                currentPageIndex >= 0 &&
                currentPageIndex < PageCount &&
                scenarioPages[currentPageIndex] == completionPage;
            if (
                !isCompletionPage ||
                connectedSessionCompletionHandled ||
                !LudusCollectionContext.IsConnected
            )
            {
                return;
            }

            bool ended = LudusSdk.TryEndSession(
                out _,
                out string errorMessage
            );
            connectedSessionCompletionHandled = ended;
            connectedCompletionStatus = ended
                ? "Sessão encerrada. O envio ao LUDUS Acompanha foi iniciado automaticamente."
                : "Não foi possível encerrar a sessão automaticamente: " +
                    errorMessage;

            if (ended)
            {
                Debug.Log("[LUDUS] Tutorial concluído; envio automático iniciado.");
            }
            else
            {
                Debug.LogWarning("[LUDUS] " + connectedCompletionStatus);
            }
        }

        private bool IsCurrentScenarioCompleted()
        {
            if (
                currentPageIndex < 0 ||
                currentPageIndex >= scenarioGates.Length
            )
            {
                return true;
            }

            LudusTutorialScenarioGate gate = scenarioGates[currentPageIndex];
            return gate == null || gate.IsCompleted;
        }

        private void SubscribeGates()
        {
            if (gatesSubscribed)
            {
                return;
            }

            foreach (LudusTutorialScenarioGate gate in scenarioGates)
            {
                if (gate != null)
                {
                    gate.StateChanged += HandleGateStateChanged;
                }
            }

            gatesSubscribed = true;
        }

        private void UnsubscribeGates()
        {
            if (!gatesSubscribed)
            {
                return;
            }

            foreach (LudusTutorialScenarioGate gate in scenarioGates)
            {
                if (gate != null)
                {
                    gate.StateChanged -= HandleGateStateChanged;
                }
            }

            gatesSubscribed = false;
        }

        private void HandleGateStateChanged()
        {
            ApplyCurrentPage();
        }

        private void UpdateNavigationHint()
        {
            if (navigationHint == null)
            {
                return;
            }

            if (
                completionPage != null &&
                currentPageIndex < PageCount &&
                scenarioPages[currentPageIndex] == completionPage
            )
            {
                navigationHint.text = !string.IsNullOrWhiteSpace(
                    connectedCompletionStatus
                )
                    ? connectedCompletionStatus
                    : "Conecte-se pelo botão L antes de iniciar. O painel manual de JSON é opcional.";
                navigationHint.color = connectedCompletionStatus.StartsWith(
                    "Sessão encerrada",
                    StringComparison.Ordinal
                )
                    ? new Color(0.52f, 0.86f, 0.76f)
                    : new Color(0.92f, 0.72f, 0.38f);
                return;
            }

            if (
                currentPageIndex >= 0 &&
                currentPageIndex < scenarioGates.Length &&
                scenarioGates[currentPageIndex] != null
            )
            {
                LudusTutorialScenarioGate gate =
                    scenarioGates[currentPageIndex];
                navigationHint.text = gate.PendingMessage;
                navigationHint.color = gate.IsCompleted
                    ? new Color(0.52f, 0.86f, 0.76f)
                    : new Color(0.92f, 0.72f, 0.38f);
                return;
            }

            navigationHint.text = string.Empty;
        }
    }
}
