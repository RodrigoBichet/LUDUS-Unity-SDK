using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusRoundedGraphicTests
    {
        [Test]
        public void AddComponent_AdicionaCanvasRendererObrigatorio()
        {
            GameObject host = new GameObject(
                "Fundo arredondado de teste",
                typeof(RectTransform)
            );

            LudusRoundedGraphic graphic =
                host.AddComponent<LudusRoundedGraphic>();

            Assert.That(graphic, Is.Not.Null);
            Assert.That(host.GetComponent<CanvasRenderer>(), Is.Not.Null);

            Object.DestroyImmediate(host);
        }
    }
}
