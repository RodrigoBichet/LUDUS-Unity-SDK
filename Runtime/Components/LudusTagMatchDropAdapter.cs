using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Observa um drop recebido pelo EventSystem e compara a tag da peça com
    /// a tag esperada. Não move, reposiciona ou devolve a peça.
    /// </summary>
    [AddComponentMenu("LUDUS/Adaptadores/Resultado de arraste por tag")]
    public sealed class LudusTagMatchDropAdapter :
        MonoBehaviour,
        IDropHandler
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private string expectedTag = "Untagged";

        [SerializeField]
        private string targetDisplayName = string.Empty;

        [SerializeField]
        private string expectedItemName = string.Empty;

        [SerializeField]
        private bool recordAttemptEvent = true;

        [SerializeField]
        private bool recordOutcomeEvent = true;

        [SerializeField]
        private bool showConsoleWarnings = true;

        [SerializeField]
        private LudusSemanticBridge semanticBridge;

        public bool TrackingEnabled => trackingEnabled;
        public string ExpectedTag => expectedTag;
        public bool RecordAttemptEvent => recordAttemptEvent;
        public bool RecordOutcomeEvent => recordOutcomeEvent;
        public LudusSemanticBridge SemanticBridge => semanticBridge;

        private void Reset()
        {
            semanticBridge = GetComponent<LudusSemanticBridge>();
        }

        private void Awake()
        {
            TryResolveBridge();
        }

        public void Configure(
            string tag,
            string targetName,
            string expectedName,
            bool includeAttemptEvent = true,
            bool includeOutcomeEvent = true,
            LudusSemanticBridge sharedBridge = null
        )
        {
            expectedTag = tag;
            targetDisplayName = targetName;
            expectedItemName = expectedName;
            recordAttemptEvent = includeAttemptEvent;
            recordOutcomeEvent = includeOutcomeEvent;

            if (sharedBridge != null)
            {
                semanticBridge = sharedBridge;
            }

            TryResolveBridge();
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject draggedObject = eventData?.pointerDrag;

            bool recorded = TryEvaluateDrop(
                draggedObject,
                out _,
                out string errorMessage
            );

            if (!recorded && showConsoleWarnings)
            {
                Debug.LogWarning(
                    "[LUDUS] O arraste por tag não foi registrado: "
                        + errorMessage,
                    this
                );
            }
        }

        public bool TryEvaluateDrop(
            GameObject draggedObject,
            out bool correct,
            out string errorMessage
        )
        {
            correct = false;

            if (!trackingEnabled)
            {
                errorMessage =
                    "O adaptador de arraste por tag está desabilitado.";
                return false;
            }

            if (draggedObject == null)
            {
                errorMessage =
                    "O EventSystem não informou o objeto arrastado.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(expectedTag))
            {
                errorMessage = "Selecione a tag esperada.";
                return false;
            }

            if (!TryResolveBridge())
            {
                errorMessage =
                    "Não foi possível localizar a ponte semântica LUDUS.";
                return false;
            }

            string normalizedExpectedTag = expectedTag.Trim();

            if (!TryValidateTag(normalizedExpectedTag, out errorMessage))
            {
                return false;
            }

            correct = draggedObject.CompareTag(normalizedExpectedTag);

            string draggedName = ResolveVisualDisplayName(
                draggedObject,
                "item"
            );
            string targetName = GetDisplayName(
                targetDisplayName,
                gameObject.name
            );
            string expectedName = ResolveExpectedDisplayName(
                draggedObject,
                normalizedExpectedTag,
                correct
            );
            string[] availableOptions = ResolveAvailableOptionNames();

            return semanticBridge.TryRecordEvaluatedDrag(
                draggedName,
                targetName,
                expectedName,
                availableOptions,
                correct,
                recordAttemptEvent,
                recordOutcomeEvent,
                out errorMessage
            );
        }

        private bool TryResolveBridge()
        {
            if (semanticBridge != null)
            {
                return true;
            }

            semanticBridge = GetComponent<LudusSemanticBridge>();

            if (semanticBridge != null)
            {
                return true;
            }

            LudusSemanticBridge[] bridges =
                FindObjectsByType<LudusSemanticBridge>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );
            LudusSemanticBridge sceneBridge = null;

            foreach (LudusSemanticBridge candidate in bridges)
            {
                if (candidate.gameObject.scene != gameObject.scene)
                {
                    continue;
                }

                if (sceneBridge != null)
                {
                    return false;
                }

                sceneBridge = candidate;
            }

            semanticBridge = sceneBridge;
            return semanticBridge != null;
        }

        private string ResolveExpectedDisplayName(
            GameObject draggedObject,
            string normalizedExpectedTag,
            bool correct
        )
        {
            if (correct)
            {
                return ResolveVisualDisplayName(
                    draggedObject,
                    normalizedExpectedTag
                );
            }

            string configuredExpectedName = expectedItemName?.Trim();
            bool configuredNameIsSemantic =
                !string.IsNullOrWhiteSpace(configuredExpectedName) &&
                !string.Equals(
                    configuredExpectedName,
                    normalizedExpectedTag,
                    StringComparison.OrdinalIgnoreCase
                );

            if (configuredNameIsSemantic)
            {
                return configuredExpectedName;
            }

            foreach (GameObject option in FindActiveDraggableOptions())
            {
                if (
                    option != null &&
                    option != gameObject &&
                    option.CompareTag(normalizedExpectedTag)
                )
                {
                    return ResolveVisualDisplayName(
                        option,
                        normalizedExpectedTag
                    );
                }
            }

            return GetDisplayName(
                configuredExpectedName,
                normalizedExpectedTag
            );
        }

        private string[] ResolveAvailableOptionNames()
        {
            List<string> names = new List<string>();
            HashSet<string> uniqueNames = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

            foreach (GameObject option in FindActiveDraggableOptions())
            {
                string name = ResolveVisualDisplayName(
                    option,
                    option != null ? option.name : "item"
                );

                if (
                    !string.IsNullOrWhiteSpace(name) &&
                    uniqueNames.Add(name)
                )
                {
                    names.Add(name);
                }
            }

            return names.ToArray();
        }

        private List<GameObject> FindActiveDraggableOptions()
        {
            Transform activityRoot = ResolveActivityRoot();
            MonoBehaviour[] behaviours =
                activityRoot.GetComponentsInChildren<MonoBehaviour>(false);
            List<GameObject> options = new List<GameObject>();
            HashSet<int> visited = new HashSet<int>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (
                    behaviour == null ||
                    (!(behaviour is IBeginDragHandler) &&
                        !(behaviour is IDragHandler))
                )
                {
                    continue;
                }

                GameObject option = behaviour.gameObject;
                if (
                    option == null ||
                    !option.activeInHierarchy ||
                    !visited.Add(option.GetInstanceID())
                )
                {
                    continue;
                }

                options.Add(option);
            }

            return options;
        }

        private Transform ResolveActivityRoot()
        {
            Canvas canvas = GetComponentInParent<Canvas>(true);
            if (canvas != null)
            {
                return canvas.transform;
            }

            return transform.parent != null
                ? transform.parent
                : transform.root;
        }

        private static string ResolveVisualDisplayName(
            GameObject source,
            string fallback
        )
        {
            if (source == null)
            {
                return GetDisplayName(string.Empty, fallback);
            }

            TMP_Text[] tmpTexts =
                source.GetComponentsInChildren<TMP_Text>(false);
            foreach (TMP_Text text in tmpTexts)
            {
                if (
                    text != null &&
                    text.isActiveAndEnabled &&
                    !string.IsNullOrWhiteSpace(text.text)
                )
                {
                    return NormalizeVisualName(text.text);
                }
            }

            Text[] legacyTexts = source.GetComponentsInChildren<Text>(false);
            foreach (Text text in legacyTexts)
            {
                if (
                    text != null &&
                    text.isActiveAndEnabled &&
                    !string.IsNullOrWhiteSpace(text.text)
                )
                {
                    return NormalizeVisualName(text.text);
                }
            }

            Image image = source.GetComponent<Image>();
            if (image == null)
            {
                image = source.GetComponentInChildren<Image>(false);
            }

            Sprite imageSprite = image != null
                ? image.overrideSprite ?? image.sprite
                : null;
            if (imageSprite != null)
            {
                return NormalizeVisualName(imageSprite.name);
            }

            SpriteRenderer spriteRenderer =
                source.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer =
                    source.GetComponentInChildren<SpriteRenderer>(false);
            }

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                return NormalizeVisualName(spriteRenderer.sprite.name);
            }

            return GetDisplayName(source.name, fallback);
        }

        private static string NormalizeVisualName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value
                .Replace("(Clone)", string.Empty)
                .Replace("_", " ")
                .Trim();
        }

        private static bool TryValidateTag(
            string tag,
            out string errorMessage
        )
        {
            try
            {
                GameObject.FindWithTag(tag);
                errorMessage = string.Empty;
                return true;
            }
            catch (UnityException)
            {
                errorMessage =
                    "A tag esperada '"
                    + tag
                    + "' não existe no projeto.";
                return false;
            }
        }

        private static string GetDisplayName(
            string configuredName,
            string fallback
        )
        {
            return string.IsNullOrWhiteSpace(configuredName)
                ? fallback
                : configuredName.Trim();
        }
    }
}
