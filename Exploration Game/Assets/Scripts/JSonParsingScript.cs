using System.IO;
using UnityEngine;

public class JSonParsingScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "RelicsDB.json");

        if(File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);

            RelicArray array = JsonUtility.FromJson<RelicArray>(jsonString);

            int _i = 0;

            foreach(Relic _r in array.data)
            {
                Debug.Log((_i + 1).ToString() + $". Relic: '{_r.RelicName}'");

                //Debug.Log((_i + 1).ToString() + $". Relic Position: {_r.Position}");

                _i++;
            }
        }
        else
        {
            Debug.LogError("No file found!");
        }
    }
}
