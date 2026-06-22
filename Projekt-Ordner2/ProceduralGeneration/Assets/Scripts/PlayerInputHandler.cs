using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
    public static PlayerInputHandler Instance { get; private set; }

    private InputSystem_Actions inputActions;

    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool SpaceTriggered { get; private set; }
    public bool EnterTriggered { get; private set; }

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
        inputActions.Player.Move.canceled += _ => MovementInput = Vector2.zero;

        inputActions.Player.Look.performed += context => LookInput = context.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => LookInput = Vector2.zero;

        inputActions.Player.Sprint.performed += _ => SprintTriggered = true;
        inputActions.Player.Sprint.canceled += _ => SprintTriggered = false;

        inputActions.Player.Enter.performed += _ => EnterTriggered = true;
        inputActions.Player.Enter.canceled += _ => EnterTriggered = false;

        inputActions.Player.Space.performed += _ => SpaceTriggered = true;
        inputActions.Player.Space.canceled += _ => SpaceTriggered = false;
    }

    public void SetSpaceTriggered(bool state) {
        SpaceTriggered = state;
    }

    public void SetEnterTriggered(bool state) {
        EnterTriggered = state;
    }
}
