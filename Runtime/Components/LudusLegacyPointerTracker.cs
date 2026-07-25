using System;
using UnityEngine;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusLegacyPointerTracker : MonoBehaviour
    {
        [Header("Referência")]
        [InspectorName("Objeto controlador LUDUS SDK")]
        [Tooltip("Arraste aqui o GameObject que possui o componente LudusSessionController.")]
        public LudusSessionController sessionController;

        [Header("Trajetória")]
        [InspectorName("Capturar trajetória do mouse")]
        [Tooltip("Registra a posição do mouse em intervalos regulares.")]
        public bool captureMousePath = true;

        [InspectorName("Intervalo entre pontos (ms)")]
        [Range(10, 1000)]
        [Tooltip("Intervalo mínimo, em milissegundos, entre pontos do mouse.")]
        public int mouseSampleIntervalMs = 50;

        private float nextMouseSampleAt;

        private void OnEnable()
        {
            nextMouseSampleAt = 0f;
        }

        private void Update()
        {
            if (
                sessionController == null ||
                !sessionController.HasActiveSession ||
                !sessionController.HasActiveCaptureContext
            )
            {
                return;
            }

            try
            {
                Vector2 pointerPosition = Input.mousePosition;

                if (
                    captureMousePath &&
                    Time.unscaledTime >= nextMouseSampleAt
                )
                {
                    sessionController.TryRecordMousePoint(
                        pointerPosition,
                        out _
                    );

                    nextMouseSampleAt = Time.unscaledTime +
                        Mathf.Max(10, mouseSampleIntervalMs) / 1000f;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    sessionController.TryRecordClick(pointerPosition, out _);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }
    }
}
