using UnityEngine;

/// <summary>
/// Put this on a persistent GameObject (e.g. under "Loaders and
/// Managers", alongside the other services). Singleton entry point for
/// showing/hiding the relic-collected popup — callers (RelicTriggererScript)
/// go through Show()/Hide() here rather than touching the Addressable
/// loader or UIPanel directly.
///
/// Input mode is NOT handled here at all — the popup prefab's own UIPanel
/// component does that automatically via OnEnable/OnDisable the moment
/// the loader activates/deactivates it. This service only cares about
/// loading and configuring the popup's content.
/// </summary>
public class RelicCollectedPopupServiceScript : MonoBehaviour
{
    public static RelicCollectedPopupServiceScript Instance { get; private set; }

    [SerializeField] private AddressableUIPanelLoaderScript _popupLoader;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public async void Show(string itemName, Sprite itemSprite, Sprite logoSprite)
    {
        GameObject instance = await _popupLoader.ShowAsync();
        RelicFoundRewardCanvasScript popup = instance.GetComponent<RelicFoundRewardCanvasScript>();
        popup.Configure(itemName, itemSprite, logoSprite);
    }

    public void Hide()
    {
        _popupLoader.Hide();
    }
}