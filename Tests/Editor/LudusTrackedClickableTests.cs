using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusTrackedClickableTests
    {
        [Test]
        public void Lifecycle_ComObjetoClicavel_RegistraEventoGenericoPosicionado()
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
                    "Imagem usada como botão",
                    "clickable-object",
                    "activated",
                    new Vector2(350f, 280f)
                ),
                out string recordError
            );
            lifecycle.TryEndAndSerialize(out string json, out _);

            Assert.That(recorded, Is.True, recordError);
            Assert.That(
                json,
                Does.Contain("\"interactionKind\":\"clickable-object\"")
            );
            Assert.That(json, Does.Contain("\"action\":\"activated\""));
            Assert.That(json, Does.Contain("\"x\":350.0"));
            Assert.That(json, Does.Contain("\"y\":280.0"));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void TrackedClickable_SemNomePersonalizado_UsaNomeDoObjeto()
        {
            GameObject host = new GameObject("ImagemClicavel");
            LudusTrackedClickable trackedClickable =
                host.AddComponent<LudusTrackedClickable>();

            trackedClickable.Configure(string.Empty);

            Assert.That(
                trackedClickable.DashboardName,
                Is.EqualTo("ImagemClicavel")
            );
            Assert.That(LudusTrackedClickable.CanTrack(host), Is.True);

            Object.DestroyImmediate(host);
        }
    }
}
