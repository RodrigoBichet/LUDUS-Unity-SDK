using System;
using LudusSDK;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudusSDK.Editor
{
    internal static class LudusTutorialExerciseBuilder
    {
        private const string ExerciseRootName =
            "LUDUS — Exercício de interações";

        internal static void AddToTutorial(string tutorialScenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(tutorialScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Tutorial LUDUS",
                    "Crie primeiro a cena em LUDUS > Criar tutorial de teste do SDK.",
                    "Entendi"
                );
                return;
            }

            if (SceneManager.GetActiveScene().path != tutorialScenePath)
            {
                bool openTutorial = EditorUtility.DisplayDialog(
                    "Tutorial LUDUS",
                    "O exercício será adicionado somente à cena tutorial isolada. Deseja abri-la agora?",
                    "Abrir e continuar",
                    "Cancelar"
                );

                if (
                    !openTutorial ||
                    !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
                )
                {
                    return;
                }

                EditorSceneManager.OpenScene(tutorialScenePath);
            }

            bool cameraCreated = EnsureTutorialCamera();
            GameObject existingRoot = GameObject.Find(ExerciseRootName);

            if (existingRoot != null)
            {
                bool editorBridgeAdded = EnsureEditorPointerBridge(
                    existingRoot
                );

                if (cameraCreated || editorBridgeAdded)
                {
                    EditorSceneManager.MarkSceneDirty(
                        SceneManager.GetActiveScene()
                    );
                    EditorSceneManager.SaveScene(
                        SceneManager.GetActiveScene(),
                        tutorialScenePath
                    );
                }

                Selection.activeGameObject = existingRoot;
                EditorGUIUtility.PingObject(existingRoot);
                EditorUtility.DisplayDialog(
                    "Tutorial LUDUS",
                    cameraCreated || editorBridgeAdded
                        ? "O exercício já existia e não foi duplicado. A compatibilidade da Game View no Editor foi atualizada."
                        : "O exercício de interações já existe nesta cena. Nenhum objeto foi duplicado.",
                    "Entendi"
                );
                return;
            }

            GameObject exerciseRoot = CreateExerciseCanvas();
            EnsureEventSystem();
            Undo.RegisterCreatedObjectUndo(
                exerciseRoot,
                "Adicionar exercício de interações LUDUS"
            );

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(
                SceneManager.GetActiveScene(),
                tutorialScenePath
            );
            Selection.activeGameObject = exerciseRoot;
            EditorGUIUtility.PingObject(exerciseRoot);
            EditorUtility.DisplayDialog(
                "Exercício adicionado",
                "A cena tutorial agora possui um botão, um campo de texto e uma peça arrastável. "
                    + "Abra LUDUS > Configurar interações desta cena e arraste cada um desses objetos da Hierarchy para o campo 'Arraste um objeto aqui'. "
                    + "O painel do exercício fica à direita de propósito, pois o painel da sessão aparecerá à esquerda durante o Play.",
                "Continuar"
            );
        }

        internal static bool EnsureTutorialCamera()
        {
            if (UnityEngine.Object.FindFirstObjectByType<Camera>() != null)
            {
                return false;
            }

            GameObject cameraObject = new GameObject(
                "Câmera do tutorial LUDUS",
                typeof(Camera)
            );
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera tutorialCamera = cameraObject.GetComponent<Camera>();
            tutorialCamera.orthographic = true;
            tutorialCamera.clearFlags = CameraClearFlags.SolidColor;
            tutorialCamera.backgroundColor = new Color(
                0.012f,
                0.022f,
                0.038f
            );
            tutorialCamera.cullingMask = 0;
            tutorialCamera.depth = -100f;
            return true;
        }

        private static GameObject CreateExerciseCanvas()
        {
            GameObject canvasObject = new GameObject(
                ExerciseRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEditorPointerBridge(canvasObject);

            GameObject panel = CreateUiObject("Painel do exercício", canvasObject.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetAnchors(
                panelRect,
                new Vector2(0.52f, 0.08f),
                new Vector2(0.96f, 0.92f)
            );
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.055f, 0.09f, 0.145f, 0.97f);

            CreateText(
                "Título",
                panel.transform,
                "Exercício opcional de interações",
                34,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.97f, 1f),
                42f,
                42f,
                30f,
                52f
            );
            CreateText(
                "Instruções",
                panel.transform,
                "Inicie a sessão no painel LUDUS. Depois clique no botão, conclua o campo e arraste a peça. "
                    + "Antes do Play, arraste os três objetos da Hierarchy para a janela Interações LUDUS.",
                21,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Color(0.72f, 0.8f, 0.9f),
                42f,
                42f,
                94f,
                94f
            );

            CreateButton(panel.transform);
            CreateInputField(panel.transform);
            CreateDragArea(panel.transform);
            return canvasObject;
        }

        private static bool EnsureEditorPointerBridge(GameObject exerciseRoot)
        {
            if (
                exerciseRoot.GetComponent<LudusTutorialEditorPointerBridge>() !=
                null
            )
            {
                return false;
            }

            exerciseRoot.AddComponent<LudusTutorialEditorPointerBridge>();
            return true;
        }

        private static void CreateButton(Transform parent)
        {
            GameObject buttonObject = CreateUiObject("BotaoTutorial", parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetTopAnchored(rect, 42f, 42f, 202f, 68f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.67f, 0.55f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.2f, 0.78f, 0.65f);
            colors.pressedColor = new Color(0.1f, 0.5f, 0.42f);
            button.colors = colors;

            CreateStretchText(
                "Rótulo",
                buttonObject.transform,
                "CLIQUE PARA TESTAR",
                23,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                16f
            );
        }

        private static void CreateInputField(Transform parent)
        {
            GameObject fieldObject = CreateUiObject("CampoTextoTutorial", parent);
            RectTransform rect = fieldObject.GetComponent<RectTransform>();
            SetTopAnchored(rect, 42f, 42f, 290f, 68f);

            Image background = fieldObject.AddComponent<Image>();
            background.color = new Color(0.96f, 0.97f, 0.99f);
            InputField inputField = fieldObject.AddComponent<InputField>();
            inputField.targetGraphic = background;

            Text placeholder = CreateStretchText(
                "Placeholder",
                fieldObject.transform,
                "Digite uma resposta fictícia",
                21,
                FontStyle.Italic,
                TextAnchor.MiddleLeft,
                new Color(0.42f, 0.47f, 0.55f),
                18f
            );
            Text valueText = CreateStretchText(
                "Texto",
                fieldObject.transform,
                string.Empty,
                21,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Color(0.08f, 0.12f, 0.18f),
                18f
            );
            inputField.placeholder = placeholder;
            inputField.textComponent = valueText;
        }

        private static void CreateDragArea(Transform parent)
        {
            GameObject area = CreateUiObject("Área de arraste", parent);
            RectTransform areaRect = area.GetComponent<RectTransform>();
            SetTopAnchored(areaRect, 42f, 42f, 382f, 420f);
            Image areaImage = area.AddComponent<Image>();
            areaImage.color = new Color(0.085f, 0.135f, 0.21f, 0.92f);

            CreateText(
                "Orientação do arraste",
                area.transform,
                "Arraste a peça dentro desta área",
                21,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.72f, 0.82f, 0.94f),
                24f,
                24f,
                20f,
                44f
            );

            GameObject piece = CreateUiObject("PecaArrastavelTutorial", area.transform);
            RectTransform pieceRect = piece.GetComponent<RectTransform>();
            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.pivot = new Vector2(0.5f, 0.5f);
            pieceRect.sizeDelta = new Vector2(170f, 170f);
            pieceRect.anchoredPosition = new Vector2(0f, -20f);

            Image pieceImage = piece.AddComponent<Image>();
            pieceImage.color = new Color(0.58f, 0.3f, 0.95f);
            piece.AddComponent<LudusTutorialDraggableItem>();
            CreateStretchText(
                "Rótulo",
                piece.transform,
                "ARRASTE",
                21,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                10f
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
            Type inputSystemModuleType = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
            );

            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer)
            );
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string content,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color,
            float left,
            float right,
            float top,
            float height
        )
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            SetTopAnchored(rect, left, right, top, height);
            Text text = textObject.AddComponent<Text>();
            ConfigureText(
                text,
                content,
                fontSize,
                fontStyle,
                alignment,
                color
            );
            return text;
        }

        private static Text CreateStretchText(
            string name,
            Transform parent,
            string content,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color,
            float horizontalPadding
        )
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontalPadding, 6f);
            rect.offsetMax = new Vector2(-horizontalPadding, -6f);
            Text text = textObject.AddComponent<Text>();
            ConfigureText(
                text,
                content,
                fontSize,
                fontStyle,
                alignment,
                color
            );
            return text;
        }

        private static void ConfigureText(
            Text text,
            string content,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color
        )
        {
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static void SetAnchors(
            RectTransform rect,
            Vector2 minimum,
            Vector2 maximum
        )
        {
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetTopAnchored(
            RectTransform rect,
            float left,
            float right,
            float top,
            float height
        )
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
