using UnityEngine;
using UnityEngine.InputSystem;

namespace LudusSDK
{
    [AddComponentMenu("LUDUS/Coletor de mouse (novo Input System)")]
    public sealed class LudusInputSystemPointerTracker : MonoBehaviour
    {
        private const float DragMinimumDistancePixels = 5f;

        [Tooltip("Opcional: a base LUDUS SDK Ã© localizada automaticamente.")]
        public LudusSessionController sessionController;

        [Tooltip("Registra pontos da trajetÃ³ria enquanto hÃ¡ um recorte ativo.")]
        public bool captureMousePath = true;

        [Min(1)]
        [Tooltip("Intervalo mÃ­nimo entre pontos da trajetÃ³ria, em milissegundos.")]
        public int mouseSampleIntervalMs = 50;

        private InputAction pointerPositionAction;
        private InputAction pointerPressAction;
        private float nextMouseSampleAt;
        private float nextDragSampleAt;
        private bool warnedAboutUnavailablePointer;
        private bool receivedInputSystemPointer;
        private bool hasLastFallbackPointerPosition;
        private Vector2 lastFallbackPointerPosition;
        private bool isDragging;
        private bool hasRecordedDragStart;
        private bool hasDragStartPosition;
        private Vector2 dragStartPosition;

        private void Awake()
        {
            if (sessionController == null)
            {
                sessionController =
                    FindFirstObjectByType<LudusSessionController>();
            }
        }

        private void OnEnable()
        {
            pointerPositionAction = new InputAction(
                "LudusPointerPosition",
                InputActionType.PassThrough,
                "<Pointer>/position"
            );
            pointerPressAction = new InputAction(
                "LudusPointerPress",
                InputActionType.Button,
                "<Pointer>/press"
            );

            pointerPressAction.started += HandlePointerPressed;
            pointerPressAction.canceled += HandlePointerReleased;
            pointerPositionAction.Enable();
            pointerPressAction.Enable();
            nextMouseSampleAt = 0f;
            nextDragSampleAt = 0f;
            warnedAboutUnavailablePointer = false;
            receivedInputSystemPointer = false;
            hasLastFallbackPointerPosition = false;
            isDragging = false;
            hasRecordedDragStart = false;
            hasDragStartPosition = false;
        }

        private void OnDisable()
        {
            if (pointerPressAction != null)
            {
                pointerPressAction.started -= HandlePointerPressed;
                pointerPressAction.canceled -= HandlePointerReleased;
                pointerPressAction.Disable();
                pointerPressAction.Dispose();
                pointerPressAction = null;
            }

            if (pointerPositionAction != null)
            {
                pointerPositionAction.Disable();
                pointerPositionAction.Dispose();
                pointerPositionAction = null;
            }
        }

        private void Update()
        {
            if (sessionController == null || !sessionController.HasActiveSession ||
                !sessionController.HasActiveCaptureContext)
            {
                return;
            }

            if (!TryGetPointerPosition(out Vector2 position))
            {
                return;
            }

            if (captureMousePath && Time.unscaledTime >= nextMouseSampleAt)
            {
                sessionController.TryRecordMousePoint(position, out _);
                nextMouseSampleAt = Time.unscaledTime +
                    Mathf.Max(10, mouseSampleIntervalMs) / 1000f;
            }

            TryRecordDragMovement(position);
            receivedInputSystemPointer = true;
        }

        private void HandlePointerPressed(InputAction.CallbackContext context)
        {
            if (sessionController == null || !sessionController.HasActiveSession ||
                !sessionController.HasActiveCaptureContext)
            {
                return;
            }

            isDragging = true;
            hasRecordedDragStart = false;
            hasDragStartPosition = false;

            if (TryGetPointerPosition(out Vector2 position))
            {
                sessionController.TryRecordClick(position, out _);
                dragStartPosition = position;
                hasDragStartPosition = true;
                receivedInputSystemPointer = true;
            }
        }

        private void HandlePointerReleased(InputAction.CallbackContext context)
        {
            if (!isDragging)
            {
                return;
            }

            if (TryGetPointerPosition(out Vector2 position))
            {
                if (hasRecordedDragStart)
                {
                    sessionController.TryRecordDragPoint(position, "end", out _);
                }
                receivedInputSystemPointer = true;
            }

            isDragging = false;
            hasRecordedDragStart = false;
            hasDragStartPosition = false;
        }

        private void OnGUI()
        {
            if (receivedInputSystemPointer || sessionController == null ||
                !sessionController.HasActiveSession ||
                !sessionController.HasActiveCaptureContext)
            {
                return;
            }

            Event currentEvent = Event.current;
            bool isMouseEvent = currentEvent != null && currentEvent.isMouse;
            bool isRepaint = currentEvent != null &&
                currentEvent.type == EventType.Repaint;
            if (!isMouseEvent && !isRepaint)
            {
                return;
            }

            Vector2 position = new Vector2(
                currentEvent.mousePosition.x,
                Mathf.Max(0f, Screen.height - currentEvent.mousePosition.y)
            );

            if (currentEvent.type == EventType.MouseDown)
            {
                sessionController.TryRecordClick(position, out _);
                isDragging = true;
                hasRecordedDragStart = false;
                dragStartPosition = position;
                hasDragStartPosition = true;
            }

            bool isMouseMovement = currentEvent.type == EventType.MouseMove ||
                currentEvent.type == EventType.MouseDrag || isRepaint;
            TryRecordFallbackMousePoint(position, isMouseMovement);

            if (currentEvent.type == EventType.MouseDrag)
            {
                TryRecordDragMovement(position);
            }

            if (currentEvent.type == EventType.MouseUp && isDragging)
            {
                if (hasRecordedDragStart)
                {
                    sessionController.TryRecordDragPoint(position, "end", out _);
                }
                isDragging = false;
                hasRecordedDragStart = false;
                hasDragStartPosition = false;
            }
        }

        private void TryRecordFallbackMousePoint(
            Vector2 position,
            bool isMouseMovement
        )
        {
            if (!captureMousePath || !isMouseMovement ||
                Time.unscaledTime < nextMouseSampleAt)
            {
                return;
            }

            if (hasLastFallbackPointerPosition &&
                (position - lastFallbackPointerPosition).sqrMagnitude < 0.25f)
            {
                return;
            }

            sessionController.TryRecordMousePoint(position, out _);
            lastFallbackPointerPosition = position;
            hasLastFallbackPointerPosition = true;
            nextMouseSampleAt = Time.unscaledTime +
                Mathf.Max(10, mouseSampleIntervalMs) / 1000f;
        }

        private void TryRecordDragMovement(Vector2 position)
        {
            if (!isDragging)
            {
                return;
            }

            if (!hasDragStartPosition)
            {
                dragStartPosition = position;
                hasDragStartPosition = true;
                return;
            }

            if (!hasRecordedDragStart)
            {
                if (
                    (position - dragStartPosition).sqrMagnitude <
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

            if (!hasRecordedDragStart)
            {
                return;
            }

            if (Time.unscaledTime < nextDragSampleAt)
            {
                return;
            }

            sessionController.TryRecordDragPoint(position, "move", out _);
            nextDragSampleAt = Time.unscaledTime +
                Mathf.Max(10, mouseSampleIntervalMs) / 1000f;
        }

        private bool TryGetPointerPosition(out Vector2 position)
        {
            position = pointerPositionAction == null
                ? Vector2.zero
                : pointerPositionAction.ReadValue<Vector2>();

            if (position != Vector2.zero)
            {
                return true;
            }

            if (!warnedAboutUnavailablePointer && sessionController != null &&
                sessionController.Config != null && sessionController.Config.debugMode)
            {
                Debug.Log(
                    "[LUDUS] O novo Input System ainda não forneceu uma posição de ponteiro válida no Editor. A coleta usa os eventos da Game View como compatibilidade e não registra pontos (0,0).",
                    this
                );
                warnedAboutUnavailablePointer = true;
            }

            return false;
        }
    }
}
