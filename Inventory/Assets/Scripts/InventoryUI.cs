using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour {

    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 50f;
    [SerializeField] private GameObject container;
    [SerializeField] private InventoryDragAndDropSystem dragAndDropSystem;

    public Transform backgroundTemplate;
    public Transform gridVisual;

    //public event EventHandler<PlacedObject> OnObjectPlaced;

    private Grid<GridObject> grid;
    private RectTransform itemContainer;

    private void Awake() {
        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize);

        //Grid mit GridObjects füllen
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                grid.SetGridObject(x, y, new GridObject(grid, x, y));
            }
        }

        itemContainer = container.GetComponent<RectTransform>();
    }

    public class GridObject {
        private Grid<GridObject> grid;
        private int x;
        private int y;
        public PlacedObject placedObject;

        public GridObject(Grid<GridObject> grid, int x, int y) {
            this.grid = grid;
            this.x = x;
            this.y = y;
            placedObject = null;
        }

        public override string ToString() {
            return x + ", " + y + "\n" + placedObject;
        }

        public void SetPlacedObject(PlacedObject placedObject) {
            this.placedObject = placedObject;
            //grid.TriggerGridObjectChanged(x, y);
        }

        public void ClearPlacedObject() {
            placedObject = null;
            //grid.TriggerGridObjectChanged(x, y);
        }

        public PlacedObject GetPlacedObject() {
            return placedObject;
        }

        public bool CanBuild() {
            return placedObject == null;
        }

        public bool HasPlacedObject() {
            return placedObject != null;
        }

    }

    public Grid<GridObject> GetGrid() {
        return grid;
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition) {
        grid.GetXY(worldPosition, out int x, out int z);
        return new Vector2Int(x, z);
    }

    public bool IsValidGridPosition(Vector2Int gridPosition) {
        return grid.IsValidGridPosition(gridPosition);
    }

    /// <summary>
    /// Versucht ein Item an einer bestimmten Stelle zu platzieren
    /// </summary>
    /// <param name="itemSO">Das Item, dass platziert werden soll</param>
    /// <param name="placedObjectOrigin">Der Punkt, wo das Item platziert werden soll</param>
    /// <param name="dir">Die Ausrichtung des Items</param>
    /// <returns>true wenn erfolgreich, false sonst</returns>
    public bool TryPlaceItem(ItemSO itemSO, Vector2Int placedObjectOrigin, ItemSO.Direction dir, float amount = 1f) {
        // Test Can Build
        List<Vector2Int> gridPositionList = itemSO.GetGridPositionList(placedObjectOrigin, dir);

        foreach (Vector2Int gridPosition in gridPositionList) {

            //Are all Positions inside of the Grid?
            if (!grid.IsValidGridPosition(gridPosition)) {
                // Not valid
                return false;
            }
        }

        //Is here the exact same Item Type and has it room for more?
        PlacedObject existingPlacedObject = null;
        foreach (Vector2Int gridPosition in gridPositionList) {

            GridObject gridObject = grid.GetGridObject(gridPosition.x, gridPosition.y);

            if (gridObject.HasPlacedObject()) {
                PlacedObject targetPlacedObject = gridObject.GetPlacedObject();

                //Exact same type?
                if (targetPlacedObject.GetItemSO() == itemSO) {
                    existingPlacedObject = targetPlacedObject;
                    break;//Yes
                } else {
                    return false;//No
                }
            }
        }

        //Stack on an existing Object
        if (existingPlacedObject != null) {
            if (existingPlacedObject.GetIsFull()) {
                return false;
            }

            float currentStack = existingPlacedObject.GetCurrentStackSize();
            float maxStack = existingPlacedObject.GetMaxStackSize();

            if (currentStack + amount <= maxStack) {//Stack is not going to be full
                existingPlacedObject.SetStackSize(currentStack + amount);
                existingPlacedObject.UpdateVisual();
                return true;
            } else {//Stack is going to be full
                float amountLeft = (currentStack + amount) - maxStack;
                existingPlacedObject.SetStackSize(maxStack);
                existingPlacedObject.UpdateVisual();
                return TryPlaceItemSomewhere(itemSO, amountLeft);
            }
        }

        //Field is empty
        Vector2Int rotationOffset = itemSO.GetRotationOffset(dir);
        Vector3 placedObjectWorldPosition = grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y) + new Vector3(rotationOffset.x, rotationOffset.y) * grid.GetCellSize();

        PlacedObject placedObject = PlacedObject.CreateCanvas(itemContainer, placedObjectWorldPosition, placedObjectOrigin, dir, itemSO, amount);
        placedObject.transform.rotation = Quaternion.Euler(0, 0, -itemSO.GetRotationAngle(dir));

        placedObject.GetComponent<InventoryDragAndDrop>().Setup(this);

        //Connect Grid-Cells with the new Object
        foreach (Vector2Int gridPosition in gridPositionList) {
            grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
        }

        //OnObjectPlaced?.Invoke(this, placedObject);

        // Object Placed!
        return true;
    }

    /// <summary>
    /// Versucht ein Item beim ersten freien Platz zu platzieren
    /// </summary>
    /// <param name="itemSO">Das Item, dass platziert werden soll</param>
    /// <returns>true wenn erfolgreich, false sonst</returns>
    public bool TryPlaceItemSomewhere(ItemSO itemSO, float amount) {
        ItemSO.Direction dir = ItemSO.Direction.Down;

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                Vector2Int testOrigin = new Vector2Int(x, y);

                //Wenn das Item plaziert werden kann, dann bricht die Schleife ab und gibt true zurück
                if (TryPlaceItem(itemSO, testOrigin, dir, amount)) {
                    //Debug.Log($"Item '{itemSO.nameString}' erfolgreich bei ({x}, {y}) platziert.");
                    return true;
                }
            }
        }

        // Wenn die Schleife durchläuft, ohne dass TryPlaceItem true zurückgibt, ist kein Platz frei.
        //Debug.LogWarning($"Kein freier Platz für '{itemSO.nameString}' im Inventar gefunden!");
        return false;
    }

    public void RemoveItemAt(Vector2Int removeGridPosition) {
        PlacedObject placedObject = grid.GetGridObject(removeGridPosition.x, removeGridPosition.y).GetPlacedObject();

        if (placedObject != null) {
            // Demolish
            placedObject.DestroySelf();

            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
            foreach (Vector2Int gridPosition in gridPositionList) {
                grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
            }
        }
    }

    public RectTransform GetItemContainer() {
        return itemContainer;
    }

    public InventoryDragAndDropSystem GetInventoryDragAndDropSystem() {
        return dragAndDropSystem;
    }
}
