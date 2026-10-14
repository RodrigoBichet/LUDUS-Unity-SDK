using UnityEngine;
using System.Text;

namespace LudusSDK
{
    public static class LudusSdk
    {
        public const string StudentIdPendingManualImport = "000000000000000000000000";

        public static bool TryStartSession(
            string studentId,
            string playerId,
            out string errorMessage
        )
        {
            return TryStartSession(
                studentId,
                playerId,
                out _,
                out errorMessage
            );
        }

        internal static bool TryStartSession(
            string studentId,
            string playerId,
            out LudusSessionController controller,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryStartSession(
                studentId,
                playerId,
                out errorMessage
            );
        }

        public static bool TrySetParticipant(
            string studentId,
            string playerId,
            out string errorMessage
        )
        {
            return LudusParticipantContext.TryConfigure(
                studentId,
                playerId,
                out errorMessage
            );
        }

        public static void ClearParticipant()
        {
            LudusParticipantContext.Clear();
            LudusCollectionContext.Clear();
        }

        public static bool IsConnectedToCollection =>
            LudusCollectionContext.IsConnected;

        public static bool TryEndSession(
            out string json,
            out string errorMessage
        )
        {
            json = string.Empty;

            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryEndAndSerialize(out json, out errorMessage);
        }

        public static string BeginNewRun()
        {
            return LudusRunContext.BeginNewRun();
        }

        public static bool TryCreateCurrentRunBundle(
            out string bundleJson,
            out string errorMessage
        )
        {
            bundleJson = string.Empty;
            string activeSessionId = string.Empty;
            string activeSessionJson = string.Empty;

            if (
                TryGetSingleSessionController(
                    out LudusSessionController controller,
                    out _
                ) &&
                controller.HasActiveSession
            )
            {
                if (
                    !controller.TrySerializeSnapshot(
                        out LudusSession snapshot,
                        out activeSessionJson,
                        out errorMessage
                    )
                )
                {
                    return false;
                }

                activeSessionId = snapshot.sessionId;
            }

            return LudusRunArchive.TryCreateBundle(
                activeSessionId,
                activeSessionJson,
                out bundleJson,
                out errorMessage
            );
        }

        public static bool TryDownloadCurrentRun(out string errorMessage)
        {
            if (
                !TryGetSingleSessionController(
                    out LudusSessionController controller,
                    out errorMessage
                ) ||
                controller.Config == null
            )
            {
                return false;
            }

            if (!TryCreateCurrentRunBundle(out string json, out errorMessage))
            {
                return false;
            }

            return LudusWebGlJsonDownload.TryDownload(
                LudusWebGlJsonDownload.CreateRunBundleFileName(
                    controller.Config
                ),
                json,
                out errorMessage
            );
        }

        public static bool TryRecordTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryRecordTrackedInteraction(
                displayName,
                interactionKind,
                action,
                out errorMessage
            );
        }

        public static bool TryRecordTrackedInteraction(
            string displayName,
            string interactionKind,
            string action,
            Vector2 position,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryRecordTrackedInteraction(
                displayName,
                interactionKind,
                action,
                position,
                out errorMessage
            );
        }

        internal static bool TryRecordGameEvent(
            string eventType,
            string payloadJson,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryRecordGameEvent(
                eventType,
                payloadJson,
                out errorMessage
            );
        }

        public static bool TryRecordTextInputCompletion(
            string displayName,
            int characterCount,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryRecordTextInputCompletion(
                displayName,
                characterCount,
                out errorMessage
            );
        }

        public static bool TryRecordTrackedDrag(
            string displayName,
            Vector2 startPosition,
            Vector2 endPosition,
            int durationMs,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryRecordTrackedDrag(
                displayName,
                startPosition,
                endPosition,
                durationMs,
                out errorMessage
            );
        }

        public static bool TryCaptureScreenshot(out string errorMessage)
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryCaptureScreenshot(out errorMessage);
        }

        public static bool TryRecordTextInputCompletion(
            string displayName,
            int characterCount,
            Vector2 position,
            out string errorMessage
        )
        {
            if (!TryGetSingleSessionController(
                out LudusSessionController controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryRecordTextInputCompletion(
                displayName,
                characterCount,
                position,
                out errorMessage
            );
        }

        public static bool TryStartSessionForManualImport(
            string sessionDisplayName,
            out string errorMessage
        )
        {
            return TryStartSessionForManualImport(
                sessionDisplayName,
                out _,
                out errorMessage
            );
        }

        internal static bool TryStartSessionForManualImport(
            string sessionDisplayName,
            out LudusSessionController controller,
            out string errorMessage
        )
        {
            string resolvedDisplayName = string.IsNullOrWhiteSpace(sessionDisplayName)
                ? "Sessão sem vínculo"
                : sessionDisplayName.Trim();

            if (!TryGetSingleSessionController(
                out controller,
                out errorMessage
            ))
            {
                return false;
            }

            return controller.TryStartSession(
                StudentIdPendingManualImport,
                resolvedDisplayName,
                out errorMessage
            );
        }

        internal static bool TryGetSingleSessionController(
            out LudusSessionController controller,
            out string errorMessage
        )
        {
            LudusSessionController[] controllers =
                Object.FindObjectsByType<LudusSessionController>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            if (controllers.Length == 0)
            {
                controller = null;
                errorMessage =
                    "Não foi encontrada uma base LUDUS SDK ativa. Use LUDUS > Adicionar coleta ao meu jogo na cena inicial.";
                return false;
            }

            if (controllers.Length > 1)
            {
                controller = null;
                errorMessage =
                    "Foram encontradas " + controllers.Length +
                    " bases LUDUS SDK ativas. Mantenha somente uma base por jogo. " +
                    "Bases encontradas: " + DescribeControllers(controllers);
                return false;
            }

            controller = controllers[0];
            errorMessage = string.Empty;
            return true;
        }

        private static string DescribeControllers(
            LudusSessionController[] controllers
        )
        {
            StringBuilder description = new StringBuilder();

            for (int index = 0; index < controllers.Length; index++)
            {
                LudusSessionController current = controllers[index];

                if (index > 0)
                {
                    description.Append("; ");
                }

                description.Append('"');
                description.Append(current.gameObject.name);
                description.Append("\" na cena \"");
                description.Append(current.gameObject.scene.name);
                description.Append('"');
            }

            return description.ToString();
        }
    }
}
