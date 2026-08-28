using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem.iOS;
using UnityEngine.UI;

public class RecyclableScrollView : MonoBehaviour
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

    List<ItemData> _allData = new List<ItemData>();
    List<ScrollItemObject> _currentItemObjects = new List<ScrollItemObject>();
    Queue<ScrollItemObject> _inactiveItemObjects = new Queue<ScrollItemObject>();

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

    void Start()
    {
        // Fake data initialization for testing

        GetRelicsList();

        UpdateContentHeight();

        LayoutVisibleItems(0);

        _scrollRect.onValueChanged.AddListener(OnScroll);
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
        if(_contentY < 0) { _contentY = 0; }
        int _row = Mathf.FloorToInt(_contentY / (_cellSize.y + _spacing.y));
        return Mathf.Clamp(_row, 0, Mathf.CeilToInt((float)_allDataCount / _itemsPerRow) - 1);
    }

    void LayoutVisibleItems(int startRow)
    {
        float viewPortHeight = ((RectTransform)_scrollRect.transform).rect.height;
        _visibleRowCount = Mathf.CeilToInt(viewPortHeight / (_cellSize.y + _spacing.y)) / 2;

        int startIndex = startRow * _itemsPerRow;
        if(startIndex == _lastStartIndex)
        {
            return;
        }

        foreach(var _obj in _currentItemObjects)
        {
            _obj.gameObject.SetActive(false);
            _inactiveItemObjects.Enqueue(_obj);
        }

        _currentItemObjects.Clear();

        int endIndex = Mathf.Min(startIndex + (_visibleRowCount * _itemsPerRow), _allDataCount);

        for(int _i = startIndex; _i < endIndex; _i++)
        {
            ScrollItemObject _itemObject = GetPooledItem();

            if(_itemObject == null)
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

    ScrollItemObject GetPooledItem()
    {
        if(_inactiveItemObjects.Count > 0)
        {
            return _inactiveItemObjects.Dequeue();
        }

        GameObject newObj = Instantiate(_itemPrefab, _content);
        newObj.GetComponent<RectTransform>().sizeDelta = _cellSize;
        newObj.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        return newObj.GetComponent<ScrollItemObject>();
    }

    /*
    void SetupContainer()
    {
        // Adjust the Content height to fit the full structural length of the dataset
        //float totalHeight = (_allData.Count * _itemHeight) + ((_allData.Count - 1) * _spacing.y);
        int _totalRows = Mathf.CeilToInt((float)_totalItemCount / _itemsPerRow);
        float totalHeight = _totalRows * (_itemHeight + _spacing.y) - _spacing.y;
        _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalHeight);
    }

    void SpawnPool()
    {
        // Calculate exactly how many items fit into the viewport
        float viewPortHeight = _scrollRect.viewport.rect.height;
        _totalVisibleItems = Mathf.CeilToInt(viewPortHeight / (_itemHeight + _spacing.y)) + _bufferCount;

        for (int i = 0; i < _totalVisibleItems; i++)
        {
            GameObject obj = Instantiate(_itemPrefab, _content);
            RectTransform rect = obj.GetComponent<RectTransform>();

            // Fix anchors to Top-Left for precise manual positioning
            //rect.anchorMin = new Vector2(0, 1);
            //rect.anchorMax = new Vector2(1, 1);
            //rect.pivot = new Vector2(0.5f, 1);

            rect.anchorMin = new Vector2(0f, 1);
            rect.anchorMax = new Vector2(0f, 1);
            rect.pivot = new Vector2(0.5f, 1);

            _pooledItems.Add(rect);
        }
    }

    void OnScroll(Vector2 normalizedPos)
    {
        // Determine which data index should currently be at the top of the viewport
        float contentY = _content.anchoredPosition.y;
        int topIndex = Mathf.FloorToInt(contentY / (_itemHeight + _spacing.y));
        topIndex = Mathf.Clamp(topIndex, 0, Mathf.Max(0, _allData.Count - _totalVisibleItems));

        if (topIndex != _previousTopIndex)
        {
            _previousTopIndex = topIndex;
            UpdateItems(topIndex);
        }
    }

    void UpdateItems(int topIndex)
    {
        float ySpacing = -(_itemHeight + _spacing.y);
        float xSpacing = _itemWidth + _spacing.x;

        float yPos = -_padding.y;
        float xPos = _padding.x;

        for (int i = 0; i < _totalVisibleItems; i++)
        {
            int dataIndex = topIndex + i;
            int poolIndex = dataIndex % _totalVisibleItems; // Cycle through pool list smoothly

            RectTransform itemRect = _pooledItems[poolIndex];

            if (dataIndex < _allData.Count)
            {
                itemRect.gameObject.SetActive(true);

                // Calculate exact local Y coordinate position for this row
                //float yPos = -((dataIndex * _itemHeight) + (dataIndex * _spacing));
                //float yPos = -((dataIndex * _itemHeight) + (dataIndex * _spacing));

                //itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, yPos);
                itemRect.anchoredPosition = new Vector2(xPos, yPos);

                if ((i % _itemsPerRow) == (_itemsPerRow - 1))
                {
                    yPos = yPos + ySpacing;
                    xPos = _padding.x;
                }
                else
                {
                    xPos = xPos + xSpacing;
                }

                // Update UI display contents
                ScrollItemObject itemScript = itemRect.GetComponent<ScrollItemObject>();
                itemScript.SetData(_allData[dataIndex]);
            }
            else
            {
                itemRect.gameObject.SetActive(false);
            }
        }
    }
    */
    void GetRelicsList()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "RelicsDB.json");

        _allDataCount = 0;

        //_totalItemCount = 0;

        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);

            RelicArray array = JsonUtility.FromJson<RelicArray>(jsonString);

            int _i = 0;

            foreach (Relic _r in array.data)
            {
                _allData.Add(new ItemData { title = $"{_r.RelicName}", index = _i });

                _i++;
            }

            _allDataCount = _allData.Count;
        }
    }
}
