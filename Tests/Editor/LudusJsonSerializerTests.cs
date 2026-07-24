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
    }
}