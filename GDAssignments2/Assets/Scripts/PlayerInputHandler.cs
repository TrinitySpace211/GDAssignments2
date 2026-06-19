using UnityEngine;

public class PlayerInputHandler : MonoBehaviour {
    public static PlayerInputHandler Instance { get; private set; }

    private InputSystem_Actions inputActions;

    public Vector2 MovementInput { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        SubscribeToInputActions();
    }

    private void SubscribeToInputActions() {
        inputActions.Player.Move.performed += context => MovementInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => MovementInput = Vector2.zero;

        inputActions.Player.Sprint.performed += _ => SprintTriggered = true;
        inputActions.Player.Sprint.canceled += _ => SprintTriggered = false;

        inputActions.Player.Interact.performed += _ => InteractTriggered = true;
        inputActions.Player.Interact.canceled += _ => InteractTriggered = false;
    }
}
