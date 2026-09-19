using System.IO;
using UnityEngine;

public class RelicContainerScript : MonoBehaviour
{
    [SerializeField] RelicTriggererScript _triggerer;
    [SerializeField] GameObject _relic;

    [SerializeField] string _id;

    private string filePath;

    public GameObject Relic {  get { return _relic; } }

    public string RelicID {  get { return _id; } }
    void Start()
    {
        // Already collected in a previous session — don't let this relic
        // show up in the world again. Checked in Awake() (before any
        // Start(), including RelicTriggererScript's / the visibility
        // manager's) so it's removed before anything else on it gets a
        // chance to run.
        if (RelicSaveDataClass.IsCollected(_id))
        {
            Destroy(gameObject);
        }
    }

    // Called by RelicTriggererScript.Collect() when the player picks this
    // relic up. Marks it collected in the actual save-data authority
    // (persistentDataPath/RelicSaveData.json via RelicSaveDataScript)
    // instead of rewriting RelicsDB.json — RelicsDB.json stays static,
    // read-only relic metadata; RelicSaveDataScript is the only thing that
    // ever records player progress.
    public void SetIsCollected()
    {
        RelicSaveDataClass.MarkCollected(_id);

        Destroy(gameObject);
    }
}
