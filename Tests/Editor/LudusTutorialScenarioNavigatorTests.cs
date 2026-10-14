using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK.Tests
{
    public sealed class LudusTutorialScenarioNavigatorTests
    {
        [TearDown]
        public void TearDown()
        {
            LudusCollectionContext.Clear();
            LudusParticipantContext.Clear();
        }

        [Test]
        public void Navegacao_AtivaUmaPaginaEAtualizaControles()
        {
            GameObject root = new GameObject("Laboratorio tutorial");
            LudusTutorialScenarioNavigator navigator =
                root.AddComponent<LudusTutorialScenarioNavigator>();

            GameObject firstPage = new GameObject("Pagina 1");
            GameObject secondPage = new GameObject("Pagina 2");
            firstPage.transform.SetParent(root.transform);
            secondPage.transform.SetParent(root.transform);

            Button previous = new GameObject("Anterior")
                .AddComponent<Button>();
            Button next = new GameObject("Proximo").AddComponent<Button>();
            Text indicator = new GameObject("Indicador").AddComponent<Text>();

            navigator.Configure(
                new[] { firstPage, secondPage },
                previous,
                next,
                indicator
            );

            Assert.That(firstPage.activeSelf, Is.True);
            Assert.That(secondPage.activeSelf, Is.False);
            Assert.That(previous.interactable, Is.False);
            Assert.That(next.interactable, Is.True);
            Assert.That(indicator.text, Is.EqualTo("Cenário 1 de 2"));

            navigator.ShowNextPage();

            Assert.That(firstPage.activeSelf, Is.False);
            Assert.That(secondPage.activeSelf, Is.True);
            Assert.That(previous.interactable, Is.True);
            Assert.That(next.interactable, Is.False);
            Assert.That(indicator.text, Is.EqualTo("Cenário 2 de 2"));

            Object.DestroyImmediate(previous.gameObject);
            Object.DestroyImmediate(next.gameObject);
            Object.DestroyImmediate(indicator.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PaginaForaDoIntervalo_ELimitadaSemExcecao()
        {
            GameObject root = new GameObject("Laboratorio tutorial");
            LudusTutorialScenarioNavigator navigator =
                root.AddComponent<LudusTutorialScenarioNavigator>();
            GameObject page = new GameObject("Pagina unica");
            page.transform.SetParent(root.transform);

            navigator.Configure(
                new[] { page },
                null,
                null,
                null
            );

            Assert.DoesNotThrow(() => navigator.ShowPage(99));
            Assert.That(navigator.CurrentPageIndex, Is.EqualTo(0));
            Assert.That(page.activeSelf, Is.True);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void DoisCallbacksDoBotaoNoMesmoFrame_AvancamUmaPagina()
        {
            GameObject root = new GameObject("Laboratorio tutorial");
            LudusTutorialScenarioNavigator navigator =
                root.AddComponent<LudusTutorialScenarioNavigator>();
            GameObject firstPage = new GameObject("Pagina 1");
            GameObject secondPage = new GameObject("Pagina 2");
            GameObject thirdPage = new GameObject("Pagina 3");
            firstPage.transform.SetParent(root.transform);
            secondPage.transform.SetParent(root.transform);
            thirdPage.transform.SetParent(root.transform);
            Button previous = new GameObject("Anterior")
                .AddComponent<Button>();
            Button next = new GameObject("Proximo").AddComponent<Button>();

            navigator.Configure(
                new[] { firstPage, secondPage, thirdPage },
                previous,
                next,
                null
            );
            MethodInfo registerListeners = typeof(
                LudusTutorialScenarioNavigator
            ).GetMethod(
                "RegisterListeners",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(registerListeners, Is.Not.Null);
            registerListeners.Invoke(navigator, null);

            next.onClick.Invoke();
            next.onClick.Invoke();

            Assert.That(navigator.CurrentPageIndex, Is.EqualTo(1));
            Assert.That(secondPage.activeSelf, Is.True);
            Assert.That(thirdPage.activeSelf, Is.False);

            Object.DestroyImmediate(previous.gameObject);
            Object.DestroyImmediate(next.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void UltimoCenario_AvancaParaFinalizacaoSemVirarSetimoCenario()
        {
            GameObject root = new GameObject("Laboratorio tutorial");
            LudusTutorialScenarioNavigator navigator =
                root.AddComponent<LudusTutorialScenarioNavigator>();
            GameObject firstScenario = new GameObject("Cenario 1");
            GameObject sixthScenario = new GameObject("Cenario 6");
            GameObject completion = new GameObject("Finalizacao");
            firstScenario.transform.SetParent(root.transform);
            sixthScenario.transform.SetParent(root.transform);
            completion.transform.SetParent(root.transform);
            Button previous = new GameObject("Anterior")
                .AddComponent<Button>();
            Button next = new GameObject("Proximo").AddComponent<Button>();
            Text indicator = new GameObject("Indicador").AddComponent<Text>();

            navigator.Configure(
                new[] { firstScenario, sixthScenario, completion },
                previous,
                next,
                indicator,
                completion
            );
            navigator.ShowPage(1);

            Assert.That(next.interactable, Is.True);
            Assert.That(indicator.text, Is.EqualTo("Cenário 2 de 2"));

            navigator.ShowNextPage();

            Assert.That(completion.activeSelf, Is.True);
            Assert.That(indicator.text, Is.EqualTo("Tutorial concluído"));
            Assert.That(next.interactable, Is.False);
            Assert.That(navigator.ScenarioCount, Is.EqualTo(2));

            Object.DestroyImmediate(previous.gameObject);
            Object.DestroyImmediate(next.gameObject);
            Object.DestroyImmediate(indicator.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Finalizacao_ComColetaConectada_EncerraSessaoAutomaticamente()
        {
            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.gameName = "Tutorial LUDUS";
            config.sendOnSessionEnd = false;

            GameObject controllerObject = new GameObject("LUDUS SDK");
            LudusSessionController controller =
                controllerObject.AddComponent<LudusSessionController>();
            controller.Configure(config);

            Assert.That(
                LudusCollectionContext.TryConfigure(
                    "collection-001",
                    "participant-001",
                    "Aluna Fictícia",
                    "token-temporario-ficticio",
                    System.DateTime.UtcNow.AddMinutes(10).ToString("O"),
                    out string connectionError
                ),
                Is.True,
                connectionError
            );
            Assert.That(
                LudusSdk.TryStartSession(
                    LudusSdk.StudentIdPendingManualImport,
                    "Aluna Fictícia",
                    out string sessionError
                ),
                Is.True,
                sessionError
            );

            GameObject root = new GameObject("Laboratorio tutorial");
            LudusTutorialScenarioNavigator navigator =
                root.AddComponent<LudusTutorialScenarioNavigator>();
            GameObject scenario = new GameObject("Cenario");
            GameObject completion = new GameObject("Finalizacao");
            scenario.transform.SetParent(root.transform);
            completion.transform.SetParent(root.transform);

            navigator.Configure(
                new[] { scenario, completion },
                null,
                null,
                null,
                completion
            );
            navigator.ShowPage(1);

            Assert.That(controller.HasActiveSession, Is.False);
            Assert.That(controller.LastCompletedSession, Is.Not.Null);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void CenarioPendente_BloqueiaProximoAteInteracao()
        {
            GameObject root = new GameObject("Laboratorio tutorial");
            LudusTutorialScenarioNavigator navigator =
                root.AddComponent<LudusTutorialScenarioNavigator>();
            GameObject firstPage = new GameObject("Cenario 1");
            GameObject secondPage = new GameObject("Cenario 2");
            firstPage.transform.SetParent(root.transform);
            secondPage.transform.SetParent(root.transform);
            Button activity = new GameObject("Atividade")
                .AddComponent<Button>();
            LudusTutorialScenarioGate gate =
                firstPage.AddComponent<LudusTutorialScenarioGate>();
            gate.ConfigureAnyButton(
                new[] { activity },
                "Realize a atividade."
            );
            Button previous = new GameObject("Anterior")
                .AddComponent<Button>();
            Button next = new GameObject("Proximo").AddComponent<Button>();

            navigator.Configure(
                new[] { firstPage, secondPage },
                previous,
                next,
                null,
                null,
                new[] { gate }
            );
            InvokePrivate(gate, "RegisterListeners");
            InvokePrivate(navigator, "SubscribeGates");

            navigator.ShowNextPage();
            Assert.That(navigator.CurrentPageIndex, Is.EqualTo(0));
            Assert.That(next.interactable, Is.False);

            activity.onClick.Invoke();
            Assert.That(next.interactable, Is.True);

            navigator.ShowNextPage();
            Assert.That(navigator.CurrentPageIndex, Is.EqualTo(1));
            Assert.That(previous.interactable, Is.True);

            navigator.ShowPreviousPage();
            Assert.That(navigator.CurrentPageIndex, Is.EqualTo(0));
            Assert.That(next.interactable, Is.True);

            Object.DestroyImmediate(activity.gameObject);
            Object.DestroyImmediate(previous.gameObject);
            Object.DestroyImmediate(next.gameObject);
            Object.DestroyImmediate(root);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, null);
        }
    }
}
