using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialSemanticFlowControlsTests
    {
        [Test]
        public void Fluxo_LiberaSomenteAProximaEtapa()
        {
            GameObject root = new GameObject("Fluxo");
            Button[] buttons =
            {
                new GameObject("Categoria").AddComponent<Button>(),
                new GameObject("Inicio").AddComponent<Button>(),
                new GameObject("Resultado").AddComponent<Button>(),
                new GameObject("Conclusao").AddComponent<Button>(),
            };
            Text progress = new GameObject("Progresso").AddComponent<Text>();
            LudusTutorialSemanticFlowControls controls =
                root.AddComponent<LudusTutorialSemanticFlowControls>();
            controls.Configure(buttons, progress);
            InvokePrivate(controls, "RegisterListeners");

            Assert.That(buttons[0].interactable, Is.True);
            Assert.That(buttons[1].interactable, Is.False);
            Assert.That(buttons[1].colors.disabledColor.r, Is.LessThan(0.1f));

            buttons[0].onClick.Invoke();

            Assert.That(controls.CurrentStep, Is.EqualTo(1));
            Assert.That(buttons[0].interactable, Is.False);
            Assert.That(buttons[1].interactable, Is.True);
            Assert.That(progress.text, Does.Contain("✓ Categoria"));

            foreach (Button button in buttons)
            {
                Object.DestroyImmediate(button.gameObject);
            }
            Object.DestroyImmediate(progress.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void RemoverControle_NaoApagaListenerDoJogo()
        {
            GameObject root = new GameObject("Fluxo");
            Button button = new GameObject("Categoria").AddComponent<Button>();
            int externalCalls = 0;
            button.onClick.AddListener(() => externalCalls++);
            LudusTutorialSemanticFlowControls controls =
                root.AddComponent<LudusTutorialSemanticFlowControls>();
            controls.Configure(new[] { button }, null);
            InvokePrivate(controls, "RegisterListeners");
            InvokePrivate(controls, "RemoveListeners");

            button.onClick.Invoke();

            Assert.That(externalCalls, Is.EqualTo(1));
            Assert.That(controls.CurrentStep, Is.EqualTo(0));

            Object.DestroyImmediate(button.gameObject);
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
