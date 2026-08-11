using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusTrackedDraggableTests
    {
        [Test]
        public void Lifecycle_ComObjetoArrastado_RegistraResumoSemInferirResultado()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de Teste";

            LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();
            lifecycle.TryStartSession(
                config,
                new LudusParticipant(
                    "000000000000000000000000",
                    "Sessão Fictícia"
                ),
                new LudusViewport(1280, 720),
                out _
            );
            lifecycle.TryBeginCaptureContext(
                new LudusCaptureContext("Cena de teste", "scene"),
                out _
            );

            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    "Peça azul",
                    new Vector2(100f, 150f),
                    new Vector2(460f, 330f),
                    1250
                ),
                out string recordError
            );
            lifecycle.TryEndAndSerialize(out string json, out _);

            Assert.That(recorded, Is.True, recordError);
            Assert.That(
                json,
                Does.Contain("\"interactionKind\":\"draggable-object\"")
            );
            Assert.That(json, Does.Contain("\"action\":\"completed\""));
            Assert.That(json, Does.Contain("\"startX\":100.0"));
            Assert.That(json, Does.Contain("\"startY\":150.0"));
            Assert.That(json, Does.Contain("\"endX\":460.0"));
            Assert.That(json, Does.Contain("\"endY\":330.0"));
            Assert.That(json, Does.Contain("\"durationMs\":1250"));
            Assert.That(json, Does.Contain("\"distancePx\":"));
            Assert.That(json, Does.Contain("\"x\":460.0"));
            Assert.That(json, Does.Contain("\"y\":330.0"));
            Assert.That(json, Does.Not.Contain("\"isCorrect\""));
            Assert.That(json, Does.Not.Contain("\"result\""));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Lifecycle_ComInicioDoArrasteForaDoViewport_RejeitaEvento()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de Teste";

            LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();
            lifecycle.TryStartSession(
                config,
                new LudusParticipant(
                    "000000000000000000000000",
                    "Sessão Fictícia"
                ),
                new LudusViewport(1280, 720),
                out _
            );
            lifecycle.TryBeginCaptureContext(
                new LudusCaptureContext("Cena de teste", "scene"),
                out _
            );

            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    "Peça fora da tela",
                    new Vector2(-10f, 150f),
                    new Vector2(460f, 330f),
                    500
                ),
                out string recordError
            );

            Assert.That(recorded, Is.False);
            Assert.That(recordError, Does.Contain("coordenadas inválidas"));

            lifecycle.TryEndAndSerialize(out _, out _);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void TrackedDraggable_SemNomePersonalizado_UsaNomeDoObjeto()
        {
            GameObject host = new GameObject("PecaArrastavel");
            LudusTrackedDraggable trackedDraggable =
                host.AddComponent<LudusTrackedDraggable>();

            trackedDraggable.Configure(string.Empty);

            Assert.That(
                trackedDraggable.DashboardName,
                Is.EqualTo("PecaArrastavel")
            );
            Assert.That(trackedDraggable.MinimumDistancePixels, Is.EqualTo(5f));
            Assert.That(LudusTrackedDraggable.CanTrack(host), Is.True);

            Object.DestroyImmediate(host);
        }
    }
}
