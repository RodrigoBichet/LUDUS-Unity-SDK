using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    [CustomEditor(typeof(LudusSessionScope))]
    public sealed class LudusSessionScopeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Use um escopo para a atividade completa. Canvas e painéis internos podem abrir e fechar sem reiniciar a sessão. Adicione o componente a um objeto que permaneça ativo durante toda a categoria.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("startWhenEnabled"),
                new GUIContent("Iniciar ao ativar este objeto")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("endWhenDisabled"),
                new GUIContent("Encerrar ao desativar ou sair da cena")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("sessionDisplayName"),
                new GUIContent("Nome da atividade no arquivo")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("showConsoleMessages"),
                new GUIContent("Exibir avisos no Console")
            );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Eventos opcionais",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "Você pode ligar a Ponte semântica ao início e ao fim. Assim, categoria e fase são registradas explicitamente sem misturar a lógica dos Canvas internos.",
                MessageType.None
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("onSessionStarted"),
                new GUIContent("Depois de iniciar a sessão")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("onBeforeSessionEnded"),
                new GUIContent("Antes de encerrar a sessão")
            );

            if (Application.isPlaying)
            {
                LudusSessionScope scope = (LudusSessionScope)target;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Sessão iniciada por este escopo",
                    scope.OwnsActiveSession ? "Sim" : "Não"
                );
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
