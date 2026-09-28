using UnityEngine;
using UnityEngine.EventSystems;

namespace LudusSDK
{
    /// <summary>
    /// Movimenta exclusivamente a peça demonstrativa da cena tutorial.
    /// O registro da telemetria continua sob responsabilidade de
    /// LudusTrackedDraggable, configurado pela pessoa desenvolvedora.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class LudusTutorialDraggableItem :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private RectTransform itemRect;
        private RectTransform movementArea;
        private Vector2 pointerOffset;
        private bool dragStarted;

        private void Awake()
        {
            CacheRectTransforms();
        }

        private void OnDisable()
        {
            dragStarted = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!TryGetPointerPosition(eventData, out Vector2 localPosition))
            {
                dragStarted = false;
                return;
            }

            pointerOffset = (Vector2)itemRect.localPosition - localPosition;
            dragStarted = true;
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragStarted)
            {
                OnBeginDrag(eventData);
            }

            if (
                !dragStarted ||
                !TryGetPointerPosition(eventData, out Vector2 localPosition)
            )
            {
                return;
            }

            Vector2 clampedPosition = ClampToMovementArea(
                localPosition + pointerOffset
            );
            itemRect.localPosition = new Vector3(
                clampedPosition.x,
                clampedPosition.y,
                itemRect.localPosition.z
            );
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragStarted = false;
        }

        private bool TryGetPointerPosition(
            PointerEventData eventData,
            out Vector2 localPosition
        )
        {
            CacheRectTransforms();

            if (eventData == null || itemRect == null || movementArea == null)
            {
                localPosition = default;
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                movementArea,
                eventData.position,
                eventData.pressEventCamera,
                out localPosition
            );
        }

        private Vector2 ClampToMovementArea(Vector2 desiredPosition)
        {
            Rect area = movementArea.rect;
            Rect item = itemRect.rect;
            Vector2 pivot = itemRect.pivot;

            float minimumX = area.xMin + item.width * pivot.x;
            float maximumX = area.xMax - item.width * (1f - pivot.x);
            float minimumY = area.yMin + item.height * pivot.y;
            float maximumY = area.yMax - item.height * (1f - pivot.y);

            return new Vector2(
                Mathf.Clamp(desiredPosition.x, minimumX, maximumX),
                Mathf.Clamp(desiredPosition.y, minimumY, maximumY)
            );
        }

        private void CacheRectTransforms()
        {
            if (itemRect == null)
            {
                itemRect = transform as RectTransform;
            }

            if (movementArea == null && transform.parent != null)
            {
                movementArea = transform.parent as RectTransform;
            }
        }
    }
}
