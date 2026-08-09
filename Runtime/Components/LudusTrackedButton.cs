using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class LudusTrackedButton : MonoBehaviour
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private string dashboardName = string.Empty;

        private Button trackedButton;
        private int lastAutomaticActivationFrame = -1;

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
            trackedButton = GetComponent<Button>();
            lastAutomaticActivationFrame = -1;

#if UNITY_EDITOR
            editorPressStartedInside = false;
#endif

            if (trackedButton == null)
            {
                return;
            }

            trackedButton.onClick.RemoveListener(HandleButtonActivated);
            trackedButton.onClick.AddListener(HandleButtonActivated);
        }

        private void OnDisable()
        {
            if (trackedButton != null)
            {
                trackedButton.onClick.RemoveListener(HandleButtonActivated);
            }

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

        public bool TryRecordActivation(out string errorMessage)
        {
            if (!trackingEnabled)
            {
                errorMessage = "O acompanhamento deste botão está desativado.";
                return false;
            }

            return LudusSdk.TryRecordTrackedInteraction(
                DashboardName,
                "button",
                "activated",
                out errorMessage
            );
        }

        public static bool CanTrack(GameObject target)
        {
            return target != null && target.GetComponent<Button>() != null;
        }

        private void HandleButtonActivated()
        {
            TryRecordAutomaticActivation();
        }


#if UNITY_EDITOR
        private void OnGUI()
        {
            if (
                !trackingEnabled ||
                trackedButton == null ||
                !trackedButton.IsActive() ||
                !trackedButton.IsInteractable()
            )
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
                TryRecordAutomaticActivation();
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

        private void TryRecordAutomaticActivation()
        {
            if (lastAutomaticActivationFrame == Time.frameCount)
            {
                return;
            }

            if (TryRecordActivation(out _))
            {
                lastAutomaticActivationFrame = Time.frameCount;
            }
        }
    }
}
