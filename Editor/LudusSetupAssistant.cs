using System;
using System.Collections.Generic;
using LudusSDK;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudusSDK.Editor
{
    public static class LudusSetupAssistant
    {
        private const string TutorialFolderPath = "Assets/LUDUS/Tutorial";
        private const string TutorialScenePath =
            TutorialFolderPath + "/TutorialLudus.unity";

        [MenuItem("LUDUS/Criar tutorial de teste do SDK", false, 10)]
        private static void CreateTutorialScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TutorialScenePath) != null)
            {
                bool openExisting = EditorUtility.DisplayDialog(
                    "Tutorial LUDUS",
                    "A cena tutorial já existe em Assets/LUDUS/Tutorial. Deseja abri-la?",
                    "Abrir tutorial",
                    "Cancelar"
                );

                if (openExisting)
                {
                    EditorSceneManager.OpenScene(TutorialScenePath);
                }

                return;
            }

            EnsureFolder(TutorialFolderPath);
            Scene tutorialScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );
            LudusSdkConfig config = CreateTutorialConfig();
            GameObject root = CreateConfiguredCaptureBase(config);
            root.name = "LUDUS SDK — Tutorial";
            root.AddComponent<LudusTutorialTestPanel>();
            LudusTutorialExerciseBuilder.EnsureTutorialCamera();

            EditorSceneManager.SaveScene(tutorialScene, TutorialScenePath);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            EditorUtility.DisplayDialog(
                "Tutorial LUDUS",
                "A cena de tutorial foi criada em Assets/LUDUS/Tutorial. "
                    + "Ela é isolada e não modifica as cenas do seu jogo. "
                    + "Pressione Play e siga as instruções exibidas na Game View para testar o SDK.",
                "Entendi"
            );
        }

        [MenuItem("LUDUS/Tutorial/Adicionar exercício de interações", false, 20)]
        private static void AddTutorialInteractionExercise()
        {
            LudusTutorialExerciseBuilder.AddToTutorial(TutorialScenePath);
        }

        [MenuItem("GameObject/LUDUS/Adicionar coleta ao meu jogo", false, 10)]
        private static void CreateCaptureBase()
        {
            LudusSdkConfig config = CreateGameConfig();
            GameObject root = CreateConfiguredCaptureBase(config);
            Undo.RegisterCreatedObjectUndo(root, "Criar base de coleta LUDUS");

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        [MenuItem("GameObject/LUDUS/Atualizar coletor de mouse da base selecionada", false, 11)]
        private static void UpdateSelectedPointerTracker()
        {
            GameObject root = Selection.activeGameObject;
            LudusSessionController controller = root == null
                ? null
                : root.GetComponent<LudusSessionController>();

            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "LUDUS",
                    "Selecione o objeto LUDUS SDK que possui o controlador da sessão.",
                    "Entendi"
                );
                return;
            }

            Type inputSystemTrackerType = GetInputSystemTrackerType();
            if (inputSystemTrackerType == null)
            {
                EditorUtility.DisplayDialog(
                    "LUDUS",
                    "O pacote novo Input System não está disponível neste projeto. A base continuará usando o coletor clássico da Unity.",
                    "Entendi"
                );
                return;
            }

            Component inputSystemTracker = root.GetComponent(inputSystemTrackerType);
            if (inputSystemTracker == null)
            {
                inputSystemTracker = Undo.AddComponent(root, inputSystemTrackerType);
            }

            AssignController(inputSystemTracker, controller);

            LudusLegacyPointerTracker legacyTracker =
                root.GetComponent<LudusLegacyPointerTracker>();
            if (legacyTracker != null)
            {
                Undo.DestroyObjectImmediate(legacyTracker);
            }

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        [MenuItem("GameObject/LUDUS/Marcar como área de observação", false, 12)]
        private static void MarkSelectedAsObservationArea()
        {
            GameObject selectedObject = Selection.activeGameObject;
            LudusCaptureContextTrigger trigger = ConfigureObservationArea(
                selectedObject
            );

            if (trigger == null)
            {
                return;
            }

            Selection.activeGameObject = selectedObject;
            EditorGUIUtility.PingObject(selectedObject);
        }

        [MenuItem("GameObject/LUDUS/Marcar como área de observação", true)]
        private static bool CanMarkSelectedAsObservationArea()
        {
            GameObject selectedObject = Selection.activeGameObject;
            return selectedObject != null &&
                selectedObject.scene.IsValid() &&
                !EditorUtility.IsPersistent(selectedObject);
        }

        internal static LudusCaptureContextTrigger ConfigureObservationArea(
            GameObject targetObject
        )
        {
            if (
                targetObject == null ||
                !targetObject.scene.IsValid() ||
                EditorUtility.IsPersistent(targetObject)
            )
            {
                return null;
            }

            LudusCaptureContextTrigger trigger =
                targetObject.GetComponent<LudusCaptureContextTrigger>();

            if (trigger == null)
            {
                trigger = Undo.AddComponent<LudusCaptureContextTrigger>(
                    targetObject
                );
            }
            else
            {
                Undo.RecordObject(trigger, "Marcar área de observação LUDUS");
            }

            trigger.captureVisualReference = true;
            trigger.contextKind =
                targetObject.GetComponent<Canvas>() != null ||
                targetObject.GetComponentInParent<Canvas>() != null
                    ? LudusCaptureContextKind.Canvas
                    : LudusCaptureContextKind.Activity;

            EditorUtility.SetDirty(trigger);
            EditorSceneManager.MarkSceneDirty(targetObject.scene);
            return trigger;
        }

        private static void AddPreferredPointerTracker(
            GameObject root,
            LudusSessionController controller
        )
        {
            Type inputSystemTrackerType = GetInputSystemTrackerType();

            if (inputSystemTrackerType != null)
            {
                Component tracker = root.AddComponent(inputSystemTrackerType);
                AssignController(tracker, controller);
                return;
            }

            LudusLegacyPointerTracker legacyTracker =
                root.AddComponent<LudusLegacyPointerTracker>();
            legacyTracker.sessionController = controller;
        }

        private static Type GetInputSystemTrackerType()
        {
            return Type.GetType(
                "LudusSDK.LudusInputSystemPointerTracker, Ludus.Unity.InputSystem"
            );
        }

        private static void AssignController(
            Component tracker,
            LudusSessionController controller
        )
        {
            SerializedObject serializedTracker = new SerializedObject(tracker);
            serializedTracker.FindProperty("sessionController").objectReferenceValue =
                controller;
            serializedTracker.ApplyModifiedPropertiesWithoutUndo();
        }

        private static LudusSdkConfig CreateGameConfig()
        {
            const string folderPath = "Assets/LUDUS";

            EnsureFolder(folderPath);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                folderPath + "/ConfiguracaoLudus.asset"
            );
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static LudusSdkConfig CreateTutorialConfig()
        {
            string assetPath = TutorialFolderPath + "/ConfiguracaoTutorialLudus.asset";
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameName = "Tutorial LUDUS";
            config.sceneCaptureMode = LudusSceneCaptureMode.AllScenes;
            config.sendOnSessionEnd = false;
            config.saveLocalCopyOnSessionEnd = true;
            config.downloadJsonOnSessionEnd = true;
            config.enableLocalFallback = true;
            config.debugMode = true;

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static GameObject CreateConfiguredCaptureBase(
            LudusSdkConfig config
        )
        {
            GameObject root = new GameObject("LUDUS SDK");
            LudusSessionController controller =
                root.AddComponent<LudusSessionController>();
            LudusSessionExporter exporter =
                root.AddComponent<LudusSessionExporter>();
            LudusSceneCaptureCoordinator sceneCoordinator =
                root.AddComponent<LudusSceneCaptureCoordinator>();

            controller.config = config;
            controller.persistAcrossScenes = true;
            AddPreferredPointerTracker(root, controller);
            exporter.sessionController = controller;
            sceneCoordinator.sessionController = controller;
            return root;
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] pathParts = folderPath.Split('/');
            string currentPath = pathParts[0];

            for (int index = 1; index < pathParts.Length; index++)
            {
                string nextPath = currentPath + "/" + pathParts[index];

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[index]);
                }

                currentPath = nextPath;
            }
        }
    }

    [CustomEditor(typeof(LudusSessionController))]
    public sealed class LudusSessionControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "A base de coleta cria e atribui uma configuração automaticamente em Assets/LUDUS. Se precisar, você pode trocar o arquivo abaixo.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("config"),
                new GUIContent("Configuração do jogo")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("persistAcrossScenes"),
                new GUIContent("Manter ativo ao trocar de cena")
            );

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(LudusSdkConfig))]
    public sealed class LudusSdkConfigEditor : UnityEditor.Editor
    {
        private static bool showAdvancedConnectionOptions;
        private static readonly Dictionary<string, SceneAsset>
            SceneAssetsByName = new Dictionary<string, SceneAsset>(
                StringComparer.Ordinal
            );
        private static bool sceneAssetCacheNeedsRefresh = true;

        private void OnEnable()
        {
            EditorApplication.projectChanged += InvalidateSceneAssetCache;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= InvalidateSceneAssetCache;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Preencha o nome do jogo. O SDK gera internamente o identificador técnico exigido pelo contrato e registra a versão do jogo como 1.0.0.",
                MessageType.Info
            );

            EditorGUILayout.LabelField(
                "Identificação do jogo",
                EditorStyles.boldLabel
            );
            DrawProperty("gameName", "Nome do jogo");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Envio e cópia local", EditorStyles.boldLabel);
            DrawProperty("sendOnSessionEnd", "Enviar sessões para LUDUS Acompanha");
            DrawProperty(
                "saveLocalCopyOnSessionEnd",
                "Guardar também uma cópia de segurança local"
            );
            DrawProperty(
                "downloadJsonOnSessionEnd",
                "Baixar arquivo JSON ao encerrar (WebGL)"
            );
            DrawProperty(
                "downloadFileLabel",
                "Rótulo do arquivo (opcional)"
            );
            DrawDeliveryStatus();

            EditorGUILayout.Space();
            showAdvancedConnectionOptions = EditorGUILayout.Foldout(
                showAdvancedConnectionOptions,
                "Conexão com ambiente LUDUS (avançado)",
                true
            );

            if (showAdvancedConnectionOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Informe somente a origem da API fornecida pelo ambiente LUDUS, sem /api no final. Não informe JWT, tokens, senhas ou outras credenciais no Unity.",
                    MessageType.Info
                );
                DrawProperty("apiBaseUrl", "URL da API LUDUS");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cenas acompanhadas", EditorStyles.boldLabel);
            DrawProperty("sceneCaptureMode", "Capturar automaticamente em");
            DrawSceneSelection();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Coleta essencial", EditorStyles.boldLabel);
            SerializedProperty capabilities =
                serializedObject.FindProperty("capabilities");
            DrawCapability(capabilities, "clicks", "Cliques");
            DrawCapability(capabilities, "mousePath", "Trajetória do mouse");
            DrawCapability(capabilities, "dragPath", "Trajetória de arraste");
            EditorGUILayout.HelpBox(
                "O SDK acompanha as interações por cena automaticamente. Se uma mesma cena possuir telas ou atividades internas diferentes, você poderá marcá-las opcionalmente na seção de capturas visuais.",
                MessageType.None
            );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Resultados informados pelo jogo",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "Habilite somente os resultados realmente conectados pela Ponte semântica, pelos adaptadores ou pela API C#. O SDK não deduz sozinho o que é acerto, erro, fase ou categoria.",
                MessageType.Info
            );
            DrawCapability(capabilities, "categoryEvents", "Categorias");
            DrawCapability(capabilities, "phaseEvents", "Eventos de fase");
            DrawCapability(capabilities, "correctWrong", "Acertos e erros");
            DrawCapability(
                capabilities,
                "customEvents",
                "Eventos personalizados"
            );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Capturas visuais",
                EditorStyles.boldLabel
            );
            DrawCapability(
                capabilities,
                "screenshots",
                "Capturar imagem da tela"
            );

            if (capabilities.FindPropertyRelative("screenshots").boolValue)
            {
                EditorGUI.indentLevel++;
                DrawProperty(
                    "captureScreenshotOnContextStart",
                    "Capturar após a primeira interação"
                );
                EditorGUI.indentLevel--;

                DrawProperty(
                    "sceneScreenshotMode",
                    "Imagens automáticas nas cenas"
                );
                DrawScreenshotSceneSelection();
                DrawObservationAreaSelection();

                EditorGUILayout.HelpBox(
                    "A imagem pode registrar conteúdo visível do jogo. Habilite somente quando houver finalidade definida e autorização adequada. O SDK mantém uma imagem por cena ou recorte e conserva até quatro, priorizando os locais com mais interações e usando o tempo como desempate.",
                    MessageType.Warning
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Desativado por padrão para reduzir o payload e preservar a privacidade. Quando habilitado, o mapa da sessão pode usar a imagem como fundo.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space();
            DrawProperty("debugMode", "Exibir mensagens detalhadas no Console");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperty(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(propertyName),
                new GUIContent(label)
            );
        }

        private void DrawSceneSelection()
        {
            SerializedProperty sceneCaptureMode =
                serializedObject.FindProperty("sceneCaptureMode");

            if (
                sceneCaptureMode.enumValueIndex !=
                (int)LudusSceneCaptureMode.SelectedScenes
            )
            {
                EditorGUILayout.HelpBox(
                    "O SDK cria automaticamente um recorte para cada cena ativa e registra mouse/cliques em todas elas.",
                    MessageType.Info
                );
                return;
            }

            SerializedProperty selectedSceneNames =
                serializedObject.FindProperty("selectedSceneNames");
            EditorGUILayout.HelpBox(
                "Arraste do Project somente as cenas que devem ser acompanhadas. Nas demais cenas, o SDK mantém a sessão ativa, mas pausa mouse e cliques.",
                MessageType.Info
            );

            DrawSceneAssetList(
                selectedSceneNames,
                "Nenhuma cena selecionada. Arraste uma cena do Project para começar.",
                false
            );
            RemoveUntrackedScreenshotScenes();
        }

        private void DrawScreenshotSceneSelection()
        {
            SerializedProperty screenshotMode =
                serializedObject.FindProperty("sceneScreenshotMode");

            if (
                screenshotMode.enumValueIndex !=
                (int)LudusSceneScreenshotMode.SelectedScenes
            )
            {
                return;
            }

            SerializedProperty screenshotScenes =
                serializedObject.FindProperty(
                    "selectedScreenshotSceneNames"
                );

            EditorGUILayout.HelpBox(
                "Arraste do Project somente cenas acompanhadas cuja imagem ajude a interpretar o mapa.",
                MessageType.Info
            );

            DrawSceneAssetList(
                screenshotScenes,
                "Nenhuma cena usa imagem automática. Arraste aqui uma cena acompanhada.",
                true
            );
        }

        private void DrawSceneAssetList(
            SerializedProperty sceneNames,
            string emptyMessage,
            bool requireTrackedScene
        )
        {
            List<string> scenesOutsideBuild = new List<string>();
            List<string> missingScenes = new List<string>();

            for (int index = 0; index < sceneNames.arraySize; index++)
            {
                string sceneName = sceneNames
                    .GetArrayElementAtIndex(index)
                    .stringValue;
                SceneAsset sceneAsset = FindSceneAsset(sceneName);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUIContent sceneLabel = new GUIContent(
                    sceneName,
                    EditorGUIUtility.IconContent("SceneAsset Icon").image
                );
                EditorGUILayout.LabelField(sceneLabel);

                using (new EditorGUI.DisabledScope(sceneAsset == null))
                {
                    if (GUILayout.Button("Localizar", GUILayout.Width(70f)))
                    {
                        Selection.activeObject = sceneAsset;
                        EditorGUIUtility.PingObject(sceneAsset);
                    }
                }

                if (GUILayout.Button("Remover", GUILayout.Width(70f)))
                {
                    sceneNames.DeleteArrayElementAtIndex(index);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                if (sceneAsset == null)
                {
                    missingScenes.Add(sceneName);
                }
                else if (!IsSceneInEnabledBuildProfile(sceneName))
                {
                    scenesOutsideBuild.Add(sceneName);
                }
            }

            if (sceneNames.arraySize == 0)
            {
                EditorGUILayout.HelpBox(emptyMessage, MessageType.None);
            }

            DrawSceneDropArea(sceneNames, requireTrackedScene);

            if (missingScenes.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "Não foi possível localizar no Project: " +
                        string.Join(", ", missingScenes) +
                        ". Remova a referência ou restaure a cena.",
                    MessageType.Warning
                );
            }

            if (scenesOutsideBuild.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "Fora do Build Profile: " +
                        string.Join(", ", scenesOutsideBuild) +
                        ". A seleção foi salva, mas a cena precisa entrar no Build Profile para fazer parte do jogo publicado.",
                    MessageType.Warning
                );
            }
        }

        private void DrawSceneDropArea(
            SerializedProperty sceneNames,
            bool requireTrackedScene
        )
        {
            Rect dropArea = GUILayoutUtility.GetRect(
                0f,
                52f,
                GUILayout.ExpandWidth(true)
            );
            GUI.Box(
                dropArea,
                "Arraste aqui cenas do Project",
                EditorStyles.helpBox
            );

            Event currentEvent = Event.current;
            if (
                !dropArea.Contains(currentEvent.mousePosition) ||
                (currentEvent.type != EventType.DragUpdated &&
                    currentEvent.type != EventType.DragPerform)
            )
            {
                return;
            }

            bool hasSceneAsset = false;
            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is SceneAsset)
                {
                    hasSceneAsset = true;
                    break;
                }
            }

            if (!hasSceneAsset)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (currentEvent.type != EventType.DragPerform)
            {
                currentEvent.Use();
                return;
            }

            DragAndDrop.AcceptDrag();
            List<string> rejectedScenes = new List<string>();

            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
            {
                SceneAsset sceneAsset = draggedObject as SceneAsset;
                if (sceneAsset == null)
                {
                    continue;
                }

                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(
                    scenePath
                );

                if (requireTrackedScene && !IsSceneTracked(sceneName))
                {
                    rejectedScenes.Add(sceneName);
                    continue;
                }

                SetSceneSelected(sceneNames, sceneName, true);
            }

            if (rejectedScenes.Count > 0)
            {
                Debug.LogWarning(
                    "[LUDUS] As cenas " +
                        string.Join(", ", rejectedScenes) +
                        " não foram adicionadas às imagens porque ainda não são cenas acompanhadas."
                );
            }

            currentEvent.Use();
        }

        private void DrawObservationAreaSelection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Telas ou atividades dentro da cena (opcional)",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "Use esta opção somente quando uma mesma cena possuir diferentes Canvas, painéis ou atividades ativados em momentos distintos. A imagem continua sendo da Game View completa, mas fica identificada pelo momento e pela área observada. Se a cena representar uma única atividade, basta selecionar a cena acima.",
                MessageType.Info
            );

            DrawObservationAreaDropArea();
            DrawLoadedObservationAreas();
        }

        private static void DrawObservationAreaDropArea()
        {
            Rect dropArea = GUILayoutUtility.GetRect(
                0f,
                52f,
                GUILayout.ExpandWidth(true)
            );
            GUI.Box(
                dropArea,
                "Arraste aqui um Canvas, painel ou atividade da Hierarchy",
                EditorStyles.helpBox
            );

            Event currentEvent = Event.current;
            if (
                !dropArea.Contains(currentEvent.mousePosition) ||
                (currentEvent.type != EventType.DragUpdated &&
                    currentEvent.type != EventType.DragPerform)
            )
            {
                return;
            }

            bool hasSceneObject = false;
            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
            {
                GameObject gameObject = GetSceneGameObject(draggedObject);
                if (IsEditableSceneObject(gameObject))
                {
                    hasSceneObject = true;
                    break;
                }
            }

            if (!hasSceneObject)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            if (currentEvent.type != EventType.DragPerform)
            {
                currentEvent.Use();
                return;
            }

            DragAndDrop.AcceptDrag();
            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
            {
                GameObject gameObject = GetSceneGameObject(draggedObject);
                if (IsEditableSceneObject(gameObject))
                {
                    LudusSetupAssistant.ConfigureObservationArea(gameObject);
                }
            }

            currentEvent.Use();
        }

        private static void DrawLoadedObservationAreas()
        {
            LudusCaptureContextTrigger[] triggers =
                UnityEngine.Object.FindObjectsByType<LudusCaptureContextTrigger>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );
            Array.Sort(
                triggers,
                (left, right) => string.CompareOrdinal(
                    left.gameObject.scene.name + "/" + left.gameObject.name,
                    right.gameObject.scene.name + "/" + right.gameObject.name
                )
            );

            bool hasMarkedArea = false;
            foreach (LudusCaptureContextTrigger trigger in triggers)
            {
                if (trigger == null || !trigger.captureVisualReference)
                {
                    continue;
                }

                hasMarkedArea = true;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(
                    trigger.gameObject.scene.name + " / " + trigger.gameObject.name
                );

                if (GUILayout.Button("Selecionar", GUILayout.Width(72f)))
                {
                    Selection.activeGameObject = trigger.gameObject;
                    EditorGUIUtility.PingObject(trigger.gameObject);
                }

                if (GUILayout.Button("Desmarcar", GUILayout.Width(76f)))
                {
                    Undo.RecordObject(
                        trigger,
                        "Desmarcar área de observação LUDUS"
                    );
                    trigger.captureVisualReference = false;
                    EditorUtility.SetDirty(trigger);
                    EditorSceneManager.MarkSceneDirty(trigger.gameObject.scene);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (!hasMarkedArea)
            {
                EditorGUILayout.HelpBox(
                    "Nenhuma Área de observação LUDUS marcada nas cenas abertas.",
                    MessageType.None
                );
            }
        }

        private bool IsSceneTracked(string sceneName)
        {
            SerializedProperty trackedMode =
                serializedObject.FindProperty("sceneCaptureMode");
            return trackedMode.enumValueIndex ==
                    (int)LudusSceneCaptureMode.AllScenes ||
                ContainsSceneName(
                    serializedObject.FindProperty("selectedSceneNames"),
                    sceneName
                );
        }

        private void RemoveUntrackedScreenshotScenes()
        {
            SerializedProperty screenshotScenes =
                serializedObject.FindProperty("selectedScreenshotSceneNames");

            for (int index = screenshotScenes.arraySize - 1; index >= 0; index--)
            {
                string sceneName = screenshotScenes
                    .GetArrayElementAtIndex(index)
                    .stringValue;
                if (!IsSceneTracked(sceneName))
                {
                    screenshotScenes.DeleteArrayElementAtIndex(index);
                }
            }
        }

        private static SceneAsset FindSceneAsset(string sceneName)
        {
            RefreshSceneAssetCacheIfNeeded();
            return SceneAssetsByName.TryGetValue(
                sceneName,
                out SceneAsset sceneAsset
            )
                ? sceneAsset
                : null;
        }

        private static void InvalidateSceneAssetCache()
        {
            sceneAssetCacheNeedsRefresh = true;
        }

        private static void RefreshSceneAssetCacheIfNeeded()
        {
            if (!sceneAssetCacheNeedsRefresh)
            {
                return;
            }

            SceneAssetsByName.Clear();

            foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SceneAsset sceneAsset =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (sceneAsset == null)
                {
                    continue;
                }

                string sceneName =
                    System.IO.Path.GetFileNameWithoutExtension(path);

                if (!SceneAssetsByName.ContainsKey(sceneName))
                {
                    SceneAssetsByName.Add(sceneName, sceneAsset);
                }
            }

            sceneAssetCacheNeedsRefresh = false;
        }

        private static bool IsSceneInEnabledBuildProfile(string sceneName)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (
                    buildScene.enabled &&
                    string.Equals(
                        System.IO.Path.GetFileNameWithoutExtension(buildScene.path),
                        sceneName,
                        StringComparison.Ordinal
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject GetSceneGameObject(
            UnityEngine.Object draggedObject
        )
        {
            if (draggedObject is GameObject gameObject)
            {
                return gameObject;
            }

            return draggedObject is Component component
                ? component.gameObject
                : null;
        }

        private static bool IsEditableSceneObject(GameObject gameObject)
        {
            return gameObject != null &&
                gameObject.scene.IsValid() &&
                !EditorUtility.IsPersistent(gameObject);
        }

        private static bool ContainsSceneName(
            SerializedProperty sceneNames,
            string sceneName
        )
        {
            for (int index = 0; index < sceneNames.arraySize; index++)
            {
                if (sceneNames.GetArrayElementAtIndex(index).stringValue == sceneName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetSceneSelected(
            SerializedProperty sceneNames,
            string sceneName,
            bool selected
        )
        {
            for (int index = sceneNames.arraySize - 1; index >= 0; index--)
            {
                if (sceneNames.GetArrayElementAtIndex(index).stringValue != sceneName)
                {
                    continue;
                }

                if (!selected)
                {
                    sceneNames.DeleteArrayElementAtIndex(index);
                }

                return;
            }

            if (selected)
            {
                sceneNames.arraySize++;
                sceneNames.GetArrayElementAtIndex(
                    sceneNames.arraySize - 1
                ).stringValue = sceneName;
            }
        }

        private static void DrawCapability(
            SerializedProperty capabilities,
            string propertyName,
            string label
        )
        {
            EditorGUILayout.PropertyField(
                capabilities.FindPropertyRelative(propertyName),
                new GUIContent(label)
            );
        }

        private void DrawDeliveryStatus()
        {
            bool sendToPlatform =
                serializedObject.FindProperty("sendOnSessionEnd").boolValue;
            bool saveLocalCopy =
                serializedObject.FindProperty("saveLocalCopyOnSessionEnd").boolValue;
            bool downloadJson =
                serializedObject.FindProperty("downloadJsonOnSessionEnd").boolValue;

            if (!sendToPlatform && !saveLocalCopy)
            {
                EditorGUILayout.HelpBox(
                    "A sessão não será enviada nem salva automaticamente. Use essa combinação apenas se o jogo tratar o JSON por outro fluxo próprio.",
                    MessageType.Warning
                );
                return;
            }

            if (!sendToPlatform)
            {
                EditorGUILayout.HelpBox(
                    "A sessão não será enviada automaticamente. A cópia de segurança local não é um arquivo em Downloads; no WebGL, marque a opção de baixar JSON se quiser importá-lo manualmente no Dashboard.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    saveLocalCopy
                        ? "A sessão será enviada à plataforma e uma cópia de segurança local será preservada."
                        : "A sessão será enviada à plataforma. Se o envio falhar, o SDK guardará um fallback local automaticamente.",
                    MessageType.Info
                );
            }

            if (downloadJson)
            {
                EditorGUILayout.HelpBox(
                    "No WebGL, o navegador baixará um arquivo .json normal ao encerrar a sessão. A pasta é definida pelo navegador e sistema operacional; isso não substitui o fallback local.",
                    MessageType.Info
                );
            }
        }
    }

    [CustomEditor(typeof(LudusLegacyPointerTracker))]
    public sealed class LudusLegacyPointerTrackerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Este componente registra mouse e clique usando o Input clássico da Unity. A referência ao controlador já é preenchida ao criar a base de coleta.",
                MessageType.Info
            );
            Draw("sessionController", "Objeto controlador LUDUS SDK");
            Draw("captureMousePath", "Capturar trajetória do mouse");
            Draw("mouseSampleIntervalMs", "Intervalo entre pontos (ms)");
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(propertyName),
                new GUIContent(label)
            );
        }
    }

    [CustomEditor(typeof(LudusSessionExporter))]
    public sealed class LudusSessionExporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Envia a sessão ao encerrar ou guarda uma cópia local quando não houver URL ou o envio falhar.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("sessionController"),
                new GUIContent("Objeto controlador LUDUS SDK")
            );
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(LudusCaptureContextTrigger))]
    public sealed class LudusCaptureContextTriggerEditor : UnityEditor.Editor
    {
        private static bool showControllerOverride;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField(
                "Área de observação LUDUS",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "Esta área representa um Canvas, painel ou atividade que você quer acompanhar. O SDK encontra automaticamente a base LUDUS SDK.",
                MessageType.Info
            );
            Draw("beginWhenEnabled", "Iniciar ao ativar este objeto");
            Draw("endWhenDisabled", "Encerrar ao desativar este objeto");
            EditorGUILayout.Space();
            Draw(
                "titleForDashboard",
                "Título exibido no acompanhamento (vazio = nome do objeto)"
            );
            Draw("contextKind", "Tipo deste recorte");
            Draw("observationPurpose", "Objetivo deste recorte (opcional)");

            EditorGUILayout.Space();
            Draw(
                "captureVisualReference",
                "Usar este recorte como fundo do mapa"
            );
            EditorGUILayout.HelpBox(
                "Marque somente Canvas, painéis ou atividades cuja imagem ajude a interpretar o mapa. Menus, HUDs e telas auxiliares normalmente devem permanecer desmarcados.",
                MessageType.Info
            );

            EditorGUILayout.Space();
            showControllerOverride = EditorGUILayout.Foldout(
                showControllerOverride,
                "Referência manual (somente se houver mais de uma base LUDUS)",
                true
            );

            if (showControllerOverride)
            {
                Draw("sessionController", "Objeto controlador LUDUS SDK");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(propertyName),
                new GUIContent(label)
            );
        }
    }

    [CustomEditor(typeof(LudusSceneCaptureCoordinator))]
    public sealed class LudusSceneCaptureCoordinatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Este componente acompanha automaticamente a cena ativa. Configure no asset LUDUS se a coleta vale para todas as cenas ou somente para as selecionadas.",
                MessageType.Info
            );
        }
    }
}
