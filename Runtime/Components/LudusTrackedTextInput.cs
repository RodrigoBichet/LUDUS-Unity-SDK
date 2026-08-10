using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusTrackedTextInput : MonoBehaviour
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private string dashboardName = string.Empty;

        private InputField legacyInputField;
        private TMP_InputField tmpInputField;

        public bool TrackingEnabled => trackingEnabled;

        public string DashboardName => string.IsNullOrWhiteSpace(dashboardName)
            ? gameObject.name
            : dashboardName.Trim();

        public string DetectedFieldType => tmpInputField != null
            ? "Campo de texto TextMeshPro"
            : legacyInputField != null
                ? "Campo de texto da interface Unity"
                : "Campo de texto não detectado";

        private void Reset()
        {
            dashboardName = gameObject.name;
            FindInputField();
        }

        private void OnEnable()
        {
            FindInputField();

            if (tmpInputField != null)
            {
                tmpInputField.onEndEdit.RemoveListener(HandleTextCompleted);
                tmpInputField.onEndEdit.AddListener(HandleTextCompleted);
                return;
            }

            if (legacyInputField != null)
            {
                legacyInputField.onEndEdit.RemoveListener(HandleTextCompleted);
                legacyInputField.onEndEdit.AddListener(HandleTextCompleted);
            }
        }

        private void OnDisable()
        {
            if (tmpInputField != null)
            {
                tmpInputField.onEndEdit.RemoveListener(HandleTextCompleted);
            }

            if (legacyInputField != null)
            {
                legacyInputField.onEndEdit.RemoveListener(HandleTextCompleted);
            }
        }

        public void Configure(string nameForDashboard, bool enabled = true)
        {
            dashboardName = string.IsNullOrWhiteSpace(nameForDashboard)
                ? gameObject.name
                : nameForDashboard.Trim();
            trackingEnabled = enabled;
            FindInputField();
        }

        public bool TryRecordCompletion(
            string completedText,
            out string errorMessage
        )
        {
            if (!trackingEnabled)
            {
                errorMessage =
                    "O acompanhamento deste campo de texto está desativado.";
                return false;
            }

            int characterCount = completedText?.Length ?? 0;

            if (TryGetScreenCenter(out Vector2 position))
            {
                return LudusSdk.TryRecordTextInputCompletion(
                    DashboardName,
                    characterCount,
                    position,
                    out errorMessage
                );
            }

            return LudusSdk.TryRecordTextInputCompletion(
                DashboardName,
                characterCount,
                out errorMessage
            );
        }

        public static bool CanTrack(GameObject target)
        {
            return target != null &&
                (
                    target.GetComponent<TMP_InputField>() != null ||
                    target.GetComponent<InputField>() != null
                );
        }

        private void HandleTextCompleted(string completedText)
        {
            TryRecordCompletion(completedText, out _);
        }

        private void FindInputField()
        {
            tmpInputField = GetComponent<TMP_InputField>();
            legacyInputField = tmpInputField == null
                ? GetComponent<InputField>()
                : null;
        }

        private bool TryGetScreenCenter(out Vector2 position)
        {
            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                position = default;
                return false;
            }

            Canvas ownerCanvas = GetComponentInParent<Canvas>();
            Camera eventCamera =
                ownerCanvas == null ||
                ownerCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : ownerCanvas.worldCamera;

            position = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                rectTransform.TransformPoint(rectTransform.rect.center)
            );

            return position.x >= 0f &&
                position.y >= 0f &&
                position.x <= Screen.width &&
                position.y <= Screen.height;
        }
    }
}
