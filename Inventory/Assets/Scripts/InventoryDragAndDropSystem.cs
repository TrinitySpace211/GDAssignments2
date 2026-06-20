using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryDragAndDropSystem : MonoBehaviour {

    [SerializeField] private InventoryUI inventoryUI;
    private PlacedObject draggingPlacedObject;
    private PlacedObject previousPlacedObject;
    private Vector2Int mouseDragGridPositionOffset;
    private Vector2 mouseDragAnchoredPositionOffset;
    private ItemSO.Direction dir;

    /* private void Start() {
        inventoryUI.OnObjectPlaced += (object sender, PlacedObject placedObject) => {

        };
    } */

    private void Update() {
        if (PlayerInputHandler.Instance.RotateItemTriggered) {
            dir = ItemSO.GetNextDirection(dir);
            PlayerInputHandler.Instance.SetRotateItemTriggered(false);
        }

        if (draggingPlacedObject != null) {
            // Calculate target position to move the dragged item
            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryUI.GetItemContainer(), Mouse.current.position.value, null, out Vector2 targetPosition);
            targetPosition += new Vector2(-mouseDragAnchoredPositionOffset.x, -mouseDragAnchoredPositionOffset.y);

            // Apply rotation offset to target position
            Vector2Int rotationOffset = draggingPlacedObject.GetItemSO().GetRotationOffset(dir);
            targetPosition += new Vector2(rotationOffset.x, rotationOffset.y) * inventoryUI.GetGrid().GetCellSize();

            // Snap position
            targetPosition /= 10f;// draggingInventoryTetris.GetGrid().GetCellSize();
            targetPosition = new Vector2(Mathf.Floor(targetPosition.x), Mathf.Floor(targetPosition.y));
            targetPosition *= 10f;

            // Move and rotate dragged object
            draggingPlacedObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(draggingPlacedObject.GetComponent<RectTransform>().anchoredPosition, targetPosition, Time.deltaTime * 20f);
            draggingPlacedObject.transform.rotation = Quaternion.Lerp(draggingPlacedObject.transform.rotation, Quaternion.Euler(0, 0, -draggingPlacedObject.GetItemSO().GetRotationAngle(dir)), Time.deltaTime * 15f);
        }
    }

    public void StartedDragging(InventoryUI inventoryUI, PlacedObject placedObject) {
        // Started Dragging
        this.inventoryUI = inventoryUI;
        draggingPlacedObject = placedObject;

        Cursor.visible = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryUI.GetItemContainer(), Mouse.current.position.value, null, out Vector2 anchoredPosition);
        Vector2Int mouseGridPosition = inventoryUI.GetGridPosition(anchoredPosition);

        // Calculate Grid Position offset from the placedObject origin to the mouseGridPosition
        mouseDragGridPositionOffset = mouseGridPosition - placedObject.GetGridPosition();

        // Calculate the anchored poisiton offset, where exactly on the image the player clicked
        mouseDragAnchoredPositionOffset = anchoredPosition - placedObject.GetComponent<RectTransform>().anchoredPosition;

        // Save initial direction when started draggign
        dir = placedObject.GetDir();

        // Apply rotation offset to drag anchored position offset
        Vector2Int rotationOffset = draggingPlacedObject.GetItemSO().GetRotationOffset(dir);
        mouseDragAnchoredPositionOffset += new Vector2(rotationOffset.x, rotationOffset.y) * inventoryUI.GetGrid().GetCellSize();
    }

    public void StoppedDragging(InventoryUI fromInventoryTetris, PlacedObject placedObject) {
        draggingPlacedObject = null;

        Cursor.visible = true;

        // Remove item from its current inventory
        fromInventoryTetris.RemoveItemAt(placedObject.GetGridPosition());

        // Check if it's on top of a InventoryTetris
        Vector3 screenPoint = Mouse.current.position.value;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(fromInventoryTetris.GetItemContainer(), screenPoint, null, out Vector2 anchoredPosition);
        Vector2Int placedObjectOrigin = fromInventoryTetris.GetGridPosition(anchoredPosition);
        placedObjectOrigin = placedObjectOrigin - mouseDragGridPositionOffset;

        bool tryPlaceItem = fromInventoryTetris.TryPlaceItem(placedObject.GetItemSO(), placedObjectOrigin, dir, placedObject.GetCurrentStackSize());

        if (tryPlaceItem) {
            // Item placed!
        } else {
            // Cannot drop item here!
            Debug.Log("Cannot drop item here!");
            fromInventoryTetris.TryPlaceItem(placedObject.GetItemSO(), placedObject.GetGridPosition(), placedObject.GetDir(), placedObject.GetCurrentStackSize());
        }
    }

    /* public void CancelDrag(InventoryUI fromInventoryTetris, PlacedObject placedObject) {
        if (draggingPlacedObject == null) return;

        Cursor.visible = true;

        bool placed = fromInventoryTetris.TryPlaceItem(placedObject.GetItemSO(), placedObject.GetGridPosition(), placedObject.GetDir(), placedObject.GetCurrentStackSize());

        Debug.Log(placed);

        Destroy(draggingPlacedObject.gameObject);

        draggingPlacedObject = null;
    } */
}
