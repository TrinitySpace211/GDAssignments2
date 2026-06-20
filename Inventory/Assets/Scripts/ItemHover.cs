using System;
using UnityEngine;

public class ItemHover : MonoBehaviour {
    public static event Action<ItemSO> OnItemCollected;

    [SerializeField] private ItemSO itemSO;
    [SerializeField] private InputInfoUI inputInfoUI;
    [SerializeField] private bool animateItem = false;

    private float rangeMultiplier = 0.2f;
    private float animationSpeed = 3f;
    private float animationTimer = 0f;
    private float rotationSpeed = 20f;

    private bool isInRange = false;
    private Transform itemInstance;

    private void Start() {
        itemInstance = Instantiate(itemSO.drop);
        itemInstance.SetParent(transform);
        itemInstance.localPosition = Vector3.zero;

        animationTimer = UnityEngine.Random.Range(1, 10);
    }

    private void Update() {
        if (animateItem) {
            AnimateItem();
            RotateItem();
        }

        if (isInRange && PlayerInputHandler.Instance.InteractTriggered) {
            OnItemCollected?.Invoke(itemSO);
            PlayerInputHandler.Instance.SetInteractTriggered(false);
        }
    }

    private void AnimateItem() {
        animationTimer += Time.deltaTime;

        float position = 1f + rangeMultiplier * Mathf.Sin(animationTimer * animationSpeed);
        itemInstance.transform.localPosition = Vector3.up * position;
    }

    private void RotateItem() {
        itemInstance.transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * rotationSpeed);
    }


    private void OnTriggerEnter(Collider other) {
        inputInfoUI.Show();
        isInRange = true;
    }

    private void OnTriggerExit(Collider other) {
        inputInfoUI.Hide();
        isInRange = false;
    }
}
