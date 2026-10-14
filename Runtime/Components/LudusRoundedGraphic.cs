using UnityEngine;
using UnityEngine.UI;

namespace LudusSDK
{
    /// <summary>
    /// Fundo arredondado vetorial para a UI do SDK, sem depender de sprites.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("LUDUS/UI/Fundo arredondado")]
    public sealed class LudusRoundedGraphic : MaskableGraphic
    {
        [SerializeField, Min(0f)] private float cornerRadius = 16f;
        [SerializeField, Range(1, 16)] private int cornerSegments = 6;

        public float CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public int CornerSegments
        {
            get => cornerSegments;
            set
            {
                cornerSegments = Mathf.Clamp(value, 1, 16);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float radius = Mathf.Min(
                cornerRadius,
                Mathf.Min(rect.width, rect.height) * 0.5f
            );
            int segments = Mathf.Clamp(cornerSegments, 1, 16);
            Color32 vertexColor = color;

            AddVertex(vertexHelper, rect.center, rect, vertexColor);

            Vector2[] centers =
            {
                new Vector2(rect.xMax - radius, rect.yMax - radius),
                new Vector2(rect.xMin + radius, rect.yMax - radius),
                new Vector2(rect.xMin + radius, rect.yMin + radius),
                new Vector2(rect.xMax - radius, rect.yMin + radius),
            };

            for (int corner = 0; corner < centers.Length; corner++)
            {
                float startAngle = corner * 90f;
                for (int segment = 0; segment <= segments; segment++)
                {
                    float angle = (
                        startAngle + segment * 90f / segments
                    ) * Mathf.Deg2Rad;
                    Vector2 position = centers[corner] + new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) * radius;
                    AddVertex(vertexHelper, position, rect, vertexColor);
                }
            }

            int boundaryCount = centers.Length * (segments + 1);
            for (int index = 0; index < boundaryCount; index++)
            {
                int current = index + 1;
                int next = index + 1 == boundaryCount ? 1 : current + 1;
                vertexHelper.AddTriangle(0, current, next);
            }
        }

        private static void AddVertex(
            VertexHelper vertexHelper,
            Vector2 position,
            Rect rect,
            Color32 vertexColor
        )
        {
            float normalizedX = Mathf.InverseLerp(
                rect.xMin,
                rect.xMax,
                position.x
            );
            float normalizedY = Mathf.InverseLerp(
                rect.yMin,
                rect.yMax,
                position.y
            );
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = vertexColor;
            vertex.uv0 = new Vector2(normalizedX, normalizedY);
            vertexHelper.AddVert(vertex);
        }
    }
}
