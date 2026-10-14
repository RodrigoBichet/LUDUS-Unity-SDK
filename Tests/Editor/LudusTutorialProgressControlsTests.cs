using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialProgressControlsTests
    {
        [Test]
        public void Controles_AtualizamValorVisualSemAdaptador()
        {
            GameObject root = new GameObject("Controlador");
            LudusTutorialProgressControls controls =
                root.AddComponent<LudusTutorialProgressControls>();
            Button increase = new GameObject("Adicionar")
                .AddComponent<Button>();
            Button decrease = new GameObject("Subtrair")
                .AddComponent<Button>();
            Button reset = new GameObject("Reiniciar")
                .AddComponent<Button>();
            Text label = new GameObject("Valor").AddComponent<Text>();

            controls.Configure(increase, decrease, reset, label);
            controls.Increase();
            controls.Increase();
            controls.Decrease();

            Assert.That(controls.CurrentValue, Is.EqualTo(1));
            Assert.That(label.text, Is.EqualTo("Valor atual: 1 / Meta: 3"));

            controls.ResetValue();

            Assert.That(controls.CurrentValue, Is.EqualTo(0));
            Assert.That(label.text, Is.EqualTo("Valor atual: 0 / Meta: 3"));

            Object.DestroyImmediate(increase.gameObject);
            Object.DestroyImmediate(decrease.gameObject);
            Object.DestroyImmediate(reset.gameObject);
            Object.DestroyImmediate(label.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void DoisCallbacksDoMesmoBotaoNoFrame_AlteramValorUmaVez()
        {
            GameObject root = new GameObject("Controlador");
            LudusTutorialProgressControls controls =
                root.AddComponent<LudusTutorialProgressControls>();
            Button increase = new GameObject("Adicionar")
                .AddComponent<Button>();
            Button decrease = new GameObject("Subtrair")
                .AddComponent<Button>();
            Button reset = new GameObject("Reiniciar")
                .AddComponent<Button>();
            Text label = new GameObject("Valor").AddComponent<Text>();

            controls.Configure(increase, decrease, reset, label);
            MethodInfo registerListeners = typeof(
                LudusTutorialProgressControls
            ).GetMethod(
                "RegisterListeners",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(registerListeners, Is.Not.Null);
            registerListeners.Invoke(controls, null);

            increase.onClick.Invoke();
            increase.onClick.Invoke();

            Assert.That(controls.CurrentValue, Is.EqualTo(1));
            Assert.That(label.text, Is.EqualTo("Valor atual: 1 / Meta: 3"));

            Object.DestroyImmediate(increase.gameObject);
            Object.DestroyImmediate(decrease.gameObject);
            Object.DestroyImmediate(reset.gameObject);
            Object.DestroyImmediate(label.gameObject);
            Object.DestroyImmediate(root);
        }
    }
}
