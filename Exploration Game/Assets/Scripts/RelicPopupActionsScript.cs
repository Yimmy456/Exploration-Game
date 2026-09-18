using UnityEngine;

/// <summary>
/// Put on the Relic Collected Popup Canvas prefab. Wire the Continue
/// button's OnClick to OnContinueButtonPressed().
/// </summary>
public class RelicPopupActionsScript : MonoBehaviour
{
    public void OnContinueButtonPressed()
    {
        RelicCollectedPopupServiceScript.Instance.Hide();
    }
}