using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Listens for the Inventory action (I key) and toggles the inventory
/// canvas.
///
/// Same reasoning as PauseMenuControllerScript: the Inventory action lives
/// in the "Global" action map so it keeps working no matter what mode
/// input is currently in — including while the inventory itself is already
/// open (which switches input into UI mode via the canvas's own UIPanel
/// component) — otherwise the same key couldn't be used to close it again.
///
/// Setup:
/// 1. In the Input Actions asset, add an action named "Inventory" to the
///    existing "Global" action map (the same one Pause is in), bound to
///    Keyboard/I.
/// 2. Put this component on the same persistent GameObject as
///    InputModeManager / PauseMenuControllerScript, assign the same
///    InputActionAsset, and assign an AddressableUIPanelLoaderScript whose
///    panel prefab is the inventory canvas — set up the same way as the
///    "Press To Collect" prompt loader / reward popup loader, if that
///    hasn't been added for the inventory canvas yet.
/// </summary>
public class InventoryMenuControllerScript : MonoBehaviour
{
    public static InventoryMenuControllerScript Instance { get; private set; }

    public const string GlobalActionMapName = "Global";
    public const string InventoryActionName = "Inventory";

    [SerializeField] private InputActionAsset _inputAssets;
    [SerializeField] private AddressableUIPanelLoaderScript _inventoryLoader;

    private InputActionMap _globalMap;
    private InputAction _inventoryAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _globalMap = _inputAssets.FindActionMap(GlobalActionMapName);
        _inventoryAction = _globalMap.FindAction(InventoryActionName);
    }

    private void OnEnable()
    {
        // Enabling an already-enabled map (e.g. PauseMenuControllerScript
        // already turned it on) is a harmless no-op — both components can
        // safely share ownership of the same Global map this way.
        _globalMap.Enable();
        _inventoryAction.performed += OnInventoryPerformed;
    }

    private void OnDisable()
    {
        _inventoryAction.performed -= OnInventoryPerformed;
        _globalMap.Disable();
    }

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (ItemDescriptionCanvasScript.Instance != null)
        {
            if (ItemDescriptionCanvasScript.Instance.IsOpen)
            {
                return;
            }
        }

        if (_inventoryLoader.IsOpen)
            _inventoryLoader.Hide();
        else
            _inventoryLoader.Show();
    }

    public void HideInventory()
    {
        _inventoryLoader.Hide();
    }
}