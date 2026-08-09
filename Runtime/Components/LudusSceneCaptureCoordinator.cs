using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudusSDK
{
    [DisallowMultipleComponent]
    public sealed class LudusSceneCaptureCoordinator : MonoBehaviour
    {
        [Tooltip("Opcional: a base LUDUS SDK é localizada automaticamente.")]
        public LudusSessionController sessionController;

        private LudusCaptureContext ownedContext;
        private string ownedSceneName;

        private void Awake()
        {
            TryResolveSessionController();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void Update()
        {
            RefreshCurrentSceneCapture();
        }

        private void HandleActiveSceneChanged(Scene previous, Scene next)
        {
            RefreshCaptureFor(next);
        }

        public void RefreshCurrentSceneCapture()
        {
            RefreshCaptureFor(SceneManager.GetActiveScene());
        }

        private void RefreshCaptureFor(Scene scene)
        {
            if (!TryResolveSessionController())
            {
                return;
            }

            if (!sessionController.HasActiveSession)
            {
                ownedContext = null;
                ownedSceneName = string.Empty;
                return;
            }

            LudusSdkConfig config = sessionController.Config;
            string sceneName = string.IsNullOrWhiteSpace(scene.name)
                ? "Cena ativa"
                : scene.name;
            bool shouldCapture =
                scene.IsValid() &&
                config != null &&
                config.ShouldCaptureScene(sceneName);

            if (!shouldCapture)
            {
                TryEndOwnedContext();
                return;
            }

            if (
                ownedContext != null &&
                sessionController.HasActiveCaptureContext &&
                string.Equals(ownedSceneName, sceneName, System.StringComparison.Ordinal)
            )
            {
                return;
            }

            TryEndOwnedContext();

            if (sessionController.HasActiveCaptureContext)
            {
                return;
            }

            LudusCaptureContext sceneContext = new LudusCaptureContext(
                sceneName,
                LudusCaptureContextKind.Scene.ToString(),
                string.Empty
            );

            if (sessionController.TryBeginCaptureContext(
                sceneContext,
                out _
            ))
            {
                ownedContext = sceneContext;
                ownedSceneName = sceneName;
            }
        }

        private void TryEndOwnedContext()
        {
            if (ownedContext == null || sessionController == null)
            {
                return;
            }

            sessionController.TryEndCaptureContext(ownedContext, out _);
            ownedContext = null;
            ownedSceneName = string.Empty;
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
}
