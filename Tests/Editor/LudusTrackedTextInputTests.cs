using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTrackedTextInputTests
    {
        [Test]
        public void Lifecycle_ComCampoTexto_RegistraResumoSemConteudo()
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
                    "Resposta do estudante",
                    "text-input",
                    "completed",
                    14,
                    new Vector2(400f, 250f)
                ),
                out string recordError
            );
            lifecycle.TryEndAndSerialize(out string json, out _);

            Assert.That(recorded, Is.True, recordError);
            Assert.That(json, Does.Contain("\"interactionKind\":\"text-input\""));
            Assert.That(json, Does.Contain("\"action\":\"completed\""));
            Assert.That(json, Does.Contain("\"characterCount\":14"));
            Assert.That(json, Does.Contain("\"wasEmpty\":false"));
            Assert.That(json, Does.Not.Contain("conteúdo digitado"));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Lifecycle_ComCampoVazio_RegistraSomenteResumoVazio()
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
                    "Resposta opcional",
                    "text-input",
                    "completed",
                    0
                ),
                out _
            );
            lifecycle.TryEndAndSerialize(out string json, out _);

            Assert.That(json, Does.Contain("\"characterCount\":0"));
            Assert.That(json, Does.Contain("\"wasEmpty\":true"));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void TrackedTextInput_DetectaCamposClassicoETextMeshPro()
        {
            GameObject legacyHost = new GameObject(
                "CampoClassico",
                typeof(RectTransform),
                typeof(InputField)
            );
            GameObject tmpHost = new GameObject(
                "CampoTMP",
                typeof(RectTransform),
                typeof(TMP_InputField)
            );

            Assert.That(LudusTrackedTextInput.CanTrack(legacyHost), Is.True);
            Assert.That(LudusTrackedTextInput.CanTrack(tmpHost), Is.True);

            Object.DestroyImmediate(legacyHost);
            Object.DestroyImmediate(tmpHost);
        }

        [Test]
        public void TrackedTextInput_SemNomePersonalizado_UsaNomeDoObjeto()
        {
            GameObject host = new GameObject(
                "RespostaDaAtividade",
                typeof(RectTransform),
                typeof(InputField)
            );
            LudusTrackedTextInput trackedTextInput =
                host.AddComponent<LudusTrackedTextInput>();

            trackedTextInput.Configure(string.Empty);

            Assert.That(
                trackedTextInput.DashboardName,
                Is.EqualTo("RespostaDaAtividade")
            );

            Object.DestroyImmediate(host);
        }
    }
}
