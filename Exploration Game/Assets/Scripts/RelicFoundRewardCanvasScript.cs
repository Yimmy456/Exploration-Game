using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Lives on the "Item Found Reward Canvas" prefab's root, alongside
/// UIPanel. The title ("Nice! You have found a new relic!") is static text
/// baked into the prefab — only the item picture, name, franchise logo,
/// and the collected/total count change per relic.
/// </summary>
[RequireComponent(typeof(UIPanel))]
public class RelicFoundRewardCanvasScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _itemNameText;
    [SerializeField] Image _itemImage;
    [SerializeField] Image _franchiseLogoImage;
    [SerializeField] TextMeshProUGUI _relicCountText;
    [SerializeField] float _franchiseLogoSize = 1000f;
    [SerializeField] Button _continueButton;

    /// <summary>
    /// Sets the relic-specific content for this showing. Call every time
    /// the popup is shown, since the same instance is reused for every
    /// relic collected during the session. The collected/total count is
    /// computed here (from RelicDatabaseClass + RelicSaveDataScript)
    /// rather than passed in, so callers don't have to duplicate that
    /// bookkeeping themselves. itemSprite/logoSprite can be null — both
    /// images just disable themselves rather than show a blank white box.
    /// </summary>
    public void Configure(string itemName, Sprite itemSprite, Sprite logoSprite)
    {
        _itemNameText.text = itemName;

        _itemImage.sprite = itemSprite;
        _itemImage.enabled = itemSprite != null;

        // Not every relic necessarily has an associated franchise logo.
        // Alpha is set explicitly here (not just left to the prefab's
        // baked-in value) so this can't silently regress the way the
        // prefab's own alpha did — visibility is driven entirely by
        // whether logoSprite is non-null, every time this runs.
        bool hasFranchiseLogo = logoSprite != null;
        _franchiseLogoImage.sprite = logoSprite;
        _franchiseLogoImage.enabled = hasFranchiseLogo;        

        if(hasFranchiseLogo)
        {
            Vector2 _spriteSize = new Vector2();

            _spriteSize.x = logoSprite.rect.width;
            _spriteSize.y = logoSprite.rect.height;

            if (_spriteSize.x == _spriteSize.y)
            {
                _franchiseLogoImage.rectTransform.sizeDelta = new Vector2(_franchiseLogoSize, _franchiseLogoSize);
            }
            else if(_spriteSize.x > _spriteSize.y)
            {
                float _newSizeY = (_franchiseLogoSize * _spriteSize.y) / _spriteSize.x;

                _franchiseLogoImage.rectTransform.sizeDelta = new Vector2(_franchiseLogoSize, _newSizeY);
            }
            else if (_spriteSize.y > _spriteSize.x)
            {
                float _newSizeX = (_franchiseLogoSize * _spriteSize.x) / _spriteSize.y;

                _franchiseLogoImage.rectTransform.sizeDelta = new Vector2(_newSizeX, _franchiseLogoSize);
            }
        }

        Color logoColor = _franchiseLogoImage.color;
        logoColor.a = hasFranchiseLogo ? 1f : 0f;
        _franchiseLogoImage.color = logoColor;

        int total = RelicDatabaseClass.All.Count();
        int collected = RelicDatabaseClass.All.Count(r => RelicSaveDataScript.IsCollected(r.RelicID));
        _relicCountText.text = collected + " / " + total;

        // Makes the Continue button respond to the UI "Submit" action
        // (Cross/A/Enter/Space by default) as well as a direct click —
        // Unity's Button already listens for Submit, but only while it's
        // the EventSystem's selected object.
        if (EventSystem.current != null && _continueButton != null)
        {
            EventSystem.current.SetSelectedGameObject(_continueButton.gameObject);
        }
    }
}