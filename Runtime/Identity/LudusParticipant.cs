using System;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusParticipant
    {
        public string studentId;
        public string playerId;

        public LudusParticipant(string studentId, string playerId)
        {
            this.studentId = studentId?.Trim() ?? string.Empty;
            this.playerId = playerId?.Trim() ?? string.Empty;
        }

        public bool TryValidate(out string errorMessage)
        {
            if (!HasMongoObjectId(studentId))
            {
                errorMessage =
                    "studentId deve conter um ObjectId MongoDB válido com 24 caracteres hexadecimais.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                errorMessage = "playerId é obrigatório.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool HasMongoObjectId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 24)
            {
                return false;
            }

            foreach (char character in value)
            {
                bool isDigit = character >= '0' && character <= '9';
                bool isLowercaseHex = character >= 'a' && character <= 'f';
                bool isUppercaseHex = character >= 'A' && character <= 'F';

                if (!isDigit && !isLowercaseHex && !isUppercaseHex)
                {
                    return false;
                }
            }

            return true;
        }
    }
}