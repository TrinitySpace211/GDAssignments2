using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private InventoryUI inventoryTetris;
    private PlacedObject placedObject;

    private void Awake() {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        placedObject = GetComponent<PlacedObject>();
    }

    public void Setup(InventoryUI inventoryTetris) {
        this.inventoryTetris = inventoryTetris;
    }

    public void CreateVisualGrid(Transform visualParentTransform, ItemSO itemTetrisSO, float cellSize) {
        Transform visualTransform = Instantiate(inventoryTetris.gridVisual, visualParentTransform);

        // Create background
        Transform template = visualTransform.Find("Template");
        template.gameObject.SetActive(false);

        for (int x = 0; x < itemTetrisSO.width; x++) {
            for (int y = 0; y < itemTetrisSO.height; y++) {
                Transform backgroundSingleTransform = Instantiate(inventoryTetris.gridVisual, visualTransform);
                backgroundSingleTransform.gameObject.SetActive(true);
            }
        }

        visualTransform.GetComponent<GridLayoutGroup>().cellSize = Vector2.one * cellSize;

        visualTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(itemTetrisSO.width, itemTetrisSO.height) * cellSize;

        visualTransform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        visualTransform.SetAsFirstSibling();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        //Debug.Log("OnBeginDrag");
        canvasGroup.alpha = .7f;
        canvasGroup.blocksRaycasts = false;

        CreateVisualGrid(transform.GetChild(0), placedObject.GetItemSO(), inventoryTetris.GetGrid().GetCellSize());
        inventoryTetris.GetInventoryDragAndDropSystem().StartedDragging(inventoryTetris, placedObject);
    }

    public void OnDrag(PointerEventData eventData) {
        //Debug.Log("OnDrag");
        //rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData) {
        //Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        inventoryTetris.GetInventoryDragAndDropSystem().StoppedDragging(inventoryTetris, placedObject);
    }

    public void OnPointerDown(PointerEventData eventData) {
        //Debug.Log("OnPointerDown");
    }

    /* //Kein guter Fix zum Drag Cancel, wenn das Inventar durch irgendwas geschlossen wird
    private void OnDisable() {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        inventoryTetris.GetInventoryDragAndDropSystem().StoppedDragging(inventoryTetris, placedObject);
    } */
}
