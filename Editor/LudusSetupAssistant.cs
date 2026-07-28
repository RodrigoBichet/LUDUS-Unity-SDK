using System;
using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    public static class LudusSetupAssistant
    {
        [MenuItem("GameObject/LUDUS/Criar base de coleta", false, 10)]
        private static void CreateCaptureBase()
        {
            LudusSdkConfig config = CreateGameConfig();
            GameObject root = new GameObject("LUDUS SDK");
            Undo.RegisterCreatedObjectUndo(root, "Criar base de coleta LUDUS");

            LudusSessionController controller =
                root.AddComponent<LudusSessionController>();
            LudusSessionExporter exporter =
                root.AddComponent<LudusSessionExporter>();

            controller.config = config;
            AddPreferredPointerTracker(root, controller);
            exporter.sessionController = controller;

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

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "LUDUS");
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                folderPath + "/ConfiguracaoLudus.asset"
            );
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            return config;
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
            DrawProperty("saveLocalCopyOnSessionEnd", "Salvar também uma cópia local");
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
                    "A sessão ficará apenas na cópia local. O envio para LUDUS Acompanha está desativado.",
                    MessageType.Info
                );
                return;
            }

            EditorGUILayout.HelpBox(
                saveLocalCopy
                    ? "A sessão será enviada à plataforma e uma cópia local será preservada."
                    : "A sessão será enviada à plataforma. Se o envio falhar, o SDK guardará um fallback local automaticamente.",
                MessageType.Info
            );
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
}
