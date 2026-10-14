using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialAttemptTargetTests
    {
        [Test]
        public void TentativaCorreta_EncaixaPecaEBloqueiaNovoArraste()
        {
            GameObject area = new GameObject("Area", typeof(RectTransform));
            GameObject pieceObject = new GameObject(
                "Peca",
                typeof(RectTransform)
            );
            pieceObject.transform.SetParent(area.transform, false);
            RectTransform pieceRect =
                pieceObject.GetComponent<RectTransform>();
            pieceRect.anchoredPosition = new Vector2(-80f, 20f);
            LudusTutorialDraggableItem piece =
                pieceObject.AddComponent<LudusTutorialDraggableItem>();
            piece.RememberInitialPosition();

            GameObject targetObject = new GameObject(
                "Destino",
                typeof(RectTransform)
            );
            targetObject.transform.SetParent(area.transform, false);
            RectTransform targetRect =
                targetObject.GetComponent<RectTransform>();
            targetRect.anchoredPosition = new Vector2(140f, -30f);
            LudusTutorialAttemptTarget target =
                targetObject.AddComponent<LudusTutorialAttemptTarget>();
            Text feedback = new GameObject("Feedback").AddComponent<Text>();
            target.Configure(
                LudusTutorialAttemptMode.UiDrop,
                "Untagged",
                feedback,
                new[] { piece }
            );
            int evaluations = 0;
            bool result = false;
            target.AttemptEvaluated += correct =>
            {
                evaluations++;
                result = correct;
            };

            pieceRect.anchoredPosition = new Vector2(100f, -70f);
            bool evaluated = target.EvaluateAttempt(pieceObject);

            Assert.That(evaluated, Is.True);
            Assert.That(result, Is.True);
            Assert.That(evaluations, Is.EqualTo(1));
            Assert.That(
                Vector3.Distance(pieceRect.position, targetRect.position),
                Is.LessThan(0.001f)
            );
            CanvasGroup canvasGroup = pieceObject.GetComponent<CanvasGroup>();
            Assert.That(canvasGroup.interactable, Is.False);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);

            piece.OnEndDrag(null);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);
            Assert.That(feedback.text, Does.Contain("correta"));

            Object.DestroyImmediate(feedback.gameObject);
            Object.DestroyImmediate(targetObject);
            Object.DestroyImmediate(area);
        }

        [Test]
        public void TentativaIncorreta_DevolvePecaEPermiteNovaTentativa()
        {
            GameObject area = new GameObject("Area", typeof(RectTransform));
            GameObject pieceObject = new GameObject(
                "Peca",
                typeof(RectTransform)
            );
            pieceObject.transform.SetParent(area.transform, false);
            RectTransform pieceRect =
                pieceObject.GetComponent<RectTransform>();
            Vector2 initialPosition = new Vector2(-80f, 20f);
            pieceRect.anchoredPosition = initialPosition;
            LudusTutorialDraggableItem piece =
                pieceObject.AddComponent<LudusTutorialDraggableItem>();
            piece.RememberInitialPosition();

            GameObject targetObject = new GameObject(
                "Destino",
                typeof(RectTransform)
            );
            LudusTutorialAttemptTarget target =
                targetObject.AddComponent<LudusTutorialAttemptTarget>();
            target.Configure(
                LudusTutorialAttemptMode.UiDrop,
                "Player",
                null,
                new[] { piece }
            );

            pieceRect.anchoredPosition = new Vector2(100f, -70f);
            bool evaluated = target.EvaluateAttempt(pieceObject);

            Assert.That(evaluated, Is.False);
            Assert.That(pieceRect.anchoredPosition, Is.EqualTo(initialPosition));
            CanvasGroup canvasGroup = pieceObject.GetComponent<CanvasGroup>();
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);

            Object.DestroyImmediate(targetObject);
            Object.DestroyImmediate(area);
        }
    }
}
