using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Listens for the Pause action (P key / gamepad Start or Menu button) and
/// toggles the pause menu.
///
/// The Pause action must live in its own action map — here called "Global"
/// — that InputModeManager never touches. If it lived in "Player" or
/// "In-Game UI" instead, it would stop working the moment that map gets
/// disabled, which defeats the point of a pause key that should work no
/// matter what mode you're currently in.
///
/// Setup:
/// 1. In your Input Actions asset, add a new action map named "Global".
/// 2. Add an action named "Pause" to it, with bindings for Keyboard/P and
///    Gamepad/Start (or Gamepad/Select, depending on your target platform).
/// 3. Put this component on the same persistent GameObject as
///    InputModeManager, assign the same InputActionAsset, and assign the
///    AddressableUIPanelLoader for your pause menu prefab.
/// </summary>
public class PauseMenuControllerScript : MonoBehaviour
{
    public static PauseMenuControllerScript Instance { get; private set; }

    public const string GlobalActionMapName = "Global";
    public const string PauseActionName = "Pause";

    [SerializeField] private InputActionAsset _inputAssets;
    [SerializeField] private AddressableUIPanelLoaderScript _pauseMenuLoader;

    private InputActionMap _globalMap;
    private InputAction _pauseAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _globalMap = _inputAssets.FindActionMap(GlobalActionMapName);
        _pauseAction = _globalMap.FindAction(PauseActionName);
    }

    private void OnEnable()
    {
        _globalMap.Enable();
        _pauseAction.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        _pauseAction.performed -= OnPausePerformed;
        _globalMap.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (_pauseMenuLoader.IsOpen)
            _pauseMenuLoader.Hide();
        else
            _pauseMenuLoader.Show();
    }

    public void HidePauseMenu()
    {
        _pauseMenuLoader.Hide();
    }
}