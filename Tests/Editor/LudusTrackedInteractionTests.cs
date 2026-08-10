using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusTrackedInteractionTests
    {
        [Test]
        public void Lifecycle_ComBotaoAcompanhado_RegistraEventoSemDuplicarClique()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de Teste";

            LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();

            bool started = lifecycle.TryStartSession(
                config,
                new LudusParticipant(
                    "000000000000000000000000",
                    "Sessão Fictícia"
                ),
                new LudusViewport(1280, 720),
                out string startError
            );
            bool contextStarted = lifecycle.TryBeginCaptureContext(
                new LudusCaptureContext("Cena de teste", "scene"),
                out string contextError
            );
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    "Botão inicial",
                    "button",
                    "activated",
                    new Vector2(320f, 180f)
                ),
                out string recordError
            );
            bool ended = lifecycle.TryEndAndSerialize(
                out string json,
                out string endError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(contextStarted, Is.True, contextError);
            Assert.That(recorded, Is.True, recordError);
            Assert.That(ended, Is.True, endError);
            Assert.That(json, Does.Contain("\"eventType\":\"TrackedInteraction\""));
            Assert.That(json, Does.Contain("\"displayName\":\"Botão inicial\""));
            Assert.That(json, Does.Contain("\"interactionKind\":\"button\""));
            Assert.That(json, Does.Contain("\"action\":\"activated\""));
            Assert.That(json, Does.Contain("\"x\":320.0"));
            Assert.That(json, Does.Contain("\"y\":180.0"));
            Assert.That(
                lifecycle.LastCompletedSession.metrics.totalClicks,
                Is.EqualTo(0)
            );

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Lifecycle_ComCoordenadaForaDoViewport_RejeitaInteracao()
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
                    "Botão fora do viewport",
                    "button",
                    "activated",
                    new Vector2(1400f, 180f)
                ),
                out string recordError
            );

            Assert.That(recorded, Is.False);
            Assert.That(recordError, Does.Contain("coordenadas inválidas"));

            lifecycle.TryEndAndSerialize(out _, out _);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Lifecycle_SemCoordenada_MantemPayloadAnteriorCompativel()
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
            lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    "Ativação sem ponteiro",
                    "button",
                    "activated"
                ),
                out _
            );
            lifecycle.TryEndAndSerialize(out string json, out _);

            Assert.That(json, Does.Contain("\"displayName\":\"Ativação sem ponteiro\""));
            Assert.That(json, Does.Not.Contain("\"x\":"));
            Assert.That(json, Does.Not.Contain("\"y\":"));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Lifecycle_SemContextoAtivo_RejeitaInteracaoAcompanhada()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de Teste";

            LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();
            bool started = lifecycle.TryStartSession(
                config,
                new LudusParticipant(
                    "000000000000000000000000",
                    "Sessão Fictícia"
                ),
                new LudusViewport(1280, 720),
                out string startError
            );
            bool recorded = lifecycle.TryRecordTrackedInteraction(
                new LudusTrackedInteraction(
                    "Botão fora do recorte",
                    "button",
                    "activated"
                ),
                out string recordError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(recorded, Is.False);
            Assert.That(recordError, Does.Contain("contexto de captura ativo"));

            lifecycle.TryEndAndSerialize(out _, out _);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void TrackedButton_SemNomePersonalizado_UsaNomeDoObjeto()
        {
            GameObject host = new GameObject("ButtonStart");
            LudusTrackedButton trackedButton =
                host.AddComponent<LudusTrackedButton>();

            trackedButton.Configure(string.Empty);

            Assert.That(trackedButton.DashboardName, Is.EqualTo("ButtonStart"));
            Assert.That(LudusTrackedButton.CanTrack(host), Is.True);

            Object.DestroyImmediate(host);
        }

    }
}
