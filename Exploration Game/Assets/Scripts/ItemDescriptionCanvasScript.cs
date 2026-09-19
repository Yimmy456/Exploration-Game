using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionCanvasScript : MonoBehaviour
{
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
    Image _itemImage;

    [SerializeField]
    Image _franchiseLogoImage;

    [SerializeField]
    float _franchiseLogoSize = 155f;

}
