using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusButtonOutcomeAdapterTests
    {
        [Test]
        public void AdaptadoresDeBotao_PreservamListenerERegistramResultados()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-quiz-botoes";
            config.capabilities.correctWrong = true;

            GameObject host = new GameObject("BaseLudusQuizTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject correctOption = CreateOption(
                "Alternativa A",
                LudusConfiguredOutcome.Correct,
                "Planeta Terra",
                "Planeta Terra",
                out Button correctButton
            );
            GameObject wrongOption = CreateOption(
                "Alternativa B",
                LudusConfiguredOutcome.Incorrect,
                "Lua",
                "Planeta Terra",
                out Button wrongButton
            );

            int originalListenerCalls = 0;
            correctButton.onClick.AddListener(
                () => originalListenerCalls++
            );

            Assert.That(
                LudusSdk.TryStartSession(
                    "000000000000000000000024",
                    "Estudante Fictício",
                    out string startError
                ),
                Is.True,
                startError
            );

            correctButton.onClick.Invoke();
            wrongButton.onClick.Invoke();

            Assert.That(
                LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(originalListenerCalls, Is.EqualTo(1));
            Assert.That(
                controller.LastCompletedSession.metrics.totalCorrect,
                Is.EqualTo(1)
            );
            Assert.That(
                controller.LastCompletedSession.metrics.totalWrong,
                Is.EqualTo(1)
            );
            Assert.That(json, Does.Contain("\"item\":\"Planeta Terra\""));
            Assert.That(json, Does.Contain("\"draggedItem\":\"Lua\""));
            Assert.That(
                json,
                Does.Contain("\"expectedItem\":\"Planeta Terra\"")
            );

            Object.DestroyImmediate(wrongOption);
            Object.DestroyImmediate(correctOption);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        private static GameObject CreateOption(
            string objectName,
            LudusConfiguredOutcome outcome,
            string answerName,
            string expectedAnswerName,
            out Button button
        )
        {
            GameObject option = new GameObject(objectName);
            button = option.AddComponent<Button>();
            option.AddComponent<LudusSemanticBridge>();
            LudusButtonOutcomeAdapter adapter =
                option.AddComponent<LudusButtonOutcomeAdapter>();
            adapter.Configure(outcome, answerName, expectedAnswerName);
            return option;
        }
    }
}
