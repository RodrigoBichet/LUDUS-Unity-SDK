using System;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Controla somente a demonstração visual de uma meta na cena tutorial.
    /// A regra semântica continua pertencendo ao LudusProgressThresholdAdapter.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialProgressControls : MonoBehaviour
    {
        [SerializeField]
        private Button increaseButton;

        [SerializeField]
        private Button decreaseButton;

        [SerializeField]
        private Button resetButton;

        [SerializeField]
        private Text valueLabel;

        [SerializeField]
        private int currentValue;

        private bool listenersRegistered;
        private int lastButtonActionFrame = int.MinValue;

        public int CurrentValue => currentValue;

        public event Action<int> ValueChanged;

        public void Configure(
            Button increase,
            Button decrease,
            Button reset,
            Text label
        )
        {
            RemoveListeners();
            increaseButton = increase;
            decreaseButton = decrease;
            resetButton = reset;
            valueLabel = label;
            currentValue = 0;
            if (Application.isPlaying && isActiveAndEnabled)
            {
                RegisterListeners();
            }
            UpdateValueLabel();
            ValueChanged?.Invoke(currentValue);
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                RegisterListeners();
            }
            UpdateValueLabel();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        public void Increase()
        {
            SetValue(currentValue + 1);
        }

        public void Decrease()
        {
            SetValue(currentValue - 1);
        }

        public void ResetValue()
        {
            currentValue = 0;
            LudusProgressThresholdAdapter adapter =
                GetComponent<LudusProgressThresholdAdapter>();
            if (adapter != null)
            {
                adapter.ReiniciarValorEMeta();
            }

            UpdateValueLabel();
            ValueChanged?.Invoke(currentValue);
        }

        private void SetValue(int value)
        {
            currentValue = Mathf.Max(0, value);
            LudusProgressThresholdAdapter adapter =
                GetComponent<LudusProgressThresholdAdapter>();
            if (adapter != null)
            {
                adapter.DefinirValor(currentValue);
            }

            UpdateValueLabel();
            ValueChanged?.Invoke(currentValue);
        }

        private void RegisterListeners()
        {
            if (listenersRegistered)
            {
                return;
            }

            increaseButton?.onClick.AddListener(HandleIncreaseButtonClick);
            decreaseButton?.onClick.AddListener(HandleDecreaseButtonClick);
            resetButton?.onClick.AddListener(HandleResetButtonClick);
            listenersRegistered = true;
        }

        private void RemoveListeners()
        {
            if (!listenersRegistered)
            {
                return;
            }

            increaseButton?.onClick.RemoveListener(HandleIncreaseButtonClick);
            decreaseButton?.onClick.RemoveListener(HandleDecreaseButtonClick);
            resetButton?.onClick.RemoveListener(HandleResetButtonClick);
            listenersRegistered = false;
        }

        private void HandleIncreaseButtonClick()
        {
            if (TryBeginButtonAction())
            {
                Increase();
            }
        }

        private void HandleDecreaseButtonClick()
        {
            if (TryBeginButtonAction())
            {
                Decrease();
            }
        }

        private void HandleResetButtonClick()
        {
            if (TryBeginButtonAction())
            {
                ResetValue();
            }
        }

        private bool TryBeginButtonAction()
        {
            int frame = Time.frameCount;
            if (lastButtonActionFrame == frame)
            {
                return false;
            }

            lastButtonActionFrame = frame;
            return true;
        }

        private void UpdateValueLabel()
        {
            if (valueLabel != null)
            {
                valueLabel.text = $"Valor atual: {currentValue} / Meta: 3";
            }
        }
    }
}
