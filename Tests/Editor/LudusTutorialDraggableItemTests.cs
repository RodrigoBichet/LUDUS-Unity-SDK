using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialDraggableItemTests
    {
        [Test]
        public void ComponenteDidatico_ImplementaCicloCompletoDeArraste()
        {
            GameObject host = new GameObject(
                "PecaTutorial",
                typeof(RectTransform),
                typeof(LudusTutorialDraggableItem)
            );
            LudusTutorialDraggableItem item =
                host.GetComponent<LudusTutorialDraggableItem>();

            Assert.That(item, Is.InstanceOf<IBeginDragHandler>());
            Assert.That(item, Is.InstanceOf<IDragHandler>());
            Assert.That(item, Is.InstanceOf<IEndDragHandler>());

            Object.DestroyImmediate(host);
        }

        [Test]
        public void ArrasteSemEvento_NaoMoveNemLancaExcecao()
        {
            GameObject area = new GameObject("Area", typeof(RectTransform));
            GameObject host = new GameObject(
                "PecaTutorial",
                typeof(RectTransform),
                typeof(LudusTutorialDraggableItem)
            );
            host.transform.SetParent(area.transform, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(25f, -30f);
            LudusTutorialDraggableItem item =
                host.GetComponent<LudusTutorialDraggableItem>();

            Assert.DoesNotThrow(() => item.OnBeginDrag(null));
            Assert.DoesNotThrow(() => item.OnDrag(null));
            Assert.DoesNotThrow(() => item.OnEndDrag(null));
            Assert.That(rect.anchoredPosition, Is.EqualTo(new Vector2(25f, -30f)));

            Object.DestroyImmediate(area);
        }

        [Test]
        public void PonteDoTutorial_PodeSerAdicionadaSemDependenciaDoJogo()
        {
            GameObject host = new GameObject("ExercicioTutorial");
            LudusTutorialEditorPointerBridge bridge =
                host.AddComponent<LudusTutorialEditorPointerBridge>();

            Assert.That(bridge, Is.Not.Null);

            Object.DestroyImmediate(host);
        }
    }
}
