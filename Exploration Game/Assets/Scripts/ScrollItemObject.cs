using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollItemObject : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _titleText;

    [SerializeField]
    Image _itemImage;

    [SerializeField]
    TextMeshProUGUI _indexText;

    public void SetData(ItemData _data)
    {
        _titleText.text = _data.title;

        _indexText.text = (_data.index + 1).ToString();
    }
}
