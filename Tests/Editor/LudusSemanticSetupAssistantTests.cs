using System.Collections.Generic;
using LudusSDK.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace LudusSDK.Tests
{
    public sealed class LudusSemanticSetupTestDropReceiver :
        MonoBehaviour,
        IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
        }
    }

    public sealed class LudusSemanticSetupAssistantTests
    {
        [Test]
        public void Assistente_ListaSomenteCenasHabilitadasDoBuildProfile()
        {
            EditorBuildSettingsScene[] buildScenes =
            {
                new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Fase01.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Teste.unity", false),
                new EditorBuildSettingsScene(string.Empty, true),
            };

            List<string> paths =
                LudusSemanticSetupAssistant.FindEnabledBuildScenePaths(
                    buildScenes
                );

            Assert.That(paths.Count, Is.EqualTo(2));
            Assert.That(paths[0], Is.EqualTo("Assets/Scenes/Menu.unity"));
            Assert.That(paths[1], Is.EqualTo("Assets/Scenes/Fase01.unity"));
        }

        [Test]
        public void Assistente_LocalizaCenaAtualNoFluxoSemDependerDeMaiusculas()
        {
            List<string> paths = new List<string>
            {
                "Assets/Scenes/Menu.unity",
                "Assets/Scenes/Fase01.unity",
                "Assets/Scenes/Fase02.unity",
            };

            Assert.That(
                LudusSemanticSetupAssistant.FindBuildSceneIndex(
                    paths,
                    "assets/scenes/FASE01.unity"
                ),
                Is.EqualTo(1)
            );
            Assert.That(
                LudusSemanticSetupAssistant.FindBuildSceneIndex(
                    paths,
                    "Assets/Scenes/ForaDoBuild.unity"
                ),
                Is.EqualTo(-1)
            );
        }

        [Test]
        public void Assistente_SugereNomeDoEscopoOuDaCenaAtual()
        {
            Scene scene = SceneManager.GetActiveScene();
            string expectedSceneName = string.IsNullOrWhiteSpace(scene.name)
                ? "Atividade acompanhada"
                : scene.name;
            Assert.That(
                LudusSemanticSetupAssistant.ResolveSuggestedActivityName(
                    scene,
                    null
                ),
                Is.EqualTo(expectedSceneName)
            );

            GameObject scopeObject = new GameObject("Escopo de teste");
            LudusSessionScope scope =
                scopeObject.AddComponent<LudusSessionScope>();
            scope.Configure("  Alimentos  ", false, false);

            Assert.That(
                LudusSemanticSetupAssistant.ResolveSuggestedActivityName(
                    scene,
                    scope
                ),
                Is.EqualTo("Alimentos")
            );

            Object.DestroyImmediate(scopeObject);
        }

        [Test]
        public void Assistente_DescobreDropComTagEIgnoraAdaptadorComoOrigem()
        {
            GameObject compatible = new GameObject("Destino compatível");
            compatible.tag = "Player";
            compatible.AddComponent<LudusSemanticSetupTestDropReceiver>();

            GameObject untagged = new GameObject("Destino sem tag");
            untagged.AddComponent<LudusSemanticSetupTestDropReceiver>();

            GameObject adapterOnly = new GameObject("Somente adaptador");
            adapterOnly.AddComponent<LudusTagMatchDropAdapter>();

            List<LudusTagDropCandidate> candidates =
                LudusSemanticSetupAssistant.FindTagDropCandidates(
                    SceneManager.GetActiveScene()
                );

            LudusTagDropCandidate compatibleCandidate =
                FindCandidate(candidates, compatible);
            LudusTagDropCandidate untaggedCandidate =
                FindCandidate(candidates, untagged);

            Assert.That(compatibleCandidate, Is.Not.Null);
            Assert.That(compatibleCandidate.IsCompatible, Is.True);
            Assert.That(compatibleCandidate.Selected, Is.True);
            Assert.That(untaggedCandidate, Is.Not.Null);
            Assert.That(untaggedCandidate.IsCompatible, Is.False);
            Assert.That(untaggedCandidate.Selected, Is.False);
            Assert.That(FindCandidate(candidates, adapterOnly), Is.Null);

            Object.DestroyImmediate(adapterOnly);
            Object.DestroyImmediate(untagged);
            Object.DestroyImmediate(compatible);
        }

        [Test]
        public void Assistente_ConfiguraPonteCompartilhadaSemDuplicarLigacoes()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject scopeObject = new GameObject("Categoria de teste");
            LudusSessionScope scope =
                scopeObject.AddComponent<LudusSessionScope>();
            scope.Configure("Associação", false, false);

            GameObject activityCanvas = new GameObject(
                "Atividade visual",
                typeof(Canvas)
            );
            GameObject target = new GameObject("Destino com tag");
            target.transform.SetParent(activityCanvas.transform);
            target.tag = "Player";
            target.AddComponent<LudusSemanticSetupTestDropReceiver>();

            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            config.capabilities.categoryEvents = false;
            config.capabilities.phaseEvents = false;
            config.capabilities.correctWrong = false;
            config.capabilities.customEvents = false;
            config.capabilities.screenshots = true;

            List<LudusTagDropCandidate> candidates =
                LudusSemanticSetupAssistant.FindTagDropCandidates(scene);
            KeepOnlyCandidate(candidates, target);

            Assert.That(
                LudusSemanticSetupAssistant.TryApplyTagDropSetup(
                    scene,
                    scope,
                    config,
                    candidates,
                    out LudusSemanticSetupResult firstResult,
                    out string firstError
                ),
                Is.True,
                firstError
            );

            LudusSemanticBridge bridge =
                scopeObject.GetComponent<LudusSemanticBridge>();
            LudusTagMatchDropAdapter adapter =
                target.GetComponent<LudusTagMatchDropAdapter>();

            Assert.That(firstResult.ConfiguredTargets, Is.EqualTo(1));
            Assert.That(firstResult.ReusedTargets, Is.EqualTo(0));
            Assert.That(
                firstResult.ConfiguredObservationAreas,
                Is.EqualTo(1)
            );
            Assert.That(bridge, Is.Not.Null);
            Assert.That(adapter, Is.Not.Null);
            Assert.That(adapter.SemanticBridge, Is.SameAs(bridge));
            Assert.That(
                activityCanvas.GetComponents<LudusCaptureContextTrigger>()
                    .Length,
                Is.EqualTo(1)
            );
            Assert.That(scope.OnSessionStarted.GetPersistentEventCount(), Is.EqualTo(2));
            Assert.That(scope.OnBeforeSessionEnded.GetPersistentEventCount(), Is.EqualTo(1));
            Assert.That(config.capabilities.categoryEvents, Is.True);
            Assert.That(config.capabilities.phaseEvents, Is.True);
            Assert.That(config.capabilities.correctWrong, Is.True);
            Assert.That(config.capabilities.customEvents, Is.True);

            candidates =
                LudusSemanticSetupAssistant.FindTagDropCandidates(scene);
            KeepOnlyCandidate(candidates, target);

            Assert.That(
                LudusSemanticSetupAssistant.TryApplyTagDropSetup(
                    scene,
                    scope,
                    config,
                    candidates,
                    out LudusSemanticSetupResult secondResult,
                    out string secondError
                ),
                Is.True,
                secondError
            );
            Assert.That(secondResult.ConfiguredTargets, Is.EqualTo(1));
            Assert.That(secondResult.ReusedTargets, Is.EqualTo(1));
            Assert.That(
                target.GetComponents<LudusTagMatchDropAdapter>().Length,
                Is.EqualTo(1)
            );
            Assert.That(
                activityCanvas.GetComponents<LudusCaptureContextTrigger>()
                    .Length,
                Is.EqualTo(1)
            );
            Assert.That(scope.OnSessionStarted.GetPersistentEventCount(), Is.EqualTo(2));
            Assert.That(scope.OnBeforeSessionEnded.GetPersistentEventCount(), Is.EqualTo(1));

            Object.DestroyImmediate(activityCanvas);
            Object.DestroyImmediate(scopeObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Assistente_CriaSessaoAutomaticamenteSemDuplicarAoReaplicar()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject activityCanvas = new GameObject(
                "Atividade automática",
                typeof(Canvas)
            );
            GameObject target = new GameObject("Destino automático");
            target.transform.SetParent(activityCanvas.transform);
            target.tag = "Player";
            target.AddComponent<LudusSemanticSetupTestDropReceiver>();

            LudusSdkConfig config =
                ScriptableObject.CreateInstance<LudusSdkConfig>();
            List<LudusTagDropCandidate> candidates =
                LudusSemanticSetupAssistant.FindTagDropCandidates(scene);
            KeepOnlyCandidate(candidates, target);

            Assert.That(
                LudusSemanticSetupAssistant.TryApplyTagDropSetup(
                    scene,
                    null,
                    config,
                    candidates,
                    "Associação",
                    out LudusSemanticSetupResult firstResult,
                    out string firstError
                ),
                Is.True,
                firstError
            );

            List<LudusSessionScope> scopes =
                LudusSemanticSetupAssistant.FindSessionScopes(scene);
            Assert.That(firstResult.CreatedSessionScope, Is.True);
            Assert.That(scopes.Count, Is.EqualTo(1));
            Assert.That(scopes[0].SessionDisplayName, Is.EqualTo("Associação"));
            Assert.That(scopes[0].gameObject.name, Is.EqualTo("LUDUS — Associação"));

            candidates =
                LudusSemanticSetupAssistant.FindTagDropCandidates(scene);
            KeepOnlyCandidate(candidates, target);

            Assert.That(
                LudusSemanticSetupAssistant.TryApplyTagDropSetup(
                    scene,
                    null,
                    config,
                    candidates,
                    "Associação",
                    out LudusSemanticSetupResult secondResult,
                    out string secondError
                ),
                Is.True,
                secondError
            );
            Assert.That(secondResult.CreatedSessionScope, Is.False);
            Assert.That(
                LudusSemanticSetupAssistant.FindSessionScopes(scene).Count,
                Is.EqualTo(1)
            );

            Object.DestroyImmediate(scopes[0].gameObject);
            Object.DestroyImmediate(activityCanvas);
            Object.DestroyImmediate(config);
        }

        private static LudusTagDropCandidate FindCandidate(
            List<LudusTagDropCandidate> candidates,
            GameObject gameObject
        )
        {
            foreach (LudusTagDropCandidate candidate in candidates)
            {
                if (candidate.GameObject == gameObject)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void KeepOnlyCandidate(
            List<LudusTagDropCandidate> candidates,
            GameObject gameObject
        )
        {
            foreach (LudusTagDropCandidate candidate in candidates)
            {
                candidate.Selected = candidate.GameObject == gameObject;
            }
        }
    }
}
