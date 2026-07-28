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

[Test]
public void SessionLifecycle_ComContextoAtivo_RegistraCliqueETrajetoria()
{
    LudusSdkConfig config =
        ScriptableObject.CreateInstance<LudusSdkConfig>();

    config.gameId = "jogo-teste";
    config.gameVersion = "0.1.0-teste";

    LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();

    bool started = lifecycle.TryStartSession(
        config,
        new LudusParticipant(
            "000000000000000000000005",
            "Estudante Fictício"
        ),
        new LudusViewport(1280, 720, "pixel", "bottom-left"),
        out string startError
    );

    bool clickOutsideContext = lifecycle.TryRecordClick(
        100f,
        200f,
        out string outsideContextError
    );

    bool contextStarted = lifecycle.TryBeginCaptureContext(
        new LudusCaptureContext(
            "Atividade principal",
            "canvas",
            "Observação de interação"
        ),
        out string contextError
    );

    bool clickRecorded = lifecycle.TryRecordClick(
        100f,
        200f,
        out string clickError
    );

    bool mousePointRecorded = lifecycle.TryRecordMousePoint(
        150f,
        250f,
        out string mousePointError
    );

    bool ended = lifecycle.TryEndAndSerialize(
        out string json,
        out string endError
    );

    Object.DestroyImmediate(config);

    Assert.That(started, Is.True, startError);
    Assert.That(clickOutsideContext, Is.False);
    Assert.That(outsideContextError, Does.Contain("contexto"));
    Assert.That(contextStarted, Is.True, contextError);
    Assert.That(clickRecorded, Is.True, clickError);
    Assert.That(mousePointRecorded, Is.True, mousePointError);
    Assert.That(ended, Is.True, endError);
    Assert.That(
        lifecycle.LastCompletedSession.clicks.Count,
        Is.EqualTo(1)
    );
    Assert.That(
        lifecycle.LastCompletedSession.mousePath.Count,
        Is.EqualTo(1)
    );
    Assert.That(
        lifecycle.LastCompletedSession.metrics.totalClicks,
        Is.EqualTo(1)
    );
    Assert.That(json, Does.Contain("\"clicks\":["));
    Assert.That(json, Does.Contain("\"mousePath\":["));
}

        [Test]
        public void SessionController_ComConfigFicticia_IniciaEExportaSessao()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";

            GameObject host = new GameObject("LudusSessionControllerTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();

            controller.Configure(config);

            bool started = controller.TryStartSession(
                "000000000000000000000006",
                "Estudante Fictício",
                out string startError
            );

            bool ended = controller.TryEndAndSerialize(
                out string json,
                out string endError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(ended, Is.True, endError);
            Assert.That(controller.HasActiveSession, Is.False);
            Assert.That(json, Does.Contain("\"sessionId\":"));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void LudusSdk_ComUmaBaseAtiva_IniciaEEncerraSemReferenciaManual()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-teste";

            GameObject host = new GameObject("LudusSdkFacadeTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            bool started = LudusSdk.TryStartSession(
                "000000000000000000000011",
                "Estudante Fictício",
                out string startError
            );
            bool ended = LudusSdk.TryEndSession(
                out string json,
                out string endError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(ended, Is.True, endError);
            Assert.That(json, Does.Contain("\"sessionId\":"));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void LudusSdk_ComDuasBasesAtivas_ExplicaOndeEstaoAsBases()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-teste";

            GameObject firstHost = new GameObject("BaseLudusPrimeira");
            LudusSessionController firstController =
                firstHost.AddComponent<LudusSessionController>();
            firstController.Configure(config);

            GameObject secondHost = new GameObject("BaseLudusSegunda");
            LudusSessionController secondController =
                secondHost.AddComponent<LudusSessionController>();
            secondController.Configure(config);

            bool started = LudusSdk.TryStartSession(
                "000000000000000000000011",
                "Estudante Fictício",
                out string errorMessage
            );

            Assert.That(started, Is.False);
            Assert.That(errorMessage, Does.Contain("2 bases LUDUS SDK"));
            Assert.That(errorMessage, Does.Contain("BaseLudusPrimeira"));
            Assert.That(errorMessage, Does.Contain("BaseLudusSegunda"));

            Object.DestroyImmediate(firstHost);
            Object.DestroyImmediate(secondHost);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Config_ComCenasSelecionadas_CapturaSomenteCenasMarcadas()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.sceneCaptureMode = LudusSceneCaptureMode.SelectedScenes;
            config.selectedSceneNames.Add("Atividade 1");
            config.selectedSceneNames.Add("Atividade 3");

            Assert.That(config.ShouldCaptureScene("Atividade 1"), Is.True);
            Assert.That(config.ShouldCaptureScene("Menu"), Is.False);
            Assert.That(config.ShouldCaptureScene("Atividade 3"), Is.True);

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Config_ComCenasSelecionadasSemEscolhas_RejeitaSessao()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.sceneCaptureMode = LudusSceneCaptureMode.SelectedScenes;

            bool valid = config.TryValidateForSession(out string errorMessage);

            Assert.That(valid, Is.False);
            Assert.That(errorMessage, Does.Contain("ao menos uma cena"));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void SceneCoordinator_ComTodasAsCenas_IniciaRecorteAutomatico()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-teste";

            GameObject host = new GameObject("LudusSceneCoordinatorTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            LudusSceneCaptureCoordinator coordinator =
                host.AddComponent<LudusSceneCaptureCoordinator>();
            controller.Configure(config);
            coordinator.sessionController = controller;

            bool started = controller.TryStartSession(
                "000000000000000000000008",
                "Estudante Fictício",
                out string startError
            );

            coordinator.RefreshCurrentSceneCapture();

            Assert.That(started, Is.True, startError);
            Assert.That(controller.HasActiveCaptureContext, Is.True);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SceneCoordinator_ComCenaAtivaNaoSelecionada_MantemCapturaPausada()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-teste";
            config.sceneCaptureMode = LudusSceneCaptureMode.SelectedScenes;
            config.selectedSceneNames.Add("OutraCena");

            GameObject host = new GameObject("LudusSceneCoordinatorPausadoTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            LudusSceneCaptureCoordinator coordinator =
                host.AddComponent<LudusSceneCaptureCoordinator>();
            controller.Configure(config);
            coordinator.sessionController = controller;

            bool started = controller.TryStartSession(
                "000000000000000000000009",
                "Estudante Fictício",
                out string startError
            );

            coordinator.RefreshCurrentSceneCapture();

            Assert.That(started, Is.True, startError);
            Assert.That(controller.HasActiveCaptureContext, Is.False);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SessionController_ComContextoAtivo_ExpõeEstadoECapturaInteração()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";

            GameObject host = new GameObject("LudusPointerTrackerTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();

            controller.Configure(config);

            bool started = controller.TryStartSession(
                "000000000000000000000007",
                "Estudante Fictício",
                out string startError
            );

            bool clickOutsideContext = controller.TryRecordClick(
                new Vector2(100f, 200f),
                out string outsideContextError
            );

            bool contextStarted = controller.TryBeginCaptureContext(
                "Atividade de teste",
                "canvas",
                "Validação de captura",
                out string contextError
            );

            bool clickRecorded = controller.TryRecordClick(
                new Vector2(100f, 200f),
                out string clickError
            );

            bool mousePointRecorded = controller.TryRecordMousePoint(
                new Vector2(150f, 250f),
                out string mousePointError
            );

            bool contextWasActive = controller.HasActiveCaptureContext;

            bool ended = controller.TryEndAndSerialize(
                out string json,
                out string endError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(clickOutsideContext, Is.False);
            Assert.That(outsideContextError, Does.Contain("contexto"));
            Assert.That(contextStarted, Is.True, contextError);
            Assert.That(contextWasActive, Is.True);
            Assert.That(clickRecorded, Is.True, clickError);
            Assert.That(mousePointRecorded, Is.True, mousePointError);
            Assert.That(ended, Is.True, endError);
            Assert.That(controller.HasActiveCaptureContext, Is.False);
            Assert.That(
                controller.LastCompletedSession.clicks.Count,
                Is.EqualTo(1)
            );
            Assert.That(
                controller.LastCompletedSession.mousePath.Count,
                Is.EqualTo(1)
            );
            Assert.That(json, Does.Contain("\"mousePath\":["));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SessionLifecycle_AoEncerrarContextoSubstituido_PreservaOAtual()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";

            LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();
            LudusCaptureContext firstContext = new LudusCaptureContext(
                "Painel inicial",
                "canvas"
            );
            LudusCaptureContext secondContext = new LudusCaptureContext(
                "Atividade principal",
                "activity"
            );

            bool started = lifecycle.TryStartSession(
                config,
                new LudusParticipant(
                    "000000000000000000000008",
                    "Estudante Fictício"
                ),
                new LudusViewport(1280, 720, "pixel", "bottom-left"),
                out string startError
            );

            bool firstContextStarted = lifecycle.TryBeginCaptureContext(
                firstContext,
                out string firstContextError
            );
            bool secondContextStarted = lifecycle.TryBeginCaptureContext(
                secondContext,
                out string secondContextError
            );
            bool oldContextEnded = lifecycle.TryEndCaptureContext(
                firstContext,
                out string oldContextError
            );
            bool contextRemainedActive = lifecycle.HasActiveCaptureContext;
            bool activeContextEnded = lifecycle.TryEndCaptureContext(
                secondContext,
                out string activeContextError
            );

            Object.DestroyImmediate(config);

            Assert.That(started, Is.True, startError);
            Assert.That(firstContextStarted, Is.True, firstContextError);
            Assert.That(secondContextStarted, Is.True, secondContextError);
            Assert.That(oldContextEnded, Is.False);
            Assert.That(oldContextError, Does.Contain("não é mais"));
            Assert.That(contextRemainedActive, Is.True);
            Assert.That(activeContextEnded, Is.True, activeContextError);
            Assert.That(lifecycle.HasActiveCaptureContext, Is.False);
        }

        [Test]
        public void SessionController_AoSerializar_DisparaEventoComJson()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";

            GameObject host = new GameObject("LudusExporterTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            bool eventReceived = false;
            string eventJson = string.Empty;
            controller.SessionSerialized += (_, json) =>
            {
                eventReceived = true;
                eventJson = json;
            };

            bool started = controller.TryStartSession(
                "000000000000000000000009",
                "Estudante Fictício",
                out string startError
            );
            bool ended = controller.TryEndAndSerialize(
                out string json,
                out string endError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(ended, Is.True, endError);
            Assert.That(eventReceived, Is.True);
            Assert.That(eventJson, Is.EqualTo(json));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Config_ComPastaDeFallbackInsegura_RejeitaSessao()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameId = "jogo-teste";
            config.gameVersion = "0.1.0-teste";
            config.enableLocalFallback = true;
            config.fallbackFolderName = "../fora-do-escopo";

            bool valid = config.TryValidateForSession(
                out string errorMessage
            );

            Object.DestroyImmediate(config);

            Assert.That(valid, Is.False);
            Assert.That(errorMessage, Does.Contain("fallbackFolderName"));
        }

        [Test]
        public void Config_ComNomeDoJogo_GeraIdentificadorCanonico()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            config.gameName = "Histórias Divertidas!";
            config.gameVersion = "1.0.0";

            LudusSession session = LudusSession.Create(
                config,
                new LudusParticipant(
                    "000000000000000000000010",
                    "Estudante Fictício"
                ),
                new LudusViewport(1280, 720, "pixel", "bottom-left")
            );

            Object.DestroyImmediate(config);

            Assert.That(session.gameId, Is.EqualTo("historias-divertidas"));
            Assert.That(session.gameVersion, Is.EqualTo("1.0.0"));
        }

        [Test]
        public void Config_Padrao_HabilitaApenasColetaImplementada()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();

            Assert.That(config.capabilities.clicks, Is.True);
            Assert.That(config.capabilities.mousePath, Is.True);
            Assert.That(config.capabilities.customEvents, Is.True);
            Assert.That(config.capabilities.dragPath, Is.False);
            Assert.That(config.capabilities.inactivity, Is.False);

            Object.DestroyImmediate(config);
        }

        [Test]
        public void ContextTrigger_SemTituloOuReferencia_UsaObjetoEControladorAtivo()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de Teste";

            GameObject controllerHost =
                new GameObject("Base LUDUS de Teste");
            LudusSessionController controller =
                controllerHost.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject contextHost = new GameObject("Painel da atividade");
            LudusCaptureContextTrigger trigger =
                contextHost.AddComponent<LudusCaptureContextTrigger>();
            trigger.sessionController = null;

            bool started = controller.TryStartSession(
                "000000000000000000000011",
                "Estudante Fictício",
                out string startError
            );
            bool contextStarted = trigger.TryBeginCapture(
                out string contextError
            );
            bool ended = controller.TryEndAndSerialize(
                out string json,
                out string endError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(contextStarted, Is.True, contextError);
            Assert.That(ended, Is.True, endError);
            Assert.That(json, Does.Contain("Painel da atividade"));

            Object.DestroyImmediate(contextHost);
            Object.DestroyImmediate(controllerHost);
            Object.DestroyImmediate(config);
        }
    }
}
