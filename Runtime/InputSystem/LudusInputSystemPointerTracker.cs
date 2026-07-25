using UnityEngine;
using UnityEngine.InputSystem;

namespace LudusSDK
{
    [AddComponentMenu("LUDUS/Coletor de mouse (novo Input System)")]
    public sealed class LudusInputSystemPointerTracker : MonoBehaviour
    {
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
        private bool warnedAboutUnavailablePointer;
        private bool receivedInputSystemPointer;
        private bool hasLastFallbackPointerPosition;
        private Vector2 lastFallbackPointerPosition;

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

            pointerPressAction.performed += HandlePointerPress;
            pointerPositionAction.Enable();
            pointerPressAction.Enable();
            nextMouseSampleAt = 0f;
            warnedAboutUnavailablePointer = false;
            receivedInputSystemPointer = false;
            hasLastFallbackPointerPosition = false;
        }

        private void OnDisable()
        {
            if (pointerPressAction != null)
            {
                pointerPressAction.performed -= HandlePointerPress;
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
                !sessionController.HasActiveCaptureContext || !captureMousePath ||
                Time.unscaledTime < nextMouseSampleAt)
            {
                return;
            }

            if (!TryGetPointerPosition(out Vector2 position))
            {
                return;
            }

            sessionController.TryRecordMousePoint(position, out _);
            receivedInputSystemPointer = true;
            nextMouseSampleAt = Time.unscaledTime +
                Mathf.Max(10, mouseSampleIntervalMs) / 1000f;
        }

        private void HandlePointerPress(InputAction.CallbackContext context)
        {
            if (sessionController == null || !sessionController.HasActiveSession ||
                !sessionController.HasActiveCaptureContext ||
                !TryGetPointerPosition(out Vector2 position))
            {
                return;
            }

            sessionController.TryRecordClick(position, out _);
            receivedInputSystemPointer = true;
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
            }

            bool isMouseMovement = currentEvent.type == EventType.MouseMove ||
                currentEvent.type == EventType.MouseDrag || isRepaint;
            TryRecordFallbackMousePoint(position, isMouseMovement);
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
