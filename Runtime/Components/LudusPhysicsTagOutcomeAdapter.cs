using UnityEngine;

namespace LudusSDK
{
    public enum LudusPhysicsContactMode
    {
        Trigger2D,
        Collision2D,
        Trigger3D,
        Collision3D,
    }

    /// <summary>
    /// Observa a entrada em trigger ou colisão e compara a tag do objeto que
    /// entrou em contato. Não altera colliders, rigidbodies ou movimento.
    /// </summary>
    [AddComponentMenu("LUDUS/Adaptadores/Resultado de contato por tag")]
    [RequireComponent(typeof(LudusSemanticBridge))]
    public sealed class LudusPhysicsTagOutcomeAdapter : MonoBehaviour
    {
        [SerializeField]
        private bool trackingEnabled = true;

        [SerializeField]
        private LudusPhysicsContactMode contactMode =
            LudusPhysicsContactMode.Trigger2D;

        [SerializeField]
        private string expectedTag = "Untagged";

        [SerializeField]
        private bool searchTagOnParents = true;

        [SerializeField]
        private string expectedItemName = string.Empty;

        [SerializeField]
        private bool showConsoleWarnings = true;

        [SerializeField]
        private LudusSemanticBridge semanticBridge;

        public LudusPhysicsContactMode ContactMode => contactMode;
        public string ExpectedTag => expectedTag;

        private void Reset()
        {
            semanticBridge = GetComponent<LudusSemanticBridge>();
        }

        private void Awake()
        {
            TryResolveBridge();
        }

        public void Configure(
            LudusPhysicsContactMode mode,
            string tag,
            string expectedName,
            bool inspectParents = true
        )
        {
            contactMode = mode;
            expectedTag = tag;
            expectedItemName = expectedName;
            searchTagOnParents = inspectParents;
            TryResolveBridge();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (contactMode == LudusPhysicsContactMode.Trigger2D)
            {
                EvaluateAndReport(other == null ? null : other.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (contactMode == LudusPhysicsContactMode.Collision2D)
            {
                EvaluateAndReport(
                    collision == null ? null : collision.gameObject
                );
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (contactMode == LudusPhysicsContactMode.Trigger3D)
            {
                EvaluateAndReport(other == null ? null : other.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (contactMode == LudusPhysicsContactMode.Collision3D)
            {
                EvaluateAndReport(
                    collision == null ? null : collision.gameObject
                );
            }
        }

        public bool TryEvaluateContact(
            GameObject contactedObject,
            out bool correct,
            out string errorMessage
        )
        {
            correct = false;

            if (!trackingEnabled)
            {
                errorMessage =
                    "O adaptador de contato por tag está desabilitado.";
                return false;
            }

            if (contactedObject == null)
            {
                errorMessage = "O contato não informou um objeto válido.";
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

            GameObject identifiedObject = FindObjectWithExpectedTag(
                contactedObject,
                normalizedExpectedTag
            );
            correct = identifiedObject != null;

            string contactedName = GetDisplayName(
                identifiedObject == null
                    ? contactedObject.name
                    : identifiedObject.name,
                "objeto"
            );
            string expectedName = GetDisplayName(
                expectedItemName,
                normalizedExpectedTag
            );

            return correct
                ? semanticBridge.TryRecordCorrect(
                    contactedName,
                    out errorMessage
                )
                : semanticBridge.TryRecordWrong(
                    contactedName,
                    expectedName,
                    out errorMessage
                );
        }

        private void EvaluateAndReport(GameObject contactedObject)
        {
            bool recorded = TryEvaluateContact(
                contactedObject,
                out _,
                out string errorMessage
            );

            if (!recorded && showConsoleWarnings)
            {
                Debug.LogWarning(
                    "[LUDUS] O contato por tag não foi registrado: "
                        + errorMessage,
                    this
                );
            }
        }

        private GameObject FindObjectWithExpectedTag(
            GameObject contactedObject,
            string normalizedExpectedTag
        )
        {
            Transform current = contactedObject.transform;

            while (current != null)
            {
                if (current.CompareTag(normalizedExpectedTag))
                {
                    return current.gameObject;
                }

                if (!searchTagOnParents)
                {
                    break;
                }

                current = current.parent;
            }

            return null;
        }

        private bool TryResolveBridge()
        {
            if (semanticBridge != null)
            {
                return true;
            }

            semanticBridge = GetComponent<LudusSemanticBridge>();
            return semanticBridge != null;
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
