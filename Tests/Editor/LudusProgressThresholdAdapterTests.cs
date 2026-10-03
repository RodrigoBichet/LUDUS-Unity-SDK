using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusProgressThresholdAdapterTests
    {
        [Test]
        public void MetaPorValor_RegistraSomenteAoAtingirELimitaDuplicacao()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-meta-valor";
            config.capabilities.correctWrong = true;
            config.capabilities.phaseEvents = true;

            GameObject host = new GameObject("BaseLudusMetaTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject target = new GameObject("MetaDeTresPontos");
            LudusSemanticBridge bridge =
                target.AddComponent<LudusSemanticBridge>();
            bridge.Configure(
                "Pontuação",
                "fase-pontos",
                "3 pontos",
                "Meta de pontos",
                "3 pontos",
                new string[0]
            );
            LudusProgressThresholdAdapter adapter =
                target.AddComponent<LudusProgressThresholdAdapter>();
            adapter.Configure(
                3f,
                LudusThresholdComparison.AtLeast,
                LudusThresholdAction.RecordCorrectAndCompletePhase,
                "Três pontos alcançados"
            );

            Assert.That(
                LudusSdk.TryStartSession(
                    "000000000000000000000025",
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
            Assert.That(
                adapter.TrySetValueAndEvaluate(
                    2f,
                    out bool reachedAtTwo,
                    out string twoError
                ),
                Is.True,
                twoError
            );
            Assert.That(reachedAtTwo, Is.False);
            Assert.That(
                adapter.TrySetValueAndEvaluate(
                    3f,
                    out bool reachedAtThree,
                    out string threeError
                ),
                Is.True,
                threeError
            );
            Assert.That(reachedAtThree, Is.True);
            Assert.That(
                adapter.TrySetValueAndEvaluate(
                    4f,
                    out bool reachedAtFour,
                    out string fourError
                ),
                Is.True,
                fourError
            );
            Assert.That(reachedAtFour, Is.True);
            Assert.That(
                LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(adapter.ResultAlreadyRecorded, Is.True);
            Assert.That(
                controller.LastCompletedSession.metrics.totalCorrect,
                Is.EqualTo(1)
            );
            Assert.That(
                controller.LastCompletedSession.gameEvents.FindAll(
                    item => item.eventType == "CorrectMatch"
                ).Count,
                Is.EqualTo(1)
            );
            Assert.That(
                controller.LastCompletedSession.gameEvents.FindAll(
                    item => item.eventType == "PhaseCompleted"
                ).Count,
                Is.EqualTo(1)
            );
            Assert.That(
                json,
                Does.Contain("\"item\":\"Três pontos alcançados\"")
            );

            Object.DestroyImmediate(target);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void MetaPorValor_RejeitaNumeroNaoFinito()
        {
            GameObject target = new GameObject("MetaInvalida");
            target.AddComponent<LudusSemanticBridge>();
            LudusProgressThresholdAdapter adapter =
                target.AddComponent<LudusProgressThresholdAdapter>();

            bool evaluated = adapter.TrySetValueAndEvaluate(
                float.NaN,
                out _,
                out string errorMessage
            );

            Assert.That(evaluated, Is.False);
            Assert.That(errorMessage, Does.Contain("números finitos"));

            Object.DestroyImmediate(target);
        }
    }
}
