using System;
using UnityEngine;

namespace LudusSDK
{
    /// <summary>
    /// API tipada para eventos semânticos que somente o jogo consegue
    /// informar. Esses eventos complementam a observação automática e não
    /// devem ser usados para inferir resultados que o jogo não conhece.
    /// </summary>
    public static class LudusGameEvents
    {
        public static bool TryCategorySelected(
            string category,
            out string errorMessage
        )
        {
            if (!TryRequireText(category, "category", out errorMessage))
            {
                return false;
            }

            return TryRecord(
                "CategorySelected",
                new CategorySelectedPayload { category = category.Trim() },
                out errorMessage
            );
        }

        public static bool TryPhaseStarted(
            string phaseId,
            string targetItem,
            string[] options,
            out string errorMessage
        )
        {
            if (!TryRequireText(phaseId, "phaseId", out errorMessage))
            {
                return false;
            }

            if (!TryRequireText(targetItem, "targetItem", out errorMessage))
            {
                return false;
            }

            return TryRecord(
                "PhaseStarted",
                new PhaseStartedPayload
                {
                    phaseId = phaseId.Trim(),
                    targetItem = targetItem.Trim(),
                    options = options ?? Array.Empty<string>(),
                },
                out errorMessage
            );
        }

        public static bool TryDragAttempt(
            string draggedItem,
            string targetItem,
            bool correct,
            out string errorMessage
        )
        {
            return TryDragAttempt(
                draggedItem,
                targetItem,
                string.Empty,
                Array.Empty<string>(),
                correct,
                out errorMessage
            );
        }

        public static bool TryDragAttempt(
            string draggedItem,
            string targetItem,
            string expectedItem,
            string[] options,
            bool correct,
            out string errorMessage
        )
        {
            if (!TryRequireText(
                draggedItem,
                "draggedItem",
                out errorMessage
            ))
            {
                return false;
            }

            if (!TryRequireText(targetItem, "targetItem", out errorMessage))
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(expectedItem) &&
                !TryRequireText(
                    expectedItem,
                    "expectedItem",
                    out errorMessage
                )
            )
            {
                return false;
            }

            string[] normalizedOptions = options ?? Array.Empty<string>();
            normalizedOptions = (string[])normalizedOptions.Clone();

            for (int index = 0; index < normalizedOptions.Length; index++)
            {
                string option = normalizedOptions[index];
                if (!TryRequireText(option, "options", out errorMessage))
                {
                    return false;
                }

                normalizedOptions[index] = option.Trim();
            }

            return TryRecord(
                "DragAttempt",
                new DragAttemptPayload
                {
                    draggedItem = draggedItem.Trim(),
                    targetItem = targetItem.Trim(),
                    expectedItem = string.IsNullOrWhiteSpace(expectedItem)
                        ? string.Empty
                        : expectedItem.Trim(),
                    options = normalizedOptions,
                    correct = correct,
                },
                out errorMessage
            );
        }

        public static bool TryCorrectMatch(
            string item,
            float timeSeconds,
            out string errorMessage
        )
        {
            if (!TryRequireText(item, "item", out errorMessage))
            {
                return false;
            }

            if (!TryRequireNonNegative(timeSeconds, "timeSeconds", out errorMessage))
            {
                return false;
            }

            return TryRecord(
                "CorrectMatch",
                new CorrectMatchPayload
                {
                    item = item.Trim(),
                    timeSeconds = timeSeconds,
                },
                out errorMessage
            );
        }

        public static bool TryWrongMatch(
            string draggedItem,
            string expectedItem,
            out string errorMessage
        )
        {
            if (!TryRequireText(
                draggedItem,
                "draggedItem",
                out errorMessage
            ))
            {
                return false;
            }

            if (!TryRequireText(
                expectedItem,
                "expectedItem",
                out errorMessage
            ))
            {
                return false;
            }

            return TryRecord(
                "WrongMatch",
                new WrongMatchPayload
                {
                    draggedItem = draggedItem.Trim(),
                    expectedItem = expectedItem.Trim(),
                },
                out errorMessage
            );
        }

        public static bool TryPhaseCompleted(
            int correct,
            int wrong,
            float timeSeconds,
            int stars,
            out string errorMessage
        )
        {
            return TryPhaseCompleted(
                null,
                correct,
                wrong,
                timeSeconds,
                stars,
                out errorMessage
            );
        }

        internal static bool TryPhaseCompleted(
            LudusSessionController sessionController,
            int correct,
            int wrong,
            float timeSeconds,
            int stars,
            out string errorMessage
        )
        {
            if (correct < 0 || wrong < 0 || stars < 0)
            {
                errorMessage =
                    "correct, wrong e stars devem possuir valores não negativos.";
                return false;
            }

            if (!TryRequireNonNegative(timeSeconds, "timeSeconds", out errorMessage))
            {
                return false;
            }

            return TryRecord(
                sessionController,
                "PhaseCompleted",
                new PhaseCompletedPayload
                {
                    acertos = correct,
                    erros = wrong,
                    timeSeconds = timeSeconds,
                    stars = stars,
                },
                out errorMessage
            );
        }

        private static bool TryRecord(
            string eventType,
            object payload,
            out string errorMessage
        )
        {
            return TryRecord(
                null,
                eventType,
                payload,
                out errorMessage
            );
        }

        private static bool TryRecord(
            LudusSessionController sessionController,
            string eventType,
            object payload,
            out string errorMessage
        )
        {
            string payloadJson = JsonUtility.ToJson(payload);

            if (sessionController != null)
            {
                return sessionController.TryRecordGameEvent(
                    eventType,
                    payloadJson,
                    out errorMessage
                );
            }

            return LudusSdk.TryRecordGameEvent(
                eventType,
                payloadJson,
                out errorMessage
            );
        }

        private static bool TryRequireText(
            string value,
            string fieldName,
            out string errorMessage
        )
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 200)
            {
                errorMessage =
                    fieldName + " deve possuir entre 1 e 200 caracteres.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryRequireNonNegative(
            float value,
            string fieldName,
            out string errorMessage
        )
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                errorMessage = fieldName + " deve ser um número não negativo.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        [Serializable]
        private sealed class CategorySelectedPayload
        {
            public string category;
        }

        [Serializable]
        private sealed class PhaseStartedPayload
        {
            public string phaseId;
            public string targetItem;
            public string[] options;
        }

        [Serializable]
        private sealed class DragAttemptPayload
        {
            public string draggedItem;
            public string targetItem;
            public string expectedItem;
            public string[] options;
            public bool correct;
        }

        [Serializable]
        private sealed class CorrectMatchPayload
        {
            public string item;
            public float timeSeconds;
        }

        [Serializable]
        private sealed class WrongMatchPayload
        {
            public string draggedItem;
            public string expectedItem;
        }

        [Serializable]
        private sealed class PhaseCompletedPayload
        {
            public int acertos;
            public int erros;
            public float timeSeconds;
            public int stars;
        }
    }
}
