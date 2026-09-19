using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Tracks which relics have been collected, persisted to
/// Application.persistentDataPath — writable on every platform — separate
/// from RelicsDB.json in StreamingAssets, which is read-only at runtime on
/// most platforms (mobile, WebGL, many console builds). RelicsDB.json
/// should only hold static relic definitions (ID, name) going forward;
/// this is the actual source of truth for what the player has collected.
/// </summary>
public static class RelicSaveDataClass
{
    private const string SaveFileName = "RelicSaveData.json";

    private static HashSet<string> _collectedIds;

    // Guarantees a fresh read from disk at the start of every session,
    // regardless of the Editor's "Reload Domain" setting (which, if
    // disabled, would otherwise leave the static cache above populated
    // with stale data between separate Play sessions).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetCacheOnLaunch()
    {
        _collectedIds = null;
    }

    [System.Serializable]
    private class SaveData
    {
        public List<string> CollectedIds = new List<string>();
    }

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private static void EnsureLoaded()
    {
        if (_collectedIds != null) return;

        _collectedIds = new HashSet<string>();

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data?.CollectedIds != null)
            {
                foreach (string id in data.CollectedIds)
                    _collectedIds.Add(id);
            }
        }
    }

    /// <summary>Whether a relic with this ID has been collected (persists across sessions).</summary>
    public static bool IsCollected(string relicId)
    {
        EnsureLoaded();
        return _collectedIds.Contains(relicId);
    }

    /// <summary>Marks a relic collected and writes it to disk immediately.</summary>
    public static void MarkCollected(string relicId)
    {
        EnsureLoaded();
        if (_collectedIds.Add(relicId))
        {
            var data = new SaveData { CollectedIds = new List<string>(_collectedIds) };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
    }
}