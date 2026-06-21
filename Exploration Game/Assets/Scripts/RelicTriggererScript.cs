using NUnit.Framework;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RelicTriggererScript : MonoBehaviour
{
    [SerializeField] RelicContainerScript _relic;

    const string _uiAddress = "Assets/Prefabs/UIs/Press To Take Canvas";

    GameObject _uiGameObject;

    const string PlayerName = "Playable Character";

    public bool TriggerOn { get; private set; }

    private void Awake()
    {
        TriggerOn = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == PlayerName)
        {
            OnEnter();

            TriggerOn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == PlayerName)
        {
            OnExit();

            TriggerOn = false;
        }
    }

    void OnEnter()
    {
        Addressables.InstantiateAsync(_uiAddress).Completed += handle =>
        {
            _uiGameObject = handle.Result;
        };
    }

    void OnExit()
    {
        if(_uiGameObject != null)
        {
            Addressables.ReleaseInstance(_uiGameObject);
        }
    }
}
