using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LudusSDK
{
    public enum LudusTutorialScenarioGateMode
    {
        AutomaticInteractions,
        AnyButton,
        Attempt,
        Progress,
        OrderedButtons,
        Quiz,
    }

    /// <summary>
    /// Mantém o avanço guiado exclusivamente na cena tutorial. Ele observa a
    /// interface didática, mas não registra telemetria nem resultados.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialScenarioGate : MonoBehaviour
    {
        [SerializeField]
        private LudusTutorialScenarioGateMode mode;

        [SerializeField]
        private string instruction = "Conclua a atividade para avançar.";

        [SerializeField]
        private Button[] buttons = Array.Empty<Button>();

        [SerializeField]
        private InputField inputField;

        [SerializeField]
        private LudusTutorialDraggableItem draggableItem;

        [SerializeField]
        private LudusTutorialAttemptTarget attemptTarget;

        [SerializeField]
        private LudusTutorialProgressControls progressControls;

        [SerializeField]
        private LudusTutorialQuizControls quizControls;

        [SerializeField]
        private int requiredValue = 1;

        private UnityAction[] buttonActions = Array.Empty<UnityAction>();
        private UnityAction<string> inputAction;
        private bool listenersRegistered;
        private bool automaticButtonCompleted;
        private bool automaticInputCompleted;
        private bool automaticDragCompleted;
        private int orderedButtonIndex;
        private bool isCompleted;

        public bool IsCompleted => isCompleted;

        public string PendingMessage
        {
            get
            {
                if (isCompleted)
                {
                    return "Atividade concluída. Você já pode avançar.";
                }

                if (mode == LudusTutorialScenarioGateMode.OrderedButtons)
                {
                    return $"Siga a ordem indicada: passo {orderedButtonIndex + 1} de {buttons.Length}.";
                }

                if (
                    mode == LudusTutorialScenarioGateMode.Quiz &&
                    quizControls != null
                )
                {
                    return quizControls.PendingMessage;
                }

                if (mode == LudusTutorialScenarioGateMode.AutomaticInteractions)
                {
                    return BuildAutomaticPendingMessage();
                }

                return instruction;
            }
        }

        public event Action StateChanged;

        public void ConfigureAutomaticInteractions(
            Button button,
            InputField field,
            LudusTutorialDraggableItem draggable,
            string pendingInstruction
        )
        {
            BeginConfigure(
                LudusTutorialScenarioGateMode.AutomaticInteractions,
                pendingInstruction
            );
            buttons = button == null ? Array.Empty<Button>() : new[] { button };
            inputField = field;
            draggableItem = draggable;
            EndConfigure();
        }

        public void ConfigureAnyButton(
            Button[] observedButtons,
            string pendingInstruction
        )
        {
            BeginConfigure(
                LudusTutorialScenarioGateMode.AnyButton,
                pendingInstruction
            );
            buttons = observedButtons ?? Array.Empty<Button>();
            EndConfigure();
        }

        public void ConfigureAttempt(
            LudusTutorialAttemptTarget target,
            string pendingInstruction
        )
        {
            BeginConfigure(
                LudusTutorialScenarioGateMode.Attempt,
                pendingInstruction
            );
            attemptTarget = target;
            EndConfigure();
        }

        public void ConfigureProgress(
            LudusTutorialProgressControls controls,
            int targetValue,
            string pendingInstruction
        )
        {
            BeginConfigure(
                LudusTutorialScenarioGateMode.Progress,
                pendingInstruction
            );
            progressControls = controls;
            requiredValue = Mathf.Max(1, targetValue);
            EndConfigure();
        }

        public void ConfigureOrderedButtons(
            Button[] orderedButtons,
            string pendingInstruction
        )
        {
            BeginConfigure(
                LudusTutorialScenarioGateMode.OrderedButtons,
                pendingInstruction
            );
            buttons = orderedButtons ?? Array.Empty<Button>();
            EndConfigure();
        }

        public void ConfigureQuiz(
            LudusTutorialQuizControls controls,
            string pendingInstruction
        )
        {
            BeginConfigure(
                LudusTutorialScenarioGateMode.Quiz,
                pendingInstruction
            );
            quizControls = controls;
            EndConfigure();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                RegisterListeners();
            }
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void BeginConfigure(
            LudusTutorialScenarioGateMode gateMode,
            string pendingInstruction
        )
        {
            RemoveListeners();
            mode = gateMode;
            instruction = string.IsNullOrWhiteSpace(pendingInstruction)
                ? "Conclua a atividade para avançar."
                : pendingInstruction;
            buttons = Array.Empty<Button>();
            inputField = null;
            draggableItem = null;
            attemptTarget = null;
            progressControls = null;
            quizControls = null;
            requiredValue = 1;
            automaticButtonCompleted = false;
            automaticInputCompleted = false;
            automaticDragCompleted = false;
            orderedButtonIndex = 0;
            isCompleted = false;
        }

        private void EndConfigure()
        {
            if (Application.isPlaying && isActiveAndEnabled)
            {
                RegisterListeners();
            }

            NotifyStateChanged();
        }

        private void RegisterListeners()
        {
            if (listenersRegistered)
            {
                return;
            }

            buttonActions = new UnityAction[buttons.Length];
            for (int index = 0; index < buttons.Length; index++)
            {
                int capturedIndex = index;
                UnityAction action = () => HandleButtonClick(capturedIndex);
                buttonActions[index] = action;
                buttons[index]?.onClick.AddListener(action);
            }

            inputAction = HandleInputCompleted;
            inputField?.onEndEdit.AddListener(inputAction);
            if (draggableItem != null)
            {
                draggableItem.DragCompleted += HandleDragCompleted;
            }

            if (attemptTarget != null)
            {
                attemptTarget.AttemptEvaluated += HandleAttemptEvaluated;
            }

            if (progressControls != null)
            {
                progressControls.ValueChanged += HandleProgressChanged;
            }

            if (quizControls != null)
            {
                quizControls.StateChanged += HandleQuizStateChanged;
            }

            listenersRegistered = true;
        }

        private void RemoveListeners()
        {
            if (!listenersRegistered)
            {
                return;
            }

            for (int index = 0; index < buttons.Length; index++)
            {
                if (index < buttonActions.Length)
                {
                    buttons[index]?.onClick.RemoveListener(
                        buttonActions[index]
                    );
                }
            }

            if (inputAction != null)
            {
                inputField?.onEndEdit.RemoveListener(inputAction);
            }

            if (draggableItem != null)
            {
                draggableItem.DragCompleted -= HandleDragCompleted;
            }

            if (attemptTarget != null)
            {
                attemptTarget.AttemptEvaluated -= HandleAttemptEvaluated;
            }

            if (progressControls != null)
            {
                progressControls.ValueChanged -= HandleProgressChanged;
            }

            if (quizControls != null)
            {
                quizControls.StateChanged -= HandleQuizStateChanged;
            }

            buttonActions = Array.Empty<UnityAction>();
            inputAction = null;
            listenersRegistered = false;
        }

        private void HandleButtonClick(int index)
        {
            if (isCompleted)
            {
                return;
            }

            if (mode == LudusTutorialScenarioGateMode.AnyButton)
            {
                Complete();
                return;
            }

            if (mode == LudusTutorialScenarioGateMode.AutomaticInteractions)
            {
                automaticButtonCompleted = true;
                EvaluateAutomaticInteractions();
                return;
            }

            if (mode == LudusTutorialScenarioGateMode.OrderedButtons)
            {
                if (index == orderedButtonIndex)
                {
                    orderedButtonIndex++;
                    if (orderedButtonIndex >= buttons.Length)
                    {
                        Complete();
                    }
                    else
                    {
                        NotifyStateChanged();
                    }
                }
            }
        }

        private void HandleInputCompleted(string value)
        {
            if (
                mode != LudusTutorialScenarioGateMode.AutomaticInteractions ||
                string.IsNullOrWhiteSpace(value)
            )
            {
                return;
            }

            automaticInputCompleted = true;
            EvaluateAutomaticInteractions();
        }

        private void HandleDragCompleted(LudusTutorialDraggableItem item)
        {
            if (mode != LudusTutorialScenarioGateMode.AutomaticInteractions)
            {
                return;
            }

            automaticDragCompleted = true;
            EvaluateAutomaticInteractions();
        }

        private void HandleAttemptEvaluated(bool correct)
        {
            if (mode == LudusTutorialScenarioGateMode.Attempt && correct)
            {
                Complete();
            }
        }

        private void HandleProgressChanged(int value)
        {
            if (
                mode == LudusTutorialScenarioGateMode.Progress &&
                value >= requiredValue
            )
            {
                Complete();
            }
        }

        private void HandleQuizStateChanged()
        {
            if (
                mode == LudusTutorialScenarioGateMode.Quiz &&
                quizControls != null &&
                quizControls.IsCompleted
            )
            {
                Complete();
            }
            else
            {
                NotifyStateChanged();
            }
        }

        private void EvaluateAutomaticInteractions()
        {
            if (
                automaticButtonCompleted &&
                automaticInputCompleted &&
                automaticDragCompleted
            )
            {
                Complete();
            }
            else
            {
                NotifyStateChanged();
            }
        }

        private string BuildAutomaticPendingMessage()
        {
            string pending = "Conclua: ";
            if (!automaticButtonCompleted)
            {
                pending += "clique no botão; ";
            }

            if (!automaticInputCompleted)
            {
                pending += "preencha e confirme o campo; ";
            }

            if (!automaticDragCompleted)
            {
                pending += "arraste a peça; ";
            }

            return pending.TrimEnd(' ', ';', '.').Trim() + ".";
        }

        private void Complete()
        {
            if (isCompleted)
            {
                return;
            }

            isCompleted = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
