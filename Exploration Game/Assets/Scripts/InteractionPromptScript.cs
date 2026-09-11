using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Lightweight, always-in-scene HUD prompt (e.g. "Press 'X' to collect
/// it."). Unlike UIPanel/InputModeManagerScript, showing this does NOT
/// pause the game or unlock the cursor — it's a plain gameplay HUD element
/// shown while the player still has full control.
///
/// Reference-counted the same way UIPanel is, since more than one
/// interactable (e.g. two nearby relics) could ask for the prompt at the
/// same time — it only fully hides once every requester has released it.
///
/// Setup: put this on a persistent HUD Canvas already in your scene (not
/// Addressable — it's cheap and always needed), with a child prompt
/// panel/text assigned below.
/// </summary>
public class InteractionPromptScript : MonoBehaviour
{
    public static InteractionPromptScript Instance { get; private set; }

    [SerializeField] private GameObject _promptRoot; // e.g. a small background + text panel
    [SerializeField] private TextMeshProUGUI _messageText;

    private readonly List<(object requester, string message)> _requests = new List<(object, string)>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _promptRoot.SetActive(false);
    }

    /// <summary>Show (or update) the prompt on behalf of `requester`.</summary>
    public void Show(object requester, string message)
    {
        int index = _requests.FindIndex(r => r.requester == requester);
        if (index >= 0)
            _requests[index] = (requester, message);
        else
            _requests.Add((requester, message));

        Refresh();
    }

    /// <summary>Release this requester's claim on the prompt.</summary>
    public void Hide(object requester)
    {
        _requests.RemoveAll(r => r.requester == requester);
        Refresh();
    }

    private void Refresh()
    {
        if (_requests.Count == 0)
        {
            _promptRoot.SetActive(false);
            return;
        }

        // Show whichever requester's prompt is most recent.
        var latest = _requests[_requests.Count - 1];
        _messageText.text = latest.message;
        _promptRoot.SetActive(true);
    }
}