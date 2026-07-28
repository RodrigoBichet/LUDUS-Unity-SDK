using UnityEngine;
using System.Text;

namespace LudusSDK
{
    public static class LudusSdk
    {
        public static bool TryStartSession(
            string studentId,
            string playerId,
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

            return controller.TryStartSession(
                studentId,
                playerId,
                out errorMessage
            );
        }

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

        private static bool TryGetSingleSessionController(
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
