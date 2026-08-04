using UnityEngine;

namespace LudusSDK
{
    /// <summary>
    /// Painel criado somente pelo tutorial do SDK. Ele usa uma identidade
    /// fictícia e não deve ser adicionado às cenas de produção do jogo.
    /// </summary>
    public sealed class LudusTutorialTestPanel : MonoBehaviour
    {
        [Header("Identificação do teste")]

        [InspectorName("Nome exibido no arquivo (opcional)")]
        [Tooltip("Use um rótulo simples, como 'teste-webgl'. O aluno será escolhido no Dashboard ao importar o JSON.")]
        public string sessionDisplayName = "Teste do tutorial";

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

            bool started = LudusSdk.TryStartSessionForManualImport(
                sessionDisplayName,
                out string startError
            );
            statusMessage = started
                ? "Sessão iniciada. Mova e clique dentro da Game View."
                : startError;
        }
    }
}
