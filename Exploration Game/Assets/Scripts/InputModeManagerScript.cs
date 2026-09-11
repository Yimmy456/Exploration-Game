using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central manager for toggling between "Player" mode (gameplay control,
/// locked/hidden cursor) and "UI" mode (menu/inventory interaction,
/// visible/unlocked cursor).
///
/// Matches this project's actual input architecture: a single shared
/// InputActionAsset with a "Player" map (owned/enabled by
/// PlayerInputHandlerScript) and an "In-Game UI" map (owned/enabled here).
/// No PlayerInput component is used.
///
/// Setup:
/// 1. Assign the SAME InputActionAsset here and on PlayerInputHandlerScript.
/// 2. Assign the PlayerInputHandlerScript reference for the active player.
/// 3. Put this on a persistent GameObject (e.g. a GameManager).
/// </summary>
public class InputModeManagerScript : MonoBehaviour
{
    public static InputModeManagerScript Instance { get; private set; }

    public enum Mode { Player, UI }

    [Header("State")]
    [SerializeField] private Mode currentMode = Mode.Player;
    public Mode CurrentMode => currentMode;

    [Header("Input")]
    [SerializeField] private InputActionAsset _inputAssets;
    [SerializeField] private PlayerInputHandlerScript _playerInputHandler;

    private InputActionMap _uiMap;

    /// <summary>Fired whenever the mode changes. Subscribers get the new mode.</summary>
    public static event Action<Mode> OnModeChanged;

    // Tracks every canvas/system currently asking for UI mode. As long as
    // this set is non-empty we stay in UI mode. This lets the inventory,
    // pause menu, and a reward popup each open/close independently without
    // one closing early and yanking control back while another is still open.
    private readonly HashSet<object> uiRequesters = new HashSet<object>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _uiMap = _inputAssets.FindActionMap(PlayerInputHandlerScript.UIActionMapName);
    }

    private void Start()
    {
        // currentMode already defaults to Player, so force ApplyMode to
        // actually run once (rather than early-out because "nothing changed").
        currentMode = Mode.UI;
        ApplyMode(Mode.Player);
    }

    /// <summary>
    /// Ask for UI mode on behalf of `requester` (typically "this" from a
    /// canvas script, e.g. UIPanel). Safe to call even if already in UI
    /// mode. UI mode persists until every requester that asked for it has
    /// called ReleaseUIMode.
    /// </summary>
    public void RequestUIMode(object requester)
    {
        uiRequesters.Add(requester);
        ApplyMode(Mode.UI);
    }

    /// <summary>
    /// Release this requester's claim on UI mode. If no other canvas/system
    /// is still holding a claim, this switches back to Player mode.
    /// </summary>
    public void ReleaseUIMode(object requester)
    {
        uiRequesters.Remove(requester);
        if (uiRequesters.Count == 0)
            ApplyMode(Mode.Player);
        // else: something else is still open, stay in UI mode.
    }

    /// <summary>
    /// Force Player mode and clear all outstanding requesters. Use sparingly
    /// (e.g. loading into gameplay from a main menu) since it bypasses the
    /// normal request/release bookkeeping.
    /// </summary>
    public void ForcePlayerMode()
    {
        uiRequesters.Clear();
        ApplyMode(Mode.Player);
    }

    private void ApplyMode(Mode newMode)
    {
        if (currentMode == newMode) return;

        currentMode = newMode;

        switch (newMode)
        {
            case Mode.Player:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                _uiMap.Disable();
                if (_playerInputHandler != null)
                {
                    _playerInputHandler.enabled = true; // its OnEnable resubscribes + enables the "Player" map
                    Debug.Log("Player mode 'On'.");
                }
                break;

            case Mode.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (_playerInputHandler != null) { 
                    _playerInputHandler.enabled = false; // its OnDisable unsubscribes, disables "Player" map, and zeroes held input
                    Debug.Log("Player mode 'Off'.");
                }
                _uiMap.Enable();
                break;
        }

        OnModeChanged?.Invoke(currentMode);
    }
}