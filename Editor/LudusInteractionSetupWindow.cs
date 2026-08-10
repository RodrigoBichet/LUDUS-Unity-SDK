using System.Linq;
using LudusSDK;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudusSDK.Editor
{
    public sealed class LudusInteractionSetupWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        [MenuItem("LUDUS/Configurar interações desta cena", false, 30)]
        private static void OpenWindow()
        {
            LudusInteractionSetupWindow window =
                GetWindow<LudusInteractionSetupWindow>();
            window.titleContent = new GUIContent("Interações LUDUS");
            window.minSize = new Vector2(520f, 320f);
            window.Show();
        }

        private void OnGUI()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            EditorGUILayout.LabelField(
                "Interações acompanhadas",
                EditorStyles.boldLabel
            );
            EditorGUILayout.LabelField(
                "Cena atual: " + activeScene.name,
                EditorStyles.miniLabel
            );
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Arraste um botão ou campo de texto da Hierarchy. O SDK detecta o tipo, usa o nome do objeto e configura o acompanhamento automaticamente. Em campos de texto, o conteúdo digitado nunca é coletado.",
                MessageType.Info
            );

            GameObject droppedObject = EditorGUILayout.ObjectField(
                "Arraste um objeto aqui",
                null,
                typeof(GameObject),
                true
            ) as GameObject;

            if (droppedObject != null)
            {
                TryAddTrackedInteraction(droppedObject, activeScene);
            }

            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawTrackedButtons(activeScene);
            EditorGUILayout.Space();
            DrawTrackedTextInputs(activeScene);
            EditorGUILayout.EndScrollView();
        }

        private void DrawTrackedButtons(Scene activeScene)
        {
            LudusTrackedButton[] trackedButtons =
                Object.FindObjectsByType<LudusTrackedButton>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(item => item.gameObject.scene == activeScene)
                .OrderBy(item => item.gameObject.name)
                .ToArray();

            EditorGUILayout.LabelField(
                $"Botões acompanhados ({trackedButtons.Length})",
                EditorStyles.boldLabel
            );

            if (trackedButtons.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhum botão acompanhado nesta cena.",
                    MessageType.None
                );
                return;
            }

            foreach (LudusTrackedButton trackedButton in trackedButtons)
            {
                DrawTrackedButton(trackedButton);
            }
        }

        private void DrawTrackedTextInputs(Scene activeScene)
        {
            LudusTrackedTextInput[] trackedTextInputs =
                Object.FindObjectsByType<LudusTrackedTextInput>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(item => item.gameObject.scene == activeScene)
                .OrderBy(item => item.gameObject.name)
                .ToArray();

            EditorGUILayout.LabelField(
                $"Campos de texto acompanhados ({trackedTextInputs.Length})",
                EditorStyles.boldLabel
            );

            if (trackedTextInputs.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhum campo de texto acompanhado nesta cena.",
                    MessageType.None
                );
                return;
            }

            foreach (LudusTrackedTextInput trackedTextInput in trackedTextInputs)
            {
                DrawTrackedTextInput(trackedTextInput);
            }
        }

        private static void DrawTrackedTextInput(
            LudusTrackedTextInput trackedTextInput
        )
        {
            SerializedObject serializedInput =
                new SerializedObject(trackedTextInput);
            serializedInput.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                trackedTextInput.gameObject.name,
                EditorStyles.boldLabel
            );

            if (GUILayout.Button("Selecionar", GUILayout.Width(80f)))
            {
                Selection.activeGameObject = trackedTextInput.gameObject;
                EditorGUIUtility.PingObject(trackedTextInput.gameObject);
            }

            if (GUILayout.Button("Remover", GUILayout.Width(75f)))
            {
                Scene ownerScene = trackedTextInput.gameObject.scene;
                Undo.DestroyObjectImmediate(trackedTextInput);
                EditorSceneManager.MarkSceneDirty(ownerScene);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.EndHorizontal();

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190f;
            EditorGUILayout.PropertyField(
                serializedInput.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este campo")
            );
            EditorGUILayout.PropertyField(
                serializedInput.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.LabelField(
                "Tipo detectado",
                trackedTextInput.DetectedFieldType
            );
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUILayout.HelpBox(
                "Privacidade: somente a conclusão, a quantidade de caracteres e se o campo ficou vazio são registradas. O texto digitado não entra no JSON.",
                MessageType.None
            );

            serializedInput.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }

        private static void TryAddTrackedInteraction(
            GameObject target,
            Scene activeScene
        )
        {
            if (target.scene != activeScene)
            {
                EditorUtility.DisplayDialog(
                    "Interações LUDUS",
                    "Arraste um objeto que pertença à cena ativa.",
                    "Entendi"
                );
                return;
            }

            if (LudusTrackedTextInput.CanTrack(target))
            {
                TryAddTrackedTextInput(target, activeScene);
                return;
            }

            if (LudusTrackedButton.CanTrack(target))
            {
                TryAddTrackedButton(target, activeScene);
                return;
            }

            EditorUtility.DisplayDialog(
                "Interações LUDUS",
                "O objeto selecionado não possui um Button, InputField ou TMP_InputField compatível.",
                "Entendi"
            );
        }

        private void DrawTrackedButton(LudusTrackedButton trackedButton)
        {
            SerializedObject serializedButton =
                new SerializedObject(trackedButton);
            serializedButton.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                trackedButton.gameObject.name,
                EditorStyles.boldLabel
            );

            if (GUILayout.Button("Selecionar", GUILayout.Width(80f)))
            {
                Selection.activeGameObject = trackedButton.gameObject;
                EditorGUIUtility.PingObject(trackedButton.gameObject);
            }

            if (GUILayout.Button("Remover", GUILayout.Width(75f)))
            {
                Scene ownerScene = trackedButton.gameObject.scene;
                Undo.DestroyObjectImmediate(trackedButton);
                EditorSceneManager.MarkSceneDirty(ownerScene);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.EndHorizontal();

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190f;
            EditorGUILayout.PropertyField(
                serializedButton.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este botão")
            );
            EditorGUILayout.PropertyField(
                serializedButton.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.LabelField("Tipo detectado", "Botão");
            EditorGUIUtility.labelWidth = previousLabelWidth;

            serializedButton.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }

        private static void TryAddTrackedButton(
            GameObject target,
            Scene activeScene
        )
        {
            if (target.scene != activeScene)
            {
                EditorUtility.DisplayDialog(
                    "Interações LUDUS",
                    "Arraste um objeto que pertença à cena ativa.",
                    "Entendi"
                );
                return;
            }

            if (!LudusTrackedButton.CanTrack(target))
            {
                EditorUtility.DisplayDialog(
                    "Interações LUDUS",
                    "O objeto selecionado ainda não possui um componente Button. Nesta primeira etapa, o acompanhamento automático aceita botões da interface da Unity.",
                    "Entendi"
                );
                return;
            }

            LudusTrackedButton existing =
                target.GetComponent<LudusTrackedButton>();

            if (existing != null)
            {
                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
                return;
            }

            LudusTrackedButton trackedButton =
                Undo.AddComponent<LudusTrackedButton>(target);
            Undo.RecordObject(trackedButton, "Configurar botão LUDUS");
            trackedButton.Configure(target.name);
            EditorUtility.SetDirty(trackedButton);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.activeGameObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private static void TryAddTrackedTextInput(
            GameObject target,
            Scene activeScene
        )
        {
            LudusTrackedTextInput existing =
                target.GetComponent<LudusTrackedTextInput>();

            if (existing != null)
            {
                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
                return;
            }

            LudusTrackedTextInput trackedTextInput =
                Undo.AddComponent<LudusTrackedTextInput>(target);
            Undo.RecordObject(
                trackedTextInput,
                "Configurar campo de texto LUDUS"
            );
            trackedTextInput.Configure(target.name);
            EditorUtility.SetDirty(trackedTextInput);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.activeGameObject = target;
            EditorGUIUtility.PingObject(target);
        }
    }

    [CustomEditor(typeof(LudusTrackedButton))]
    public sealed class LudusTrackedButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "O SDK registra automaticamente quando este botão é acionado durante uma sessão e um recorte acompanhados.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este botão")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(LudusTrackedTextInput))]
    public sealed class LudusTrackedTextInputEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LudusTrackedTextInput trackedTextInput =
                (LudusTrackedTextInput)target;

            EditorGUILayout.HelpBox(
                "O SDK registra quando o preenchimento deste campo é concluído. O conteúdo digitado nunca é coletado; somente a quantidade de caracteres e se o campo ficou vazio.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este campo")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.LabelField(
                "Tipo detectado",
                trackedTextInput.DetectedFieldType
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
