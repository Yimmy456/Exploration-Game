using UnityEngine;

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
        var playerInput = other.GetComponent<PlayerInputScript>();
        if (playerInput == null) return;

        // PlayerInputScript already holds a correctly Inspector-wired
        // reference to Handler regardless of where Handler sits in the
        // hierarchy — reuse it instead of doing a second, hierarchy-
        // dependent GetComponent search that could silently fail again.
        _playerInputHandler = playerInput.Handler;
        _playerCamera = playerInput.Camera;

        RelicVisibilityManagerScript.Instance.Register(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerInputScript>() == null) return;

        RelicVisibilityManagerScript.Instance.Unregister(this);
        HidePromptIfShown();
    }

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

        if (Physics.Raycast(origin, toRelic.normalized, out RaycastHit hit, distance, _obstructionMask, QueryTriggerInteraction.Ignore))
        {
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

        // _relic.SetIsCollected() calls Destroy(gameObject) on the relic's
        // container, but Unity defers actual destruction to the end of
        // the frame — so it's safe to keep using _relic/this object below,
        // and to call the popup service right here.
        //
        // itemSprite/logoSprite are passed null for now — RelicDatabaseClass's
        // Relic has RelicThumbnailAddress (an Addressable string key), but
        // nothing yet loads that into an actual Sprite, and there's no
        // franchise-logo field in the data model at all yet. Configure()
        // already handles null gracefully (the Image just disables itself),
        // so this isn't blocking — just flagging what's still open.
        Relic relicData = RelicDatabaseClass.GetById(_relic.RelicID);
        string itemName = relicData != null ? relicData.RelicName : _relic.RelicID;
        RelicCollectedPopupServiceScript.Instance.Show(itemName, null, null);
    }

    private void HidePromptIfShown()
    {
        if (!_promptShown) return;
        _promptShown = false;
        InteractionPromptScript.Instance.Hide(this);
    }
}