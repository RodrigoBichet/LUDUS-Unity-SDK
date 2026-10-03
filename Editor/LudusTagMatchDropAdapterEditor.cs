using LudusSDK;
using UnityEditor;
using UnityEngine;

namespace LudusSDK.Editor
{
    [CustomEditor(typeof(LudusTagMatchDropAdapter))]
    public sealed class LudusTagMatchDropAdapterEditor :
        UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Adicione este componente à área que recebe o drop. Ele compara a tag da peça solta com a tag esperada e informa o resultado ao LUDUS. A movimentação e a resposta visual continuam sob responsabilidade do jogo.",
                MessageType.Info
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("trackingEnabled"),
                new GUIContent("Acompanhar este destino")
            );

            SerializedProperty expectedTag =
                serializedObject.FindProperty("expectedTag");
            expectedTag.stringValue = EditorGUILayout.TagField(
                "Tag considerada correta",
                expectedTag.stringValue
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("targetDisplayName"),
                new GUIContent("Nome do destino no Dashboard")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("expectedItemName"),
                new GUIContent(
                    "Resposta esperada no Dashboard",
                    "Deixe vazio para usar automaticamente o texto ou o nome da imagem da opção correta ativa."
                )
            );

            EditorGUILayout.HelpBox(
                "Os nomes apresentados no Dashboard são obtidos do texto visível, do sprite atual ou, como último recurso, do nome do objeto. A tag permanece apenas como regra técnica de comparação.",
                MessageType.None
            );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Eventos enviados",
                EditorStyles.boldLabel
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("recordAttemptEvent"),
                new GUIContent("Registrar tentativa de arraste")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("recordOutcomeEvent"),
                new GUIContent("Registrar acerto ou erro")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("showConsoleWarnings"),
                new GUIContent("Avisar falhas no Console")
            );

            SerializedProperty semanticBridge =
                serializedObject.FindProperty("semanticBridge");
            EditorGUILayout.PropertyField(
                semanticBridge,
                new GUIContent("Ponte semântica compartilhada")
            );

            if (semanticBridge.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "Nenhuma ponte foi indicada. O adaptador tentará usar a única Ponte semântica LUDUS existente nesta cena.",
                    MessageType.Info
                );
            }

            if (
                !serializedObject.FindProperty("recordAttemptEvent").boolValue &&
                !serializedObject.FindProperty("recordOutcomeEvent").boolValue
            )
            {
                EditorGUILayout.HelpBox(
                    "Habilite ao menos um evento para que o adaptador registre o drop.",
                    MessageType.Warning
                );
            }

            EditorGUILayout.HelpBox(
                "Requisitos para UI: a peça deve ser o pointerDrag do EventSystem e o destino precisa receber OnDrop. Este adaptador não cobre arrastes implementados somente por física ou código próprio.",
                MessageType.None
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
