using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudusSDK.Editor
{
    public static class LudusCollectionAccessSetup
    {
        private static readonly Color LudusBlue = new Color32(79, 70, 229, 255);
        private static readonly Color LudusNavy = new Color32(28, 43, 58, 255);
        private static readonly Color LudusCard = new Color32(243, 241, 255, 255);
        private static Font preferredFont;

        [MenuItem(
            "GameObject/LUDUS/Adicionar acesso por nome e código",
            false,
            13
        )]
        private static void CreateCollectionAccess()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog(
                    "LUDUS",
                    "Abra a cena de menu do jogo antes de criar o acesso à coleta.",
                    "Entendi"
                );
                return;
            }

            if (
                UnityEngine.Object.FindFirstObjectByType<
                    LudusCollectionAccessPanel
                >() != null
            )
            {
                EditorUtility.DisplayDialog(
                    "LUDUS",
                    "Esta cena já possui um acesso por nome e código.",
                    "Entendi"
                );
                return;
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Criar acesso à coleta LUDUS");

            EnsureEventSystem();

            GameObject root = CreateUiObject(
                "LUDUS — Acesso à coleta",
                null
            );
            LudusCollectionAccessPanel accessPanel =
                root.AddComponent<LudusCollectionAccessPanel>();

            Button openButton = CreateButton(
                "Abrir acesso",
                root.transform,
                "L",
                LudusBlue,
                36f
            );
            Text openButtonLabel = openButton.GetComponentInChildren<Text>();
            openButtonLabel.fontSize = 38;
            SetAnchoredRect(
                openButton.GetComponent<RectTransform>(),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-28f, -28f),
                new Vector2(72f, 72f)
            );

            GameObject modal = CreateUiObject("Modal", root.transform);
            RectTransform modalRect = modal.GetComponent<RectTransform>();
            Stretch(modalRect);
            Image overlay = modal.AddComponent<Image>();
            overlay.color = new Color32(23, 40, 56, 185);

            GameObject card = CreateUiObject("Conteúdo", modal.transform);
            SetAnchoredRect(
                card.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(580f, 520f)
            );
            LudusRoundedGraphic cardImage = AddRoundedBackground(
                card,
                LudusCard,
                24f
            );

            Text title = CreateText(
                "Título",
                card.transform,
                "LUDUS Acompanha",
                32,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            title.color = LudusBlue;
            SetTopRect(title.rectTransform, -38f, 500f, 58f);

            Text explanation = CreateText(
                "Explicação",
                card.transform,
                "Informe seu nome e o código da coleta.",
                18,
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );
            explanation.color = LudusNavy;
            SetTopRect(explanation.rectTransform, -102f, 500f, 58f);

            InputField nameInput = CreateInput(
                "Nome do participante",
                card.transform,
                "Nome"
            );
            SetTopRect(nameInput.GetComponent<RectTransform>(), -184f, 480f, 58f);

            InputField codeInput = CreateInput(
                "Código da coleta",
                card.transform,
                "Código"
            );
            codeInput.characterLimit = 32;
            SetTopRect(codeInput.GetComponent<RectTransform>(), -258f, 480f, 58f);

            Text status = CreateText(
                "Mensagem",
                card.transform,
                string.Empty,
                18,
                FontStyle.Normal,
                TextAnchor.MiddleCenter
            );
            SetTopRect(status.rectTransform, -326f, 480f, 56f);

            Button connectButton = CreateButton(
                "Conectar",
                card.transform,
                "Entrar",
                LudusBlue,
                12f
            );
            SetAnchoredRect(
                connectButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(100f, 42f),
                new Vector2(210f, 58f)
            );

            Button cancelButton = CreateButton(
                "Cancelar",
                card.transform,
                "Voltar",
                LudusNavy,
                12f
            );
            SetAnchoredRect(
                cancelButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(-130f, 42f),
                new Vector2(210f, 58f)
            );

            accessPanel.Configure(
                openButton,
                modal,
                card.GetComponent<RectTransform>(),
                nameInput,
                codeInput,
                connectButton,
                cancelButton,
                status,
                openButtonLabel
            );
            accessPanel.ConfigureAutomaticSessionStart(
                UnityEngine.Object.FindFirstObjectByType<
                    LudusTutorialScenarioNavigator
                >() != null
            );
            accessPanel.EnsureStatusIndicator();
            modal.SetActive(false);

            Undo.RegisterCreatedObjectUndo(
                root,
                "Criar acesso à coleta LUDUS"
            );
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);

            EditorUtility.DisplayDialog(
                "Acesso à coleta criado",
                "Na cena principal, arraste “LUDUS — Acesso à coleta” para o Canvas em que o usuário fará a conexão com o Dashboard. Depois, posicione “Abrir acesso” onde preferir.",
                "Entendi"
            );
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem)
            );
            Type inputSystemModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
            );
            if (inputSystemModule != null)
            {
                eventSystemObject.AddComponent(inputSystemModule);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
            Undo.RegisterCreatedObjectUndo(
                eventSystemObject,
                "Criar EventSystem LUDUS"
            );
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(
                name,
                typeof(RectTransform)
            );
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }
            return gameObject;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Color color,
            float cornerRadius
        )
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            LudusRoundedGraphic background = AddRoundedBackground(
                buttonObject,
                color,
                cornerRadius
            );
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;

            Text text = CreateText(
                "Texto",
                buttonObject.transform,
                label,
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter
            );
            text.color = Color.white;
            Stretch(text.rectTransform);
            return button;
        }

        private static InputField CreateInput(
            string name,
            Transform parent,
            string placeholderText
        )
        {
            GameObject inputObject = CreateUiObject(name, parent);
            LudusRoundedGraphic background = AddRoundedBackground(
                inputObject,
                Color.white,
                10f
            );
            InputField input = inputObject.AddComponent<InputField>();
            input.targetGraphic = background;

            Text placeholder = CreateText(
                "Placeholder",
                inputObject.transform,
                placeholderText,
                20,
                FontStyle.Italic,
                TextAnchor.MiddleLeft
            );
            placeholder.color = new Color(0.42f, 0.42f, 0.42f, 0.75f);
            SetInputTextRect(placeholder.rectTransform);

            Text value = CreateText(
                "Texto",
                inputObject.transform,
                string.Empty,
                20,
                FontStyle.Normal,
                TextAnchor.MiddleLeft
            );
            value.color = new Color(0.12f, 0.14f, 0.17f, 1f);
            SetInputTextRect(value.rectTransform);

            input.placeholder = placeholder;
            input.textComponent = value;
            return input;
        }

        private static LudusRoundedGraphic AddRoundedBackground(
            GameObject target,
            Color color,
            float cornerRadius
        )
        {
            if (target.GetComponent<CanvasRenderer>() == null)
            {
                target.AddComponent<CanvasRenderer>();
            }

            LudusRoundedGraphic background =
                target.AddComponent<LudusRoundedGraphic>();
            background.color = color;
            background.CornerRadius = cornerRadius;
            return background;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string content,
            int fontSize,
            FontStyle style,
            TextAnchor alignment
        )
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = ResolvePreferredFont();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.10f, 0.13f, 0.18f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Font ResolvePreferredFont()
        {
            if (preferredFont != null)
            {
                return preferredFont;
            }

            string[] candidates = AssetDatabase.FindAssets(
                "Roboto-Regular t:Font"
            );
            if (candidates.Length == 0)
            {
                candidates = AssetDatabase.FindAssets("Roboto t:Font");
            }

            foreach (string guid in candidates)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (font != null)
                {
                    preferredFont = font;
                    return preferredFont;
                }
            }

            preferredFont = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            return preferredFont;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetTopRect(
            RectTransform rect,
            float top,
            float width,
            float height
        )
        {
            SetAnchoredRect(
                rect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, top),
                new Vector2(width, height)
            );
        }

        private static void SetInputTextRect(RectTransform rect)
        {
            Stretch(rect);
            rect.offsetMin = new Vector2(18f, 6f);
            rect.offsetMax = new Vector2(-18f, -6f);
        }

        private static void SetAnchoredRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }
    }
}
