using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusSemanticBridgeTests
    {
        [Test]
        public void PonteVisual_RegistraFluxoSemanticoEAtualizaMetricas()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-ponte-visual";
            config.capabilities.categoryEvents = true;
            config.capabilities.phaseEvents = true;
            config.capabilities.correctWrong = true;
            config.capabilities.customEvents = true;

            GameObject host = new GameObject("LudusPonteVisualTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            LudusSemanticBridge bridge =
                host.AddComponent<LudusSemanticBridge>();
            bridge.Configure(
                "Associação",
                "fase-ponte-1",
                "mesa",
                "cadeira",
                "mesa",
                new[] { "mesa", "cadeira" },
                2
            );

            Assert.That(
                LudusSdk.TryStartSession(
                    "000000000000000000000020",
                    "Estudante Fictício",
                    out string startError
                ),
                Is.True,
                startError
            );
            Assert.That(
                bridge.TryRecordCategory(out string categoryError),
                Is.True,
                categoryError
            );
            Assert.That(
                bridge.TryRecordPhaseStarted(out string phaseError),
                Is.True,
                phaseError
            );
            Assert.That(
                bridge.TryRecordDragAttempt(false, out string attemptError),
                Is.True,
                attemptError
            );
            Assert.That(
                bridge.TryRecordWrong(out string wrongError),
                Is.True,
                wrongError
            );
            Assert.That(
                bridge.TryRecordCorrect(out string correctError),
                Is.True,
                correctError
            );
            Assert.That(
                bridge.TryRecordPhaseCompleted(out string completedError),
                Is.True,
                completedError
            );
            Assert.That(
                LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(bridge.PhaseCorrectCount, Is.EqualTo(1));
            Assert.That(bridge.PhaseWrongCount, Is.EqualTo(1));
            Assert.That(
                controller.LastCompletedSession.metrics.totalCorrect,
                Is.EqualTo(1)
            );
            Assert.That(
                controller.LastCompletedSession.metrics.totalWrong,
                Is.EqualTo(1)
            );
            Assert.That(json, Does.Contain("\"CategorySelected\""));
            Assert.That(json, Does.Contain("\"PhaseStarted\""));
            Assert.That(json, Does.Contain("\"DragAttempt\""));
            Assert.That(json, Does.Contain("\"WrongMatch\""));
            Assert.That(json, Does.Contain("\"CorrectMatch\""));
            Assert.That(json, Does.Contain("\"PhaseCompleted\""));
            Assert.That(json, Does.Contain("\"acertos\":1"));
            Assert.That(json, Does.Contain("\"erros\":1"));
            Assert.That(json, Does.Contain("\"stars\":2"));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PonteVisual_DesabilitadaNaoRegistraEvento()
        {
            GameObject host = new GameObject("LudusPonteDesabilitadaTeste");
            LudusSemanticBridge bridge =
                host.AddComponent<LudusSemanticBridge>();
            bridge.SetSemanticTrackingEnabled(false);

            bool recorded = bridge.TryRecordCorrect(
                out string errorMessage
            );

            Assert.That(recorded, Is.False);
            Assert.That(errorMessage, Does.Contain("desabilitada"));

            Object.DestroyImmediate(host);
        }

        [Test]
        public void PonteVisual_ConcluiComControladorDaFaseMesmoInativo()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-ponte-encerramento";
            config.capabilities.phaseEvents = true;

            GameObject host = new GameObject("LudusBaseEncerramentoTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject bridgeObject = new GameObject("Ponte da fase");
            LudusSemanticBridge bridge =
                bridgeObject.AddComponent<LudusSemanticBridge>();
            bridge.Configure(
                "Ação",
                "fase-1",
                "agua",
                "agua",
                "agua",
                new[] { "agua" }
            );

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
                bridge.TryRecordPhaseStarted(out string phaseError),
                Is.True,
                phaseError
            );

            host.SetActive(false);

            Assert.That(
                bridge.TryRecordPhaseCompleted(out string completedError),
                Is.True,
                completedError
            );
            Assert.That(
                controller.TryEndAndSerialize(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );
            Assert.That(json, Does.Contain("\"PhaseCompleted\""));

            Object.DestroyImmediate(bridgeObject);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }
    }
}
