using UnityEngine;

/// <summary>
/// Attach to any UI canvas/panel GameObject that should switch the game into
/// UI mode while it's open — inventory, pause menu, "reward collected" popup,
/// dialogue box, etc. Requests UI mode when enabled, releases its claim when
/// disabled. Because InputModeManager reference-counts requesters, it's safe
/// for several of these to be open at once (e.g. a reward popup appearing
/// while the inventory is already open) — Player mode only resumes once ALL
/// of them have closed.
///
/// Usage: put this on the root GameObject of each panel (the one you
/// SetActive(true/false) to show/hide it), or just call Show()/Hide() below
/// instead of SetActive directly.
/// </summary>
public class UIPanel : MonoBehaviour
{
    private void OnEnable()
    {
        InputModeManagerScript.Instance?.RequestUIMode(this);
    }

    private void OnDisable()
    {
        InputModeManagerScript.Instance?.ReleaseUIMode(this);
    }

    /// <summary>Convenience so other scripts can do panel.Show() / panel.Hide().</summary>
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}