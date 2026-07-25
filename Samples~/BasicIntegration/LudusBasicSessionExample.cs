using LudusSDK;
using UnityEngine;

public sealed class LudusBasicSessionExample : MonoBehaviour
{
    [InspectorName("Objeto controlador LUDUS SDK")]
    [Tooltip("Arraste aqui o GameObject que possui o componente LudusSessionController.")]
    public LudusSessionController sessionController;

    [ContextMenu("LUDUS/Iniciar sessão fictícia")]
    public void StartFictionalSession()
    {
        if (sessionController == null)
        {
            Debug.LogError("[LUDUS] Configure o LudusSessionController.");
            return;
        }

        bool started = sessionController.TryStartSession(
            "000000000000000000000010",
            "Estudante Fictício",
            out string errorMessage
        );

        Debug.Log(started
            ? "[LUDUS] Sessão fictícia iniciada."
            : "[LUDUS] " + errorMessage);
    }

    [ContextMenu("LUDUS/Encerrar sessão e exibir JSON")]
    public void EndSessionAndLogJson()
    {
        if (sessionController == null)
        {
            Debug.LogError("[LUDUS] Configure o LudusSessionController.");
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
}
