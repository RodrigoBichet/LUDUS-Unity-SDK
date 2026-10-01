using UnityEngine;

namespace LudusSDK
{
    /// <summary>
    /// Painel criado somente pelo tutorial do SDK. Ele usa uma identidade
    /// fictícia e não deve ser adicionado às cenas de produção do jogo.
    /// </summary>
    public sealed class LudusTutorialTestPanel : MonoBehaviour
    {
        private const float MinimumPanelWidth = 520f;
        private const float MaximumPanelWidth = 720f;
        private const float BasePanelHeight = 300f;
        private const float ExercisePanelHeight = 365f;

        [Header("Identificação do teste")]

        [InspectorName("Nome exibido no arquivo (opcional)")]
        [Tooltip("Use um rótulo simples, como 'teste-webgl'. O aluno será escolhido no Dashboard ao importar o JSON.")]
        public string sessionDisplayName = "Teste do tutorial";

        private string statusMessage =
            "Pronto para iniciar uma sessão fictícia.";

        private void OnApplicationQuit()
        {
            LudusSessionController controller =
                FindFirstObjectByType<LudusSessionController>();

            if (controller == null || !controller.HasActiveSession)
            {
                return;
            }

            bool ended = LudusSdk.TryEndSession(
                out string json,
                out string endError
            );

            if (ended)
            {
                LogSessionSummary(
                    json,
                    "A sessão ativa do tutorial foi encerrada "
                        + "automaticamente ao sair do Play Mode."
                );
                return;
            }

            Debug.LogWarning(
                "[LUDUS] Não foi possível encerrar automaticamente a "
                    + "sessão do tutorial: "
                    + endError
            );
        }

        private void OnGUI()
        {
            LudusSessionController controller =
                FindFirstObjectByType<LudusSessionController>();
            bool hasActiveSession =
                controller != null && controller.HasActiveSession;
            bool hasInteractionExercise =
                FindFirstObjectByType<LudusTutorialDraggableItem>() != null;

            float scale = Mathf.Clamp(Screen.height / 1080f, 0.8f, 1.25f);
            float horizontalMargin = Mathf.Clamp(
                Screen.width * 0.025f,
                16f,
                40f
            );
            float verticalMargin = Mathf.Clamp(
                Screen.height * 0.025f,
                16f,
                32f
            );
            float availableWidth = Mathf.Max(
                1f,
                Screen.width - horizontalMargin * 2f
            );
            float availableHeight = Mathf.Max(
                1f,
                Screen.height - verticalMargin * 2f
            );
            float panelWidth = Mathf.Min(
                Mathf.Clamp(
                    Screen.width * 0.38f,
                    MinimumPanelWidth,
                    MaximumPanelWidth
                ),
                availableWidth
            );
            float desiredPanelHeight = hasInteractionExercise
                ? ExercisePanelHeight
                : BasePanelHeight;
            float panelHeight = Mathf.Min(
                desiredPanelHeight * scale,
                availableHeight
            );
            Rect panelRect = new Rect(
                horizontalMargin,
                verticalMargin,
                panelWidth,
                panelHeight
            );

            GUI.Box(panelRect, GUIContent.none);

            float contentPadding = 24f * scale;
            Rect contentRect = new Rect(
                panelRect.x + contentPadding,
                panelRect.y + contentPadding,
                Mathf.Max(1f, panelRect.width - contentPadding * 2f),
                Mathf.Max(1f, panelRect.height - contentPadding * 2f)
            );

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(22f * scale),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };
            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(16f * scale),
                wordWrap = true,
            };
            GUIStyle statusStyle = new GUIStyle(bodyStyle)
            {
                fontStyle = FontStyle.Bold,
            };

            string buttonLabel = hasActiveSession
                ? "Encerrar sessão e gerar JSON"
                : "Iniciar sessão fictícia";

            GUILayout.BeginArea(contentRect);
            GUILayout.Label("LUDUS — Tutorial de teste", titleStyle);
            GUILayout.Space(6f * scale);
            GUILayout.Label(
                "Esta cena é isolada e não altera as cenas do seu jogo.",
                bodyStyle
            );
            GUILayout.Space(8f * scale);
            string instructions = hasInteractionExercise
                ? "1. Inicie a sessão fictícia.\n"
                    + "2. Clique no botão, conclua o campo de texto e arraste a peça.\n"
                    + "3. Encerre a sessão antes de sair do Play Mode para conferir o JSON."
                : "1. Inicie a sessão fictícia.\n"
                    + "2. Mova o ponteiro e clique na Game View.\n"
                    + "3. Encerre a sessão antes de sair do Play Mode para conferir o JSON.";
            GUILayout.Label(instructions, bodyStyle);
            GUILayout.FlexibleSpace();

            if (
                GUILayout.Button(
                    buttonLabel,
                    GUILayout.Height(52f * scale)
                )
            )
            {
                HandleSessionButton(hasActiveSession);
            }

            GUILayout.Space(8f * scale);
            GUILayout.Label(statusMessage, statusStyle);
            GUILayout.EndArea();
        }

        private void HandleSessionButton(bool hasActiveSession)
        {
            if (hasActiveSession)
            {
                bool ended = LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                );
                statusMessage = ended
                    ? "Sessão encerrada. JSON gerado e entregue conforme a configuração do SDK."
                    : endError;

                if (ended)
                {
                    LogSessionSummary(
                        json,
                        "Sessão do tutorial encerrada com sucesso."
                    );
                }

                return;
            }

            bool started = LudusSdk.TryStartSessionForManualImport(
                sessionDisplayName,
                out string startError
            );
            statusMessage = started
                ? "Sessão iniciada. Realize as interações dentro da Game View."
                : startError;
        }

        private static void LogSessionSummary(
            string json,
            string message
        )
        {
            int jsonLength = string.IsNullOrEmpty(json) ? 0 : json.Length;

            Debug.Log(
                "[LUDUS] "
                    + message
                    + " JSON gerado com "
                    + jsonLength
                    + " caracteres; o conteúdo completo não é exibido "
                    + "no Console para evitar expor ou duplicar imagens Base64."
            );
        }
    }
}
