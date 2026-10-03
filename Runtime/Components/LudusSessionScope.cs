using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LudusSDK
{
    /// <summary>
    /// Delimita uma sessão LUDUS por ativação de um objeto, painel ou cena.
    /// Não interpreta regras do jogo e encerra somente a sessão que iniciou.
    /// </summary>
    [AddComponentMenu("LUDUS/Fluxo/Sessão acompanhada")]
    [DisallowMultipleComponent]
    public sealed class LudusSessionScope : MonoBehaviour
    {
        [SerializeField]
        private bool startWhenEnabled = true;

        [SerializeField]
        private bool endWhenDisabled = true;

        [SerializeField]
        private string sessionDisplayName = string.Empty;

        [SerializeField]
        private bool showConsoleMessages = true;

        [SerializeField]
        private UnityEvent onSessionStarted = new UnityEvent();

        [SerializeField]
        private UnityEvent onBeforeSessionEnded = new UnityEvent();

        private bool ownsActiveSession;
        private LudusSessionController ownedSessionController;

        public bool OwnsActiveSession => ownsActiveSession;
        public bool StartWhenEnabled => startWhenEnabled;
        public bool EndWhenDisabled => endWhenDisabled;
        public string SessionDisplayName => sessionDisplayName;
        public UnityEvent OnSessionStarted => onSessionStarted;
        public UnityEvent OnBeforeSessionEnded => onBeforeSessionEnded;

        public void Configure(
            string displayName,
            bool startAutomatically = true,
            bool endAutomatically = true
        )
        {
            sessionDisplayName = displayName;
            startWhenEnabled = startAutomatically;
            endWhenDisabled = endAutomatically;
        }

        private void OnEnable()
        {
            if (Application.isPlaying && startWhenEnabled)
            {
                IniciarSessao();
            }
        }

        private void OnDisable()
        {
            if (
                Application.isPlaying &&
                endWhenDisabled &&
                ownsActiveSession
            )
            {
                EncerrarSessao();
            }
        }

        /// <summary>
        /// Método sem retorno para ligação por UnityEvent.
        /// </summary>
        public void IniciarSessao()
        {
            bool started = TryStartOwnedSession(out string errorMessage);
            ReportResult("iniciar a sessão", started, errorMessage);
        }

        public bool TryStartOwnedSession(out string errorMessage)
        {
            if (ownsActiveSession)
            {
                errorMessage =
                    "Este escopo já possui uma sessão ativa.";
                return false;
            }

            string displayName = ResolveSessionDisplayName();
            bool started = LudusSdk.TryStartSessionForManualImport(
                displayName,
                out LudusSessionController controller,
                out errorMessage
            );

            if (!started)
            {
                return false;
            }

            ownsActiveSession = true;
            ownedSessionController = controller;
            InvokeSafely(onSessionStarted, "início da sessão");
            return true;
        }

        /// <summary>
        /// Método sem retorno para ligação por UnityEvent.
        /// </summary>
        public void EncerrarSessao()
        {
            bool ended = TryEndOwnedSession(
                out string json,
                out string errorMessage
            );
            ReportResult("encerrar a sessão", ended, errorMessage);

            if (ended && showConsoleMessages)
            {
                Debug.Log(
                    "[LUDUS] Sessão encerrada pelo escopo '" +
                    gameObject.name +
                    "'. JSON gerado com " +
                    (json == null ? 0 : json.Length) +
                    " caracteres.",
                    this
                );
            }
        }

        public bool TryEndOwnedSession(
            out string json,
            out string errorMessage
        )
        {
            json = string.Empty;

            if (!ownsActiveSession)
            {
                errorMessage =
                    "Este escopo não iniciou a sessão ativa e não pode encerrá-la.";
                return false;
            }

            if (ownedSessionController == null)
            {
                ownsActiveSession = false;
                errorMessage =
                    "A base LUDUS SDK que iniciou esta sessão não está mais disponível.";
                return false;
            }

            InvokeSafely(onBeforeSessionEnded, "fim da sessão");

            bool ended = ownedSessionController.TryEndAndSerialize(
                out json,
                out errorMessage
            );

            if (ended || !ownedSessionController.HasActiveSession)
            {
                ownsActiveSession = false;
                ownedSessionController = null;
            }

            return ended;
        }

        private string ResolveSessionDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(sessionDisplayName))
            {
                return sessionDisplayName.Trim();
            }

            string sceneName = SceneManager.GetActiveScene().name;
            return string.IsNullOrWhiteSpace(sceneName)
                ? gameObject.name
                : sceneName;
        }

        private void ReportResult(
            string action,
            bool succeeded,
            string errorMessage
        )
        {
            if (succeeded || !showConsoleMessages)
            {
                return;
            }

            Debug.LogWarning(
                "[LUDUS] Não foi possível " +
                action +
                " no escopo '" +
                gameObject.name +
                "': " +
                errorMessage,
                this
            );
        }

        private void InvokeSafely(UnityEvent unityEvent, string moment)
        {
            try
            {
                unityEvent?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[LUDUS] Um evento conectado ao " +
                    moment +
                    " lançou uma exceção. A sessão continuará sendo processada.",
                    this
                );
                Debug.LogException(exception, this);
            }
        }

    }
}
