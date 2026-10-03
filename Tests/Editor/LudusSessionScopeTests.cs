using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusSessionScopeTests
    {
        [Test]
        public void Escopo_IniciaInvocaEventosEEncerraSessaoPropria()
        {
            LudusSdkConfig config = CreateConfig();
            GameObject baseObject = CreateBase(config);
            GameObject scopeObject = new GameObject("Categoria Ação");
            LudusSessionScope scope =
                scopeObject.AddComponent<LudusSessionScope>();
            scope.Configure("Ação", false, false);

            int startedEventCount = 0;
            int endingEventCount = 0;
            scope.OnSessionStarted.AddListener(() => startedEventCount++);
            scope.OnBeforeSessionEnded.AddListener(
                () => endingEventCount++
            );

            Assert.That(
                scope.TryStartOwnedSession(out string startError),
                Is.True,
                startError
            );
            Assert.That(scope.OwnsActiveSession, Is.True);
            Assert.That(startedEventCount, Is.EqualTo(1));
            Assert.That(
                scope.TryEndOwnedSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            LudusSessionController controller =
                baseObject.GetComponent<LudusSessionController>();
            Assert.That(scope.OwnsActiveSession, Is.False);
            Assert.That(controller.HasActiveSession, Is.False);
            Assert.That(endingEventCount, Is.EqualTo(1));
            Assert.That(json, Does.Contain("\"playerId\":\"Ação\""));

            DestroyObjects(scopeObject, baseObject, config);
        }

        [Test]
        public void Escopo_DesativadoEncerraComoFallback()
        {
            LudusSdkConfig config = CreateConfig();
            GameObject baseObject = CreateBase(config);
            GameObject scopeObject = new GameObject("Atividade");
            LudusSessionScope scope =
                scopeObject.AddComponent<LudusSessionScope>();
            scope.Configure("Atividade", false, true);

            Assert.That(
                scope.TryStartOwnedSession(out string startError),
                Is.True,
                startError
            );

            MethodInfo onDisable = typeof(LudusSessionScope).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(onDisable, Is.Not.Null);
            onDisable.Invoke(scope, null);

            LudusSessionController controller =
                baseObject.GetComponent<LudusSessionController>();

            if (Application.isPlaying)
            {
                Assert.That(controller.HasActiveSession, Is.False);
            }
            else
            {
                Assert.That(controller.HasActiveSession, Is.True);
                Assert.That(
                    scope.TryEndOwnedSession(out _, out string endError),
                    Is.True,
                    endError
                );
            }

            DestroyObjects(scopeObject, baseObject, config);
        }

        [Test]
        public void Escopo_EncerraComMesmaBaseQuandoBuscaGlobalNaoEncontraAtivos()
        {
            LudusSdkConfig config = CreateConfig();
            GameObject baseObject = CreateBase(config);
            LudusSessionController controller =
                baseObject.GetComponent<LudusSessionController>();
            GameObject scopeObject = new GameObject("Categoria persistente");
            LudusSessionScope scope =
                scopeObject.AddComponent<LudusSessionScope>();
            scope.Configure("Ação", false, false);

            Assert.That(
                scope.TryStartOwnedSession(out string startError),
                Is.True,
                startError
            );

            baseObject.SetActive(false);

            Assert.That(
                LudusSdk.TryEndSession(out _, out string globalError),
                Is.False
            );
            Assert.That(globalError, Does.Contain("base LUDUS SDK ativa"));
            Assert.That(
                scope.TryEndOwnedSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );
            Assert.That(controller.HasActiveSession, Is.False);
            Assert.That(scope.OwnsActiveSession, Is.False);
            Assert.That(json, Does.Contain("\"playerId\":\"Ação\""));

            DestroyObjects(scopeObject, baseObject, config);
        }

        [Test]
        public void Escopo_NaoEncerraSessaoIniciadaPorOutroFluxo()
        {
            LudusSdkConfig config = CreateConfig();
            GameObject baseObject = CreateBase(config);
            LudusSessionController controller =
                baseObject.GetComponent<LudusSessionController>();
            GameObject scopeObject = new GameObject("Escopo visitante");
            LudusSessionScope scope =
                scopeObject.AddComponent<LudusSessionScope>();
            scope.Configure("Visitante", false, true);

            Assert.That(
                LudusSdk.TryStartSessionForManualImport(
                    "Fluxo externo",
                    out string startError
                ),
                Is.True,
                startError
            );
            Assert.That(
                scope.TryEndOwnedSession(out _, out string endError),
                Is.False
            );
            Assert.That(endError, Does.Contain("não iniciou"));
            Assert.That(controller.HasActiveSession, Is.True);

            Assert.That(
                LudusSdk.TryEndSession(out _, out string cleanupError),
                Is.True,
                cleanupError
            );

            DestroyObjects(scopeObject, baseObject, config);
        }

        private static LudusSdkConfig CreateConfig()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Jogo de teste";
            config.sendOnSessionEnd = false;
            config.saveLocalCopyOnSessionEnd = false;
            return config;
        }

        private static GameObject CreateBase(LudusSdkConfig config)
        {
            GameObject baseObject = new GameObject("LUDUS SDK");
            LudusSessionController controller =
                baseObject.AddComponent<LudusSessionController>();
            controller.Configure(config);
            return baseObject;
        }

        private static void DestroyObjects(
            GameObject scopeObject,
            GameObject baseObject,
            LudusSdkConfig config
        )
        {
            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(baseObject);
            Object.DestroyImmediate(config);
        }
    }
}
