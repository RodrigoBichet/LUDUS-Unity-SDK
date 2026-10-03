using System;
using System.Collections.Generic;
using LudusSDK;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace LudusSDK.Editor
{
    public sealed class LudusTagDropCandidate
    {
        public GameObject GameObject { get; }
        public string HierarchyPath { get; }
        public string ExpectedTag { get; }
        public GameObject ActivityRoot { get; }
        public bool IsCompatible { get; }
        public bool AlreadyConfigured { get; }
        public bool Selected { get; set; }

        internal LudusTagDropCandidate(GameObject gameObject)
        {
            GameObject = gameObject;
            HierarchyPath = BuildHierarchyPath(gameObject.transform);
            ExpectedTag = gameObject.tag;
            Canvas canvas = gameObject.GetComponentInParent<Canvas>(true);
            ActivityRoot = canvas != null
                ? canvas.gameObject
                : gameObject.transform.parent != null
                    ? gameObject.transform.parent.gameObject
                    : gameObject;
            IsCompatible = !string.Equals(
                ExpectedTag,
                "Untagged",
                StringComparison.Ordinal
            );
            AlreadyConfigured =
                gameObject.GetComponent<LudusTagMatchDropAdapter>() != null;
            Selected = IsCompatible;
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            List<string> names = new List<string>();
            Transform current = transform;

            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join(" / ", names);
        }
    }

    public readonly struct LudusSemanticSetupResult
    {
        public int ConfiguredTargets { get; }
        public int ReusedTargets { get; }
        public int ConfiguredObservationAreas { get; }
        public bool CreatedSessionScope { get; }

        public LudusSemanticSetupResult(
            int configuredTargets,
            int reusedTargets,
            int configuredObservationAreas,
            bool createdSessionScope = false
        )
        {
            ConfiguredTargets = configuredTargets;
            ReusedTargets = reusedTargets;
            ConfiguredObservationAreas = configuredObservationAreas;
            CreatedSessionScope = createdSessionScope;
        }
    }

    /// <summary>
    /// Descobre padrões estruturais explícitos da cena e configura somente
    /// os destinos confirmados pelo integrador. Não interpreta regras do jogo.
    /// </summary>
    public static class LudusSemanticSetupAssistant
    {
        public static List<LudusTagDropCandidate> FindTagDropCandidates(
            Scene scene
        )
        {
            List<LudusTagDropCandidate> candidates =
                new List<LudusTagDropCandidate>();
            HashSet<int> visitedObjects = new HashSet<int>();

            if (!scene.IsValid() || !scene.isLoaded)
            {
                return candidates;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                MonoBehaviour[] behaviours =
                    root.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (
                        behaviour == null ||
                        behaviour is LudusTagMatchDropAdapter ||
                        !(behaviour is IDropHandler)
                    )
                    {
                        continue;
                    }

                    GameObject gameObject = behaviour.gameObject;
                    if (!visitedObjects.Add(gameObject.GetInstanceID()))
                    {
                        continue;
                    }

                    candidates.Add(new LudusTagDropCandidate(gameObject));
                }
            }

            candidates.Sort(
                (left, right) => string.Compare(
                    left.HierarchyPath,
                    right.HierarchyPath,
                    StringComparison.Ordinal
                )
            );
            return candidates;
        }

        public static LudusSessionScope FindSingleSessionScope(Scene scene)
        {
            List<LudusSessionScope> scopes = FindSessionScopes(scene);
            return scopes.Count == 1 ? scopes[0] : null;
        }

        public static List<LudusSessionScope> FindSessionScopes(Scene scene)
        {
            List<LudusSessionScope> foundScopes =
                new List<LudusSessionScope>();

            if (!scene.IsValid() || !scene.isLoaded)
            {
                return foundScopes;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                LudusSessionScope[] scopes =
                    root.GetComponentsInChildren<LudusSessionScope>(true);

                foreach (LudusSessionScope scope in scopes)
                {
                    foundScopes.Add(scope);
                }
            }

            return foundScopes;
        }

        public static List<string> FindEnabledBuildScenePaths(
            IEnumerable<EditorBuildSettingsScene> buildScenes
        )
        {
            List<string> paths = new List<string>();

            if (buildScenes == null)
            {
                return paths;
            }

            foreach (EditorBuildSettingsScene buildScene in buildScenes)
            {
                if (
                    buildScene == null ||
                    !buildScene.enabled ||
                    string.IsNullOrWhiteSpace(buildScene.path)
                )
                {
                    continue;
                }

                paths.Add(buildScene.path);
            }

            return paths;
        }

        public static int FindBuildSceneIndex(
            IReadOnlyList<string> buildScenePaths,
            string scenePath
        )
        {
            if (
                buildScenePaths == null ||
                string.IsNullOrWhiteSpace(scenePath)
            )
            {
                return -1;
            }

            for (int index = 0; index < buildScenePaths.Count; index++)
            {
                if (
                    string.Equals(
                        buildScenePaths[index],
                        scenePath,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return index;
                }
            }

            return -1;
        }

        public static string ResolveSuggestedActivityName(
            Scene scene,
            LudusSessionScope sessionScope
        )
        {
            if (
                sessionScope != null &&
                !string.IsNullOrWhiteSpace(sessionScope.SessionDisplayName)
            )
            {
                return sessionScope.SessionDisplayName.Trim();
            }

            return string.IsNullOrWhiteSpace(scene.name)
                ? "Atividade acompanhada"
                : scene.name;
        }

        public static bool TryApplyTagDropSetup(
            Scene scene,
            LudusSessionScope sessionScope,
            LudusSdkConfig config,
            IReadOnlyList<LudusTagDropCandidate> candidates,
            out LudusSemanticSetupResult result,
            out string errorMessage
        )
        {
            string activityDisplayName =
                sessionScope != null &&
                !string.IsNullOrWhiteSpace(sessionScope.SessionDisplayName)
                    ? sessionScope.SessionDisplayName
                    : scene.name;

            return TryApplyTagDropSetup(
                scene,
                sessionScope,
                config,
                candidates,
                activityDisplayName,
                out result,
                out errorMessage
            );
        }

        public static bool TryApplyTagDropSetup(
            Scene scene,
            LudusSessionScope sessionScope,
            LudusSdkConfig config,
            IReadOnlyList<LudusTagDropCandidate> candidates,
            string activityDisplayName,
            out LudusSemanticSetupResult result,
            out string errorMessage
        )
        {
            result = default;

            if (!scene.IsValid() || !scene.isLoaded)
            {
                errorMessage = "Abra uma cena válida antes de configurar os resultados.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(activityDisplayName))
            {
                errorMessage =
                    "Informe o nome da atividade ou categoria acompanhada.";
                return false;
            }

            if (config == null)
            {
                errorMessage = "Selecione o asset de configuração LUDUS.";
                return false;
            }

            List<LudusSessionScope> sceneScopes = FindSessionScopes(scene);
            if (sceneScopes.Count > 1)
            {
                errorMessage =
                    "A cena possui mais de uma Sessão acompanhada LUDUS. Mantenha somente o escopo da atividade antes de aplicar.";
                return false;
            }

            if (
                sessionScope != null &&
                sessionScope.gameObject.scene != scene
            )
            {
                errorMessage =
                    "A Sessão acompanhada selecionada não pertence à cena aberta.";
                return false;
            }

            if (sessionScope == null && sceneScopes.Count == 1)
            {
                sessionScope = sceneScopes[0];
            }

            List<LudusTagDropCandidate> selected =
                new List<LudusTagDropCandidate>();

            if (candidates != null)
            {
                foreach (LudusTagDropCandidate candidate in candidates)
                {
                    if (
                        candidate == null ||
                        !candidate.Selected ||
                        !candidate.IsCompatible ||
                        candidate.GameObject == null ||
                        candidate.GameObject.scene != scene
                    )
                    {
                        continue;
                    }

                    selected.Add(candidate);
                }
            }

            if (selected.Count == 0)
            {
                errorMessage =
                    "Selecione ao menos um destino de drop com uma tag explícita.";
                return false;
            }

            Undo.SetCurrentGroupName("Configurar resultados LUDUS da cena");
            int undoGroup = Undo.GetCurrentGroup();
            bool createdSessionScope = sessionScope == null;

            if (createdSessionScope)
            {
                string normalizedName = activityDisplayName.Trim();
                GameObject scopeObject = new GameObject(
                    "LUDUS — " + normalizedName
                );
                Undo.RegisterCreatedObjectUndo(
                    scopeObject,
                    "Criar sessão acompanhada LUDUS"
                );

                if (scopeObject.scene != scene)
                {
                    SceneManager.MoveGameObjectToScene(scopeObject, scene);
                }

                sessionScope = Undo.AddComponent<LudusSessionScope>(
                    scopeObject
                );
                sessionScope.Configure(normalizedName, true, true);
            }

            LudusSemanticBridge bridge =
                sessionScope.GetComponent<LudusSemanticBridge>();
            if (bridge == null)
            {
                bridge = Undo.AddComponent<LudusSemanticBridge>(
                    sessionScope.gameObject
                );
            }

            Undo.RecordObject(bridge, "Configurar ponte semântica LUDUS");
            bridge.Configure(
                ResolveCategoryName(sessionScope, scene),
                scene.name,
                "atividade",
                "resultado",
                "objetivo",
                Array.Empty<string>()
            );
            ConfigureScopeEvents(sessionScope, bridge);
            EnableRequiredCapabilities(config);

            int configuredTargets = 0;
            int reusedTargets = 0;
            int configuredObservationAreas = 0;
            HashSet<int> configuredAreaIds = new HashSet<int>();

            foreach (LudusTagDropCandidate candidate in selected)
            {
                LudusTagMatchDropAdapter adapter =
                    candidate.GameObject.GetComponent<
                        LudusTagMatchDropAdapter
                    >();

                if (adapter == null)
                {
                    adapter = Undo.AddComponent<LudusTagMatchDropAdapter>(
                        candidate.GameObject
                    );
                }
                else
                {
                    reusedTargets++;
                    Undo.RecordObject(
                        adapter,
                        "Atualizar resultado de arraste LUDUS"
                    );
                }

                adapter.Configure(
                    candidate.ExpectedTag,
                    candidate.ActivityRoot != null
                        ? candidate.ActivityRoot.name
                        : candidate.HierarchyPath,
                    string.Empty,
                    true,
                    true,
                    bridge
                );
                EditorUtility.SetDirty(adapter);
                configuredTargets++;

                if (
                    config.capabilities != null &&
                    config.capabilities.screenshots &&
                    candidate.ActivityRoot != null &&
                    configuredAreaIds.Add(
                        candidate.ActivityRoot.GetInstanceID()
                    ) &&
                    LudusSetupAssistant.ConfigureObservationArea(
                        candidate.ActivityRoot
                    ) != null
                )
                {
                    configuredObservationAreas++;
                }
            }

            EditorUtility.SetDirty(bridge);
            EditorUtility.SetDirty(sessionScope);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);

            result = new LudusSemanticSetupResult(
                configuredTargets,
                reusedTargets,
                configuredObservationAreas,
                createdSessionScope
            );
            errorMessage = string.Empty;
            return true;
        }

        private static void ConfigureScopeEvents(
            LudusSessionScope scope,
            LudusSemanticBridge bridge
        )
        {
            Undo.RecordObject(scope, "Conectar eventos semânticos LUDUS");
            AddPersistentListenerOnce(
                scope.OnSessionStarted,
                bridge,
                nameof(LudusSemanticBridge.RegistrarCategoriaSelecionada),
                bridge.RegistrarCategoriaSelecionada
            );
            AddPersistentListenerOnce(
                scope.OnSessionStarted,
                bridge,
                nameof(LudusSemanticBridge.RegistrarInicioDeFase),
                bridge.RegistrarInicioDeFase
            );
            AddPersistentListenerOnce(
                scope.OnBeforeSessionEnded,
                bridge,
                nameof(LudusSemanticBridge.RegistrarConclusaoDeFase),
                bridge.RegistrarConclusaoDeFase
            );
        }

        private static void AddPersistentListenerOnce(
            UnityEngine.Events.UnityEvent unityEvent,
            UnityEngine.Object target,
            string methodName,
            UnityEngine.Events.UnityAction action
        )
        {
            for (
                int index = 0;
                index < unityEvent.GetPersistentEventCount();
                index++
            )
            {
                if (
                    unityEvent.GetPersistentTarget(index) == target &&
                    unityEvent.GetPersistentMethodName(index) == methodName
                )
                {
                    return;
                }
            }

            UnityEventTools.AddPersistentListener(unityEvent, action);
        }

        private static void EnableRequiredCapabilities(
            LudusSdkConfig config
        )
        {
            Undo.RecordObject(config, "Habilitar resultados semânticos LUDUS");
            config.capabilities ??= new LudusCapabilities();
            config.capabilities.categoryEvents = true;
            config.capabilities.phaseEvents = true;
            config.capabilities.correctWrong = true;
            config.capabilities.customEvents = true;
            EditorUtility.SetDirty(config);
        }

        private static string ResolveCategoryName(
            LudusSessionScope scope,
            Scene scene
        )
        {
            return string.IsNullOrWhiteSpace(scope.SessionDisplayName)
                ? scene.name
                : scope.SessionDisplayName.Trim();
        }

        private static string[] ToArray(HashSet<string> values)
        {
            string[] result = new string[values.Count];
            values.CopyTo(result);
            Array.Sort(result, StringComparer.Ordinal);
            return result;
        }
    }

    public sealed class LudusSemanticSetupAssistantWindow : EditorWindow
    {
        private readonly List<LudusTagDropCandidate> candidates =
            new List<LudusTagDropCandidate>();
        private readonly List<string> buildScenePaths =
            new List<string>();
        private Vector2 scrollPosition;
        private LudusSessionScope sessionScope;
        private int sessionScopeCount;
        private LudusSdkConfig config;
        private string activityDisplayName = string.Empty;
        private string observedSceneIdentity = string.Empty;
        private string statusMessage = string.Empty;
        private MessageType statusType = MessageType.None;

        [MenuItem("LUDUS/Configurar resultados da cena", false, 30)]
        private static void OpenWindow()
        {
            LudusSemanticSetupAssistantWindow window =
                GetWindow<LudusSemanticSetupAssistantWindow>(
                    "Resultados LUDUS"
                );
            window.minSize = new Vector2(620f, 420f);
            window.RefreshCandidates();
        }

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += HandleHierarchyChanged;
            RefreshCandidates();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= HandleHierarchyChanged;
        }

        private void HandleHierarchyChanged()
        {
            RefreshCandidates();
            Repaint();
        }

        private void RefreshCandidates()
        {
            Scene scene = SceneManager.GetActiveScene();
            string sceneIdentity = string.IsNullOrWhiteSpace(scene.path)
                ? scene.name + "#" + scene.handle
                : scene.path;
            bool sceneChanged = !string.Equals(
                observedSceneIdentity,
                sceneIdentity,
                StringComparison.Ordinal
            );
            List<LudusSessionScope> scopes =
                LudusSemanticSetupAssistant.FindSessionScopes(scene);
            sessionScopeCount = scopes.Count;
            sessionScope = scopes.Count == 1 ? scopes[0] : null;

            if (
                sceneChanged ||
                string.IsNullOrWhiteSpace(activityDisplayName)
            )
            {
                activityDisplayName =
                    LudusSemanticSetupAssistant.ResolveSuggestedActivityName(
                        scene,
                        sessionScope
                    );
            }
            observedSceneIdentity = sceneIdentity;

            buildScenePaths.Clear();
            buildScenePaths.AddRange(
                LudusSemanticSetupAssistant.FindEnabledBuildScenePaths(
                    EditorBuildSettings.scenes
                )
            );

            candidates.Clear();
            candidates.AddRange(
                LudusSemanticSetupAssistant.FindTagDropCandidates(scene)
            );

            if (config == null)
            {
                config = FindSingleConfigAsset();
            }
        }

        private void OnGUI()
        {
            Scene scene = SceneManager.GetActiveScene();

            EditorGUILayout.LabelField(
                "Configurar resultados da cena",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "O assistente localiza objetos que já recebem drop pelo EventSystem e possuem uma tag explícita. A tag continua decidindo acerto ou erro, mas o Dashboard recebe o texto ou a imagem visível em tempo de execução. Com capturas visuais habilitadas, o Canvas de cada atividade também é preparado como recorte. O SDK não altera a movimentação nem inventa a regra pedagógica.",
                MessageType.Info
            );

            EditorGUILayout.LabelField("Cena aberta", scene.name);
            DrawBuildSceneNavigation(scene);
            activityDisplayName = EditorGUILayout.TextField(
                "Nome da atividade/categoria",
                activityDisplayName
            );
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Sessão acompanhada",
                    sessionScope,
                    typeof(LudusSessionScope),
                    true
                );
            }
            config = (LudusSdkConfig)EditorGUILayout.ObjectField(
                "Configuração LUDUS",
                config,
                typeof(LudusSdkConfig),
                false
            );

            if (sessionScopeCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "Esta cena ainda não possui uma Sessão acompanhada. O assistente criará o objeto automaticamente ao aplicar.",
                    MessageType.Info
                );
            }
            else if (sessionScopeCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "A cena possui mais de uma Sessão acompanhada. Mantenha somente o escopo da atividade antes de continuar.",
                    MessageType.Error
                );
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Destinos encontrados (" + candidates.Count + ")",
                EditorStyles.boldLabel
            );
            if (GUILayout.Button("Verificar novamente", GUILayout.Width(140f)))
            {
                RefreshCandidates();
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (LudusTagDropCandidate candidate in candidates)
            {
                DrawCandidate(candidate);
            }
            EditorGUILayout.EndScrollView();

            if (candidates.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhum objeto que recebe drop pelo EventSystem foi encontrado nesta cena.",
                    MessageType.Warning
                );
            }

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, statusType);
            }

            bool canApply =
                !Application.isPlaying &&
                sessionScopeCount <= 1 &&
                config != null &&
                !string.IsNullOrWhiteSpace(activityDisplayName) &&
                HasSelectedCompatibleCandidate();
            int currentBuildSceneIndex =
                LudusSemanticSetupAssistant.FindBuildSceneIndex(
                    buildScenePaths,
                    scene.path
                );
            bool hasNextBuildScene =
                currentBuildSceneIndex >= 0 &&
                currentBuildSceneIndex + 1 < buildScenePaths.Count;

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!canApply))
            {
                if (GUILayout.Button("Aplicar configuração confirmada", GUILayout.Height(34f)))
                {
                    ApplySetup(scene);
                }
            }

            using (
                new EditorGUI.DisabledScope(!canApply || !hasNextBuildScene)
            )
            {
                if (
                    GUILayout.Button(
                        "Aplicar e abrir próxima →",
                        GUILayout.Height(34f)
                    ) &&
                    ApplySetup(scene)
                )
                {
                    OpenBuildScene(currentBuildSceneIndex + 1);
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Após aplicar, salve a cena e execute LUDUS > Validar integração semântica. O botão Aplicar e abrir próxima usa o diálogo normal do Unity antes de trocar de cena.",
                MessageType.None
            );
        }

        private void DrawBuildSceneNavigation(Scene scene)
        {
            int currentIndex =
                LudusSemanticSetupAssistant.FindBuildSceneIndex(
                    buildScenePaths,
                    scene.path
                );
            string positionLabel = currentIndex >= 0
                ? "Cena " + (currentIndex + 1) + " de " + buildScenePaths.Count
                : "Cena aberta fora do Build Profile";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Fluxo guiado",
                positionLabel,
                EditorStyles.miniLabel
            );

            using (
                new EditorGUI.DisabledScope(
                    Application.isPlaying || currentIndex <= 0
                )
            )
            {
                if (GUILayout.Button("← Anterior", GUILayout.Width(95f)))
                {
                    OpenBuildScene(currentIndex - 1);
                    GUIUtility.ExitGUI();
                }
            }

            int nextIndex = currentIndex < 0 ? 0 : currentIndex + 1;
            using (
                new EditorGUI.DisabledScope(
                    Application.isPlaying ||
                    nextIndex < 0 ||
                    nextIndex >= buildScenePaths.Count
                )
            )
            {
                if (GUILayout.Button("Próxima →", GUILayout.Width(95f)))
                {
                    OpenBuildScene(nextIndex);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (buildScenePaths.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhuma cena habilitada foi encontrada no Build Profile. Ainda é possível configurar a cena aberta normalmente.",
                    MessageType.Warning
                );
            }
        }

        private void OpenBuildScene(int index)
        {
            if (index < 0 || index >= buildScenePaths.Count)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            try
            {
                EditorSceneManager.OpenScene(
                    buildScenePaths[index],
                    OpenSceneMode.Single
                );
                statusMessage = string.Empty;
                statusType = MessageType.None;
                RefreshCandidates();
                Repaint();
            }
            catch (Exception exception)
            {
                statusMessage =
                    "Não foi possível abrir a cena selecionada: " +
                    exception.Message;
                statusType = MessageType.Error;
            }
        }

        private static void DrawCandidate(LudusTagDropCandidate candidate)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(!candidate.IsCompatible))
            {
                candidate.Selected = EditorGUILayout.ToggleLeft(
                    candidate.HierarchyPath,
                    candidate.Selected
                );
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                candidate.IsCompatible
                    ? "Tag correta: " + candidate.ExpectedTag
                    : "Ignorado: a tag ainda é Untagged",
                candidate.IsCompatible
                    ? EditorStyles.miniLabel
                    : EditorStyles.helpBox
            );

            if (candidate.AlreadyConfigured)
            {
                GUILayout.Label("Já configurado", EditorStyles.miniBoldLabel);
            }

            if (GUILayout.Button("Selecionar", GUILayout.Width(78f)))
            {
                Selection.activeGameObject = candidate.GameObject;
                EditorGUIUtility.PingObject(candidate.GameObject);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private bool HasSelectedCompatibleCandidate()
        {
            foreach (LudusTagDropCandidate candidate in candidates)
            {
                if (candidate.Selected && candidate.IsCompatible)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ApplySetup(Scene scene)
        {
            bool applied =
                LudusSemanticSetupAssistant.TryApplyTagDropSetup(
                    scene,
                    sessionScope,
                    config,
                    candidates,
                    activityDisplayName,
                    out LudusSemanticSetupResult result,
                    out string errorMessage
                );

            if (!applied)
            {
                statusMessage = errorMessage;
                statusType = MessageType.Error;
                return false;
            }

            string scopeStatus = result.CreatedSessionScope
                ? "Sessão acompanhada criada automaticamente; "
                : string.Empty;
            statusMessage =
                scopeStatus +
                result.ConfiguredTargets +
                " destino(s) configurado(s); " +
                result.ReusedTargets +
                " configuração(ões) existente(s) atualizada(s); " +
                result.ConfiguredObservationAreas +
                " área(s) de observação preparada(s). Salve a cena para preservar as alterações.";
            statusType = MessageType.Info;
            RefreshCandidates();
            return true;
        }

        private static LudusSdkConfig FindSingleConfigAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:LudusSdkConfig");
            if (guids.Length != 1)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<LudusSdkConfig>(path);
        }
    }
}
