using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Loads a UI canvas prefab via Addressables and shows/hides it. Assumes the
/// prefab has a UIPanel (or subclass) component on its root, so opening it
/// automatically requests UI mode and closing it releases that claim —
/// no changes needed to InputModeManagerScript or UIPanel for this to work.
///
/// Two lifetime strategies are provided:
/// - KeepLoaded (default): load once, then just SetActive(true/false) after
///   that. Good for panels you open/close often (inventory, pause menu).
/// - ReleaseOnHide: fully release the instance (and its loaded asset) each
///   time it's hidden. Good for rarely-shown, heavier panels (e.g. a
///   "reward collected" popup with big textures) where you'd rather free
///   the memory than keep it resident.
/// </summary>
public class AddressableUIPanelLoaderScript : MonoBehaviour
{
    public enum LifetimeStrategy { KeepLoaded, ReleaseOnHide }

    [SerializeField] private AssetReferenceGameObject panelPrefab;
    [SerializeField] private LifetimeStrategy strategy = LifetimeStrategy.KeepLoaded;
    [SerializeField] private Transform parent; // optional, e.g. a Canvas root

    private GameObject instance;
    private AsyncOperationHandle<GameObject> handle;
    private bool isLoading;

    /// <summary>True while the panel is loaded and currently active/visible.</summary>
    public bool IsOpen => instance != null && instance.activeSelf;

    /// <summary>
    /// Loads the panel if needed, shows it, and returns the instance once
    /// ready. Use this when you need to reach into the panel afterward
    /// (e.g. to set text/callbacks on a reusable dialog) — otherwise just
    /// call the fire-and-forget Show() below.
    /// </summary>
    public async Task<GameObject> ShowAsync()
    {
        if (instance != null)
        {
            instance.SetActive(true);
            return instance;
        }

        if (isLoading)
        {
            // Another Show() call is already loading this panel — wait for
            // it instead of starting a second concurrent load.
            while (isLoading) await Task.Yield();
            if (instance != null) instance.SetActive(true);
            return instance;
        }

        isLoading = true;

        handle = parent != null
            ? Addressables.InstantiateAsync(panelPrefab, parent)
            : Addressables.InstantiateAsync(panelPrefab);

        instance = await AwaitHandle(handle);
        isLoading = false;

        // instance is active-by-default on instantiate, so UIPanel.OnEnable
        // has already fired and requested UI mode by this point.
        return instance;
    }

    /// <summary>Fire-and-forget version of ShowAsync(), for UnityEvent/button wiring.</summary>
    public async void Show()
    {
        await ShowAsync();
    }

    /// <summary>Hides the panel per the configured lifetime strategy.</summary>
    public void Hide()
    {
        if (instance == null) return;

        if (strategy == LifetimeStrategy.KeepLoaded)
        {
            instance.SetActive(false); // fires UIPanel.OnDisable -> ReleaseUIMode
        }
        else
        {
            // ReleaseInstance destroys the GameObject (firing OnDisable first,
            // so ReleaseUIMode still runs) AND decrements the Addressables
            // ref count, freeing the underlying asset.
            Addressables.ReleaseInstance(handle);
            instance = null;
        }
    }

    private void OnDestroy()
    {
        if (instance != null && handle.IsValid())
            Addressables.ReleaseInstance(handle);
    }

    private static Task<GameObject> AwaitHandle(AsyncOperationHandle<GameObject> h)
    {
        var tcs = new TaskCompletionSource<GameObject>();
        h.Completed += op => tcs.SetResult(op.Result);
        return tcs.Task;
    }
}