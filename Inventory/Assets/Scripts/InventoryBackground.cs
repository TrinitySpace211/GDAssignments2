using UnityEngine;
using UnityEngine.UI;

public class InventoryBackground : MonoBehaviour {
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Transform template;

    private void Start() {
        // Create background
        template.gameObject.SetActive(false);

        for (int x = 0; x < inventoryUI.GetGrid().GetWidth(); x++) {
            for (int y = 0; y < inventoryUI.GetGrid().GetHeight(); y++) {
                Transform backgroundSingleTransform = Instantiate(template, transform);
                backgroundSingleTransform.gameObject.SetActive(true);
            }
        }

        GetComponent<GridLayoutGroup>().cellSize = new Vector2(inventoryUI.GetGrid().GetCellSize(), inventoryUI.GetGrid().GetCellSize());

        GetComponent<RectTransform>().sizeDelta = new Vector2(inventoryUI.GetGrid().GetWidth(), inventoryUI.GetGrid().GetHeight()) * inventoryUI.GetGrid().GetCellSize();

        GetComponent<RectTransform>().anchoredPosition = inventoryUI.GetComponent<RectTransform>().anchoredPosition;
    }
}
