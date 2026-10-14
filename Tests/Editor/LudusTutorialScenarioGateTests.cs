using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialScenarioGateTests
    {
        [Test]
        public void InteracoesAutomaticas_ExigemAsTresAcoes()
        {
            GameObject root = new GameObject("Cenario");
            LudusTutorialScenarioGate gate =
                root.AddComponent<LudusTutorialScenarioGate>();
            Button button = new GameObject("Botao").AddComponent<Button>();
            InputField input = new GameObject("Campo")
                .AddComponent<InputField>();
            GameObject area = new GameObject("Area", typeof(RectTransform));
            GameObject pieceObject = new GameObject(
                "Peca",
                typeof(RectTransform)
            );
            pieceObject.transform.SetParent(area.transform, false);
            LudusTutorialDraggableItem piece =
                pieceObject.AddComponent<LudusTutorialDraggableItem>();
            gate.ConfigureAutomaticInteractions(
                button,
                input,
                piece,
                "Faça as três ações."
            );
            InvokePrivate(gate, "RegisterListeners");

            button.onClick.Invoke();
            input.onEndEdit.Invoke("texto fictício");
            Assert.That(gate.IsCompleted, Is.False);
            Assert.That(gate.PendingMessage, Does.Contain("arraste"));

            piece.OnEndDrag(null);
            Assert.That(gate.IsCompleted, Is.True);

            Object.DestroyImmediate(button.gameObject);
            Object.DestroyImmediate(input.gameObject);
            Object.DestroyImmediate(area);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void QualquerBotao_ConcluiCenario()
        {
            GameObject root = new GameObject("Cenario");
            LudusTutorialScenarioGate gate =
                root.AddComponent<LudusTutorialScenarioGate>();
            Button button = new GameObject("Alternativa")
                .AddComponent<Button>();
            gate.ConfigureAnyButton(
                new[] { button },
                "Escolha uma alternativa."
            );
            InvokePrivate(gate, "RegisterListeners");

            button.onClick.Invoke();

            Assert.That(gate.IsCompleted, Is.True);
            Assert.That(gate.PendingMessage, Does.Contain("concluída"));

            Object.DestroyImmediate(button.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Quiz_ExigeRespostaCorretaEImagem()
        {
            GameObject root = new GameObject("Cenario");
            Button correct = new GameObject("Correta").AddComponent<Button>();
            Button incorrect = new GameObject("Incorreta").AddComponent<Button>();
            Button correctImage = new GameObject("ImagemCorreta").AddComponent<Button>();
            Button incorrectImage = new GameObject("ImagemIncorreta").AddComponent<Button>();
            Text feedback = new GameObject("Feedback").AddComponent<Text>();
            LudusTutorialQuizControls quiz =
                root.AddComponent<LudusTutorialQuizControls>();
            quiz.Configure(
                correct,
                incorrect,
                correctImage,
                incorrectImage,
                feedback
            );
            LudusTutorialScenarioGate gate =
                root.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureQuiz(quiz, "Conclua o quiz.");
            InvokePrivate(quiz, "RegisterListeners");
            InvokePrivate(gate, "RegisterListeners");

            incorrect.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.False);
            Assert.That(feedback.text, Does.Contain("Tente"));

            correct.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.False);
            Assert.That(correctImage.interactable, Is.True);
            Assert.That(incorrectImage.interactable, Is.True);
            Assert.That(gate.PendingMessage, Does.Contain("imagem"));

            incorrectImage.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.False);
            Assert.That(feedback.text, Does.Contain("apenas uma forma"));

            correctImage.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.True);

            Object.DestroyImmediate(correct.gameObject);
            Object.DestroyImmediate(incorrect.gameObject);
            Object.DestroyImmediate(correctImage.gameObject);
            Object.DestroyImmediate(incorrectImage.gameObject);
            Object.DestroyImmediate(feedback.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void BotoesOrdenados_NaoPermitemPularEtapa()
        {
            GameObject root = new GameObject("Cenario");
            LudusTutorialScenarioGate gate =
                root.AddComponent<LudusTutorialScenarioGate>();
            Button first = new GameObject("Primeiro").AddComponent<Button>();
            Button second = new GameObject("Segundo").AddComponent<Button>();
            gate.ConfigureOrderedButtons(
                new[] { first, second },
                "Siga a ordem."
            );
            InvokePrivate(gate, "RegisterListeners");

            second.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.False);
            Assert.That(gate.PendingMessage, Does.Contain("passo 1"));

            first.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.False);
            Assert.That(gate.PendingMessage, Does.Contain("passo 2"));

            second.onClick.Invoke();
            Assert.That(gate.IsCompleted, Is.True);

            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Meta_ConcluiSomenteNoValorNecessario()
        {
            GameObject root = new GameObject("Cenario");
            LudusTutorialScenarioGate gate =
                root.AddComponent<LudusTutorialScenarioGate>();
            LudusTutorialProgressControls controls =
                root.AddComponent<LudusTutorialProgressControls>();
            gate.ConfigureProgress(controls, 3, "Alcance 3.");
            InvokePrivate(gate, "RegisterListeners");

            controls.Increase();
            controls.Increase();
            Assert.That(gate.IsCompleted, Is.False);

            controls.Increase();
            Assert.That(gate.IsCompleted, Is.True);

            Object.DestroyImmediate(root);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, null);
        }
    }
}
