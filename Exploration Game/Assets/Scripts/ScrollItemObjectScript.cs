using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollItemObjectScript : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _titleText;

    [SerializeField]
    Image _itemImage;

    [SerializeField]
    TextMeshProUGUI _indexText;

    [SerializeField]
    Button _selectButton;

    /// <summary>
    /// The relic this cell currently represents. Set in SetData() so a
    /// future click handler (selecting this cell to open the relic detail
    /// popup) has something to look up RelicDatabaseClass.GetById() with.
    /// </summary>
    public string RelicId { get; private set; }

    private void Awake()
    {
        if(_selectButton != null)
        {
            _selectButton.onClick.AddListener(OnSelectClick);
        }
    }

    public void SetData(ItemData _data)
    {
        RelicId = _data.relicId;

        _titleText.text = _data.title;

        _indexText.text = (_data.index + 1).ToString();

        // _itemImage is intentionally left untouched here — loading the
        // relic's thumbnail (RelicThumbnailAddress) is an Addressable async
        // load, which needs care in a pooled/recycled list (a cell can be
        // reused for a different relic before a prior load finishes). Not
        // wired up yet.
    }

    void OnSelectClick()
    {
        if(ItemDescriptionCanvasScript.Instance != null)
        {
            ItemDescriptionCanvasScript.Instance.Show(RelicId);
        }
    }
}
