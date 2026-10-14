using System;
using LudusSDK;
using UnityEditor;
using UnityEditorInternal;
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
        private const string TutorialCorrectTag =
            "LARespostaCorretaTutorial";
        private const int TutorialScenarioCount = 6;
        private const int TutorialGeneratedPageCount =
            TutorialScenarioCount + 1;
        private const int TutorialExerciseVersion = 6;

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
                LudusTutorialScenarioNavigator existingNavigator =
                    existingRoot.GetComponent<
                        LudusTutorialScenarioNavigator
                    >();
                bool exerciseNeedsUpdate =
                    existingNavigator == null ||
                    existingNavigator.PageCount !=
                        TutorialGeneratedPageCount ||
                    existingNavigator.GeneratedVersion !=
                        TutorialExerciseVersion;

                if (exerciseNeedsUpdate)
                {
                    bool recreateExercise = EditorUtility.DisplayDialog(
                        "Atualizar exercício tutorial",
                        "Esta cena possui uma versão anterior do exercício. Para aplicar o novo laboratório em tela ampla, o SDK precisa recriar somente o objeto 'LUDUS — Exercício de interações'. As outras cenas e objetos do seu jogo não serão alterados. Configurações feitas manualmente dentro desse objeto tutorial precisarão ser refeitas.",
                        "Recriar exercício",
                        "Manter versão atual"
                    );

                    if (!recreateExercise)
                    {
                        Selection.activeGameObject = existingRoot;
                        EditorGUIUtility.PingObject(existingRoot);
                        return;
                    }

                    Undo.DestroyObjectImmediate(existingRoot);
                    existingRoot = null;
                }
            }

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
                        : "O laboratório guiado já existe nesta cena. Nenhum objeto foi duplicado.",
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
                "Laboratório guiado adicionado",
                "A cena tutorial agora possui seis cenários e uma tela de finalização: interações automáticas, alternativas por botão, arraste com destino, contato por física, meta por valor e ciclo de categoria/fase. "
                    + "Use Anterior e Próximo durante o Play para percorrê-los. Abra LUDUS > Configurar interações desta cena para praticar a seleção dos objetos e siga a documentação para conectar os resultados semânticos. "
                    + "O laboratório fica centralizado. Use o botão L para conectar nome e código da coleta; o painel manual de JSON permanece opcional no componente do tutorial.",
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
            EnsureTagExists(TutorialCorrectTag);

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
                new Vector2(0.04f, 0.035f),
                new Vector2(0.96f, 0.965f)
            );
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.045f, 0.075f, 0.125f, 0.985f);

            GameObject accent = CreateUiObject(
                "Faixa de destaque",
                panel.transform
            );
            RectTransform accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.offsetMin = new Vector2(0f, -7f);
            accentRect.offsetMax = Vector2.zero;
            Image accentImage = accent.AddComponent<Image>();
            accentImage.color = new Color(0.15f, 0.67f, 0.55f);

            CreateText(
                "Título",
                panel.transform,
                "Laboratório guiado do SDK",
                40,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.97f, 1f),
                76f,
                76f,
                30f,
                58f
            );
            CreateText(
                "Instruções",
                panel.transform,
                "Percorra os cenários com Anterior e Próximo. Configure os objetos indicados na documentação antes do Play.",
                22,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Color(0.72f, 0.8f, 0.9f),
                76f,
                76f,
                92f,
                62f
            );

            Text pageIndicator = CreateText(
                "Indicador do cenário",
                panel.transform,
                "Cenário 1 de 6",
                20,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.52f, 0.86f, 0.76f),
                76f,
                76f,
                154f,
                36f
            );

            GameObject interactionPage = CreateScenarioPage(
                "01 — Interações automáticas",
                panel.transform
            );
            LudusTutorialScenarioGate interactionGate =
                CreateAutomaticInteractionScenario(interactionPage.transform);

            GameObject buttonOutcomePage = CreateScenarioPage(
                "02 — Resultados por botão",
                panel.transform
            );
            LudusTutorialScenarioGate buttonOutcomeGate =
                CreateButtonOutcomeScenario(buttonOutcomePage.transform);

            GameObject dropOutcomePage = CreateScenarioPage(
                "03 — Arraste com destino",
                panel.transform
            );
            LudusTutorialScenarioGate dropOutcomeGate =
                CreateDropOutcomeScenario(dropOutcomePage.transform);

            GameObject physicsOutcomePage = CreateScenarioPage(
                "04 — Contato por física",
                panel.transform
            );
            LudusTutorialScenarioGate physicsOutcomeGate =
                CreatePhysicsOutcomeScenario(physicsOutcomePage.transform);

            GameObject progressPage = CreateScenarioPage(
                "05 — Meta por valor",
                panel.transform
            );
            LudusTutorialScenarioGate progressGate =
                CreateProgressScenario(progressPage.transform);

            GameObject semanticFlowPage = CreateScenarioPage(
                "06 — Categoria e fase",
                panel.transform
            );
            LudusTutorialScenarioGate semanticFlowGate =
                CreateSemanticFlowScenario(semanticFlowPage.transform);

            GameObject completionPage = CreateScenarioPage(
                "Finalização do tutorial",
                panel.transform
            );
            CreateCompletionPage(completionPage.transform);

            Text navigationHint = CreateNavigationHint(panel.transform);

            Button previousButton = CreateNavigationButton(
                "Cenário anterior",
                panel.transform,
                "ANTERIOR",
                true
            );
            Button nextButton = CreateNavigationButton(
                "Próximo cenário",
                panel.transform,
                "PRÓXIMO",
                false
            );

            LudusTutorialScenarioNavigator navigator =
                canvasObject.AddComponent<LudusTutorialScenarioNavigator>();
            navigator.Configure(
                new[]
                {
                    interactionPage,
                    buttonOutcomePage,
                    dropOutcomePage,
                    physicsOutcomePage,
                    progressPage,
                    semanticFlowPage,
                    completionPage,
                },
                previousButton,
                nextButton,
                pageIndicator,
                completionPage,
                new[]
                {
                    interactionGate,
                    buttonOutcomeGate,
                    dropOutcomeGate,
                    physicsOutcomeGate,
                    progressGate,
                    semanticFlowGate,
                },
                navigationHint,
                TutorialExerciseVersion
            );
            return canvasObject;
        }

        private static GameObject CreateScenarioPage(
            string name,
            Transform parent
        )
        {
            GameObject page = CreateUiObject(name, parent);
            RectTransform rect = page.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.07f, 0f);
            rect.anchorMax = new Vector2(0.93f, 1f);
            rect.offsetMin = new Vector2(0f, 124f);
            rect.offsetMax = new Vector2(0f, -198f);
            return page;
        }

        private static LudusTutorialScenarioGate CreateAutomaticInteractionScenario(
            Transform parent
        )
        {
            CreateScenarioHeading(
                parent,
                "1. Interações automáticas",
                "Pratique clique, conclusão de campo e arraste livre. Esses três objetos serão usados na janela Interações LUDUS."
            );
            Button button = CreateTutorialButton(
                "BotaoTutorial",
                parent,
                "CLIQUE PARA TESTAR",
                126f,
                new Color(0.15f, 0.67f, 0.55f)
            );
            InputField inputField = CreateInputField(parent, 210f);
            LudusTutorialDraggableItem draggable =
                CreateFreeDragArea(parent, 294f);
            LudusTutorialScenarioGate gate =
                parent.gameObject.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureAutomaticInteractions(
                button,
                inputField,
                draggable,
                "Use o botão, confirme um texto fictício e arraste a peça."
            );
            return gate;
        }

        private static LudusTutorialScenarioGate CreateButtonOutcomeScenario(
            Transform parent
        )
        {
            CreateScenarioHeading(
                parent,
                "2. Miniquiz de alternativas",
                "Resolva uma pergunta textual e depois uma escolha visual. Os mesmos botões poderão informar acertos e erros ao Dashboard."
            );
            CreateText(
                "Pergunta textual",
                parent,
                "Quanto é 1 + 1?",
                30,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                12f,
                12f,
                118f,
                54f
            );
            Button correctButton = CreateTutorialButton(
                "BotaoCorretoTutorial",
                parent,
                "2",
                180f,
                new Color(0.15f, 0.67f, 0.55f)
            );
            Button incorrectButton = CreateTutorialButton(
                "BotaoIncorretoTutorial",
                parent,
                "3",
                258f,
                new Color(0.86f, 0.3f, 0.34f)
            );
            CreateText(
                "Pergunta visual",
                parent,
                "Qual imagem mostra duas formas?",
                20,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                12f,
                12f,
                336f,
                32f
            );
            Button incorrectImageButton = CreateVisualAnswerButton(
                "ImagemUmaFormaTutorial",
                parent,
                374f,
                0f,
                0.5f,
                "OPÇÃO A",
                1
            );
            Button correctImageButton = CreateVisualAnswerButton(
                "ImagemDuasFormasTutorial",
                parent,
                374f,
                0.5f,
                1f,
                "OPÇÃO B",
                2
            );
            Text feedback = CreateText(
                "Feedback do miniquiz",
                parent,
                string.Empty,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.92f, 0.72f, 0.38f),
                12f,
                12f,
                464f,
                46f
            );
            LudusTutorialQuizControls quiz =
                parent.gameObject.AddComponent<LudusTutorialQuizControls>();
            quiz.Configure(
                correctButton,
                incorrectButton,
                correctImageButton,
                incorrectImageButton,
                feedback
            );
            LudusTutorialScenarioGate gate =
                parent.gameObject.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureQuiz(
                quiz,
                "Conclua as alternativas textual e visual."
            );
            return gate;
        }

        private static LudusTutorialScenarioGate CreateDropOutcomeScenario(
            Transform parent
        )
        {
            CreateScenarioHeading(
                parent,
                "3. Arraste com destino",
                "Arraste uma das peças até a área de destino. A peça verde usa a tag exclusiva LARespostaCorretaTutorial; a roxa permanece Untagged para representar uma alternativa incorreta."
            );

            GameObject area = CreateUiObject(
                "Área do cenário de arraste",
                parent
            );
            RectTransform areaRect = area.GetComponent<RectTransform>();
            SetTopAnchored(areaRect, 12f, 12f, 142f, 344f);
            Image areaImage = area.AddComponent<Image>();
            areaImage.color = new Color(0.075f, 0.12f, 0.19f, 0.94f);

            GameObject correctPiece = CreateDraggablePiece(
                "PecaCorretaTutorial",
                area.transform,
                "PEÇA CORRETA",
                new Vector2(-190f, 70f),
                new Color(0.15f, 0.67f, 0.55f),
                TutorialCorrectTag
            );
            GameObject incorrectPiece = CreateDraggablePiece(
                "PecaIncorretaTutorial",
                area.transform,
                "PEÇA INCORRETA",
                new Vector2(-190f, -92f),
                new Color(0.58f, 0.3f, 0.95f),
                "Untagged"
            );

            GameObject target = CreateUiObject(
                "DestinoArrasteTutorial",
                area.transform
            );
            RectTransform targetRect = target.GetComponent<RectTransform>();
            targetRect.anchorMin = new Vector2(0.72f, 0.5f);
            targetRect.anchorMax = new Vector2(0.72f, 0.5f);
            targetRect.pivot = new Vector2(0.5f, 0.5f);
            targetRect.sizeDelta = new Vector2(230f, 250f);
            targetRect.anchoredPosition = Vector2.zero;
            Image targetImage = target.AddComponent<Image>();
            targetImage.color = new Color(0.11f, 0.22f, 0.3f, 0.96f);
            CreateStretchText(
                "Rótulo",
                target.transform,
                "SOLTE AQUI",
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.72f, 0.9f, 1f),
                18f
            );
            Text feedback = CreateText(
                "Feedback do arraste",
                parent,
                "Faça uma tentativa para continuar.",
                17,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.92f, 0.72f, 0.38f),
                12f,
                12f,
                494f,
                38f
            );
            LudusTutorialAttemptTarget attemptTarget =
                target.AddComponent<LudusTutorialAttemptTarget>();
            attemptTarget.Configure(
                LudusTutorialAttemptMode.UiDrop,
                TutorialCorrectTag,
                feedback,
                new[]
                {
                    correctPiece.GetComponent<LudusTutorialDraggableItem>(),
                    incorrectPiece.GetComponent<LudusTutorialDraggableItem>(),
                }
            );
            LudusTutorialScenarioGate gate =
                parent.gameObject.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureAttempt(
                attemptTarget,
                "Leve a peça correta até a área SOLTE AQUI."
            );
            return gate;
        }

        private static LudusTutorialScenarioGate CreatePhysicsOutcomeScenario(
            Transform parent
        )
        {
            CreateScenarioHeading(
                parent,
                "4. Contato por física 2D",
                "Arraste uma peça até a zona de contato. Este cenário permite praticar Trigger2D sem depender da lógica de uma cena real."
            );

            GameObject area = CreateUiObject(
                "Área do cenário de contato",
                parent
            );
            RectTransform areaRect = area.GetComponent<RectTransform>();
            SetTopAnchored(areaRect, 12f, 12f, 142f, 344f);
            Image areaImage = area.AddComponent<Image>();
            areaImage.color = new Color(0.075f, 0.12f, 0.19f, 0.94f);

            GameObject correctPiece = CreateDraggablePiece(
                "PecaContatoCorretaTutorial",
                area.transform,
                "CONTATO CORRETO",
                new Vector2(-190f, 70f),
                new Color(0.15f, 0.67f, 0.55f),
                TutorialCorrectTag
            );
            PreparePhysicsPiece(correctPiece);

            GameObject incorrectPiece = CreateDraggablePiece(
                "PecaContatoIncorretaTutorial",
                area.transform,
                "CONTATO INCORRETO",
                new Vector2(-190f, -92f),
                new Color(0.86f, 0.3f, 0.34f),
                "Untagged"
            );
            PreparePhysicsPiece(incorrectPiece);

            GameObject target = CreateUiObject(
                "DestinoContatoTutorial",
                area.transform
            );
            RectTransform targetRect = target.GetComponent<RectTransform>();
            targetRect.anchorMin = new Vector2(0.72f, 0.5f);
            targetRect.anchorMax = new Vector2(0.72f, 0.5f);
            targetRect.pivot = new Vector2(0.5f, 0.5f);
            targetRect.sizeDelta = new Vector2(230f, 250f);
            targetRect.anchoredPosition = Vector2.zero;
            Image targetImage = target.AddComponent<Image>();
            targetImage.color = new Color(0.18f, 0.24f, 0.12f, 0.96f);
            BoxCollider2D targetCollider = target.AddComponent<BoxCollider2D>();
            targetCollider.size = targetRect.sizeDelta;
            targetCollider.isTrigger = true;
            CreateStretchText(
                "Rótulo",
                target.transform,
                "ZONA DE CONTATO",
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.82f, 0.95f, 0.7f),
                18f
            );
            Text feedback = CreateText(
                "Feedback do contato",
                parent,
                "Faça uma tentativa para continuar.",
                17,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.92f, 0.72f, 0.38f),
                12f,
                12f,
                494f,
                38f
            );
            LudusTutorialAttemptTarget attemptTarget =
                target.AddComponent<LudusTutorialAttemptTarget>();
            attemptTarget.Configure(
                LudusTutorialAttemptMode.Physics2D,
                TutorialCorrectTag,
                feedback,
                new[]
                {
                    correctPiece.GetComponent<LudusTutorialDraggableItem>(),
                    incorrectPiece.GetComponent<LudusTutorialDraggableItem>(),
                }
            );
            LudusTutorialScenarioGate gate =
                parent.gameObject.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureAttempt(
                attemptTarget,
                "Leve a peça correta até a zona de contato."
            );
            return gate;
        }

        private static LudusTutorialScenarioGate CreateProgressScenario(
            Transform parent
        )
        {
            CreateScenarioHeading(
                parent,
                "5. Meta por valor ou pontuação",
                "Use os controles para chegar ao valor 3. Depois de configurar o adaptador, a mesma atualização visual também informará a meta ao LUDUS."
            );

            GameObject controller = CreateUiObject(
                "ControladorMetaTutorial",
                parent
            );
            RectTransform controllerRect =
                controller.GetComponent<RectTransform>();
            SetTopAnchored(controllerRect, 12f, 12f, 142f, 344f);
            Image background = controller.AddComponent<Image>();
            background.color = new Color(0.075f, 0.12f, 0.19f, 0.94f);

            Text valueLabel = CreateText(
                "Valor da meta",
                controller.transform,
                "Valor atual: 0 / Meta: 3",
                28,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.82f, 0.95f, 0.7f),
                24f,
                24f,
                24f,
                58f
            );
            Button increase = CreateTutorialButton(
                "AdicionarPontoTutorial",
                controller.transform,
                "+ ADICIONAR 1",
                108f,
                new Color(0.15f, 0.67f, 0.55f)
            );
            Button decrease = CreateTutorialButton(
                "SubtrairPontoTutorial",
                controller.transform,
                "− SUBTRAIR 1",
                194f,
                new Color(0.86f, 0.48f, 0.18f)
            );
            Button reset = CreateTutorialButton(
                "ReiniciarMetaTutorial",
                controller.transform,
                "REINICIAR",
                280f,
                new Color(0.3f, 0.4f, 0.55f)
            );

            LudusTutorialProgressControls controls =
                controller.AddComponent<LudusTutorialProgressControls>();
            controls.Configure(increase, decrease, reset, valueLabel);
            LudusTutorialScenarioGate gate =
                parent.gameObject.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureProgress(
                controls,
                3,
                "Use ADICIONAR 1 até alcançar a meta 3."
            );
            return gate;
        }

        private static LudusTutorialScenarioGate CreateSemanticFlowScenario(
            Transform parent
        )
        {
            CreateScenarioHeading(
                parent,
                "6. Categoria e ciclo de fase",
                "Complete um pequeno ciclo de atividade. Cada etapa representa um evento que somente o próprio jogo conhece."
            );

            GameObject controller = CreateUiObject(
                "ControladorFluxoSemanticoTutorial",
                parent
            );
            RectTransform controllerRect =
                controller.GetComponent<RectTransform>();
            SetTopAnchored(controllerRect, 12f, 12f, 132f, 420f);
            Image background = controller.AddComponent<Image>();
            background.color = new Color(0.075f, 0.12f, 0.19f, 0.94f);

            Text flowProgress = CreateText(
                "Progresso do ciclo",
                controller.transform,
                "○ Categoria  →  ○ Início  →  ○ Resultado  →  ○ Conclusão",
                17,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.72f, 0.8f, 0.9f),
                18f,
                18f,
                14f,
                54f
            );

            Button categoryButton = CreateTutorialButton(
                "SelecionarCategoriaTutorial",
                controller.transform,
                "ESCOLHER: DESAFIO DE FORMAS",
                82f,
                new Color(0.34f, 0.28f, 0.82f)
            );
            Button phaseStartButton = CreateTutorialButton(
                "IniciarFaseTutorial",
                controller.transform,
                "COMEÇAR ATIVIDADE",
                160f,
                new Color(0.16f, 0.52f, 0.78f)
            );
            Button resultButton = CreateTutorialButton(
                "RegistrarAcertoTutorial",
                controller.transform,
                "RESPONDI CORRETAMENTE",
                238f,
                new Color(0.15f, 0.67f, 0.55f)
            );
            Button phaseEndButton = CreateTutorialButton(
                "ConcluirFaseTutorial",
                controller.transform,
                "CONCLUIR E VER RESUMO",
                316f,
                new Color(0.86f, 0.48f, 0.18f)
            );
            LudusTutorialScenarioGate gate =
                parent.gameObject.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureOrderedButtons(
                new[]
                {
                    categoryButton,
                    phaseStartButton,
                    resultButton,
                    phaseEndButton,
                },
                "Execute categoria, início, resultado e conclusão nesta ordem."
            );
            LudusTutorialSemanticFlowControls flowControls =
                controller.AddComponent<LudusTutorialSemanticFlowControls>();
            flowControls.Configure(
                new[]
                {
                    categoryButton,
                    phaseStartButton,
                    resultButton,
                    phaseEndButton,
                },
                flowProgress
            );
            return gate;
        }

        private static void CreateCompletionPage(Transform parent)
        {
            CreateScenarioHeading(
                parent,
                "Tutorial concluído",
                "Você percorreu os seis cenários. Quando o acesso foi feito por nome e código, a sessão é encerrada e enviada automaticamente ao LUDUS Acompanha."
            );

            GameObject notice = CreateUiObject(
                "Orientação para encerrar a sessão",
                parent
            );
            RectTransform noticeRect = notice.GetComponent<RectTransform>();
            SetTopAnchored(noticeRect, 12f, 12f, 150f, 300f);
            Image noticeImage = notice.AddComponent<Image>();
            noticeImage.color = new Color(0.08f, 0.24f, 0.2f, 0.96f);

            CreateStretchText(
                "Mensagem final",
                notice.transform,
                "1. Confira abaixo se o envio automático foi iniciado.\n\n2. Volte ao LUDUS Acompanha e abra a coleta utilizada.\n\n3. O painel manual de JSON é opcional e pode ser ativado no componente do tutorial.",
                22,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.9f, 1f, 0.96f),
                28f
            );

            CreateText(
                "Aviso sobre o envio",
                parent,
                "A conexão por nome e código envia a sessão diretamente. Use Anterior apenas se quiser revisar os cenários.",
                19,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Color(0.82f, 0.88f, 0.95f),
                12f,
                12f,
                472f,
                70f
            );
        }

        private static void PreparePhysicsPiece(GameObject piece)
        {
            RectTransform rect = piece.GetComponent<RectTransform>();
            BoxCollider2D collider = piece.AddComponent<BoxCollider2D>();
            collider.size = rect.sizeDelta;
            Rigidbody2D body = piece.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private static void CreateScenarioHeading(
            Transform parent,
            string title,
            string instructions
        )
        {
            CreateText(
                "Título do cenário",
                parent,
                title,
                30,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.97f, 1f),
                12f,
                12f,
                8f,
                46f
            );
            CreateText(
                "Instruções do cenário",
                parent,
                instructions,
                21,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Color(0.72f, 0.8f, 0.9f),
                12f,
                12f,
                58f,
                82f
            );
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

        private static Button CreateTutorialButton(
            string objectName,
            Transform parent,
            string label,
            float top,
            Color color
        )
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetTopAnchored(rect, 12f, 12f, top, 76f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            colors.disabledColor = new Color(0.035f, 0.055f, 0.085f, 1f);
            button.colors = colors;

            CreateStretchText(
                "Rótulo",
                buttonObject.transform,
                label,
                25,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                16f
            );
            return button;
        }

        private static Button CreateVisualAnswerButton(
            string objectName,
            Transform parent,
            float top,
            float minimumAnchorX,
            float maximumAnchorX,
            string optionLabel,
            int shapeCount
        )
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(minimumAnchorX, 1f);
            rect.anchorMax = new Vector2(maximumAnchorX, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(12f, -top - 80f);
            rect.offsetMax = new Vector2(-12f, -top);

            Image background = buttonObject.AddComponent<Image>();
            background.color = new Color(0.16f, 0.52f, 0.78f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.25f, 0.64f, 0.88f);
            colors.pressedColor = new Color(0.1f, 0.36f, 0.58f);
            colors.disabledColor = new Color(0.11f, 0.2f, 0.29f, 1f);
            button.colors = colors;

            CreateText(
                "Nome da opção",
                buttonObject.transform,
                optionLabel,
                16,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                12f,
                12f,
                4f,
                22f
            );
            for (int index = 0; index < shapeCount; index++)
            {
                float positionX = shapeCount == 1 ? 0f : index == 0 ? -24f : 24f;
                CreateVisualShape(
                    $"Forma visual {index + 1}",
                    buttonObject.transform,
                    new Vector2(positionX, -14f),
                    index == 0
                        ? new Color(0.98f, 0.72f, 0.24f)
                        : new Color(0.52f, 0.86f, 0.76f)
                );
            }
            return button;
        }

        private static void CreateVisualShape(
            string name,
            Transform parent,
            Vector2 position,
            Color color
        )
        {
            GameObject shape = CreateUiObject(name, parent);
            RectTransform rect = shape.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(44f, 44f);
            rect.anchoredPosition = position;
            LudusRoundedGraphic graphic =
                shape.AddComponent<LudusRoundedGraphic>();
            graphic.CornerRadius = 22f;
            graphic.CornerSegments = 10;
            graphic.color = color;
            graphic.raycastTarget = false;
        }

        private static Button CreateNavigationButton(
            string objectName,
            Transform parent,
            string label,
            bool previous
        )
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = previous
                ? new Vector2(0.07f, 0f)
                : new Vector2(0.5f, 0f);
            rect.anchorMax = previous
                ? new Vector2(0.5f, 0f)
                : new Vector2(0.93f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(previous ? 0f : 10f, 30f);
            rect.offsetMax = new Vector2(previous ? -10f : 0f, 100f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.17f, 0.25f, 0.36f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.24f, 0.36f, 0.5f);
            colors.pressedColor = new Color(0.12f, 0.19f, 0.28f);
            colors.disabledColor = new Color(0.12f, 0.17f, 0.24f, 1f);
            button.colors = colors;

            CreateStretchText(
                "Rótulo",
                buttonObject.transform,
                label,
                21,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                12f
            );
            return button;
        }

        private static Text CreateNavigationHint(Transform parent)
        {
            GameObject hintObject = CreateUiObject(
                "Orientação para avançar",
                parent
            );
            RectTransform rect = hintObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(0f, 110f);
            rect.offsetMax = new Vector2(0f, 154f);
            rect.anchorMin = new Vector2(0.07f, 0f);
            rect.anchorMax = new Vector2(0.93f, 0f);

            Text text = hintObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.92f, 0.72f, 0.38f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = "Conclua a atividade para avançar.";
            return text;
        }

        private static InputField CreateInputField(
            Transform parent,
            float top
        )
        {
            GameObject fieldObject = CreateUiObject("CampoTextoTutorial", parent);
            RectTransform rect = fieldObject.GetComponent<RectTransform>();
            SetTopAnchored(rect, 12f, 12f, top, 76f);

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
            return inputField;
        }

        private static LudusTutorialDraggableItem CreateFreeDragArea(
            Transform parent,
            float top
        )
        {
            GameObject area = CreateUiObject("Área de arraste livre", parent);
            RectTransform areaRect = area.GetComponent<RectTransform>();
            SetTopAnchored(areaRect, 12f, 12f, top, 216f);
            Image areaImage = area.AddComponent<Image>();
            areaImage.color = new Color(0.085f, 0.135f, 0.21f, 0.92f);

            CreateText(
                "Orientação do arraste",
                area.transform,
                "Arraste a peça dentro desta área",
                18,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.72f, 0.82f, 0.94f),
                24f,
                24f,
                12f,
                36f
            );

            GameObject piece = CreateDraggablePiece(
                "PecaArrastavelTutorial",
                area.transform,
                "ARRASTE",
                new Vector2(0f, -30f),
                new Color(0.58f, 0.3f, 0.95f),
                "Untagged"
            );
            return piece.GetComponent<LudusTutorialDraggableItem>();
        }

        private static GameObject CreateDraggablePiece(
            string objectName,
            Transform parent,
            string label,
            Vector2 position,
            Color color,
            string tag
        )
        {
            GameObject piece = CreateUiObject(objectName, parent);
            RectTransform pieceRect = piece.GetComponent<RectTransform>();
            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.pivot = new Vector2(0.5f, 0.5f);
            pieceRect.sizeDelta = new Vector2(154f, 112f);
            pieceRect.anchoredPosition = position;
            piece.tag = tag;

            Image pieceImage = piece.AddComponent<Image>();
            pieceImage.color = color;
            LudusTutorialDraggableItem draggable =
                piece.AddComponent<LudusTutorialDraggableItem>();
            draggable.RememberInitialPosition();
            CreateStretchText(
                "Rótulo",
                piece.transform,
                label,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                10f
            );
            return piece;
        }

        private static void EnsureTagExists(string tagName)
        {
            if (Array.IndexOf(InternalEditorUtility.tags, tagName) >= 0)
            {
                return;
            }

            UnityEngine.Object[] tagManagerAssets =
                AssetDatabase.LoadAllAssetsAtPath(
                    "ProjectSettings/TagManager.asset"
                );
            if (tagManagerAssets.Length == 0)
            {
                throw new InvalidOperationException(
                    "Não foi possível localizar ProjectSettings/TagManager.asset para criar a tag exclusiva do tutorial."
                );
            }

            SerializedObject tagManager = new SerializedObject(
                tagManagerAssets[0]
            );
            SerializedProperty tags = tagManager.FindProperty("tags");
            int newIndex = tags.arraySize;
            tags.InsertArrayElementAtIndex(newIndex);
            tags.GetArrayElementAtIndex(newIndex).stringValue = tagName;
            tagManager.ApplyModifiedProperties();
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
