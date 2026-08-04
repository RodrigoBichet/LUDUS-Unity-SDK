using System;
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

            EditorSceneManager.SaveScene(tutorialScene, TutorialScenePath);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            EditorUtility.DisplayDialog(
                "Tutorial LUDUS",
                "Cena criada. Pressione Play e use os botões para testar a coleta sem alterar o seu jogo.",
                "Entendi"
            );
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
        private static bool showAdvancedCollectionOptions;
        private static bool showAdvancedConnectionOptions;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Preencha o nome do jogo. O SDK gera internamente o identificador técnico exigido pelo contrato e usa versão 1.0.0 até uma edição futura do jogo ser distribuída.",
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
            EditorGUILayout.HelpBox(
                "Os recortes de observação por Canvas, painel ou atividade são registrados automaticamente quando você adiciona LudusCaptureContextTrigger ao objeto desejado.",
                MessageType.None
            );

            EditorGUILayout.Space();
            showAdvancedCollectionOptions = EditorGUILayout.Foldout(
                showAdvancedCollectionOptions,
                "Opções avançadas (em evolução)",
                true
            );

            if (showAdvancedCollectionOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Habilite somente recursos que já tenham sido integrados ao seu jogo. Arraste e inatividade ainda não são coletados automaticamente nesta versão.",
                    MessageType.Warning
                );
                DrawCapability(capabilities, "dragPath", "Trajetória de arraste");
                DrawCapability(capabilities, "screenshots", "Capturas de tela");
                DrawCapability(capabilities, "inactivity", "Períodos de inatividade");
                DrawCapability(capabilities, "focusEvents", "Mudanças de foco");
                DrawCapability(capabilities, "phaseEvents", "Eventos de fase informados pelo jogo");
                DrawCapability(capabilities, "correctWrong", "Acertos e erros informados pelo jogo");
                DrawCapability(capabilities, "categoryEvents", "Categorias informadas pelo jogo");

                if (capabilities.FindPropertyRelative("inactivity").boolValue)
                {
                    DrawProperty(
                        "inactivityThresholdSeconds",
                        "Tempo para considerar inatividade (segundos)"
                    );
                }

                EditorGUI.indentLevel--;
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
                "Marque as cenas do Build Profile que devem ser acompanhadas. Nas demais cenas, o SDK mantém a sessão ativa, mas pausa mouse e cliques.",
                MessageType.Info
            );

            EditorBuildSettingsScene[] buildScenes =
                EditorBuildSettings.scenes;
            bool hasEnabledScene = false;

            foreach (EditorBuildSettingsScene buildScene in buildScenes)
            {
                if (!buildScene.enabled)
                {
                    continue;
                }

                hasEnabledScene = true;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(
                    buildScene.path
                );
                bool selected = ContainsSceneName(selectedSceneNames, sceneName);
                bool nextSelected = EditorGUILayout.ToggleLeft(
                    sceneName,
                    selected
                );

                if (nextSelected != selected)
                {
                    SetSceneSelected(
                        selectedSceneNames,
                        sceneName,
                        nextSelected
                    );
                }
            }

            if (!hasEnabledScene)
            {
                EditorGUILayout.HelpBox(
                    "Adicione cenas ao Build Profile para selecioná-las aqui.",
                    MessageType.Warning
                );
            }
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
            EditorGUILayout.HelpBox(
                "Adicione este componente ao Canvas, painel ou objeto que representa o recorte que você quer acompanhar. O SDK encontra automaticamente a base LUDUS SDK.",
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
