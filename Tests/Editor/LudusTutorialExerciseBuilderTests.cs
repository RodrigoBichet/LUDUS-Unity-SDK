using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialExerciseBuilderTests
    {
        [Test]
        public void Gerador_CriaSeisCenariosComObjetosEsperados()
        {
            Type builderType = Type.GetType(
                "LudusSDK.Editor.LudusTutorialExerciseBuilder, Ludus.Unity.Editor"
            );
            MethodInfo createExercise = builderType?.GetMethod(
                "CreateExerciseCanvas",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.That(builderType, Is.Not.Null);
            Assert.That(createExercise, Is.Not.Null);

            GameObject root = (GameObject)createExercise.Invoke(null, null);

            Assert.That(root, Is.Not.Null);
            Assert.That(
                root.GetComponent<LudusTutorialScenarioNavigator>(),
                Is.Not.Null
            );
            Assert.That(
                root.GetComponent<LudusTutorialScenarioNavigator>().PageCount,
                Is.EqualTo(7)
            );
            Assert.That(
                root.GetComponent<LudusTutorialScenarioNavigator>()
                    .ScenarioCount,
                Is.EqualTo(6)
            );
            Assert.That(
                root.GetComponent<LudusTutorialScenarioNavigator>()
                    .GeneratedVersion,
                Is.EqualTo(6)
            );
            Assert.That(
                root.GetComponentsInChildren<
                    LudusTutorialScenarioGate
                >(true).Length,
                Is.EqualTo(6)
            );

            AssertChildExists(
                root,
                "Painel do exercício/01 — Interações automáticas/BotaoTutorial"
            );
            AssertChildExists(
                root,
                "Painel do exercício/01 — Interações automáticas/CampoTextoTutorial"
            );
            AssertChildExists(
                root,
                "Painel do exercício/02 — Resultados por botão/BotaoCorretoTutorial"
            );
            AssertChildExists(
                root,
                "Painel do exercício/02 — Resultados por botão/BotaoIncorretoTutorial"
            );
            AssertChildExists(
                root,
                "Painel do exercício/02 — Resultados por botão/ImagemUmaFormaTutorial"
            );
            AssertChildExists(
                root,
                "Painel do exercício/02 — Resultados por botão/ImagemDuasFormasTutorial/Forma visual 1"
            );
            AssertChildExists(
                root,
                "Painel do exercício/02 — Resultados por botão/ImagemDuasFormasTutorial/Forma visual 2"
            );
            Assert.That(
                root.transform.Find(
                    "Painel do exercício/02 — Resultados por botão"
                ).GetComponent<LudusTutorialQuizControls>(),
                Is.Not.Null
            );

            Transform correctPiece = AssertChildExists(
                root,
                "Painel do exercício/03 — Arraste com destino/Área do cenário de arraste/PecaCorretaTutorial"
            );
            Transform incorrectPiece = AssertChildExists(
                root,
                "Painel do exercício/03 — Arraste com destino/Área do cenário de arraste/PecaIncorretaTutorial"
            );
            Transform dropTarget = AssertChildExists(
                root,
                "Painel do exercício/03 — Arraste com destino/Área do cenário de arraste/DestinoArrasteTutorial"
            );
            Transform physicsTarget = AssertChildExists(
                root,
                "Painel do exercício/04 — Contato por física/Área do cenário de contato/DestinoContatoTutorial"
            );
            Transform progressController = AssertChildExists(
                root,
                "Painel do exercício/05 — Meta por valor/ControladorMetaTutorial"
            );
            AssertChildExists(
                root,
                "Painel do exercício/06 — Categoria e fase/ControladorFluxoSemanticoTutorial/ConcluirFaseTutorial"
            );
            Assert.That(
                root.transform.Find(
                    "Painel do exercício/06 — Categoria e fase/ControladorFluxoSemanticoTutorial"
                ).GetComponent<LudusTutorialSemanticFlowControls>(),
                Is.Not.Null
            );
            AssertChildExists(
                root,
                "Painel do exercício/Finalização do tutorial/Orientação para encerrar a sessão"
            );
            AssertChildExists(
                root,
                "Painel do exercício/Finalização do tutorial/Aviso sobre o envio"
            );

            Assert.That(
                correctPiece.gameObject.tag,
                Is.EqualTo("LARespostaCorretaTutorial")
            );
            Assert.That(incorrectPiece.gameObject.tag, Is.EqualTo("Untagged"));
            Assert.That(
                physicsTarget.GetComponent<BoxCollider2D>().isTrigger,
                Is.True
            );
            Assert.That(
                dropTarget.GetComponent<LudusTutorialAttemptTarget>(),
                Is.Not.Null
            );
            Assert.That(
                physicsTarget.GetComponent<LudusTutorialAttemptTarget>(),
                Is.Not.Null
            );
            Assert.That(
                progressController.GetComponent<
                    LudusTutorialProgressControls
                >(),
                Is.Not.Null
            );

            RectTransform previous = AssertChildExists(
                root,
                "Painel do exercício/Cenário anterior"
            ).GetComponent<RectTransform>();
            RectTransform next = AssertChildExists(
                root,
                "Painel do exercício/Próximo cenário"
            ).GetComponent<RectTransform>();
            Assert.That(previous.anchorMax.x, Is.EqualTo(0.5f));
            Assert.That(next.anchorMin.x, Is.EqualTo(0.5f));

            RectTransform panel = AssertChildExists(
                root,
                "Painel do exercício"
            ).GetComponent<RectTransform>();
            Assert.That(panel.anchorMin.x, Is.EqualTo(0.04f));
            Assert.That(panel.anchorMax.x, Is.EqualTo(0.96f));
            Assert.That(panel.anchorMin.y, Is.EqualTo(0.035f));
            Assert.That(panel.anchorMax.y, Is.EqualTo(0.965f));
            AssertChildExists(root, "Painel do exercício/Faixa de destaque");

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void ModoSimples_CriaFluxoObservacionalSemConfiguracaoManual()
        {
            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de teste";

            Type setupType = Type.GetType(
                "LudusSDK.Editor.LudusSetupAssistant, Ludus.Unity.Editor"
            );
            MethodInfo createBase = setupType?.GetMethod(
                "CreateConfiguredCaptureBase",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            Assert.That(setupType, Is.Not.Null);
            Assert.That(createBase, Is.Not.Null);

            GameObject root = (GameObject)createBase.Invoke(
                null,
                new object[] { config }
            );

            Type accessSetupType = Type.GetType(
                "LudusSDK.Editor.LudusCollectionAccessSetup, Ludus.Unity.Editor"
            );
            MethodInfo createAccess = accessSetupType?.GetMethod(
                "CreateForSimpleMode",
                BindingFlags.Static |
                    BindingFlags.NonPublic |
                    BindingFlags.Public
            );
            Assert.That(accessSetupType, Is.Not.Null);
            Assert.That(createAccess, Is.Not.Null);

            LudusCollectionAccessPanel panel =
                (LudusCollectionAccessPanel)createAccess.Invoke(
                    null,
                    new object[] { root.transform }
                );

            LudusSessionController controller =
                root.GetComponent<LudusSessionController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.Config, Is.SameAs(config));
            Assert.That(controller.persistAcrossScenes, Is.True);
            Assert.That(root.GetComponent<LudusSessionExporter>(), Is.Not.Null);
            Assert.That(
                root.GetComponent<LudusSceneCaptureCoordinator>(),
                Is.Not.Null
            );

            Assert.That(panel, Is.Not.Null);
            Canvas canvas = root.GetComponentInChildren<Canvas>(true);
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            Assert.That(canvas.sortingOrder, Is.EqualTo(short.MaxValue));
            Assert.That(
                root.GetComponentInChildren<EventSystem>(true),
                Is.Not.Null
            );

            SerializedObject serializedPanel = new SerializedObject(panel);
            Assert.That(
                serializedPanel.FindProperty("hideModalOnStart").boolValue,
                Is.False
            );
            Assert.That(
                serializedPanel.FindProperty(
                    "startSessionAfterConnection"
                ).boolValue,
                Is.True
            );
            Assert.That(
                serializedPanel.FindProperty(
                    "finishSessionFromConnectedView"
                ).boolValue,
                Is.True
            );
            Assert.That(
                serializedPanel.FindProperty(
                    "ensureEventSystemAcrossScenes"
                ).boolValue,
                Is.True
            );
            Assert.That(
                config.apiBaseUrl,
                Is.EqualTo(LudusSdkConfig.DefaultApiBaseUrl)
            );

            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(config);
        }

        private static Transform AssertChildExists(
            GameObject root,
            string path
        )
        {
            Transform child = root.transform.Find(path);
            Assert.That(child, Is.Not.Null, $"Objeto ausente: {path}");
            return child;
        }
    }
}
