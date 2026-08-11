using UnityEngine;
using UnityEngine.EventSystems;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusTrackedDraggable :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private string dashboardName = string.Empty;

        [SerializeField]
        [Min(0f)]
        private float minimumDistancePixels = 5f;

        private bool pointerTrackingActive;
        private bool dragDetected;
        private Vector2 startPosition;
        private float startedAtUnscaledTime;
        private int lastRecordedFrame = -1;

#if UNITY_EDITOR
        private bool editorPressStartedInside;
        private bool editorDragDetected;
        private Vector2 editorStartPosition;
        private float editorStartedAtUnscaledTime;
#endif

        public bool TrackingEnabled => trackingEnabled;

        public string DashboardName => string.IsNullOrWhiteSpace(dashboardName)
            ? gameObject.name
            : dashboardName.Trim();

        public float MinimumDistancePixels => minimumDistancePixels;

        private void Reset()
        {
            dashboardName = gameObject.name;
            minimumDistancePixels = 5f;
        }

        private void OnEnable()
        {
            lastRecordedFrame = -1;
            ResetTrackingState();

#if UNITY_EDITOR
            ResetEditorTrackingState();
#endif
        }

        private void OnDisable()
        {
            ResetTrackingState();

#if UNITY_EDITOR
            ResetEditorTrackingState();
#endif
        }

        public void Configure(
            string nameForDashboard,
            bool enabled = true,
            float minimumDistance = 5f
        )
        {
            dashboardName = string.IsNullOrWhiteSpace(nameForDashboard)
                ? gameObject.name
                : nameForDashboard.Trim();
            trackingEnabled = enabled;
            minimumDistancePixels = Mathf.Max(0f, minimumDistance);
        }

        public bool TryRecordCompletedDrag(
            Vector2 dragStartPosition,
            Vector2 dragEndPosition,
            int durationMs,
            out string errorMessage
        )
        {
            if (lastRecordedFrame == Time.frameCount)
            {
                errorMessage =
                    "Este arraste já foi registrado neste quadro.";
                return false;
            }

            if (!trackingEnabled)
            {
                errorMessage =
                    "O acompanhamento deste objeto arrastável está desativado.";
                return false;
            }

            if (
                Vector2.Distance(dragStartPosition, dragEndPosition) <
                minimumDistancePixels
            )
            {
                errorMessage =
                    "O movimento foi menor que a distância mínima configurada para um arraste.";
                return false;
            }

            bool recorded = LudusSdk.TryRecordTrackedDrag(
                DashboardName,
                dragStartPosition,
                dragEndPosition,
                Mathf.Max(0, durationMs),
                out errorMessage
            );

            if (recorded)
            {
                lastRecordedFrame = Time.frameCount;
            }

            return recorded;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BeginTracking(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!pointerTrackingActive)
            {
                BeginTracking(eventData);
            }

            dragDetected = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (pointerTrackingActive)
            {
                dragDetected = true;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CompleteTracking(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CompleteTracking(eventData);
        }

        public static bool CanTrack(GameObject target)
        {
            return target != null;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!trackingEnabled)
            {
                return;
            }

            Event currentEvent = Event.current;

            if (currentEvent == null || !currentEvent.isMouse)
            {
                return;
            }

            Vector2 screenPosition = new Vector2(
                currentEvent.mousePosition.x,
                Mathf.Max(0f, Screen.height - currentEvent.mousePosition.y)
            );

            if (currentEvent.type == EventType.MouseDown)
            {
                editorPressStartedInside =
                    IsScreenPositionInside(screenPosition);
                editorDragDetected = false;
                editorStartPosition = screenPosition;
                editorStartedAtUnscaledTime = Time.unscaledTime;
                return;
            }

            if (
                currentEvent.type == EventType.MouseDrag &&
                editorPressStartedInside
            )
            {
                editorDragDetected =
                    Vector2.Distance(editorStartPosition, screenPosition) >=
                    minimumDistancePixels;
                return;
            }

            if (
                currentEvent.type != EventType.MouseUp ||
                !editorPressStartedInside
            )
            {
                return;
            }

            float distance = Vector2.Distance(
                editorStartPosition,
                screenPosition
            );
            bool shouldRecord =
                editorDragDetected || distance >= minimumDistancePixels;
            int durationMs = Mathf.Max(
                0,
                Mathf.RoundToInt(
                    (Time.unscaledTime - editorStartedAtUnscaledTime) * 1000f
                )
            );
            Vector2 recordedStart = editorStartPosition;
            ResetEditorTrackingState();

            if (shouldRecord)
            {
                TryRecordCompletedDrag(
                    recordedStart,
                    screenPosition,
                    durationMs,
                    out _
                );
            }
        }

        private bool IsScreenPositionInside(Vector2 screenPosition)
        {
            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                return false;
            }

            Canvas ownerCanvas = GetComponentInParent<Canvas>();
            Camera eventCamera =
                ownerCanvas == null ||
                ownerCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : ownerCanvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                screenPosition,
                eventCamera
            );
        }

        private void ResetEditorTrackingState()
        {
            editorPressStartedInside = false;
            editorDragDetected = false;
            editorStartPosition = default;
            editorStartedAtUnscaledTime = 0f;
        }
#endif

        private void BeginTracking(PointerEventData eventData)
        {
            if (!trackingEnabled || eventData == null)
            {
                ResetTrackingState();
                return;
            }

            pointerTrackingActive = true;
            dragDetected = false;
            startPosition = eventData.position;
            startedAtUnscaledTime = Time.unscaledTime;
        }

        private void CompleteTracking(PointerEventData eventData)
        {
            if (!pointerTrackingActive || eventData == null)
            {
                return;
            }

            Vector2 endPosition = eventData.position;
            float distance = Vector2.Distance(startPosition, endPosition);
            bool shouldRecord =
                dragDetected && distance >= minimumDistancePixels;
            int durationMs = Mathf.Max(
                0,
                Mathf.RoundToInt(
                    (Time.unscaledTime - startedAtUnscaledTime) * 1000f
                )
            );
            Vector2 recordedStart = startPosition;
            ResetTrackingState();

            if (shouldRecord)
            {
                TryRecordCompletedDrag(
                    recordedStart,
                    endPosition,
                    durationMs,
                    out _
                );
            }
        }

        private void ResetTrackingState()
        {
            pointerTrackingActive = false;
            dragDetected = false;
            startPosition = default;
            startedAtUnscaledTime = 0f;
        }
    }
}
