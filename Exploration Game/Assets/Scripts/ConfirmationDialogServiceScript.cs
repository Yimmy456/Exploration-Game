using System;
using UnityEngine;

/// <summary>
/// Global entry point for showing a Yes/No confirmation prompt from
/// anywhere — the pause menu's Quit button, a settings panel's Save/Cancel,
/// etc. — without each caller needing its own reference to the dialog
/// prefab or loader.
///
/// Because panels stack via the reference-counted UIPanel system, showing
/// this on top of another open panel (e.g. the pause menu) and then
/// dismissing it on "No" naturally reveals whatever was underneath again —
/// nothing needs to explicitly "go back" to the pause menu, since it was
/// never actually hidden.
///
/// Setup: put this on a persistent GameObject (e.g. alongside
/// InputModeManagerScript), and assign a ConfirmationDialogLoader
/// (AddressableUIPanelLoaderScript pointing at the confirmation dialog
/// prefab) to _dialogLoader.
/// </summary>
public class ConfirmationDialogServiceScript : MonoBehaviour
{
    public static ConfirmationDialogServiceScript Instance { get; private set; }

    [SerializeField] private AddressableUIPanelLoaderScript _dialogLoader;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Shows the confirmation dialog with the given message. onYes/onNo run
    /// when the corresponding button is pressed; pass null for either if
    /// you don't need to react to that outcome.
    /// </summary>
    public async void Confirm(string message, Action onYes, Action onNo = null)
    {
        GameObject instance = await _dialogLoader.ShowAsync();
        if (instance == null) return; // load failed or was superseded

        ConfirmationDialogScript dialog = instance.GetComponent<ConfirmationDialogScript>();
        dialog.Configure(message, onYes, onNo);
    }
}