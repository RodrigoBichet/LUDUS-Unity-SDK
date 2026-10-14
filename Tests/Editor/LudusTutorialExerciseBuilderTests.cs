using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

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
