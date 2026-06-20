using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    [Header("References")]
    [SerializeField] private InventoryManagement inventoryManagement;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool cursorVisible = false;

    [Header("Move Parameters")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float upDownLookRange = 80f;

    private Vector3 currentMovement;
    private float verticalRotation;
    private float currentSpeed => walkSpeed * (PlayerInputHandler.Instance.SprintTriggered ? sprintMultiplier : 1f);

    private void Start() {
        Cursor.visible = cursorVisible;
    }

    private void Update() {
        if (!inventoryManagement.GetInventoryOpen()) {
            HandleMovement();
            HandleLook();

            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                Application.Quit();
            }
        } else if (PlayerInputHandler.Instance.MovementInput != Vector2.zero || Keyboard.current.escapeKey.wasPressedThisFrame) {
            inventoryManagement.HideInventory();
        }
    }

    private void HandleMovement() {
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 inputDirection = cameraRight * PlayerInputHandler.Instance.MovementInput.x +
                                    cameraForward * PlayerInputHandler.Instance.MovementInput.y;

        currentMovement.x = inputDirection.normalized.x * currentSpeed;
        currentMovement.y = 0f;
        currentMovement.z = inputDirection.normalized.z * currentSpeed;

        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void HandleLook() {
        float mouseXRotation = PlayerInputHandler.Instance.LookInput.x * mouseSensitivity;
        float mouseYRotation = PlayerInputHandler.Instance.LookInput.y * mouseSensitivity;

        //Horizontal Camera Rotation
        transform.Rotate(0, mouseXRotation, 0);

        //Vertical Camera Rotation
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

}
