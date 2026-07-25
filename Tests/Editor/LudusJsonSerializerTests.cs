using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusJsonSerializerTests
    {
        [Test]
        public void TrySerialize_ComSessaoFicticia_GeraPayloadComoObjetoJson()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";

            LudusParticipant participant = new LudusParticipant(
                "000000000000000000000001",
                "Estudante Fictício"
            );

            LudusSession session = LudusSession.Create(
                config,
                participant,
                new LudusViewport(1280, 720, "pixel", "bottom-left")
            );

            session.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = "EventoDeTeste",
                    timestamp = 250,
                    payloadJson = "{\"fase\":1,\"resultado\":\"teste\"}",
                }
            );

            session.End(1000);

            bool serialized = LudusJsonSerializer.TrySerialize(
                session,
                out string json,
                out string errorMessage
            );

            Object.DestroyImmediate(config);

            Assert.That(serialized, Is.True, errorMessage);
            Assert.That(json, Does.Contain("\"schemaVersion\":\"1.0.0\""));
            Assert.That(json, Does.Contain("\"captureMode\":\"sdk\""));
            Assert.That(
                json,
                Does.Contain(
                    "\"payload\":{\"fase\":1,\"resultado\":\"teste\"}"
                )
            );
            Assert.That(json, Does.Not.Contain("\"payloadJson\""));
        }

        [Test]
        public void TrySerialize_ComPayloadInvalido_RejeitaSessao()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";

            LudusParticipant participant = new LudusParticipant(
                "000000000000000000000002",
                "Estudante Fictício"
            );

            LudusSession session = LudusSession.Create(
                config,
                participant,
                new LudusViewport(1280, 720, "pixel", "bottom-left")
            );

            session.gameEvents.Add(
                new LudusGameEvent
                {
                    eventType = "EventoDeTeste",
                    timestamp = 100,
                    payloadJson = "{ resultado: \"inválido\" }",
                }
            );

            session.End(1000);

            bool serialized = LudusJsonSerializer.TrySerialize(
                session,
                out _,
                out string errorMessage
            );

            Object.DestroyImmediate(config);

            Assert.That(serialized, Is.False);
            Assert.That(errorMessage, Does.Contain("payload inválido"));
        }

        [Test]
public void SessionLifecycle_ComDadosFicticios_EncerraEExportaJson()
{
    LudusSdkConfig config =
        ScriptableObject.CreateInstance<LudusSdkConfig>();

    config.gameId = "jogo-teste";
    config.gameVersion = "0.1.0-teste";

    LudusParticipant participant = new LudusParticipant(
        "000000000000000000000003",
        "Estudante Fictício"
    );

    LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();

    bool started = lifecycle.TryStartSession(
        config,
        participant,
        new LudusViewport(1280, 720, "pixel", "bottom-left"),
        out string startError
    );

    bool ended = lifecycle.TryEndAndSerialize(
        out string json,
        out string endError
    );

    Object.DestroyImmediate(config);

    Assert.That(started, Is.True, startError);
    Assert.That(ended, Is.True, endError);
    Assert.That(lifecycle.HasActiveSession, Is.False);
    Assert.That(lifecycle.LastCompletedSession, Is.Not.Null);
    Assert.That(json, Does.Contain("\"durationMs\":"));
}

[Test]
public void SessionLifecycle_AoTrocarContexto_EncerraOAnterior()
{
    LudusSdkConfig config =
        ScriptableObject.CreateInstance<LudusSdkConfig>();

    config.gameId = "jogo-teste";
    config.gameVersion = "0.1.0-teste";

    LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();

    bool started = lifecycle.TryStartSession(
        config,
        new LudusParticipant(
            "000000000000000000000004",
            "Estudante Fictício"
        ),
        new LudusViewport(1280, 720, "pixel", "bottom-left"),
        out string startError
    );

    bool firstContextStarted = lifecycle.TryBeginCaptureContext(
        new LudusCaptureContext(
            "Menu principal",
            "canvas",
            "Navegação inicial"
        ),
        out string firstContextError
    );

    bool secondContextStarted = lifecycle.TryBeginCaptureContext(
        new LudusCaptureContext(
            "Atividade principal",
            "canvas",
            "Observação da atividade"
        ),
        out string secondContextError
    );

    bool ended = lifecycle.TryEndAndSerialize(
        out string json,
        out string endError
    );

    Object.DestroyImmediate(config);

    Assert.That(started, Is.True, startError);
    Assert.That(firstContextStarted, Is.True, firstContextError);
    Assert.That(secondContextStarted, Is.True, secondContextError);
    Assert.That(ended, Is.True, endError);
    Assert.That(lifecycle.HasActiveCaptureContext, Is.False);
    Assert.That(
        lifecycle.LastCompletedSession.gameEvents.Count,
        Is.EqualTo(4)
    );
    Assert.That(json, Does.Contain("CaptureContextStarted"));
    Assert.That(json, Does.Contain("CaptureContextEnded"));
}
    }
}