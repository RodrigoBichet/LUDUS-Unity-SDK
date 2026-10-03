using System.Collections.Generic;
using LudusSDK;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudusSDK.Editor
{
    public enum LudusSemanticValidationSeverity
    {
        Info,
        Warning,
        Error,
    }

    public enum LudusSemanticValidationCode
    {
        SceneUnavailable,
        MissingSessionController,
        MultipleSessionControllers,
        MissingConfiguration,
        MissingCapabilities,
        NoSemanticIntegration,
        ManualBridgeReview,
        CorrectWrongDisabled,
        PhaseEventsDisabled,
        CustomEventsDisabled,
    }

    public sealed class LudusSemanticValidationIssue
    {
        public LudusSemanticValidationIssue(
            LudusSemanticValidationSeverity severity,
            LudusSemanticValidationCode code,
            string message,
            Object context = null
        )
        {
            Severity = severity;
            Code = code;
            Message = message;
            Context = context;
        }

        public LudusSemanticValidationSeverity Severity { get; }
        public LudusSemanticValidationCode Code { get; }
        public string Message { get; }
        public Object Context { get; }
    }

    /// <summary>
    /// Verifica somente a ligação estrutural entre a cena, a configuração e
    /// os adaptadores LUDUS. A regra pedagógica continua pertencendo ao jogo.
    /// </summary>
    public static class LudusSemanticIntegrationValidator
    {
        public static List<LudusSemanticValidationIssue> ValidateScene(
            Scene scene
        )
        {
            List<LudusSemanticValidationIssue> issues =
                new List<LudusSemanticValidationIssue>();

            if (!scene.IsValid() || !scene.isLoaded)
            {
                issues.Add(new LudusSemanticValidationIssue(
                    LudusSemanticValidationSeverity.Error,
                    LudusSemanticValidationCode.SceneUnavailable,
                    "Abra uma cena válida antes de verificar a integração semântica."
                ));
                return issues;
            }

            List<LudusSessionController> controllers =
                FindComponents<LudusSessionController>(scene);
            List<LudusSemanticBridge> bridges =
                FindComponents<LudusSemanticBridge>(scene);
            List<LudusButtonOutcomeAdapter> buttonAdapters =
                FindComponents<LudusButtonOutcomeAdapter>(scene);
            List<LudusPhysicsTagOutcomeAdapter> physicsAdapters =
                FindComponents<LudusPhysicsTagOutcomeAdapter>(scene);
            List<LudusTagMatchDropAdapter> dropAdapters =
                FindComponents<LudusTagMatchDropAdapter>(scene);
            List<LudusProgressThresholdAdapter> thresholdAdapters =
                FindComponents<LudusProgressThresholdAdapter>(scene);

            int adapterCount =
                buttonAdapters.Count +
                physicsAdapters.Count +
                dropAdapters.Count +
                thresholdAdapters.Count;

            if (bridges.Count == 0 && adapterCount == 0)
            {
                issues.Add(new LudusSemanticValidationIssue(
                    LudusSemanticValidationSeverity.Info,
                    LudusSemanticValidationCode.NoSemanticIntegration,
                    "Esta cena ainda não possui ponte nem adaptadores semânticos LUDUS. A coleta observacional pode continuar funcionando normalmente."
                ));
            }
            else if (adapterCount == 0)
            {
                issues.Add(new LudusSemanticValidationIssue(
                    LudusSemanticValidationSeverity.Info,
                    LudusSemanticValidationCode.ManualBridgeReview,
                    "Há uma Ponte semântica do jogo sem adaptadores verificáveis. Revise manualmente os UnityEvents ou as chamadas públicas conectadas a ela.",
                    bridges[0]
                ));
            }

            if (controllers.Count == 0)
            {
                bool canUsePersistentBase =
                    IsAfterInitialBuildScene(scene);
                issues.Add(new LudusSemanticValidationIssue(
                    canUsePersistentBase
                        ? LudusSemanticValidationSeverity.Warning
                        : LudusSemanticValidationSeverity.Error,
                    LudusSemanticValidationCode.MissingSessionController,
                    canUsePersistentBase
                        ? "Esta cena não contém uma base LUDUS própria. Isso é válido quando o jogo sempre começa pela primeira cena do Build Profile e mantém a base persistente; valide também a cena inicial."
                        : "A cena inicial não contém uma base LUDUS com LudusSessionController."
                ));
                return issues;
            }

            if (controllers.Count > 1)
            {
                issues.Add(new LudusSemanticValidationIssue(
                    LudusSemanticValidationSeverity.Error,
                    LudusSemanticValidationCode.MultipleSessionControllers,
                    "A cena contém mais de um LudusSessionController. Mantenha apenas a base responsável pela sessão.",
                    controllers[0]
                ));
            }

            LudusSessionController controller = controllers[0];
            LudusSdkConfig config = controller.Config;

            if (config == null)
            {
                issues.Add(new LudusSemanticValidationIssue(
                    LudusSemanticValidationSeverity.Error,
                    LudusSemanticValidationCode.MissingConfiguration,
                    "A base LUDUS não possui um asset de configuração atribuído.",
                    controller
                ));
                return issues;
            }

            LudusCapabilities capabilities = config.capabilities;
            if (capabilities == null)
            {
                issues.Add(new LudusSemanticValidationIssue(
                    LudusSemanticValidationSeverity.Error,
                    LudusSemanticValidationCode.MissingCapabilities,
                    "O asset de configuração não possui a seção de capacidades LUDUS.",
                    config
                ));
                return issues;
            }

            ValidateCorrectWrongAdapters(
                buttonAdapters,
                physicsAdapters,
                capabilities,
                issues
            );
            ValidateDropAdapters(dropAdapters, capabilities, issues);
            ValidateThresholdAdapters(
                thresholdAdapters,
                capabilities,
                issues
            );

            return issues;
        }

        private static bool IsAfterInitialBuildScene(Scene scene)
        {
            string scenePath = scene.path;

            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return false;
            }

            foreach (EditorBuildSettingsScene buildScene in
                EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                {
                    continue;
                }

                return !string.Equals(
                    buildScene.path,
                    scenePath,
                    System.StringComparison.Ordinal
                );
            }

            return false;
        }

        private static void ValidateCorrectWrongAdapters(
            List<LudusButtonOutcomeAdapter> buttonAdapters,
            List<LudusPhysicsTagOutcomeAdapter> physicsAdapters,
            LudusCapabilities capabilities,
            List<LudusSemanticValidationIssue> issues
        )
        {
            if (capabilities.correctWrong)
            {
                return;
            }

            foreach (LudusButtonOutcomeAdapter adapter in buttonAdapters)
            {
                AddCapabilityIssue(
                    issues,
                    LudusSemanticValidationCode.CorrectWrongDisabled,
                    "O adaptador de botão registra acerto ou erro, mas a capacidade Acertos e erros está desativada.",
                    adapter
                );
            }

            foreach (LudusPhysicsTagOutcomeAdapter adapter in physicsAdapters)
            {
                AddCapabilityIssue(
                    issues,
                    LudusSemanticValidationCode.CorrectWrongDisabled,
                    "O adaptador de contato por tag registra acerto ou erro, mas a capacidade Acertos e erros está desativada.",
                    adapter
                );
            }
        }

        private static void ValidateDropAdapters(
            List<LudusTagMatchDropAdapter> adapters,
            LudusCapabilities capabilities,
            List<LudusSemanticValidationIssue> issues
        )
        {
            foreach (LudusTagMatchDropAdapter adapter in adapters)
            {
                if (
                    adapter.RecordOutcomeEvent &&
                    !capabilities.correctWrong
                )
                {
                    AddCapabilityIssue(
                        issues,
                        LudusSemanticValidationCode.CorrectWrongDisabled,
                        "O resultado do arraste por tag está habilitado, mas a capacidade Acertos e erros está desativada.",
                        adapter
                    );
                }

                if (
                    adapter.RecordAttemptEvent &&
                    !capabilities.customEvents
                )
                {
                    AddCapabilityIssue(
                        issues,
                        LudusSemanticValidationCode.CustomEventsDisabled,
                        "A tentativa de arraste está habilitada, mas a capacidade Eventos personalizados está desativada.",
                        adapter
                    );
                }
            }
        }

        private static void ValidateThresholdAdapters(
            List<LudusProgressThresholdAdapter> adapters,
            LudusCapabilities capabilities,
            List<LudusSemanticValidationIssue> issues
        )
        {
            foreach (LudusProgressThresholdAdapter adapter in adapters)
            {
                bool recordsCorrect =
                    adapter.Action == LudusThresholdAction.RecordCorrect ||
                    adapter.Action ==
                        LudusThresholdAction.RecordCorrectAndCompletePhase;
                bool completesPhase =
                    adapter.Action == LudusThresholdAction.CompletePhase ||
                    adapter.Action ==
                        LudusThresholdAction.RecordCorrectAndCompletePhase;

                if (recordsCorrect && !capabilities.correctWrong)
                {
                    AddCapabilityIssue(
                        issues,
                        LudusSemanticValidationCode.CorrectWrongDisabled,
                        "A meta registra um acerto, mas a capacidade Acertos e erros está desativada.",
                        adapter
                    );
                }

                if (completesPhase && !capabilities.phaseEvents)
                {
                    AddCapabilityIssue(
                        issues,
                        LudusSemanticValidationCode.PhaseEventsDisabled,
                        "A meta conclui uma fase, mas a capacidade Eventos de fase está desativada.",
                        adapter
                    );
                }
            }
        }

        private static void AddCapabilityIssue(
            List<LudusSemanticValidationIssue> issues,
            LudusSemanticValidationCode code,
            string message,
            Object context
        )
        {
            issues.Add(new LudusSemanticValidationIssue(
                LudusSemanticValidationSeverity.Error,
                code,
                message,
                context
            ));
        }

        private static List<T> FindComponents<T>(Scene scene)
            where T : Component
        {
            List<T> components = new List<T>();

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                components.AddRange(
                    rootObject.GetComponentsInChildren<T>(true)
                );
            }

            return components;
        }
    }

    public sealed class LudusSemanticIntegrationValidatorWindow :
        EditorWindow
    {
        private Vector2 scrollPosition;
        private List<LudusSemanticValidationIssue> issues =
            new List<LudusSemanticValidationIssue>();

        [MenuItem("LUDUS/Validar integração semântica", false, 31)]
        private static void OpenWindow()
        {
            LudusSemanticIntegrationValidatorWindow window =
                GetWindow<LudusSemanticIntegrationValidatorWindow>(
                    "Validação semântica"
                );
            window.minSize = new Vector2(520f, 300f);
            window.RefreshValidation();
        }

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += HandleHierarchyChanged;
            Undo.undoRedoPerformed += HandleHierarchyChanged;
            RefreshValidation();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= HandleHierarchyChanged;
            Undo.undoRedoPerformed -= HandleHierarchyChanged;
        }

        private void HandleHierarchyChanged()
        {
            RefreshValidation();
            Repaint();
        }

        private void RefreshValidation()
        {
            issues = LudusSemanticIntegrationValidator.ValidateScene(
                SceneManager.GetActiveScene()
            );
        }

        private void OnGUI()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            EditorGUILayout.LabelField(
                "Validação da integração semântica",
                EditorStyles.boldLabel
            );
            EditorGUILayout.LabelField(
                "Cena aberta",
                activeScene.IsValid() ? activeScene.name : "Nenhuma"
            );
            EditorGUILayout.HelpBox(
                "Esta verificação confirma a base LUDUS, o asset de configuração e as capacidades exigidas pelos adaptadores. Ela não decide o que é acerto ou erro no seu jogo.",
                MessageType.Info
            );

            if (GUILayout.Button("Verificar novamente"))
            {
                RefreshValidation();
            }

            EditorGUILayout.Space();
            DrawSummary();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (LudusSemanticValidationIssue issue in issues)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.HelpBox(
                    issue.Message,
                    ToMessageType(issue.Severity)
                );

                if (
                    issue.Context != null &&
                    GUILayout.Button("Selecionar item")
                )
                {
                    Selection.activeObject = issue.Context;
                    EditorGUIUtility.PingObject(issue.Context);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSummary()
        {
            int errorCount = 0;
            int warningCount = 0;

            foreach (LudusSemanticValidationIssue issue in issues)
            {
                if (
                    issue.Severity == LudusSemanticValidationSeverity.Error
                )
                {
                    errorCount++;
                }
                else if (
                    issue.Severity ==
                    LudusSemanticValidationSeverity.Warning
                )
                {
                    warningCount++;
                }
            }

            if (errorCount == 0 && warningCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhum problema estrutural foi encontrado nesta cena.",
                    MessageType.Info
                );
                return;
            }

            EditorGUILayout.HelpBox(
                errorCount + " erro(s) e " + warningCount +
                    " aviso(s) encontrados.",
                errorCount > 0 ? MessageType.Error : MessageType.Warning
            );
        }

        private static MessageType ToMessageType(
            LudusSemanticValidationSeverity severity
        )
        {
            switch (severity)
            {
                case LudusSemanticValidationSeverity.Error:
                    return MessageType.Error;
                case LudusSemanticValidationSeverity.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }
    }
}
