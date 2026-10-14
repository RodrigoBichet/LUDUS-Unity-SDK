using System;
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
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LudusTutorialDraggableItem :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private RectTransform itemRect;
        private RectTransform movementArea;
        private CanvasGroup canvasGroup;
        private Vector2 pointerOffset;
        private Vector2 initialAnchoredPosition;
        private bool initialPositionCaptured;
        private bool dragStarted;

        public event Action<LudusTutorialDraggableItem> DragCompleted;

        private void Awake()
        {
            RememberInitialPosition();
        }

        private void OnEnable()
        {
            CacheRectTransforms();
            CaptureInitialPosition();
        }

        public void RememberInitialPosition()
        {
            CacheRectTransforms();
            if (itemRect != null)
            {
                initialAnchoredPosition = itemRect.anchoredPosition;
                initialPositionCaptured = true;
            }
        }

        private void OnDisable()
        {
            dragStarted = false;
            SetRaycastBlocking(true);
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
            SetRaycastBlocking(false);
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
            CacheRectTransforms();
            dragStarted = false;
            SetRaycastBlocking(
                canvasGroup == null || canvasGroup.interactable
            );
            DragCompleted?.Invoke(this);
        }

        public void ReturnToInitialPosition()
        {
            CacheRectTransforms();
            CaptureInitialPosition();
            if (itemRect != null)
            {
                itemRect.anchoredPosition = initialAnchoredPosition;
            }

            dragStarted = false;
            SetInteractionEnabled(true);
        }

        public void SetInteractionEnabled(bool enabled)
        {
            CacheRectTransforms();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = enabled;
                canvasGroup.blocksRaycasts = enabled;
            }
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

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void CaptureInitialPosition()
        {
            if (!initialPositionCaptured && itemRect != null)
            {
                initialAnchoredPosition = itemRect.anchoredPosition;
                initialPositionCaptured = true;
            }
        }

        private void SetRaycastBlocking(bool shouldBlock)
        {
            CacheRectTransforms();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = shouldBlock;
            }
        }
    }
}
