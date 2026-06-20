using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManagement : MonoBehaviour {

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject collection;

    private InventoryUI inventoryUI;

    private bool isOpen = false;

    private void Awake() {
        inventoryUI = GetComponent<InventoryUI>();
    }

    private void Start() {
        HideInventory();

        ItemHover.OnItemCollected += ItemHover_OnItemCollected;
    }

    private void Update() {
        if (PlayerInputHandler.Instance.InventoryTriggered) {
            if (collection.activeInHierarchy) {
                HideInventory();
            } else {
                ShowInventory();
            }
            PlayerInputHandler.Instance.SetInventoryTriggered(false);
        }
    }

    private void ItemHover_OnItemCollected(ItemSO itemSO) {
        bool success = inventoryUI.TryPlaceItemSomewhere(itemSO, 1);

        Debug.Log("Placed?: " + success);
    }

    public void ShowInventory() {
        background.SetActive(true);
        collection.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isOpen = true;
    }

    public void HideInventory() {
        //inventoryUI.GetInventoryDragAndDropSystem().CancelDrag(inventoryUI);
        background.SetActive(false);
        collection.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isOpen = false;
    }

    public bool GetInventoryOpen() {
        return isOpen;
    }

}
