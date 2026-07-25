using LudusSDK;
using UnityEngine;

public sealed class LudusBasicSessionExample : MonoBehaviour
{
    [Tooltip("Opcional: o SDK localiza automaticamente a base LUDUS SDK.")]
    public LudusSessionController sessionController;

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

        bool started = sessionController.TryStartSession(
            "000000000000000000000010",
            "Estudante Fictício",
            out string errorMessage
        );

        if (!started)
        {
            Debug.LogError("[LUDUS] " + errorMessage);
            return;
        }

        bool contextStarted = sessionController.TryBeginCaptureContext(
            "Atividade de teste",
            "activity",
            "Validação fictícia da integração do SDK.",
            out string contextError
        );

        Debug.Log(contextStarted
            ? "[LUDUS] Sessão e recorte fictícios iniciados."
            : "[LUDUS] Sessão iniciada, mas o recorte falhou: " + contextError);
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

        bool ended = sessionController.TryEndAndSerialize(
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
