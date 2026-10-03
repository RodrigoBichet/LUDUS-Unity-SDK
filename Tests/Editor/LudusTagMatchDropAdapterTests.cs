using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTagMatchTestDraggable :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
        }
    }

    public sealed class LudusTagMatchDropAdapterTests
    {
        [Test]
        public void AdaptadorPorTag_RegistraAcertoEErroComObjetosDiferentes()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-arraste-tag";
            config.capabilities.correctWrong = true;
            config.capabilities.customEvents = true;

            GameObject host = new GameObject("BaseLudusArrasteTagTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject target = new GameObject("Destino escolar");
            target.AddComponent<LudusSemanticBridge>();
            LudusTagMatchDropAdapter adapter =
                target.AddComponent<LudusTagMatchDropAdapter>();
            adapter.Configure(
                "Player",
                "Área dos objetos escolares",
                "Objeto com tag Player"
            );

            GameObject correctItem = new GameObject("Cadeira");
            correctItem.tag = "Player";
            GameObject wrongItem = new GameObject("Bola");
            wrongItem.tag = "MainCamera";

            Assert.That(
                LudusSdk.TryStartSession(
                    "000000000000000000000021",
                    "Estudante Fictício",
                    out string startError
                ),
                Is.True,
                startError
            );
            Assert.That(
                adapter.TryEvaluateDrop(
                    correctItem,
                    out bool correct,
                    out string correctError
                ),
                Is.True,
                correctError
            );
            Assert.That(correct, Is.True);
            Assert.That(
                adapter.TryEvaluateDrop(
                    wrongItem,
                    out bool wrongWasCorrect,
                    out string wrongError
                ),
                Is.True,
                wrongError
            );
            Assert.That(wrongWasCorrect, Is.False);
            Assert.That(
                LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(
                controller.LastCompletedSession.metrics.totalCorrect,
                Is.EqualTo(1)
            );
            Assert.That(
                controller.LastCompletedSession.metrics.totalWrong,
                Is.EqualTo(1)
            );
            Assert.That(json, Does.Contain("\"DragAttempt\""));
            Assert.That(json, Does.Contain("\"CorrectMatch\""));
            Assert.That(json, Does.Contain("\"WrongMatch\""));
            Assert.That(json, Does.Contain("\"draggedItem\":\"Cadeira\""));
            Assert.That(json, Does.Contain("\"draggedItem\":\"Bola\""));
            Assert.That(
                json,
                Does.Contain(
                    "\"targetItem\":\"Área dos objetos escolares\""
                )
            );

            Object.DestroyImmediate(wrongItem);
            Object.DestroyImmediate(correctItem);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void AdaptadorPorTag_TagInexistenteExplicaConfiguracao()
        {
            GameObject target = new GameObject("DestinoTagInvalida");
            target.AddComponent<LudusSemanticBridge>();
            LudusTagMatchDropAdapter adapter =
                target.AddComponent<LudusTagMatchDropAdapter>();
            adapter.Configure(
                "TagQueNaoExisteNoProjeto",
                "Destino",
                "Esperado"
            );

            GameObject draggedItem = new GameObject("Item");
            bool recorded = adapter.TryEvaluateDrop(
                draggedItem,
                out _,
                out string errorMessage
            );

            Assert.That(recorded, Is.False);
            Assert.That(errorMessage, Does.Contain("não existe"));

            Object.DestroyImmediate(draggedItem);
            Object.DestroyImmediate(target);
        }

        [Test]
        public void AdaptadorPorTag_UsaSpritesAtivosComoNomesSemanticos()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-opcoes-visuais";
            config.capabilities.correctWrong = true;
            config.capabilities.customEvents = true;

            GameObject controllerHost = new GameObject("Base LUDUS");
            LudusSessionController controller =
                controllerHost.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject canvasObject = new GameObject(
                "Pergunta atual",
                typeof(Canvas)
            );
            GameObject target = new GameObject("Destino");
            target.transform.SetParent(canvasObject.transform);
            target.tag = "Player";
            target.AddComponent<LudusSemanticBridge>();
            LudusTagMatchDropAdapter adapter =
                target.AddComponent<LudusTagMatchDropAdapter>();
            adapter.Configure("Player", "Área de resposta", string.Empty);

            Texture2D correctTexture = new Texture2D(2, 2);
            Texture2D wrongTexture = new Texture2D(2, 2);
            Sprite correctSprite = Sprite.Create(
                correctTexture,
                new Rect(0, 0, 2, 2),
                Vector2.one * 0.5f
            );
            correctSprite.name = "Armário";
            Sprite wrongSprite = Sprite.Create(
                wrongTexture,
                new Rect(0, 0, 2, 2),
                Vector2.one * 0.5f
            );
            wrongSprite.name = "Cama";

            GameObject correctItem = CreateVisualOption(
                canvasObject.transform,
                "Opção correta técnica",
                "Player",
                correctSprite
            );
            GameObject wrongItem = CreateVisualOption(
                canvasObject.transform,
                "Opção incorreta técnica",
                "MainCamera",
                wrongSprite
            );

            Assert.That(
                controller.TryStartSession(
                    "000000000000000000000022",
                    "Estudante Fictício",
                    out string startError
                ),
                Is.True,
                startError
            );
            Assert.That(
                adapter.TryEvaluateDrop(
                    wrongItem,
                    out bool correct,
                    out string dropError
                ),
                Is.True,
                dropError
            );
            Assert.That(correct, Is.False);
            Assert.That(
                controller.TryEndAndSerialize(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(json, Does.Contain("\"draggedItem\":\"Cama\""));
            Assert.That(json, Does.Contain("\"expectedItem\":\"Armário\""));
            Assert.That(json, Does.Contain("\"options\":[\"Armário\",\"Cama\"]"));
            Assert.That(json, Does.Not.Contain("Opção incorreta técnica"));

            Object.DestroyImmediate(canvasObject);
            Object.DestroyImmediate(controllerHost);
            Object.DestroyImmediate(correctSprite);
            Object.DestroyImmediate(wrongSprite);
            Object.DestroyImmediate(correctTexture);
            Object.DestroyImmediate(wrongTexture);
            Object.DestroyImmediate(config);
        }

        private static GameObject CreateVisualOption(
            Transform parent,
            string name,
            string tag,
            Sprite sprite
        )
        {
            GameObject option = new GameObject(name);
            option.transform.SetParent(parent);
            option.tag = tag;
            option.AddComponent<LudusTagMatchTestDraggable>();
            option.AddComponent<Image>().sprite = sprite;
            return option;
        }
    }
}
