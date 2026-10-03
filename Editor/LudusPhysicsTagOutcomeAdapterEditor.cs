using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    [CustomEditor(typeof(LudusPhysicsTagOutcomeAdapter))]
    public sealed class LudusPhysicsTagOutcomeAdapterEditor :
        UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Use este adaptador quando a regra do jogo considera correto o contato com uma tag conhecida. Ele somente observa OnTriggerEnter ou OnCollisionEnter e não altera a física.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este contato")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("contactMode"),
                new GUIContent("Tipo de contato")
            );

            SerializedProperty expectedTag =
                serializedObject.FindProperty("expectedTag");
            expectedTag.stringValue = EditorGUILayout.TagField(
                "Tag considerada correta",
                expectedTag.stringValue
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("searchTagOnParents"),
                new GUIContent("Procurar tag nos objetos pais")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("expectedItemName"),
                new GUIContent("Resposta esperada no Dashboard")
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

            EditorGUILayout.HelpBox(
                "A configuração física continua sob responsabilidade do jogo: colliders, Is Trigger, Rigidbody/Rigidbody2D, layers e matriz de colisão precisam permitir que o callback escolhido aconteça.",
                MessageType.None
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
