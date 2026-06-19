using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private CharacterController characterController;
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    private Vector3 currentMovement;
    private float currentSpeed => walkSpeed * (PlayerInputHandler.Instance.SprintTriggered ? sprintMultiplier : 1f);

    private void Update() {
        HandleMovement();
    }

    private void HandleMovement() {
        float xMoveInput = PlayerInputHandler.Instance.MovementInput.x;
        float zMoveInput = PlayerInputHandler.Instance.MovementInput.y;

        currentMovement.x = xMoveInput * currentSpeed;
        currentMovement.y = 0f;
        currentMovement.z = zMoveInput * currentSpeed;


        characterController.Move(currentMovement * Time.deltaTime);
    }
}
