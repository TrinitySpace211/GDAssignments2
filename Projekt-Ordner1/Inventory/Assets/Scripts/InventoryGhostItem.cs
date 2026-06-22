using UnityEngine;

public class InventoryGhostItem : MonoBehaviour {
    private RectTransform rectTransform;
    private Transform visual;
    private ItemSO placedObjectTypeSO;

    /* private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        RefreshVisual();
    }

    private void LateUpdate() {
        Vector2 targetPosition = InventoryTetrisManualPlacement.Instance.GetCanvasSnappedPosition();

        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, InventoryTetrisManualPlacement.Instance.GetPlacedObjectRotation(), Time.deltaTime * 15f);
    }

    private void RefreshVisual() {
        if (visual != null) {
            Destroy(visual.gameObject);
            visual = null;
        }

        ItemSO placedObjectTypeSO = InventoryTetrisManualPlacement.Instance.GetPlacedObjectTypeSO();

        if (placedObjectTypeSO != null) {
            visual = Instantiate(placedObjectTypeSO.icon, transform);
            visual.localPosition = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;
        }
    } */
}
