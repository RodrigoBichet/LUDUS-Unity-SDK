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
                "Arraste um objeto da Hierarchy. O SDK detecta Button e campos de texto automaticamente. Para imagens, sprites e outros objetos genéricos, você escolhe se a função acompanhada é clicável ou arrastável.",
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
            DrawTrackedClickables(activeScene);
            EditorGUILayout.Space();
            DrawTrackedTextInputs(activeScene);
            EditorGUILayout.Space();
            DrawTrackedDraggables(activeScene);
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
                "Detectado automaticamente",
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

        private void DrawTrackedClickables(Scene activeScene)
        {
            LudusTrackedClickable[] trackedClickables =
                Object.FindObjectsByType<LudusTrackedClickable>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(item => item.gameObject.scene == activeScene)
                .OrderBy(item => item.gameObject.name)
                .ToArray();

            EditorGUILayout.LabelField(
                $"Objetos clicáveis acompanhados ({trackedClickables.Length})",
                EditorStyles.boldLabel
            );

            if (trackedClickables.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhum objeto clicável genérico acompanhado nesta cena.",
                    MessageType.None
                );
                return;
            }

            foreach (LudusTrackedClickable trackedClickable in trackedClickables)
            {
                DrawTrackedClickable(trackedClickable);
            }
        }

        private static void DrawTrackedClickable(
            LudusTrackedClickable trackedClickable
        )
        {
            SerializedObject serializedClickable =
                new SerializedObject(trackedClickable);
            serializedClickable.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                trackedClickable.gameObject.name,
                EditorStyles.boldLabel
            );

            if (GUILayout.Button("Selecionar", GUILayout.Width(80f)))
            {
                Selection.activeGameObject = trackedClickable.gameObject;
                EditorGUIUtility.PingObject(trackedClickable.gameObject);
            }

            if (GUILayout.Button("Remover", GUILayout.Width(75f)))
            {
                Scene ownerScene = trackedClickable.gameObject.scene;
                Undo.DestroyObjectImmediate(trackedClickable);
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
                serializedClickable.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este objeto")
            );
            EditorGUILayout.PropertyField(
                serializedClickable.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.LabelField("Função escolhida", "Objeto clicável");
            EditorGUIUtility.labelWidth = previousLabelWidth;

            serializedClickable.ApplyModifiedProperties();

            if (GUILayout.Button("Trocar função para objeto arrastável"))
            {
                SwitchToTrackedDraggable(trackedClickable);
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTrackedDraggables(Scene activeScene)
        {
            LudusTrackedDraggable[] trackedDraggables =
                Object.FindObjectsByType<LudusTrackedDraggable>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(item => item.gameObject.scene == activeScene)
                .OrderBy(item => item.gameObject.name)
                .ToArray();

            EditorGUILayout.LabelField(
                $"Objetos arrastáveis acompanhados ({trackedDraggables.Length})",
                EditorStyles.boldLabel
            );

            if (trackedDraggables.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nenhum objeto arrastável acompanhado nesta cena.",
                    MessageType.None
                );
                return;
            }

            foreach (LudusTrackedDraggable trackedDraggable in trackedDraggables)
            {
                DrawTrackedDraggable(trackedDraggable);
            }
        }

        private static void DrawTrackedDraggable(
            LudusTrackedDraggable trackedDraggable
        )
        {
            SerializedObject serializedDraggable =
                new SerializedObject(trackedDraggable);
            serializedDraggable.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                trackedDraggable.gameObject.name,
                EditorStyles.boldLabel
            );

            if (GUILayout.Button("Selecionar", GUILayout.Width(80f)))
            {
                Selection.activeGameObject = trackedDraggable.gameObject;
                EditorGUIUtility.PingObject(trackedDraggable.gameObject);
            }

            if (GUILayout.Button("Remover", GUILayout.Width(75f)))
            {
                Scene ownerScene = trackedDraggable.gameObject.scene;
                Undo.DestroyObjectImmediate(trackedDraggable);
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
                serializedDraggable.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este objeto")
            );
            EditorGUILayout.PropertyField(
                serializedDraggable.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.PropertyField(
                serializedDraggable.FindProperty("minimumDistancePixels"),
                new GUIContent("Movimento mínimo (pixels)")
            );
            EditorGUILayout.LabelField("Função escolhida", "Objeto arrastável");
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUILayout.HelpBox(
                "O SDK apenas observa o gesto. O movimento visual e as regras de destino continuam sob responsabilidade do jogo.",
                MessageType.None
            );

            serializedDraggable.ApplyModifiedProperties();

            if (GUILayout.Button("Trocar função para objeto clicável"))
            {
                SwitchToTrackedClickable(trackedDraggable);
                EditorGUILayout.EndVertical();
                GUIUtility.ExitGUI();
                return;
            }

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

            LudusTrackedClickable existingClickable =
                target.GetComponent<LudusTrackedClickable>();

            if (existingClickable != null)
            {
                int existingChoice = EditorUtility.DisplayDialogComplex(
                    "Objeto já acompanhado como clicável",
                    "Este objeto já possui uma função LUDUS, possivelmente copiada durante uma duplicação. Você pode mantê-la ou trocar para arrastável.",
                    "Manter clicável",
                    "Cancelar",
                    "Trocar para arrastável"
                );

                if (existingChoice == 2)
                {
                    SwitchToTrackedDraggable(existingClickable);
                }

                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
                return;
            }

            LudusTrackedDraggable existingDraggable =
                target.GetComponent<LudusTrackedDraggable>();

            if (existingDraggable != null)
            {
                int existingChoice = EditorUtility.DisplayDialogComplex(
                    "Objeto já acompanhado como arrastável",
                    "Este objeto já possui uma função LUDUS, possivelmente copiada durante uma duplicação. Você pode mantê-la ou trocar para clicável.",
                    "Manter arrastável",
                    "Cancelar",
                    "Trocar para clicável"
                );

                if (existingChoice == 2)
                {
                    SwitchToTrackedClickable(existingDraggable);
                }

                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
                return;
            }

            int chosenFunction = EditorUtility.DisplayDialogComplex(
                "Como este objeto funciona no jogo?",
                "O SDK não deve deduzir a função apenas pela aparência do objeto. Escolha como esta interação deve ser acompanhada.",
                "Objeto arrastável",
                "Cancelar",
                "Objeto clicável"
            );

            if (chosenFunction == 0)
            {
                TryAddTrackedDraggable(target, activeScene);
            }
            else if (chosenFunction == 2)
            {
                TryAddTrackedClickable(target, activeScene);
            }
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
            EditorGUILayout.LabelField("Detectado automaticamente", "Botão");
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
                    "O objeto selecionado não possui um componente Button. O acompanhamento automático de botões aceita componentes Button da interface Unity. Para outros elementos, adicione o objeto como clicável ou arrastável.",
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

        private static void TryAddTrackedDraggable(
            GameObject target,
            Scene activeScene
        )
        {
            LudusTrackedDraggable existing =
                target.GetComponent<LudusTrackedDraggable>();

            if (existing != null)
            {
                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
                return;
            }

            LudusTrackedDraggable trackedDraggable =
                Undo.AddComponent<LudusTrackedDraggable>(target);
            Undo.RecordObject(
                trackedDraggable,
                "Configurar objeto arrastável LUDUS"
            );
            trackedDraggable.Configure(target.name);
            EditorUtility.SetDirty(trackedDraggable);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.activeGameObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private static void TryAddTrackedClickable(
            GameObject target,
            Scene activeScene
        )
        {
            LudusTrackedClickable existing =
                target.GetComponent<LudusTrackedClickable>();

            if (existing != null)
            {
                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
                return;
            }

            LudusTrackedClickable trackedClickable =
                Undo.AddComponent<LudusTrackedClickable>(target);
            Undo.RecordObject(
                trackedClickable,
                "Configurar objeto clicável LUDUS"
            );
            trackedClickable.Configure(target.name);
            EditorUtility.SetDirty(trackedClickable);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.activeGameObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private static void SwitchToTrackedClickable(
            LudusTrackedDraggable trackedDraggable
        )
        {
            GameObject target = trackedDraggable.gameObject;
            Scene ownerScene = target.scene;
            string dashboardName = trackedDraggable.DashboardName;
            bool trackingEnabled = trackedDraggable.TrackingEnabled;

            Undo.DestroyObjectImmediate(trackedDraggable);
            LudusTrackedClickable trackedClickable =
                Undo.AddComponent<LudusTrackedClickable>(target);
            Undo.RecordObject(
                trackedClickable,
                "Trocar função LUDUS para clicável"
            );
            trackedClickable.Configure(dashboardName, trackingEnabled);
            EditorUtility.SetDirty(trackedClickable);
            EditorSceneManager.MarkSceneDirty(ownerScene);
        }

        private static void SwitchToTrackedDraggable(
            LudusTrackedClickable trackedClickable
        )
        {
            GameObject target = trackedClickable.gameObject;
            Scene ownerScene = target.scene;
            string dashboardName = trackedClickable.DashboardName;
            bool trackingEnabled = trackedClickable.TrackingEnabled;

            Undo.DestroyObjectImmediate(trackedClickable);
            LudusTrackedDraggable trackedDraggable =
                Undo.AddComponent<LudusTrackedDraggable>(target);
            Undo.RecordObject(
                trackedDraggable,
                "Trocar função LUDUS para arrastável"
            );
            trackedDraggable.Configure(dashboardName, trackingEnabled);
            EditorUtility.SetDirty(trackedDraggable);
            EditorSceneManager.MarkSceneDirty(ownerScene);
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

    [CustomEditor(typeof(LudusTrackedDraggable))]
    public sealed class LudusTrackedDraggableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "O SDK registra o início, o fim, a duração e a distância do gesto. Ele não move o objeto e não classifica o resultado como certo ou errado.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este objeto")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("minimumDistancePixels"),
                new GUIContent("Movimento mínimo (pixels)")
            );

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(LudusTrackedClickable))]
    public sealed class LudusTrackedClickableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Use este acompanhamento quando o objeto tem função de clique ou toque no jogo, mesmo sem possuir um componente Button. O SDK observa o acionamento e não altera a lógica do objeto.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este objeto")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
