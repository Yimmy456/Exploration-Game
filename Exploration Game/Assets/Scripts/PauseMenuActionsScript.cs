using UnityEditor;
using UnityEngine;

/// <summary>
/// Put this on the Pause Menu Canvas prefab. Wire the Quit button's OnClick
/// to OnQuitButtonPressed() (no parameters needed, so it's a plain
/// Inspector-assignable UnityEvent target).
/// </summary>
public class PauseMenuActionsScript : MonoBehaviour
{
    public void OnResumeButtonPressed()
    {
        PauseMenuControllerScript.Instance?.HidePauseMenu();
    }


    public void OnQuitButtonPressed()
    {
        ConfirmationDialogServiceScript.Instance.Confirm(
            "Are you sure you want to quit?",
            onYes: () => QuitGame()
            // onNo left null — dismissing the dialog is enough; the pause
            // menu underneath was never hidden, so it's already showing again.
        );
    }

    private void QuitGame()
    {
        // Application.Quit() is a no-op inside the Editor's Play mode — it
        // only actually quits in a built player. This branch lets you verify
        // the button in the Editor by just stopping Play mode instead.
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}