using UnityEngine;

/// <summary>
/// Lives on the "Triggerer" child object — the one that actually owns the
/// trigger Collider. Forwards proximity/collect events up to the relic's
/// RelicContainerScript, since OnTriggerEnter/Exit only fire on the exact
/// GameObject the Collider is attached to, not its parent.
///
/// Has no Update() of its own — TickVisibility() is called once per frame
/// by RelicVisibilityManagerScript, but only while this relic is
/// registered (i.e. the player is currently inside its trigger). Relics
/// the player hasn't approached cost nothing per frame.
///
/// A relic is only actually collectable when the player is BOTH within
/// range AND has it in sight (within the camera's view and not blocked by
/// anything) — re-checked every tick since the player can turn away from a
/// relic without ever leaving the trigger volume.
/// </summary>
public class RelicTriggererScript : MonoBehaviour
{
    [SerializeField] private RelicContainerScript _relic;
    [SerializeField] private string _promptMessage = "Press 'X' to collect it.";

    [Header("Line of Sight")]
    [Tooltip("Layers that should block line of sight (e.g. walls/environment). Should NOT include the Player or the relic's own layer.")]
    [SerializeField] private LayerMask _obstructionMask;

    private PlayerInputHandlerScript _playerInputHandler;
    private Camera _playerCamera;
    private bool _promptShown;
    private bool _collected;

    private void OnTriggerEnter(Collider other)
    {
        // Look for the handler on whatever entered, rather than matching by
        // name — works regardless of what the player object is called, and
        // regardless of where in its hierarchy the trigger collider is.
        var handler = other.GetComponentInParent<PlayerInputHandlerScript>();
        if (handler == null) return;

        _playerInputHandler = handler;
        _playerCamera = other.GetComponentInParent<PlayerInputScript>()?.Camera;
        RelicVisibilityManagerScript.Instance.Register(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerInputHandlerScript>() == null) return;

        RelicVisibilityManagerScript.Instance.Unregister(this);
        HidePromptIfShown();
    }

    /// <summary>
    /// Called once per frame by RelicVisibilityManagerScript while this
    /// relic is registered. Checks sight, shows/hides the prompt as that
    /// changes, and collects if the Collect input fires while visible.
    /// </summary>
    public void TickVisibility()
    {
        if (_collected || _playerInputHandler == null) return;

        bool visible = IsRelicVisible();

        if (visible != _promptShown)
        {
            _promptShown = visible;
            if (visible)
                InteractionPromptScript.Instance.Show(this, _promptMessage);
            else
                InteractionPromptScript.Instance.Hide(this);
        }

        if (visible && _playerInputHandler.CollectInput)
        {
            Collect();
        }
    }

    /// <summary>
    /// True if the relic is within the camera's viewport AND nothing on
    /// _obstructionMask sits between the camera and the relic.
    /// </summary>
    private bool IsRelicVisible()
    {
        if (_playerCamera == null || _relic == null) return false;

        Transform relicTransform = _relic.transform;
        Vector3 viewportPoint = _playerCamera.WorldToViewportPoint(relicTransform.position);

        bool inFrontOfCamera = viewportPoint.z > 0f;
        bool withinScreenBounds = viewportPoint.x >= 0f && viewportPoint.x <= 1f
                                && viewportPoint.y >= 0f && viewportPoint.y <= 1f;

        if (!inFrontOfCamera || !withinScreenBounds) return false;

        Vector3 origin = _playerCamera.transform.position;
        Vector3 toRelic = relicTransform.position - origin;
        float distance = toRelic.magnitude;

        // Ignore trigger colliders (like this very Triggerer's own
        // BoxCollider) so they can't falsely block their own line of sight.
        if (Physics.Raycast(origin, toRelic.normalized, out RaycastHit hit, distance, _obstructionMask, QueryTriggerInteraction.Ignore))
        {
            // Hit something before reaching the relic that isn't the relic
            // itself (e.g. it has its own solid collider) — actually blocked.
            if (hit.transform != relicTransform && !hit.transform.IsChildOf(relicTransform))
                return false;
        }

        return true;
    }

    private void Collect()
    {
        _collected = true;
        RelicVisibilityManagerScript.Instance.Unregister(this);
        HidePromptIfShown();
        _relic.SetIsCollected();
    }

    private void HidePromptIfShown()
    {
        if (!_promptShown) return;
        _promptShown = false;
        InteractionPromptScript.Instance.Hide(this);
    }
}