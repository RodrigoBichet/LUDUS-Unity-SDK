using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    [CustomEditor(typeof(LudusProgressThresholdAdapter))]
    public sealed class LudusProgressThresholdAdapterEditor :
        UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Conecte os eventos de pontuação do jogo a AdicionarUm, SubtrairUm, Adicionar, DefinirValor ou AvaliarAgora. O adaptador não procura variáveis privadas nem modifica a pontuação do jogo.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar esta meta")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("currentValue"),
                new GUIContent("Valor inicial")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("targetValue"),
                new GUIContent("Valor da meta")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("comparison"),
                new GUIContent("Condição da meta")
            );

            if (
                (LudusThresholdComparison)serializedObject
                    .FindProperty("comparison")
                    .enumValueIndex == LudusThresholdComparison.Equal
            )
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("equalityTolerance"),
                    new GUIContent("Tolerância de igualdade")
                );
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("action"),
                new GUIContent("Ao atingir a meta")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("resultDisplayName"),
                new GUIContent("Nome exibido no Dashboard")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("recordOnlyOnce"),
                new GUIContent("Registrar somente uma vez")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("showConsoleWarnings"),
                new GUIContent("Avisar falhas no Console")
            );

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("semanticBridge"),
                    new GUIContent("Ponte semântica")
                );
            }

            if (Application.isPlaying)
            {
                LudusProgressThresholdAdapter adapter =
                    (LudusProgressThresholdAdapter)target;
                EditorGUILayout.LabelField(
                    "Valor atual em execução",
                    adapter.CurrentValue.ToString("0.###")
                );
                EditorGUILayout.LabelField(
                    "Resultado já registrado",
                    adapter.ResultAlreadyRecorded ? "Sim" : "Não"
                );
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
