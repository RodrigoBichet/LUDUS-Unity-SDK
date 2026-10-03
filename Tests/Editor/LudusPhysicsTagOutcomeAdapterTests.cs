using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusPhysicsTagOutcomeAdapterTests
    {
        [Test]
        public void AdaptadorDeContato_RegistraAcertoPelaTagDoObjetoPai()
        {
            LudusSdkConfig config = CreateConfig();
            GameObject host = new GameObject("BaseLudusContatoTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject target = new GameObject("DestinoContato");
            target.AddComponent<LudusSemanticBridge>();
            LudusPhysicsTagOutcomeAdapter adapter =
                target.AddComponent<LudusPhysicsTagOutcomeAdapter>();
            adapter.Configure(
                LudusPhysicsContactMode.Trigger2D,
                "Player",
                "Objeto esperado",
                true
            );

            GameObject parent = new GameObject("Objeto correto");
            parent.tag = "Player";
            GameObject childCollider = new GameObject("Collider filho");
            childCollider.transform.SetParent(parent.transform);

            Assert.That(
                LudusSdk.TryStartSession(
                    "000000000000000000000022",
                    "Estudante Fictício",
                    out string startError
                ),
                Is.True,
                startError
            );
            Assert.That(
                adapter.TryEvaluateContact(
                    childCollider,
                    out bool correct,
                    out string contactError
                ),
                Is.True,
                contactError
            );
            Assert.That(correct, Is.True);
            Assert.That(
                LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(
                controller.LastCompletedSession.metrics.totalCorrect,
                Is.EqualTo(1)
            );
            Assert.That(json, Does.Contain("\"CorrectMatch\""));
            Assert.That(json, Does.Contain("\"item\":\"Objeto correto\""));

            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void AdaptadorDeContato_RegistraErroQuandoTagDiverge()
        {
            LudusSdkConfig config = CreateConfig();
            GameObject host = new GameObject("BaseLudusContatoErroTeste");
            LudusSessionController controller =
                host.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject target = new GameObject("DestinoContatoErro");
            target.AddComponent<LudusSemanticBridge>();
            LudusPhysicsTagOutcomeAdapter adapter =
                target.AddComponent<LudusPhysicsTagOutcomeAdapter>();
            adapter.Configure(
                LudusPhysicsContactMode.Collision3D,
                "Player",
                "Objeto esperado"
            );

            GameObject contactedObject = new GameObject("Objeto incorreto");
            contactedObject.tag = "MainCamera";

            Assert.That(
                LudusSdk.TryStartSession(
                    "000000000000000000000023",
                    "Estudante Fictício",
                    out string startError
                ),
                Is.True,
                startError
            );
            Assert.That(
                adapter.TryEvaluateContact(
                    contactedObject,
                    out bool correct,
                    out string contactError
                ),
                Is.True,
                contactError
            );
            Assert.That(correct, Is.False);
            Assert.That(
                LudusSdk.TryEndSession(
                    out string json,
                    out string endError
                ),
                Is.True,
                endError
            );

            Assert.That(
                controller.LastCompletedSession.metrics.totalWrong,
                Is.EqualTo(1)
            );
            Assert.That(json, Does.Contain("\"WrongMatch\""));
            Assert.That(
                json,
                Does.Contain("\"draggedItem\":\"Objeto incorreto\"")
            );

            Object.DestroyImmediate(contactedObject);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(config);
        }

        private static LudusSdkConfig CreateConfig()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "jogo-contato-tag";
            config.capabilities.correctWrong = true;
            return config;
        }
    }
}
