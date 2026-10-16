using System.Collections.Generic;
using System.IO;
using LudusSDK;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudusSDK.Editor
{
    public sealed class LudusVisualMapSetupWindow : EditorWindow
    {
        private sealed class SceneOption
        {
            public string Name;
            public bool Selected;
        }

        private readonly List<SceneOption> sceneOptions =
            new List<SceneOption>();
        private Vector2 scrollPosition;
        private bool useAllTrackedScenes = true;

        [MenuItem("LUDUS/Adicionar imagens ao mapa (Etapa 2)", false, 2)]
        private static void OpenWindow()
        {
            LudusVisualMapSetupWindow window =
                GetWindow<LudusVisualMapSetupWindow>();
            window.titleContent = new GUIContent("Etapa 2 LUDUS");
            window.minSize = new Vector2(520f, 430f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSceneOptions();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Adicionar imagens ao mapa",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "A Etapa 2 usa uma imagem da tela como fundo do mapa de cliques, movimentos e arrastes. Se alguma cena exibir senha, perfil ou outra informação pessoal que você não queira registrar, use a seleção manual e deixe essa cena desmarcada.",
                MessageType.Info
            );

            EditorGUILayout.HelpBox(
                "A seleção manual mostra as cenas cadastradas em File > Build Profiles e também a cena que estiver aberta agora. Se criar uma cena depois, adicione-a ao Build Profiles e abra esta tela novamente para revisar a seleção.",
                MessageType.None
            );

            EditorGUILayout.Space(8f);
            useAllTrackedScenes = EditorGUILayout.ToggleLeft(
                "Usar todas as cenas acompanhadas (recomendado)",
                useAllTrackedScenes
            );

            using (new EditorGUI.DisabledScope(useAllTrackedScenes))
            {
                EditorGUILayout.Space(5f);
                EditorGUILayout.LabelField(
                    "Ou escolha as cenas que terão imagem",
                    EditorStyles.boldLabel
                );
                scrollPosition = EditorGUILayout.BeginScrollView(
                    scrollPosition,
                    GUILayout.Height(210f)
                );

                if (sceneOptions.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Nenhuma cena foi encontrada. Adicione as cenas do jogo em File > Build Profiles antes de usar a seleção manual.",
                        MessageType.Warning
                    );
                }

                foreach (SceneOption sceneOption in sceneOptions)
                {
                    sceneOption.Selected = EditorGUILayout.ToggleLeft(
                        sceneOption.Name,
                        sceneOption.Selected
                    );
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "Durante a jogada, a primeira interação relevante de cada cena prepara a imagem. Ela será protegida automaticamente no próximo salvamento parcial.",
                MessageType.None
            );

            using (new EditorGUI.DisabledScope(
                !useAllTrackedScenes && !HasSelectedScene()
            ))
            {
                if (GUILayout.Button("Ativar imagens no mapa", GUILayout.Height(38f)))
                {
                    ApplyConfiguration();
                }
            }
        }

        private void ApplyConfiguration()
        {
            List<string> selectedSceneNames = new List<string>();
            foreach (SceneOption sceneOption in sceneOptions)
            {
                if (sceneOption.Selected)
                {
                    selectedSceneNames.Add(sceneOption.Name);
                }
            }

            bool configured = LudusSetupAssistant.TryEnableVisualMap(
                useAllTrackedScenes
                    ? LudusSceneScreenshotMode.AllTrackedScenes
                    : LudusSceneScreenshotMode.SelectedScenes,
                selectedSceneNames,
                out string message
            );

            EditorUtility.DisplayDialog(
                configured ? "Etapa 2 pronta" : "Não foi possível configurar",
                configured
                    ? message +
                        " Pressione Play, conecte-se à coleta e jogue normalmente."
                    : message,
                "Entendi"
            );

            if (configured)
            {
                Close();
            }
        }

        private void LoadSceneOptions()
        {
            sceneOptions.Clear();
            HashSet<string> sceneNames = new HashSet<string>();

            foreach (EditorBuildSettingsScene buildScene in
                EditorBuildSettings.scenes)
            {
                string sceneName = Path.GetFileNameWithoutExtension(
                    buildScene.path
                );
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    sceneNames.Add(sceneName);
                }
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && !string.IsNullOrWhiteSpace(activeScene.name))
            {
                sceneNames.Add(activeScene.name);
            }

            foreach (string sceneName in sceneNames)
            {
                sceneOptions.Add(new SceneOption
                {
                    Name = sceneName,
                    Selected = false,
                });
            }
            sceneOptions.Sort((left, right) =>
                string.CompareOrdinal(left.Name, right.Name));
        }

        private bool HasSelectedScene()
        {
            foreach (SceneOption sceneOption in sceneOptions)
            {
                if (sceneOption.Selected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
