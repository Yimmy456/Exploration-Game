using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandlerScript : MonoBehaviour
{

    [SerializeField] InputActionAsset _inputAssets;

    public const string ActionMapName = "Player";
    public const string UIActionMapName = "UI";

    public const string Movement = "Move";
    public const string Rotation = "Look";
    public const string Jump = "Jump";
    public const string Collect = "Collect";

    public const string Pause = "Pause";

    // UI action map actions — referenced by name here (rather than as
    // literals wherever they're looked up) so every other script that
    // needs to find them, such as RelicPopupActionsScript, has a single
    // shared source for the strings instead of duplicating them.
    public const string Submit = "Submit";
    public const string Cancel = "Cancel";

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
        // Skip the very first OnEnable — Unity fires it right after Awake,
        // and SubscribeInputs() is called there via OnEnable already once
        // the map/actions exist. Every OnEnable after a disable (e.g. when
        // this component is toggled off/on for UI mode) subscribes again.
        SubscribeInputs();
    }

    private void OnDisable()
    {
        UnsubscribeInputs();
    }

    private void OnDestroy()
    {
        // Make sure we're unsubscribed even if OnDisable didn't run (e.g.
        // object destroyed while inactive), then release the map.
        UnsubscribeInputs();
        _inputAssets.FindActionMap(ActionMapName)?.Dispose();
    }

    void Initialize()
    {
        InputActionMap _map = _inputAssets.FindActionMap(ActionMapName);

        MovementAction = _map.FindAction(Movement);
        RotationAction = _map.FindAction(Rotation);
        JumpAction = _map.FindAction(Jump);
        CollectAction = _map.FindAction(Collect);

        // Note: subscribing here was removed — Unity always calls OnEnable()
        // right after Awake() for an active object, so SubscribeInputs()
        // firing from OnEnable alone is sufficient and avoids a double-subscribe.
    }

    void SubscribeInputs()
    {
        _inputAssets.FindActionMap(ActionMapName).Enable();

        MovementAction.performed += OnMovementChanged;
        MovementAction.canceled += OnMovementChanged;

        RotationAction.performed += OnRotationChanged;
        RotationAction.canceled += OnRotationChanged;

        JumpAction.performed += OnJumpPerformed;
        JumpAction.canceled += OnJumpCanceled;

        CollectAction.performed += OnCollectPerformed;
        CollectAction.canceled += OnCollectCanceled;
    }

    void UnsubscribeInputs()
    {
        MovementAction.performed -= OnMovementChanged;
        MovementAction.canceled -= OnMovementChanged;

        RotationAction.performed -= OnRotationChanged;
        RotationAction.canceled -= OnRotationChanged;

        JumpAction.performed -= OnJumpPerformed;
        JumpAction.canceled -= OnJumpCanceled;

        CollectAction.performed -= OnCollectPerformed;
        CollectAction.canceled -= OnCollectCanceled;

        _inputAssets.FindActionMap(ActionMapName).Disable();

        // Belt-and-suspenders: don't rely on a canceled callback reaching us
        // after we've just unsubscribed (it won't) to reset held state.
        // If a key was held the instant this component gets disabled, this
        // is what actually stops the character from moving/turning forever.
        MovementInput = Vector2.zero;
        RotationInput = Vector2.zero;
        JumpInput = false;
        CollectInput = false;
    }

    // Named handler methods (instead of inline lambdas) so the exact same
    // delegate instance can be used for both += and -=, which is what makes
    // unsubscribing actually work.
    private void OnMovementChanged(InputAction.CallbackContext ctx) => MovementInput = ctx.ReadValue<Vector2>();
    private void OnRotationChanged(InputAction.CallbackContext ctx) => RotationInput = ctx.ReadValue<Vector2>();
    private void OnJumpPerformed(InputAction.CallbackContext ctx) => JumpInput = true;
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => JumpInput = false;
    private void OnCollectPerformed(InputAction.CallbackContext ctx) => CollectInput = true;
    private void OnCollectCanceled(InputAction.CallbackContext ctx) => CollectInput = false;
}