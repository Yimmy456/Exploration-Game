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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "RelicsDB.json");
    }

    void SetIsCollectedJSON()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found!");

            return;
        }

        // 1. Read local JSON file
        string jsonText = File.ReadAllText(filePath);

        // 2. Parse JSON to C# object
        RelicArray array = JsonUtility.FromJson<RelicArray>(jsonText);

        foreach (var relic in array.data)
        {
            if (relic.RelicID == _id)
            {
                relic.IsCollected = true;

                Debug.Log("Relic found!");

                break;
            }
        }

        string updatedJson = JsonUtility.ToJson(array, true);

        File.WriteAllText(filePath, updatedJson);
    }

    public void SetIsCollected()
    {
        SetIsCollectedJSON();

        Destroy(gameObject);
    }
}
