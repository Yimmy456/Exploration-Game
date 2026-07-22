using System.Globalization;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerInputHandlerScript : MonoBehaviour
{

    [SerializeField] InputActionAsset _inputAssets;

    public const string ActionMapName = "Player";

    public const string Movement = "Move";
    public const string Rotation = "Look";
    public const string Jump = "Jump";
    public const string Collect = "Collect";

    public InputAction MovementAction { get; private set; }
    public InputAction RotationAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction CollectAction { get; private set; }

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool CollectInput { get; private set; }


    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        SubscribeInputs();
    }

    private void OnDisable()
    {
        UnsubscribeInputs();
    }

    private void OnDestroy()
    {
        UnsubscribeInputs(true);
    }

    void Initialize()
    {
        InputActionMap _map = _inputAssets.FindActionMap(ActionMapName);

        MovementAction = _map.FindAction(Movement);
        RotationAction = _map.FindAction(Rotation);
        JumpAction = _map.FindAction(Jump);
        CollectAction = _map.FindAction(Collect);

        SubscribeInputs();
    }

    void SubscribeInputs()
    {
        _inputAssets.FindActionMap(ActionMapName).Enable();

        MovementAction.performed += ctx => MovementInput = ctx.ReadValue<Vector2>();
        MovementAction.canceled += ctx => MovementInput = Vector2.zero;

        RotationAction.performed += ctx => RotationInput = ctx.ReadValue<Vector2>();
        RotationAction.canceled += ctx => RotationInput = Vector2.zero;

        JumpAction.performed += ctx => JumpInput = true;
        JumpAction.canceled += ctx => JumpInput = false;

        CollectAction.performed += ctx => CollectInput = true;
        CollectAction.canceled += ctx => CollectInput = false;
    }

    void UnsubscribeInputs(bool _dispose = false)
    {
        MovementAction.performed -= ctx => MovementInput = ctx.ReadValue<Vector2>();
        MovementAction.canceled -= ctx => MovementInput = Vector2.zero;

        RotationAction.performed -= ctx => RotationInput = ctx.ReadValue<Vector2>();
        RotationAction.canceled -= ctx => RotationInput = Vector2.zero;

        JumpAction.performed -= ctx => JumpInput = true;
        JumpAction.canceled -= ctx => JumpInput = false;

        CollectAction.performed -= ctx => CollectInput = true;
        CollectAction.canceled -= ctx => CollectInput = false;

        _inputAssets.FindActionMap(ActionMapName).Disable();
    }
}
