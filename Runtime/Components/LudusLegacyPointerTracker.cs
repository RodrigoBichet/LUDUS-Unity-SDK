using System;
using UnityEngine;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusLegacyPointerTracker : MonoBehaviour
    {
        [Header("Referência")]
        [Tooltip("Controlador que mantém a sessão LUDUS ativa.")]
        public LudusSessionController sessionController;

        [Header("Trajetória")]
        [Tooltip("Registra a posição do mouse em intervalos regulares.")]
        public bool captureMousePath = true;

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
