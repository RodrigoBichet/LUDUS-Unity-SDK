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
                "Arraste um botão da Hierarchy. O SDK detecta o componente Button, usa o nome do objeto e configura o acompanhamento automaticamente.",
                MessageType.Info
            );

            GameObject droppedObject = EditorGUILayout.ObjectField(
                "Arraste um botão aqui",
                null,
                typeof(GameObject),
                true
            ) as GameObject;

            if (droppedObject != null)
            {
                TryAddTrackedButton(droppedObject, activeScene);
            }

            EditorGUILayout.Space();
            DrawTrackedButtons(activeScene);
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
                $"Objetos configurados ({trackedButtons.Length})",
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

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (LudusTrackedButton trackedButton in trackedButtons)
            {
                DrawTrackedButton(trackedButton);
            }

            EditorGUILayout.EndScrollView();
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

            EditorGUILayout.PropertyField(
                serializedButton.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este botão")
            );
            EditorGUILayout.PropertyField(
                serializedButton.FindProperty("dashboardName"),
                new GUIContent("Nome exibido no dashboard")
            );
            EditorGUILayout.LabelField("Tipo detectado", "Botão");

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
}
