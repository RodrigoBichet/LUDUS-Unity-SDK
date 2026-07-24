using System;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusViewport
    {
        public int widthPx;
        public int heightPx;
        public string coordinateUnit;
        public string coordinateOrigin;

        public LudusViewport(
            int widthPx,
            int heightPx,
            string coordinateUnit = "pixel",
            string coordinateOrigin = "bottom-left"
        )
        {
            this.widthPx = widthPx;
            this.heightPx = heightPx;
            this.coordinateUnit = coordinateUnit;
            this.coordinateOrigin = coordinateOrigin;
        }
    }

    [Serializable]
    public sealed class LudusSessionMetrics
    {
        public int totalClicks;
        public int totalCorrect;
        public int totalWrong;
        public int firstActionMs = -1;
        public float avgTimeBetweenActionsMs;
        public int inactivityCount;
        public float totalInactivityMs;
    }

    [Serializable]
    public sealed class LudusClick
    {
        public string element;
        public float x;
        public float y;
        public int timestamp;
    }

    [Serializable]
    public sealed class LudusPathPoint
    {
        public float x;
        public float y;
        public int t;
    }

    [Serializable]
    public sealed class LudusDragPoint
    {
        public string element;
        public float x;
        public float y;
        public int t;
        public string state;
    }

    [Serializable]
    public sealed class LudusGameEvent
    {
        public string eventType;
        public int timestamp;

        // Representação interna. O serializador próprio converterá este JSON
        // em um objeto no campo payload do contrato canônico.
        public string payloadJson = "{}";
    }

    [Serializable]
    public sealed class LudusScreenshot
    {
        public int faseIndex;
        public string phaseId;
        public int timestamp;
        public string caminho;
    }
}