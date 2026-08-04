using UnityEngine;

namespace LudusSDK
{
    /// <summary>
    /// Painel criado somente pelo tutorial do SDK. Ele usa uma identidade
    /// fictícia e não deve ser adicionado às cenas de produção do jogo.
    /// </summary>
    public sealed class LudusTutorialTestPanel : MonoBehaviour
    {
        [Header("Identidade fictícia do tutorial")]

        [InspectorName("ID do estudante fictício")]
        [Tooltip("Use somente um aluno fictício criado para validação. Nunca informe dados reais neste painel.")]
        public string studentId = "000000000000000000000001";

        [InspectorName("Nome de exibição fictício")]
        [Tooltip("Nome usado apenas na sessão de demonstração.")]
        public string playerId = "Estudante Fictício";

        private string statusMessage =
            "Pronto para iniciar uma sessão fictícia.";

        private void OnGUI()
        {
            LudusSessionController controller =
                FindFirstObjectByType<LudusSessionController>();
            bool hasActiveSession =
                controller != null && controller.HasActiveSession;

            GUI.Box(
                new Rect(16, 16, 380, 170),
                "LUDUS — Tutorial de teste"
            );
            GUI.Label(
                new Rect(32, 48, 348, 38),
                "Este painel existe apenas nesta cena tutorial. " +
                "Ele não será adicionado ao seu jogo."
            );

            string buttonLabel = hasActiveSession
                ? "Encerrar sessão e exibir JSON"
                : "Iniciar sessão fictícia";

            if (GUI.Button(new Rect(32, 94, 348, 34), buttonLabel))
            {
                HandleSessionButton(hasActiveSession);
            }

            GUI.Label(new Rect(32, 138, 348, 38), statusMessage);
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
                    ? "Sessão encerrada. JSON no Console."
                    : endError;

                if (ended)
                {
                    Debug.Log("[LUDUS] JSON do tutorial: " + json);
                }

                return;
            }

            bool started = LudusSdk.TryStartSession(
                studentId,
                playerId,
                out string startError
            );
            statusMessage = started
                ? "Sessão iniciada. Mova e clique dentro da Game View."
                : startError;
        }
    }
}
