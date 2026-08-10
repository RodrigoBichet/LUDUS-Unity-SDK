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

        private readonly bool hasPosition;
        private readonly float positionX;
        private readonly float positionY;

        public bool HasPosition => hasPosition;
        public float PositionX => positionX;
        public float PositionY => positionY;

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

        public LudusTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            Vector2 position
        ) : this(displayName, interactionKind, action)
        {
            hasPosition = true;
            positionX = position.x;
            positionY = position.y;
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
            if (HasPosition)
            {
                return JsonUtility.ToJson(
                    new PositionedTrackedInteractionPayload
                    {
                        contextInstanceId = contextInstanceId,
                        displayName = displayName,
                        interactionKind = interactionKind,
                        action = action,
                        x = positionX,
                        y = positionY,
                    }
                );
            }

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

        [Serializable]
        private sealed class PositionedTrackedInteractionPayload
        {
            public string contextInstanceId;
            public string displayName;
            public string interactionKind;
            public string action;
            public float x;
            public float y;
        }
    }
}
