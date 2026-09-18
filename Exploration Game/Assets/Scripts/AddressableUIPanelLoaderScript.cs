using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Loads a UI canvas prefab via Addressables and shows/hides it. Generic —
/// one instance of this component per Addressable prefab you want to
/// manage this way (Pause Menu, Confirmation Dialog, Relic Collected
/// Popup, and now the relic "Press X to collect" prompt).
///
/// Two lifetime strategies:
/// - KeepLoaded (default): load once, then just SetActive(true/false)
///   after that. Good for panels opened/closed often, or ones — like the
///   relic prompt canvas — that should just stay loaded and resident for
///   the whole session once instantiated.
/// - ReleaseOnHide: fully destroy the instance (and free the Addressables
///   asset) each time it's hidden. Good for rarely-shown, heavier panels.
///
/// For the relic prompt canvas specifically: its root carries
/// InteractionPromptScript, which already does its own reference-counted
/// Show()/Hide() on a *child* object (_promptRoot). This loader's job is
/// only to get that root instantiated once — call ShowAsync() a single
/// time at startup and never call Hide() on this loader. Visibility of the
/// actual prompt text is entirely InteractionPromptScript's job from then on.
/// </summary>
public class AddressableUIPanelLoaderScript : MonoBehaviour
{
    public enum LifetimeStrategy { KeepLoaded, ReleaseOnHide }

    [SerializeField] private AssetReferenceGameObject panelPrefab;
    [SerializeField] private LifetimeStrategy strategy = LifetimeStrategy.KeepLoaded;
    [SerializeField] private Transform parent; // optional, e.g. a persistent Canvas root

    [Tooltip("If true, this loader calls Show() itself on Start() — for panels that should just be resident from scene start (e.g. the relic prompt canvas), so nothing else needs to trigger the initial load. Leave false for on-demand panels like the Pause Menu, which open only when something calls Show()/ShowAsync().")]
    [SerializeField] private bool loadOnStart = false;

    private GameObject instance;
    private AsyncOperationHandle<GameObject> handle;
    private bool isLoading;

    private void Start()
    {
        if (loadOnStart) Show();
    }

    /// <summary>True while the panel is loaded and currently active.</summary>
    public bool IsOpen => instance != null && instance.activeSelf;

    /// <summary>
    /// Loads the panel if needed, shows it, and returns the instance once
    /// ready. Use this when you need to reach into the panel afterward —
    /// otherwise the fire-and-forget Show() below is enough.
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
            // Another ShowAsync() call is already loading this panel —
            // wait for it instead of starting a second concurrent load.
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

        // instance is active-by-default on instantiate, so any UIPanel or
        // singleton-registering Awake() on its root has already run.
        return instance;
    }

    /// <summary>Fire-and-forget version of ShowAsync(), for UnityEvent/button wiring
    /// or a simple boot call. For the relic prompt canvas, call this once
    /// at startup and never call Hide() below.</summary>
    public async void Show()
    {
        await ShowAsync();
    }

    /// <summary>Hides the panel per the configured lifetime strategy.
    /// Do NOT call this for the relic prompt canvas loader instance —
    /// InteractionPromptScript.Hide() already handles its own visibility.</summary>
    public void Hide()
    {
        if (instance == null) return;

        if (strategy == LifetimeStrategy.KeepLoaded)
        {
            instance.SetActive(false);
        }
        else
        {
            // Destroys the GameObject and decrements the Addressables ref
            // count, freeing the underlying asset.
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