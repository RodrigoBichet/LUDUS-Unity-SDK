using System;
using UnityEngine;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusLegacyPointerTracker : MonoBehaviour
    {
        private const float DragMinimumDistancePixels = 5f;

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
        private float nextDragSampleAt;
        private bool isDragging;
        private bool hasRecordedDragStart;
        private Vector2 dragStartPosition;

        private void OnEnable()
        {
            nextMouseSampleAt = 0f;
            nextDragSampleAt = 0f;
            isDragging = false;
            hasRecordedDragStart = false;
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
                    isDragging = true;
                    hasRecordedDragStart = false;
                    dragStartPosition = pointerPosition;
                }

                if (isDragging && Input.GetMouseButton(0))
                {
                    TryRecordDragMovement(pointerPosition);
                }

                if (isDragging && Input.GetMouseButtonUp(0))
                {
                    if (hasRecordedDragStart)
                    {
                        sessionController.TryRecordDragPoint(
                            pointerPosition,
                            "end",
                            out _
                        );
                    }
                    isDragging = false;
                    hasRecordedDragStart = false;
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private void TryRecordDragMovement(Vector2 pointerPosition)
        {
            if (!hasRecordedDragStart)
            {
                if (
                    (pointerPosition - dragStartPosition).sqrMagnitude <
                    DragMinimumDistancePixels * DragMinimumDistancePixels
                )
                {
                    return;
                }

                hasRecordedDragStart = sessionController.TryRecordDragPoint(
                    dragStartPosition,
                    "start",
                    out _
                );
                nextDragSampleAt = 0f;
            }

            if (
                !hasRecordedDragStart ||
                Time.unscaledTime < nextDragSampleAt
            )
            {
                return;
            }

            sessionController.TryRecordDragPoint(
                pointerPosition,
                "move",
                out _
            );
            nextDragSampleAt = Time.unscaledTime +
                Mathf.Max(10, mouseSampleIntervalMs) / 1000f;
        }
    }
}
