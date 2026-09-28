using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LudusSDK
{
    /// <summary>
    /// Compatibilidade exclusiva da cena tutorial para eventos da Game View
    /// que o novo Input System ainda não entrega ao EventSystem no Editor.
    /// Não executa nenhuma lógica fora do Editor.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialEditorPointerBridge : MonoBehaviour
    {
#if UNITY_EDITOR
        private readonly List<RaycastResult> raycastResults =
            new List<RaycastResult>();

        private PointerEventData pointerData;
        private GameObject pressedObject;
        private GameObject draggedObject;

        private void OnDisable()
        {
            ResetPointerState();
        }

        private void OnGUI()
        {
            Event currentEvent = Event.current;

            if (
                currentEvent == null ||
                !currentEvent.isMouse ||
                EventSystem.current == null ||
                currentEvent.button != 0
            )
            {
                return;
            }

            Vector2 screenPosition = new Vector2(
                currentEvent.mousePosition.x,
                Mathf.Max(0f, Screen.height - currentEvent.mousePosition.y)
            );

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    HandlePointerDown(screenPosition);
                    break;
                case EventType.MouseDrag:
                    HandlePointerDrag(screenPosition);
                    break;
                case EventType.MouseUp:
                    HandlePointerUp(screenPosition);
                    break;
                case EventType.MouseLeaveWindow:
                    ResetPointerState();
                    break;
            }
        }

        private void HandlePointerDown(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            pointerData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                position = screenPosition,
                pressPosition = screenPosition,
                clickTime = Time.unscaledTime,
                clickCount = 1,
                eligibleForClick = true,
                useDragThreshold = true,
            };

            RaycastResult raycast = FindTutorialTarget(pointerData);
            GameObject target = raycast.gameObject;
            pointerData.pointerCurrentRaycast = raycast;
            pointerData.pointerPressRaycast = raycast;
            pointerData.rawPointerPress = target;

            if (target == null)
            {
                eventSystem.SetSelectedGameObject(null, pointerData);
                return;
            }

            pressedObject = ExecuteEvents.ExecuteHierarchy(
                target,
                pointerData,
                ExecuteEvents.pointerDownHandler
            );

            if (pressedObject == null)
            {
                pressedObject = ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    target
                );
            }

            pointerData.pointerPress = pressedObject;
            draggedObject = ExecuteEvents.GetEventHandler<IDragHandler>(target);
            pointerData.pointerDrag = draggedObject;

            if (draggedObject != null)
            {
                ExecuteEvents.Execute(
                    draggedObject,
                    pointerData,
                    ExecuteEvents.initializePotentialDrag
                );
            }

            GameObject selectedObject =
                ExecuteEvents.GetEventHandler<ISelectHandler>(target);
            eventSystem.SetSelectedGameObject(selectedObject, pointerData);
        }

        private void HandlePointerDrag(Vector2 screenPosition)
        {
            if (pointerData == null || draggedObject == null)
            {
                return;
            }

            UpdatePointerPosition(screenPosition);

            if (!pointerData.dragging)
            {
                float threshold = EventSystem.current.pixelDragThreshold;

                if (
                    Vector2.Distance(
                        pointerData.pressPosition,
                        pointerData.position
                    ) < threshold
                )
                {
                    return;
                }

                ExecuteEvents.Execute(
                    draggedObject,
                    pointerData,
                    ExecuteEvents.beginDragHandler
                );
                pointerData.dragging = true;
                pointerData.eligibleForClick = false;
            }

            ExecuteEvents.Execute(
                draggedObject,
                pointerData,
                ExecuteEvents.dragHandler
            );
        }

        private void HandlePointerUp(Vector2 screenPosition)
        {
            if (pointerData == null)
            {
                return;
            }

            UpdatePointerPosition(screenPosition);

            if (pressedObject != null)
            {
                ExecuteEvents.Execute(
                    pressedObject,
                    pointerData,
                    ExecuteEvents.pointerUpHandler
                );
            }

            RaycastResult raycast = FindTutorialTarget(pointerData);
            GameObject clickHandler = raycast.gameObject == null
                ? null
                : ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    raycast.gameObject
                );

            if (
                pointerData.eligibleForClick &&
                pressedObject != null &&
                pressedObject == clickHandler
            )
            {
                ExecuteEvents.Execute(
                    pressedObject,
                    pointerData,
                    ExecuteEvents.pointerClickHandler
                );
            }

            if (pointerData.dragging && draggedObject != null)
            {
                ExecuteEvents.Execute(
                    draggedObject,
                    pointerData,
                    ExecuteEvents.endDragHandler
                );
            }

            ResetPointerState();
        }

        private RaycastResult FindTutorialTarget(
            PointerEventData eventData
        )
        {
            raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                if (
                    result.gameObject != null &&
                    result.gameObject.transform.IsChildOf(transform)
                )
                {
                    return result;
                }
            }

            return default;
        }

        private void UpdatePointerPosition(Vector2 screenPosition)
        {
            pointerData.delta = screenPosition - pointerData.position;
            pointerData.position = screenPosition;
        }

        private void ResetPointerState()
        {
            pointerData = null;
            pressedObject = null;
            draggedObject = null;
        }
#endif
    }
}
