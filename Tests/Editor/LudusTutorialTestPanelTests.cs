using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialTestPanelTests
    {
        [Test]
        public void SairDoPlayMode_EncerraSessaoAtivaUmaUnicaVez()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameId = "tutorial-ludus";

            GameObject controllerObject = new GameObject("LUDUS SDK");
            LudusSessionController controller =
                controllerObject.AddComponent<LudusSessionController>();
            controller.Configure(config);

            GameObject panelObject = new GameObject("Painel tutorial");
            LudusTutorialTestPanel panel =
                panelObject.AddComponent<LudusTutorialTestPanel>();

            MethodInfo onApplicationQuit =
                typeof(LudusTutorialTestPanel).GetMethod(
                    "OnApplicationQuit",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            int serializedCount = 0;
            controller.SessionSerialized += (_, _) => serializedCount++;

            bool started = controller.TryStartSession(
                LudusSdk.StudentIdPendingManualImport,
                "Teste fictício",
                out string startError
            );

            Assert.That(started, Is.True, startError);
            Assert.That(onApplicationQuit, Is.Not.Null);

            onApplicationQuit.Invoke(panel, null);

            Assert.That(controller.HasActiveSession, Is.False);
            Assert.That(controller.LastCompletedSession, Is.Not.Null);
            Assert.That(serializedCount, Is.EqualTo(1));

            onApplicationQuit.Invoke(panel, null);

            Assert.That(serializedCount, Is.EqualTo(1));

            Object.DestroyImmediate(panelObject);
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(config);
        }
    }
}
