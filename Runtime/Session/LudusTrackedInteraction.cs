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
        private readonly bool hasTextSummary;
        private readonly int characterCount;
        private readonly bool hasDragSummary;
        private readonly float dragStartX;
        private readonly float dragStartY;
        private readonly float dragEndX;
        private readonly float dragEndY;
        private readonly int dragDurationMs;
        private readonly float dragDistancePx;

        public bool HasPosition => hasPosition;
        public float PositionX => positionX;
        public float PositionY => positionY;
        public bool HasTextSummary => hasTextSummary;
        public int CharacterCount => characterCount;
        public bool WasEmpty => hasTextSummary && characterCount == 0;
        public bool HasDragSummary => hasDragSummary;
        public float DragStartX => dragStartX;
        public float DragStartY => dragStartY;
        public float DragEndX => dragEndX;
        public float DragEndY => dragEndY;
        public int DragDurationMs => dragDurationMs;
        public float DragDistancePx => dragDistancePx;

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

        public LudusTrackedInteraction(
            string displayName,
            Vector2 dragStartPosition,
            Vector2 dragEndPosition,
            int durationMs
        ) : this(displayName, "draggable-object", "completed")
        {
            hasPosition = true;
            positionX = dragEndPosition.x;
            positionY = dragEndPosition.y;
            hasDragSummary = true;
            dragStartX = dragStartPosition.x;
            dragStartY = dragStartPosition.y;
            dragEndX = dragEndPosition.x;
            dragEndY = dragEndPosition.y;
            dragDurationMs = durationMs;
            dragDistancePx = Vector2.Distance(
                dragStartPosition,
                dragEndPosition
            );
        }

        public LudusTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            int textCharacterCount
        ) : this(displayName, interactionKind, action)
        {
            hasTextSummary = true;
            characterCount = textCharacterCount;
        }

        public LudusTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            int textCharacterCount,
            Vector2 position
        ) : this(
            displayName,
            interactionKind,
            action,
            textCharacterCount
        )
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

            if (hasTextSummary && characterCount < 0)
            {
                errorMessage =
                    "A quantidade de caracteres não pode ser negativa.";
                return false;
            }

            if (hasDragSummary && dragDurationMs < 0)
            {
                errorMessage =
                    "A duração do arraste não pode ser negativa.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        internal string CreatePayload(string contextInstanceId)
        {
            if (HasDragSummary)
            {
                return JsonUtility.ToJson(
                    new TrackedDragPayload
                    {
                        contextInstanceId = contextInstanceId,
                        displayName = displayName,
                        interactionKind = interactionKind,
                        action = action,
                        startX = dragStartX,
                        startY = dragStartY,
                        endX = dragEndX,
                        endY = dragEndY,
                        durationMs = dragDurationMs,
                        distancePx = dragDistancePx,
                        x = dragEndX,
                        y = dragEndY,
                    }
                );
            }

            if (HasTextSummary && HasPosition)
            {
                return JsonUtility.ToJson(
                    new PositionedTextInputPayload
                    {
                        contextInstanceId = contextInstanceId,
                        displayName = displayName,
                        interactionKind = interactionKind,
                        action = action,
                        characterCount = characterCount,
                        wasEmpty = WasEmpty,
                        x = positionX,
                        y = positionY,
                    }
                );
            }

            if (HasTextSummary)
            {
                return JsonUtility.ToJson(
                    new TextInputPayload
                    {
                        contextInstanceId = contextInstanceId,
                        displayName = displayName,
                        interactionKind = interactionKind,
                        action = action,
                        characterCount = characterCount,
                        wasEmpty = WasEmpty,
                    }
                );
            }

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

        [Serializable]
        private sealed class TextInputPayload
        {
            public string contextInstanceId;
            public string displayName;
            public string interactionKind;
            public string action;
            public int characterCount;
            public bool wasEmpty;
        }

        [Serializable]
        private sealed class PositionedTextInputPayload
        {
            public string contextInstanceId;
            public string displayName;
            public string interactionKind;
            public string action;
            public int characterCount;
            public bool wasEmpty;
            public float x;
            public float y;
        }

        [Serializable]
        private sealed class TrackedDragPayload
        {
            public string contextInstanceId;
            public string displayName;
            public string interactionKind;
            public string action;
            public float startX;
            public float startY;
            public float endX;
            public float endY;
            public int durationMs;
            public float distancePx;
            public float x;
            public float y;
        }
    }
}
