using System.Collections.Generic;
using LudusSDK.Editor;
using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusVisualMapSetupTests
    {
        [Test]
        public void Etapa2_AtivaTodasAsCenasSemDuplicarCoordenador()
        {
            GameObject root = new GameObject("LUDUS SDK — Modo simples");
            LudusSessionController controller =
                root.AddComponent<LudusSessionController>();
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            controller.config = config;

            try
            {
                Assert.That(
                    LudusSetupAssistant.TryEnableVisualMap(
                        LudusSceneScreenshotMode.AllTrackedScenes,
                        new List<string>(),
                        out string firstMessage
                    ),
                    Is.True,
                    firstMessage
                );
                Assert.That(config.capabilities.screenshots, Is.True);
                Assert.That(config.captureScreenshotOnContextStart, Is.True);
                Assert.That(
                    config.sceneScreenshotMode,
                    Is.EqualTo(LudusSceneScreenshotMode.AllTrackedScenes)
                );
                Assert.That(
                    root.GetComponents<LudusSceneCaptureCoordinator>().Length,
                    Is.EqualTo(1)
                );

                Assert.That(
                    LudusSetupAssistant.TryEnableVisualMap(
                        LudusSceneScreenshotMode.AllTrackedScenes,
                        new List<string>(),
                        out string secondMessage
                    ),
                    Is.True,
                    secondMessage
                );
                Assert.That(
                    root.GetComponents<LudusSceneCaptureCoordinator>().Length,
                    Is.EqualTo(1)
                );
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Etapa2_PreservaSomenteAsCenasMarcadas()
        {
            GameObject root = new GameObject("LUDUS SDK — Modo simples");
            LudusSessionController controller =
                root.AddComponent<LudusSessionController>();
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            controller.config = config;

            try
            {
                Assert.That(
                    LudusSetupAssistant.TryEnableVisualMap(
                        LudusSceneScreenshotMode.SelectedScenes,
                        new[] { " Fase01 ", "Fase02", "Fase01" },
                        out string message
                    ),
                    Is.True,
                    message
                );
                Assert.That(
                    config.sceneScreenshotMode,
                    Is.EqualTo(LudusSceneScreenshotMode.SelectedScenes)
                );
                Assert.That(
                    config.selectedScreenshotSceneNames,
                    Is.EqualTo(new[] { "Fase01", "Fase02" })
                );
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(config);
            }
        }
    }
}
