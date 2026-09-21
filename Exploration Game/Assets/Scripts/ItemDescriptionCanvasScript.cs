using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;


public class ItemDescriptionCanvasScript : MonoBehaviour
{
    public static ItemDescriptionCanvasScript Instance { get; private set; }

    public const string ItemDetailsActionMapName = "Item Details";
    public const string PreviousActionName = "Previous";
    public const string NextActionName = "Next";
    public const string BackActionName = "Back";

    [SerializeField]
    TextMeshProUGUI _itemNameText;

    [SerializeField]
    TextMeshProUGUI _itemDescriptionText;

    [SerializeField]
    TextMeshProUGUI _pageNumberText;

    [SerializeField]
    Button _previousButton;

    [SerializeField]
    Button _nextButton;

    [SerializeField]
    Button _backButton;

    [SerializeField]
    Image _itemImage;

    [SerializeField]
    Image _franchiseLogoImage;

    [SerializeField]
    float _franchiseLogoSize = 155f;

    [SerializeField]
    InputActionAsset _inputAssets;

    List<Relic> _relics = new List<Relic>();
    private int _currentIndex;

    private AsyncOperationHandle<Sprite> _imageHandle;
    private bool _hasImageHandle;

    public bool IsOpen => gameObject.activeSelf;

    private InputActionMap _itemDetailsMap;
    private InputAction _previousAction;
    private InputAction _nextAction;
    private InputAction _backAction;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _previousButton.onClick.AddListener(ShowPrevious);
        _nextButton.onClick.AddListener(ShowNext);
        _backButton.onClick.AddListener(OnBackClicked);

        _itemDetailsMap = _inputAssets.FindActionMap(ItemDetailsActionMapName);

        _previousAction = _itemDetailsMap.FindAction(PreviousActionName);
        _nextAction = _itemDetailsMap.FindAction(NextActionName);
        _backAction = _itemDetailsMap.FindAction(BackActionName);

        // Franchise logos aren't implemented yet — there's no
        // FranchiseLogoAddress data to load (a separate franchises
        // database is planned for later). Keep it hidden until then.
        _franchiseLogoImage.gameObject.SetActive(false);


        // Pre-loaded once (via AddressableUIPanelLoaderScript's
        // loadOnStart, same setup as InteractionPromptScript) and kept
        // alive from here on — hide immediately after registering
        // Instance so nothing else has to wait for an async load to call
        // Show() on it later.
        gameObject.SetActive(false);
    }

    void OnPreviousPerformed(InputAction.CallbackContext ctx) => ShowPrevious();
    void OnNextPerformed(InputAction.CallbackContext ctx) => ShowNext();
    void OnBackPerformed(InputAction.CallbackContext ctx) => OnBackClicked();

    private void OnEnable()
    {
        // Own dedicated map rather than the shared "Global" map — Pause
        // and InventoryMenuControllerScript enable/disable Global from an
        // always-active persistent object, so this panel toggling that
        // same map on every Show()/Hide() would fight with their state.
        // This map only ever needs to be on while this panel is visible,
        // which lines up exactly with this GameObject's own active state.
        _itemDetailsMap.Enable();
        _previousAction.performed += OnPreviousPerformed;
        _nextAction.performed += OnNextPerformed;
        _backAction.performed += OnBackPerformed;
    }


    /// <summary>
    /// Shows the gallery positioned at the given relic. Called directly on
    /// the persistent instance — e.g. from a clicked inventory grid cell —
    /// rather than through the Addressable loader, since this panel is
    /// loaded once and reused rather than instantiated per-show.
    /// </summary>
    public void Show(string _relicId)
    {
        gameObject.SetActive(true);
        Open(_relicId);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Back button handler. The inventory canvas is never hidden while this
    /// panel is shown on top of it (ScrollItemObjectScript.OnSelectClicked
    /// only ever calls Show() here, it doesn't touch the inventory panel),
    /// so simply hiding this canvas is enough to land the player back on
    /// the inventory UI underneath.
    /// </summary>
    void OnBackClicked()
    {
        Debug.Log("Going back...");

        Hide();
    }


    /// <summary>
    /// Opens the gallery positioned at the given relic. Rebuilds the
    /// collected-relics list fresh each time (same RelicDatabaseClass.All
    /// RelicSaveDataScript.IsCollected filter RecyclableScrollViewScript
    /// uses) so it reflects whatever's been collected since this was last
    /// opened, then jumps straight to relicId.
    /// </summary>
    public void Open(string relicId)
    {
        BuildRelicsList();

        _currentIndex = _relics.FindIndex(_r => _r.RelicID == relicId);

        if(_currentIndex < 0)
        {
            _currentIndex = 0;
        }

        DisplayCurrent();
    }

    private void BuildRelicsList()
    {
        _relics.Clear();

        foreach(Relic _r in RelicDatabaseClass.All)
        {
            if(RelicSaveDataClass.IsCollected(_r.RelicID))
            {
                _relics.Add(_r);
            }
        }
    }

    void ShowPrevious()
    {
        Debug.Log("Going to previous...");

        _currentIndex--;

        if(_currentIndex < 0)
        {
            _currentIndex = _relics.Count - 1;
        }

        DisplayCurrent();
    }

    void ShowNext()
    {
        Debug.Log("Going to next...");

        _currentIndex++;

        if (_currentIndex >= _relics.Count)
        {
            _currentIndex = 0;
        }

        DisplayCurrent();
    }

    void DisplayCurrent()
    {
        if (_relics.Count == 0)
        {
            return;
        }

        Relic _currentR = _relics[_currentIndex];

        _itemNameText.text = _currentR.RelicName;
        _itemDescriptionText.text = _currentR.RelicDescription;
        _pageNumberText.text = $"{_currentIndex + 1} / {_relics.Count}";

        LoadThumbnail(_currentR.RelicThumbnailAddress);
    }

    void LoadThumbnail(string _thumbnailAddress)
    {
        ReleaseImageHandle();

        _itemImage.sprite = null;

        if(string.IsNullOrEmpty(_thumbnailAddress))
        {
            return;
        }

        _imageHandle = Addressables.LoadAssetAsync<Sprite>(_thumbnailAddress);
        _hasImageHandle = true;
        _imageHandle.Completed += OnThumbnailLoaded;
    }

    void OnThumbnailLoaded(AsyncOperationHandle<Sprite> handle)
    {
        // The player may have paged to a different relic (or closed the
        // popup) before this load finished — only apply it if it's still
        // the load we're currently waiting on.
        if (!_hasImageHandle || !handle.Equals(_imageHandle))
        {
            return;
        }

        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            _itemImage.sprite = handle.Result;
        }
    }

    void ReleaseImageHandle()
    {
        if(_hasImageHandle)
        {
            Addressables.Release(_imageHandle);
            _hasImageHandle = false;
        }
    }

    private void OnDisable()
    {
        
        _previousAction.performed -= OnPreviousPerformed;
        _nextAction.performed -= OnNextPerformed;
        _backAction.performed -= OnBackPerformed;
        _itemDetailsMap.Disable();

        ReleaseImageHandle();
        _itemImage.sprite = null;
    }
}
