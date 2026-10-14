using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Apresenta o ciclo semântico como um fluxo guiado no laboratório. Os
    /// eventos LUDUS continuam sendo ligados pela pessoa desenvolvedora.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialSemanticFlowControls : MonoBehaviour
    {
        [SerializeField]
        private Button[] orderedButtons = new Button[0];

        [SerializeField]
        private Text progressLabel;

        [SerializeField]
        [Min(0)]
        private int currentStep;

        private bool listenersRegistered;
        private UnityAction[] buttonActions = new UnityAction[0];

        public int CurrentStep => currentStep;

        public void Configure(Button[] buttons, Text label)
        {
            RemoveListeners();
            orderedButtons = buttons ?? new Button[0];
            progressLabel = label;
            currentStep = 0;
            if (Application.isPlaying && isActiveAndEnabled)
            {
                RegisterListeners();
            }

            ApplyState();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                RegisterListeners();
            }

            ApplyState();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void RegisterListeners()
        {
            if (listenersRegistered)
            {
                return;
            }

            buttonActions = new UnityAction[orderedButtons.Length];
            for (int index = 0; index < orderedButtons.Length; index++)
            {
                int capturedIndex = index;
                UnityAction action = () => HandleStep(capturedIndex);
                buttonActions[index] = action;
                orderedButtons[index]?.onClick.AddListener(action);
            }

            listenersRegistered = true;
        }

        private void RemoveListeners()
        {
            if (!listenersRegistered)
            {
                return;
            }

            for (int index = 0; index < orderedButtons.Length; index++)
            {
                if (index < buttonActions.Length)
                {
                    orderedButtons[index]?.onClick.RemoveListener(
                        buttonActions[index]
                    );
                }
            }

            buttonActions = new UnityAction[0];
            listenersRegistered = false;
        }

        private void HandleStep(int index)
        {
            if (index != currentStep)
            {
                return;
            }

            currentStep++;
            ApplyState();
        }

        private void ApplyState()
        {
            for (int index = 0; index < orderedButtons.Length; index++)
            {
                if (orderedButtons[index] != null)
                {
                    ColorBlock colors = orderedButtons[index].colors;
                    colors.disabledColor = new Color(
                        0.035f,
                        0.055f,
                        0.085f,
                        1f
                    );
                    orderedButtons[index].colors = colors;
                    orderedButtons[index].interactable = index == currentStep;
                }
            }

            if (progressLabel == null)
            {
                return;
            }

            string[] markers = { "Categoria", "Início", "Resultado", "Conclusão" };
            string summary = string.Empty;
            for (int index = 0; index < markers.Length; index++)
            {
                if (index > 0)
                {
                    summary += "  →  ";
                }

                summary += index < currentStep
                    ? $"✓ {markers[index]}"
                    : $"○ {markers[index]}";
            }

            progressLabel.text = summary;
            progressLabel.color = currentStep >= orderedButtons.Length
                ? new Color(0.52f, 0.86f, 0.76f)
                : new Color(0.72f, 0.8f, 0.9f);
        }
    }
}
