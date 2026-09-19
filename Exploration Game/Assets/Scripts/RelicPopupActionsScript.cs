using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lives on the popup canvas root, alongside UIPanel and
/// RelicFoundRewardCanvasScript. Wires the Continue button's OnClick, and
/// additionally listens directly to the shared "UI" action map's Submit
/// and Cancel actions so the popup can be dismissed by keyboard/gamepad
/// even if the EventSystem's selected object gets cleared (e.g. the
/// player clicks elsewhere on the canvas first).
///
/// RelicFoundRewardCanvasScript.Configure() already selects the Continue
/// button each time this shows, which covers Submit via Unity's normal
/// Button/Selectable handling and keeps arrow-key/controller highlight
/// working — this direct subscription is what makes Cancel work at all
/// (Selectable has no built-in Cancel handling) and makes Submit robust
/// to a lost selection.
///
/// Subscribes in OnEnable and unsubscribes in OnDisable, so listening is
/// scoped exactly to while this popup is actually shown (the loader
/// SetActive(true/false)s this same root to show/hide it).
/// </summary>
public class RelicPopupActionsScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputAssets;

    private InputAction _submitAction;
    private InputAction _cancelAction;

    private void OnEnable()
    {
        if(_inputAssets == null)
        {
            return;
        }

        // Reuses the same "UI" map name constant PlayerInputHandlerScript
        // and InputModeManagerScript already key off of, so this can't
        // drift out of sync with the rest of the project if that name
        // ever changes.

        InputActionMap _uiMap = _inputAssets.FindActionMap(PlayerInputHandlerScript.UIActionMapName);

        if(_uiMap == null)
        {
            return;
        }

        _submitAction = _uiMap.FindAction(PlayerInputHandlerScript.Submit);
        _cancelAction = _uiMap.FindAction(PlayerInputHandlerScript.Cancel);

        if(_submitAction != null) { _submitAction.performed += OnDismissInput; }
        if(_cancelAction != null) { _cancelAction.performed += OnDismissInput; }
    }

    private void OnDisable()
    {
        if(_submitAction != null) { _submitAction.performed -= OnDismissInput; }
        if(_cancelAction != null) { _cancelAction.performed -= OnDismissInput; }

        _submitAction = null;
        _cancelAction = null;
    }

    void OnDismissInput(InputAction.CallbackContext ctx)
    {
        RelicCollectedPopupServiceScript.Instance.Hide();
    }

    // Still wired to the Continue button's OnClick in the Inspector —
    // covers a direct mouse/touch click, unchanged from before.
    public void OnContinueButtonPressed()
    {
        RelicCollectedPopupServiceScript.Instance.Hide();
    }
}