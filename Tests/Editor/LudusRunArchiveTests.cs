using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusRunArchiveTests
    {
        [SetUp]
        public void SetUp()
        {
            LudusSdk.BeginNewRun();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (
                LudusSessionController controller in
                Object.FindObjectsByType<LudusSessionController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
            )
            {
                Object.DestroyImmediate(controller.gameObject);
            }

            LudusSdk.BeginNewRun();
        }

        [Test]
        public void Create_DuasSessoesNaMesmaExecucao_CompartilhaRunENumeraTentativas()
        {
            LudusSdkConfig config = CreateConfig();
            string runId = LudusRunContext.CurrentRunId;

            LudusSession first = CreateSession(config, "Primeira");
            LudusSession second = CreateSession(config, "Segunda");

            Assert.That(first.runId, Is.EqualTo(runId));
            Assert.That(second.runId, Is.EqualTo(runId));
            Assert.That(first.attemptNumber, Is.EqualTo(1));
            Assert.That(second.attemptNumber, Is.EqualTo(2));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void BeginNewRun_ReiniciaIdentificadorArquivoETentativas()
        {
            LudusSdkConfig config = CreateConfig();
            LudusSession first = CreateSession(config, "Primeira");

            string nextRunId = LudusSdk.BeginNewRun();
            LudusSession next = CreateSession(config, "Nova execução");

            Assert.That(nextRunId, Is.Not.EqualTo(first.runId));
            Assert.That(next.runId, Is.EqualTo(nextRunId));
            Assert.That(next.attemptNumber, Is.EqualTo(1));
            Assert.That(LudusRunArchive.CompletedSessionCount, Is.Zero);

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Snapshot_NaoEncerraSessaoAtivaEUsaEstadoEmAndamento()
        {
            LudusSdkConfig config = CreateConfig();
            LudusSessionLifecycle lifecycle = new LudusSessionLifecycle();
            bool started = lifecycle.TryStartSession(
                config,
                CreateParticipant("Snapshot"),
                CreateViewport(),
                out string startError
            );

            bool created = lifecycle.TryCreateSnapshot(
                out LudusSession snapshot,
                out string snapshotError
            );
            bool serialized = LudusJsonSerializer.TrySerialize(
                snapshot,
                out string json,
                out string serializationError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(created, Is.True, snapshotError);
            Assert.That(serialized, Is.True, serializationError);
            Assert.That(lifecycle.HasActiveSession, Is.True);
            Assert.That(snapshot.status, Is.EqualTo("in_progress"));
            Assert.That(snapshot.endedAt, Is.Empty);
            Assert.That(json, Does.Contain("\"status\":\"in_progress\""));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Serialize_ConcluidaSemEndedAt_RejeitaEstadoIncoerente()
        {
            LudusSdkConfig config = CreateConfig();
            LudusSession session = CreateSession(config, "Incoerente");
            session.status = "completed";
            session.endedAt = string.Empty;

            bool serialized = LudusJsonSerializer.TrySerialize(
                session,
                out _,
                out string errorMessage
            );

            Assert.That(serialized, Is.False);
            Assert.That(errorMessage, Does.Contain("endedAt"));
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Bundle_ReuneConcluidasEAtualSemDuplicarSessao()
        {
            LudusSdkConfig config = CreateConfig();
            LudusSessionController controller = CreateControllerWithExporter(config);

            Assert.That(
                controller.TryStartSession(
                    "000000000000000000000101",
                    "Primeira",
                    out string firstStartError
                ),
                Is.True,
                firstStartError
            );
            Assert.That(
                controller.TryEndAndSerialize(
                    out string firstJson,
                    out string firstEndError
                ),
                Is.True,
                firstEndError
            );

            string firstSessionId = controller.LastCompletedSession.sessionId;

            Assert.That(
                controller.TryStartSession(
                    "000000000000000000000102",
                    "Segunda",
                    out string secondStartError
                ),
                Is.True,
                secondStartError
            );
            Assert.That(
                LudusSdk.TryCreateCurrentRunBundle(
                    out string bundle,
                    out string bundleError
                ),
                Is.True,
                bundleError
            );

            Assert.That(LudusRunArchive.CompletedSessionCount, Is.EqualTo(1));
            Assert.That(CountOccurrences(bundle, firstSessionId), Is.EqualTo(1));
            Assert.That(CountOccurrences(bundle, "\"sessionId\""), Is.EqualTo(2));
            Assert.That(bundle, Does.Contain("\"type\":\"ludus-session-bundle\""));
            Assert.That(bundle, Does.Contain("\"status\":\"completed\""));
            Assert.That(bundle, Does.Contain("\"status\":\"in_progress\""));
            Assert.That(firstJson, Does.Contain(firstSessionId));

            Object.DestroyImmediate(config);
        }

        private static LudusSdkConfig CreateConfig()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-teste-execucao";
            config.gameVersion = "1.0.0-teste";
            config.sendOnSessionEnd = false;
            config.downloadJsonOnSessionEnd = false;
            config.saveLocalCopyOnSessionEnd = false;
            config.enableLocalFallback = false;
            return config;
        }

        private static LudusSession CreateSession(
            LudusSdkConfig config,
            string playerId
        )
        {
            return LudusSession.Create(
                config,
                CreateParticipant(playerId),
                CreateViewport()
            );
        }

        private static LudusParticipant CreateParticipant(string playerId)
        {
            return new LudusParticipant(
                "000000000000000000000099",
                playerId
            );
        }

        private static LudusViewport CreateViewport()
        {
            return new LudusViewport(1280, 720, "pixel", "bottom-left");
        }

        private static LudusSessionController CreateControllerWithExporter(
            LudusSdkConfig config
        )
        {
            GameObject root = new GameObject("LUDUS Run Test");
            LudusSessionController controller =
                root.AddComponent<LudusSessionController>();
            controller.Configure(config);

            LudusSessionExporter exporter =
                root.AddComponent<LudusSessionExporter>();
            exporter.sessionController = controller;
            typeof(LudusSessionExporter)
                .GetMethod(
                    "OnEnable",
                    BindingFlags.Instance | BindingFlags.NonPublic
                )
                .Invoke(exporter, null);
            return controller;
        }

        private static int CountOccurrences(string value, string search)
        {
            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(search, index)) >= 0)
            {
                count++;
                index += search.Length;
            }

            return count;
        }
    }
}
