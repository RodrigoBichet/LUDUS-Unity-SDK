using UnityEngine;
using UnityEngine.EventSystems;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusTrackedClickable :
        MonoBehaviour,
        IPointerClickHandler
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private string dashboardName = string.Empty;

        private int lastRecordedFrame = -1;

#if UNITY_EDITOR
        private bool editorPressStartedInside;
#endif

        public bool TrackingEnabled => trackingEnabled;

        public string DashboardName => string.IsNullOrWhiteSpace(dashboardName)
            ? gameObject.name
            : dashboardName.Trim();

        private void Reset()
        {
            dashboardName = gameObject.name;
        }

        private void OnEnable()
        {
            lastRecordedFrame = -1;

#if UNITY_EDITOR
            editorPressStartedInside = false;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            editorPressStartedInside = false;
#endif
        }

        public void Configure(string nameForDashboard, bool enabled = true)
        {
            dashboardName = string.IsNullOrWhiteSpace(nameForDashboard)
                ? gameObject.name
                : nameForDashboard.Trim();
            trackingEnabled = enabled;
        }

        public bool TryRecordActivation(
            Vector2 position,
            out string errorMessage
        )
        {
            if (lastRecordedFrame == Time.frameCount)
            {
                errorMessage =
                    "Este acionamento já foi registrado neste quadro.";
                return false;
            }

            if (!trackingEnabled)
            {
                errorMessage =
                    "O acompanhamento deste objeto clicável está desativado.";
                return false;
            }

            bool recorded = LudusSdk.TryRecordTrackedInteraction(
                DashboardName,
                "clickable-object",
                "activated",
                position,
                out errorMessage
            );

            if (recorded)
            {
                lastRecordedFrame = Time.frameCount;
            }

            return recorded;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null)
            {
                TryRecordActivation(eventData.position, out _);
            }
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
            bool pointerInside = IsScreenPositionInside(screenPosition);

            if (currentEvent.type == EventType.MouseDown)
            {
                editorPressStartedInside = pointerInside;
                return;
            }

            if (currentEvent.type != EventType.MouseUp)
            {
                return;
            }

            bool shouldRecord = editorPressStartedInside && pointerInside;
            editorPressStartedInside = false;

            if (shouldRecord)
            {
                TryRecordActivation(screenPosition, out _);
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
#endif
    }
}
