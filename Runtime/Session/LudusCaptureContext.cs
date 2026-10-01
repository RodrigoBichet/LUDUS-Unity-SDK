using System;
using UnityEngine;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusCaptureContext
    {
        public string displayName;
        public string contextKind;
        public string observationPurpose;

        // Metadados internos de seleção visual. Eles não são enviados no
        // payload dos eventos e servem apenas para limitar e priorizar imagens.
        internal bool captureVisualReference;
        internal string visualReferenceKey;

        public LudusCaptureContext(
            string displayName,
            string contextKind,
            string observationPurpose = "",
            bool captureVisualReference = true,
            string visualReferenceKey = ""
        )
        {
            this.displayName = displayName?.Trim() ?? string.Empty;
            this.contextKind = string.IsNullOrWhiteSpace(contextKind)
                ? "other"
                : contextKind.Trim().ToLowerInvariant();
            this.observationPurpose =
                observationPurpose?.Trim() ?? string.Empty;
            this.captureVisualReference = captureVisualReference;
            this.visualReferenceKey = string.IsNullOrWhiteSpace(
                visualReferenceKey
            )
                ? this.contextKind + ":" + this.displayName
                : visualReferenceKey.Trim();
        }

        public bool TryValidate(out string errorMessage)
        {
            if (
                string.IsNullOrWhiteSpace(displayName) ||
                displayName.Length > 200
            )
            {
                errorMessage =
                    "O título do contexto é obrigatório e deve ter até 200 caracteres.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(contextKind) || contextKind.Length > 50)
            {
                errorMessage =
                    "O tipo do contexto é obrigatório e deve ter até 50 caracteres.";
                return false;
            }

            if (observationPurpose.Length > 500)
            {
                errorMessage =
                    "O objetivo observacional deve ter até 500 caracteres.";
                return false;
            }

            if (
                captureVisualReference &&
                (
                    string.IsNullOrWhiteSpace(visualReferenceKey) ||
                    visualReferenceKey.Length > 500
                )
            )
            {
                errorMessage =
                    "A identificação interna da captura visual é inválida.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        internal string CreateStartedPayload(string contextInstanceId)
        {
            return JsonUtility.ToJson(
                new CaptureContextStartedPayload
                {
                    contextInstanceId = contextInstanceId,
                    displayName = displayName,
                    contextKind = contextKind,
                    observationPurpose = observationPurpose,
                }
            );
        }

        internal string CreateEndedPayload(string contextInstanceId)
        {
            return JsonUtility.ToJson(
                new CaptureContextEndedPayload
                {
                    contextInstanceId = contextInstanceId,
                }
            );
        }

        [Serializable]
        private sealed class CaptureContextStartedPayload
        {
            public string contextInstanceId;
            public string displayName;
            public string contextKind;
            public string observationPurpose;
        }

        [Serializable]
        private sealed class CaptureContextEndedPayload
        {
            public string contextInstanceId;
        }
    }
}
