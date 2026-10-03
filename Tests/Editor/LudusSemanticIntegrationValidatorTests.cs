using System.Linq;
using LudusSDK.Editor;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusSemanticIntegrationValidatorTests
    {
        private Scene testScene;

        [SetUp]
        public void SetUp()
        {
            testScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            foreach (LudusSdkConfig config in
                Resources.FindObjectsOfTypeAll<LudusSdkConfig>())
            {
                if (!UnityEditor.EditorUtility.IsPersistent(config))
                {
                    Object.DestroyImmediate(config);
                }
            }
        }

        [Test]
        public void CenaSemBase_RelataControladorAusente()
        {
            GameObject target = new GameObject("Alternativa");
            target.AddComponent<Button>();
            target.AddComponent<LudusButtonOutcomeAdapter>();

            var issues =
                LudusSemanticIntegrationValidator.ValidateScene(testScene);

            Assert.That(
                issues.Any(issue =>
                    issue.Code ==
                    LudusSemanticValidationCode.MissingSessionController),
                Is.True
            );
        }

        [Test]
        public void BotaoComCapacidadeDesligada_RelataAcertoErro()
        {
            LudusSdkConfig config = CreateBase();
            config.capabilities.correctWrong = false;

            GameObject target = new GameObject("Alternativa");
            target.AddComponent<Button>();
            target.AddComponent<LudusButtonOutcomeAdapter>();

            var issues =
                LudusSemanticIntegrationValidator.ValidateScene(testScene);

            Assert.That(
                issues.Any(issue =>
                    issue.Code ==
                    LudusSemanticValidationCode.CorrectWrongDisabled),
                Is.True
            );

            config.capabilities.correctWrong = true;
            issues = LudusSemanticIntegrationValidator.ValidateScene(
                testScene
            );

            Assert.That(
                issues.Any(issue =>
                    issue.Code ==
                    LudusSemanticValidationCode.CorrectWrongDisabled),
                Is.False
            );
        }

        [Test]
        public void TentativaDeArrasteExigeEventosPersonalizados()
        {
            LudusSdkConfig config = CreateBase();
            config.capabilities.customEvents = false;
            config.capabilities.correctWrong = false;

            GameObject target = new GameObject("Destino");
            LudusTagMatchDropAdapter adapter =
                target.AddComponent<LudusTagMatchDropAdapter>();
            adapter.Configure("Untagged", "Destino", "Peça", true, false);

            var issues =
                LudusSemanticIntegrationValidator.ValidateScene(testScene);

            Assert.That(
                issues.Any(issue =>
                    issue.Code ==
                    LudusSemanticValidationCode.CustomEventsDisabled),
                Is.True
            );
            Assert.That(
                issues.Any(issue =>
                    issue.Code ==
                    LudusSemanticValidationCode.CorrectWrongDisabled),
                Is.False
            );
        }

        [Test]
        public void MetaComCapacidadesAtivas_NaoRelataIncompatibilidade()
        {
            LudusSdkConfig config = CreateBase();
            config.capabilities.correctWrong = true;
            config.capabilities.phaseEvents = true;

            GameObject target = new GameObject("Meta");
            LudusProgressThresholdAdapter adapter =
                target.AddComponent<LudusProgressThresholdAdapter>();
            adapter.Configure(
                3f,
                LudusThresholdComparison.AtLeast,
                LudusThresholdAction.RecordCorrectAndCompletePhase,
                "Três pontos"
            );

            var issues =
                LudusSemanticIntegrationValidator.ValidateScene(testScene);

            Assert.That(
                issues.Any(issue =>
                    issue.Code ==
                    LudusSemanticValidationCode.CorrectWrongDisabled ||
                    issue.Code ==
                    LudusSemanticValidationCode.PhaseEventsDisabled),
                Is.False
            );
        }

        private static LudusSdkConfig CreateBase()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de teste";

            GameObject baseObject = new GameObject("LUDUS SDK");
            LudusSessionController controller =
                baseObject.AddComponent<LudusSessionController>();
            controller.Configure(config);
            return config;
        }
    }
}
