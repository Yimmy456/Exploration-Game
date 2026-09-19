using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecyclableScrollViewScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] RectTransform _content;
    [SerializeField] GameObject _itemPrefab;

    [Header("Settings")]
    [SerializeField] private int _itemsPerRow = 4;
    [SerializeField] private Vector2 _cellSize;
    [SerializeField] private Vector2 _spacing;
    [SerializeField] private RectOffset _padding;
    [SerializeField] private TextMeshProUGUI _itemsCollectedCountText;

    List<ItemData> _allData = new List<ItemData>();
    List<ScrollItemObjectScript> _currentItemObjects = new List<ScrollItemObjectScript>();
    Queue<ScrollItemObjectScript> _inactiveItemObjects = new Queue<ScrollItemObjectScript>();

    private int _visibleRowCount;
    private int _lastStartIndex = -1;

    private int _allDataCount = 0;

    public int AllDataCount
    {
        get { return _allDataCount; }
    }

    public IReadOnlyList<ItemData> AllData
    {
        get { return _allData; }
    }

    private bool _hasStarted;

    void Start()
    {
        _scrollRect.onValueChanged.AddListener(OnScroll);
        Refresh();
    }

    void OnEnable()
    {
        // Re-pull relic data every time this panel is shown (e.g. via
        // UIPanel.OnEnable), so items collected since the last time it was
        // open actually show up. Skip the very first OnEnable since Start()
        // (which registers the scroll listener) hasn't run yet at that point
        // and will call Refresh() itself.
        if (_hasStarted)
        {
            Refresh();
        }
    }

    /// <summary>
    /// Reloads relic data from disk and re-lays-out the visible items.
    /// Safe to call repeatedly (e.g. every time the inventory canvas is
    /// shown) — clears previously loaded data first so entries don't
    /// duplicate on repeated opens.
    /// </summary>
    public void Refresh()
    {
        _hasStarted = true;

        _allData.Clear();
        _lastStartIndex = -1; // force LayoutVisibleItems to redraw even if scroll position is unchanged

        GetRelicsList();
        UpdateContentHeight();
        LayoutVisibleItems(0);

        if (_scrollRect != null)
        {
            _scrollRect.verticalNormalizedPosition = 1f; // reset scroll to top on refresh
        }
    }

    void UpdateContentHeight()
    {
        int _rowCount = Mathf.CeilToInt((float)_allData.Count / _itemsPerRow);
        float totalHeight = _padding.top + _padding.bottom + (_rowCount * _cellSize.y) + ((_rowCount - 1) * _spacing.y);
        _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalHeight);
    }

    private void OnScroll(Vector2 _pos)
    {
        LayoutVisibleItems(GetFirstVisibleRowIndex());
    }

    private int GetFirstVisibleRowIndex()
    {
        float _contentY = _content.anchoredPosition.y - _padding.top;
        if (_contentY < 0) { _contentY = 0; }
        int _row = Mathf.FloorToInt(_contentY / (_cellSize.y + _spacing.y));
        return Mathf.Clamp(_row, 0, Mathf.CeilToInt((float)_allDataCount / _itemsPerRow) - 1);
    }

    void LayoutVisibleItems(int startRow)
    {
        float viewPortHeight = ((RectTransform)_scrollRect.transform).rect.height;
        _visibleRowCount = Mathf.CeilToInt(viewPortHeight / (_cellSize.y + _spacing.y)) / 2;

        int startIndex = startRow * _itemsPerRow;
        if (startIndex == _lastStartIndex)
        {
            return;
        }

        foreach (var _obj in _currentItemObjects)
        {
            _obj.gameObject.SetActive(false);
            _inactiveItemObjects.Enqueue(_obj);
        }

        _currentItemObjects.Clear();

        if (_allDataCount == 0)
        {
            return;
        }

        int endIndex = Mathf.Min(startIndex + (_visibleRowCount * _itemsPerRow), _allDataCount);

        for (int _i = startIndex; _i < endIndex; _i++)
        {
            ScrollItemObjectScript _itemObject = GetPooledItem();

            if (_itemObject == null)
            {
                continue;
            }

            _itemObject.gameObject.SetActive(true);

            int _row = _i / _itemsPerRow;
            int _col = _i % _itemsPerRow;

            Vector2 _pos = new Vector2((_padding.left + _col * (_cellSize.x + _spacing.x)), (-_padding.top - _row * (_cellSize.y + _spacing.y)));

            RectTransform _rt = _itemObject.GetComponent<RectTransform>();

            _rt.anchorMin = new Vector2(0f, 1f);
            _rt.anchorMax = new Vector2(0f, 1f);

            _rt.anchoredPosition = new Vector2(_pos.x, _pos.y);

            _itemObject.SetData(_allData[_i]);
            _currentItemObjects.Add(_itemObject);
        }
    }

    ScrollItemObjectScript GetPooledItem()
    {
        if (_inactiveItemObjects.Count > 0)
        {
            return _inactiveItemObjects.Dequeue();
        }

        GameObject newObj = Instantiate(_itemPrefab, _content);
        newObj.GetComponent<RectTransform>().sizeDelta = _cellSize;
        newObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        return newObj.GetComponent<ScrollItemObjectScript>();
    }

    /// <summary>
    /// Builds the list of collected relics to display. Reads relic metadata
    /// from RelicDatabaseClass (the cached, authoritative lookup over
    /// RelicsDB.json) and filters by RelicSaveDataScript.IsCollected(id) —
    /// the actual save-data authority for "is this collected" — instead of
    /// trusting RelicsDB.json's own IsCollected field directly, which isn't
    /// kept up to date.
    /// </summary>

    void GetRelicsList()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "RelicsDB.json");

        _allDataCount = 0;

        int _totalCount = 0;
        int _collectedIndex = 0;

        foreach(Relic _r in RelicDatabaseClass.All)
        {
            _totalCount++;

            if(RelicSaveDataClass.IsCollected(_r.RelicID) == false)
            {
                continue;
            }

            _allData.Add(new ItemData {
                relicId = _r.RelicID,
                title = _r.RelicName,
                description = _r.RelicDescription,
                index = _collectedIndex
            });

            _collectedIndex++;
        }

        _allDataCount = _allData.Count;

        if(_itemsCollectedCountText != null)
        {
            _itemsCollectedCountText.text = _allDataCount + " / " + _totalCount;
        }
    }
}