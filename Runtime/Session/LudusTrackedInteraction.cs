using System;
using UnityEngine;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusTrackedInteraction
    {
        public string displayName;
        public string interactionKind;
        public string action;

        public LudusTrackedInteraction(
            string displayName,
            string interactionKind,
            string action
        )
        {
            this.displayName = displayName?.Trim() ?? string.Empty;
            this.interactionKind = NormalizeTechnicalName(
                interactionKind,
                "other"
            );
            this.action = NormalizeTechnicalName(action, "activated");
        }

        public bool TryValidate(out string errorMessage)
        {
            if (
                string.IsNullOrWhiteSpace(displayName) ||
                displayName.Length > 200
            )
            {
                errorMessage =
                    "O nome da interação é obrigatório e deve ter até 200 caracteres.";
                return false;
            }

            if (
                string.IsNullOrWhiteSpace(interactionKind) ||
                interactionKind.Length > 50
            )
            {
                errorMessage =
                    "O tipo da interação deve ter até 50 caracteres.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(action) || action.Length > 50)
            {
                errorMessage =
                    "A ação da interação deve ter até 50 caracteres.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        internal string CreatePayload(string contextInstanceId)
        {
            return JsonUtility.ToJson(
                new TrackedInteractionPayload
                {
                    contextInstanceId = contextInstanceId,
                    displayName = displayName,
                    interactionKind = interactionKind,
                    action = action,
                }
            );
        }

        private static string NormalizeTechnicalName(
            string value,
            string fallback
        )
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim().ToLowerInvariant();
        }

        [Serializable]
        private sealed class TrackedInteractionPayload
        {
            public string contextInstanceId;
            public string displayName;
            public string interactionKind;
            public string action;
        }
    }
}
