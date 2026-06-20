using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlacedObject : MonoBehaviour {

    public static PlacedObject CreateCanvas(Transform parent, Vector2 anchoredPosition, Vector2Int origin, ItemSO.Direction dir, ItemSO placedObjectTypeSO, float amount) {
        Transform placedObjectTransform = Instantiate(placedObjectTypeSO.icon, parent);
        placedObjectTransform.rotation = Quaternion.Euler(0, placedObjectTypeSO.GetRotationAngle(dir), 0);
        placedObjectTransform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

        PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();
        placedObject.itemSO = placedObjectTypeSO;
        placedObject.origin = origin;
        placedObject.dir = dir;
        placedObject.amount = amount;

        placedObject.Setup();

        return placedObject;
    }

    private ItemSO itemSO;
    private Vector2Int origin;
    private ItemSO.Direction dir;
    private TextMeshProUGUI amountText;
    private float amount = 1f;

    protected virtual void Setup() {
        UpdateVisual();
        //Debug.Log("PlacedObject.Setup() " + transform);
    }

    public void UpdateVisual() {
        if (amountText == null) amountText = GetComponentInChildren<TextMeshProUGUI>();

        if (amountText != null) {
            amountText.text = amount.ToString();
            if (amount <= 1) {
                amountText.text = "";
            }
        }
    }

    public virtual void GridSetupDone() {
        //Debug.Log("PlacedObject.GridSetupDone() " + transform);
    }

    public Vector2Int GetGridPosition() {
        return origin;
    }

    public void SetOrigin(Vector2Int origin) {
        this.origin = origin;
    }

    public List<Vector2Int> GetGridPositionList() {
        return itemSO.GetGridPositionList(origin, dir);
    }

    public float GetMaxStackSize() {
        return itemSO.maxStack;
    }

    public void SetStackSize(float amount) {
        this.amount = amount;
        UpdateVisual();
    }

    public float GetCurrentStackSize() {
        return amount;
    }

    public bool GetIsFull() {
        return itemSO.maxStack <= amount;
    }

    public ItemSO.Direction GetDir() {
        return dir;
    }

    public virtual void DestroySelf() {
        Destroy(gameObject);
    }

    public override string ToString() {
        return itemSO.nameString;
    }

    public ItemSO GetItemSO() {
        return itemSO;
    }
}
