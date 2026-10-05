using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("LUDUS/Fluxo/Botão para exportar execução")]
    public sealed class LudusRunExportButton : MonoBehaviour
    {
        [InspectorName("Ocultar quando o envio automático estiver ativo")]
        [Tooltip("Evita exibir downloads nos computadores dos jogadores quando as sessões já são enviadas ao Dashboard.")]
        public bool hideInAutomaticDashboardMode = true;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            button.onClick.AddListener(ExportCurrentRun);
            ApplyVisibility();
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ExportCurrentRun);
            }
        }

        public void ExportCurrentRun()
        {
            if (
                !LudusSdk.TryDownloadCurrentRun(
                    out string errorMessage
                )
            )
            {
                Debug.LogWarning("[LUDUS] " + errorMessage, this);
            }
        }

        private void ApplyVisibility()
        {
            if (
                !hideInAutomaticDashboardMode ||
                !LudusSdk.TryGetSingleSessionController(
                    out LudusSessionController controller,
                    out _
                ) ||
                controller.Config == null
            )
            {
                return;
            }

            if (
                controller.Config.GetDeliveryMode() ==
                LudusDeliveryMode.AutomaticDashboard
            )
            {
                gameObject.SetActive(false);
            }
        }
    }
}
