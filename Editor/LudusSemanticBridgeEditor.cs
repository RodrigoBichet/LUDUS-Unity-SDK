using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    [CustomEditor(typeof(LudusSemanticBridge))]
    public sealed class LudusSemanticBridgeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Esta ponte não tenta descobrir as regras do jogo. Vincule os eventos que já representam acerto, erro ou progressão aos métodos públicos abaixo.",
                MessageType.Info
            );

            DrawGeneralSettings();
            EditorGUILayout.Space();
            DrawCategorySettings();
            EditorGUILayout.Space();
            DrawPhaseSettings();
            EditorGUILayout.Space();
            DrawResultSettings();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Em um UnityEvent, arraste este objeto e escolha LudusSemanticBridge > RegistrarAcerto, RegistrarErro, RegistrarInicioDeFase ou RegistrarConclusaoDeFase. Para tentativas de arraste, vincule também o método correspondente.",
                MessageType.None
            );

            if (Application.isPlaying)
            {
                LudusSemanticBridge bridge =
                    (LudusSemanticBridge)target;
                EditorGUILayout.LabelField(
                    "Acertos nesta fase",
                    bridge.PhaseCorrectCount.ToString()
                );
                EditorGUILayout.LabelField(
                    "Erros nesta fase",
                    bridge.PhaseWrongCount.ToString()
                );
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField(
                "Funcionamento",
                EditorStyles.boldLabel
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("semanticTrackingEnabled"),
                new GUIContent("Registrar eventos semânticos")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("showConsoleWarnings"),
                new GUIContent("Avisar falhas no Console")
            );
        }

        private void DrawCategorySettings()
        {
            EditorGUILayout.LabelField("Categoria", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("categoryName"),
                new GUIContent("Nome da categoria")
            );
        }

        private void DrawPhaseSettings()
        {
            EditorGUILayout.LabelField("Fase ou etapa", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("phaseId"),
                new GUIContent("Identificador da fase")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("targetItem"),
                new GUIContent("Objetivo ou destino")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("phaseOptions"),
                new GUIContent("Opções apresentadas"),
                true
            );
        }

        private void DrawResultSettings()
        {
            EditorGUILayout.LabelField("Resultado", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("resultItem"),
                new GUIContent("Item ou resposta informada")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("expectedItem"),
                new GUIContent("Item ou resposta esperada")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("completionStars"),
                new GUIContent("Estrelas ao concluir")
            );
        }
    }
}
