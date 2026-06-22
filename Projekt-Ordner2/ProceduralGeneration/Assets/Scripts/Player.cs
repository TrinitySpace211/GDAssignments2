using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    [Header("References")]
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

    private float transitionDuration = 1.5f;

    public bool InDungeon { get; private set; } = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = cursorVisible;
        characterController.enabled = false;
    }

    private void Update() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Application.Quit();
        }
        if (characterController.enabled) {
            HandleMovement();
        }
    }

    private void LateUpdate() {
        if (characterController.enabled) {
            HandleLook();
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

    public void EnterDungeon(List<GameObject> availableFloorTiles) {
        GameObject targetTile = availableFloorTiles[UnityEngine.Random.Range(0, availableFloorTiles.Count)];

        StartCoroutine(MoveToTileRoutine(targetTile.transform.position + new Vector3(0, 1f, 0)));
    }

    private IEnumerator MoveToTileRoutine(Vector3 targetPosition) {
        // Sicherstellen, dass die Steuerung während der Fahrt deaktiviert ist
        characterController.enabled = false;

        Vector3 startPosition = transform.position;

        // Für die Rotation der Kamera: Wir wollen auch die Rotation des Player-Körpers nullen, 
        // damit "Vorwärts" nach dem Übergang auch wirklich Norden/Standard-Vorwärts entspricht.
        Quaternion startPlayerRotation = transform.rotation;
        Quaternion targetPlayerRotation = Quaternion.identity; // Blickrichtung geradeaus (Y=0)

        Quaternion startCameraRotation = mainCamera.transform.localRotation;
        Quaternion targetCameraRotation = Quaternion.identity; // Kamera blickt geradeaus relativ zum Spieler

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration) {
            elapsedTime += Time.deltaTime;

            // Berechne den Fortschritt (Wert zwischen 0.0 und 1.0)
            float t = elapsedTime / transitionDuration;

            // Ease In / Out
            t = Mathf.SmoothStep(0f, 1f, t);

            // Position interpolieren
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Spieler rotieren
            transform.rotation = Quaternion.Slerp(startPlayerRotation, targetPlayerRotation, t);

            // Kamera rotieren
            mainCamera.transform.localRotation = Quaternion.Slerp(startCameraRotation, targetCameraRotation, t);

            yield return null;
        }

        // Am Ende exakte Werte erzwingen, um Rundungsfehler zu vermeiden
        transform.position = targetPosition;
        transform.rotation = targetPlayerRotation;
        mainCamera.transform.localRotation = targetCameraRotation;
        verticalRotation = 0f; // Interne Look-Variable zurücksetzen, sonst springt die Kamera beim ersten Mausklick

        // Zustand anpassen und Steuerung wieder freigeben
        InDungeon = true;
        characterController.enabled = true;
    }

}
