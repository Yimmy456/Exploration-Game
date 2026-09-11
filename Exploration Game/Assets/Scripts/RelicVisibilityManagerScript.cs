using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralizes the "is the player in sight of this relic" polling into a
/// single Update() loop, instead of every RelicTriggererScript running its
/// own Update() every frame. Cost scales with how many relics are
/// CURRENTLY registered (i.e. the player is in range of), which in
/// practice is almost always 0 or 1 — not with the total number of relics
/// in the level.
///
/// Setup: put this on a persistent GameObject (e.g. alongside
/// InputModeManagerScript). No fields to assign.
/// </summary>
public class RelicVisibilityManagerScript : MonoBehaviour
{
    public static RelicVisibilityManagerScript Instance { get; private set; }

    private readonly List<RelicTriggererScript> _activeTriggerers = new List<RelicTriggererScript>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Called by a RelicTriggererScript when the player enters its trigger.</summary>
    public void Register(RelicTriggererScript triggerer)
    {
        if (!_activeTriggerers.Contains(triggerer))
            _activeTriggerers.Add(triggerer);
    }

    /// <summary>Called by a RelicTriggererScript when the player exits its trigger, or it gets collected.</summary>
    public void Unregister(RelicTriggererScript triggerer)
    {
        _activeTriggerers.Remove(triggerer);
    }

    private void Update()
    {
        // Iterate backwards since a relic can unregister itself mid-loop
        // (e.g. TickVisibility() results in a collect this same frame).
        for (int i = _activeTriggerers.Count - 1; i >= 0; i--)
        {
            _activeTriggerers[i].TickVisibility();
        }
    }
}