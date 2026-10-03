using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    [CustomEditor(typeof(LudusButtonOutcomeAdapter))]
    public sealed class LudusButtonOutcomeAdapterEditor :
        UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Use em uma alternativa cuja classificação já é conhecida. O adaptador escuta Button.onClick sem remover as ações existentes do jogo.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar esta alternativa")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("configuredOutcome"),
                new GUIContent("Resultado desta alternativa")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("answerDisplayName"),
                new GUIContent("Resposta exibida no Dashboard")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(
                    "expectedAnswerDisplayName"
                ),
                new GUIContent("Resposta esperada no Dashboard")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("showConsoleWarnings"),
                new GUIContent("Avisar falhas no Console")
            );

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("observedButton"),
                    new GUIContent("Botão observado")
                );
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("semanticBridge"),
                    new GUIContent("Ponte semântica")
                );
            }

            EditorGUILayout.HelpBox(
                "Este componente registra o resultado configurado quando o botão é acionado. Ele não verifica se o conteúdo da alternativa está correto e não altera a navegação do jogo.",
                MessageType.None
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
