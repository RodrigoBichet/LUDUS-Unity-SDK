using LudusSDK;
using UnityEngine;

public sealed class LudusBasicSessionExample : MonoBehaviour
{
    [Tooltip("Opcional: o SDK localiza automaticamente a base LUDUS SDK.")]
    public LudusSessionController sessionController;

    [Header("Identidade fictícia para o laboratório")]
    [Tooltip("Use apenas um studentId de aluno fictício criado para validação.")]
    public string studentId = "000000000000000000000010";

    [Tooltip("Nome exibido no JSON de teste.")]
    public string playerId = "Estudante Fictício";

    private void Awake()
    {
        TryResolveSessionController();
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !TryResolveSessionController())
        {
            return;
        }

        GUI.Box(new Rect(16, 16, 320, 118), "LUDUS — Laboratório");
        GUI.Label(
            new Rect(32, 44, 288, 24),
            "Experimente a coleta sem configurar uma cena."
        );

        bool hasActiveSession = sessionController.HasActiveSession;
        string buttonLabel = hasActiveSession
            ? "Encerrar sessão e exibir JSON"
            : "Iniciar sessão fictícia";

        if (GUI.Button(new Rect(32, 78, 288, 38), buttonLabel))
        {
            if (hasActiveSession)
            {
                EndSessionAndLogJson();
            }
            else
            {
                StartFictionalSession();
            }
        }
    }

    [ContextMenu("LUDUS/Iniciar sessão fictícia")]
    public void StartFictionalSession()
    {
        if (!TryResolveSessionController())
        {
            Debug.LogError(
                "[LUDUS] Não foi possível localizar a base LUDUS SDK."
            );
            return;
        }

        bool started = LudusSdk.TryStartSession(
            studentId,
            playerId,
            out string errorMessage
        );

        if (!started)
        {
            Debug.LogError("[LUDUS] " + errorMessage);
            return;
        }

        Debug.Log(
            "[LUDUS] Sessão fictícia iniciada. A cena ativa será acompanhada automaticamente."
        );
    }

    [ContextMenu("LUDUS/Encerrar sessão e exibir JSON")]
    public void EndSessionAndLogJson()
    {
        if (!TryResolveSessionController())
        {
            Debug.LogError(
                "[LUDUS] Não foi possível localizar a base LUDUS SDK."
            );
            return;
        }

        bool ended = LudusSdk.TryEndSession(
            out string json,
            out string errorMessage
        );

        Debug.Log(ended
            ? "[LUDUS] JSON fictício: " + json
            : "[LUDUS] " + errorMessage);
    }

    private bool TryResolveSessionController()
    {
        if (sessionController != null)
        {
            return true;
        }

        sessionController =
            FindFirstObjectByType<LudusSessionController>();

        return sessionController != null;
    }
}
