using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lives on the confirmation dialog prefab's root, alongside UIPanel.
/// Displays a message and Yes/No buttons, invoking whichever callback
/// corresponds to the button pressed.
///
/// This single instance gets reused for every prompt in the game (quit
/// confirmation, save/cancel settings, etc.) — ConfirmationDialogService
/// calls Configure() with new text/callbacks each time it's shown, rather
/// than each caller needing its own dialog prefab.
/// </summary>
[RequireComponent(typeof(UIPanel))]
public class ConfirmationDialogScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private UIPanel _panel;
    private Action _onYes;
    private Action _onNo;

    private void Awake()
    {
        _panel = GetComponent<UIPanel>();
        _yesButton.onClick.AddListener(HandleYes);
        _noButton.onClick.AddListener(HandleNo);
    }

    /// <summary>
    /// Sets the prompt text and outcome callbacks for this showing. Call
    /// this every time the dialog is shown — the same instance is reused
    /// for different prompts, so nothing here is meant to persist between
    /// showings. Either callback may be null if you don't need to react to
    /// that outcome (e.g. "No" often just needs to close and do nothing).
    /// </summary>
    public void Configure(string message, Action onYes, Action onNo)
    {
        _messageText.text = message;
        _onYes = onYes;
        _onNo = onNo;
    }

    private void HandleYes()
    {
        var callback = _onYes;
        _panel.Hide();
        callback?.Invoke();
    }

    private void HandleNo()
    {
        var callback = _onNo;
        _panel.Hide();
        callback?.Invoke();
    }
}