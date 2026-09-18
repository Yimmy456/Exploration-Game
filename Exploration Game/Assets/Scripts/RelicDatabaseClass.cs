using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Loads and caches RelicsDB.json once, exposing lookup by RelicID. Single
/// source of truth for relic metadata (name, description, thumbnail/logo
/// addresses) — RelicContainerScript and RecyclableScrollView both read
/// from here instead of each independently parsing the file.
///
/// Note: the JSON's own "IsCollected" field is intentionally NOT treated as
/// authoritative here — actual collected state lives solely in
/// RelicSaveDataScript, written only by real in-game collection. This
/// field is metadata only and has no effect on gameplay.
///
/// Platform note: this reads via System.IO.File, which works in the
/// Editor and on desktop/iOS builds, but NOT on Android — StreamingAssets
/// is inside the compressed APK/AAB there and needs UnityWebRequest
/// instead. Flagging since it applies to all three places that used to
/// read this file; worth addressing together if/when you target Android.
/// </summary>
public static class RelicDatabaseClass
{
    private static Dictionary<string, Relic> _relicsById;

    private static void EnsureLoaded()
    {
        if (_relicsById != null) return;

        _relicsById = new Dictionary<string, Relic>();

        string filePath = Path.Combine(Application.streamingAssetsPath, "RelicsDB.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"RelicDatabaseClass: file not found at {filePath}");
            return;
        }

        string jsonString = File.ReadAllText(filePath);
        RelicArray array = JsonUtility.FromJson<RelicArray>(jsonString);

        foreach (Relic relic in array.data)
        {
            if (_relicsById.ContainsKey(relic.RelicID))
            {
                Debug.LogWarning($"RelicDatabaseClass: duplicate RelicID '{relic.RelicID}' in RelicsDB.json — keeping the first entry.");
                continue;
            }
            _relicsById.Add(relic.RelicID, relic);
        }
    }

    /// <summary>Looks up a relic's static definition by ID. Returns null (and logs a warning) if not found.</summary>
    public static Relic GetById(string relicId)
    {
        EnsureLoaded();
        _relicsById.TryGetValue(relicId, out Relic relic);
        if (relic == null)
            Debug.LogWarning($"RelicDatabaseClass: no relic found with ID '{relicId}'.");
        return relic;
    }

    /// <summary>All relic definitions — e.g. for building the inventory list.</summary>
    public static IEnumerable<Relic> All
    {
        get
        {
            EnsureLoaded();
            return _relicsById.Values;
        }
    }
}