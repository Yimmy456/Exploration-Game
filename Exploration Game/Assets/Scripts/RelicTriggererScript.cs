using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RelicTriggererScript : MonoBehaviour
{
    [SerializeField] RelicContainerScript _relic;

    [SerializeField] Camera _camera;

    [SerializeField] PlayerInputHandlerScript _handler;

    const string _uiAddress = "Assets/Prefabs/UIs/Press To Take Canvas";

    GameObject _uiGameObject;

    public string PlayerName { get { return "Playable Character"; } }

    public bool TriggerOn { get; private set; }

    public static RelicTriggererScript _currentTriggerer { get; private set; }

    private void Awake()
    {
        TriggerOn = false;

        _currentTriggerer = null;        
    }

    private void Update()
    {
        if(_handler.CollectInput)
        {
            Debug.Log("Hi!");
        }

        if(TriggerOn && _handler.CollectInput)
        {
            TriggerOn = false;

            if(_currentTriggerer == this)
            {
                _currentTriggerer = null;
            }

            _relic.SetIsCollected();           
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == PlayerName && _currentTriggerer == null)
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

        _currentTriggerer = this;
    }

    void OnExit()
    {
        if(_uiGameObject != null)
        {
            Addressables.ReleaseInstance(_uiGameObject);
        }

        if(_currentTriggerer == this)
        {
            _currentTriggerer = null;
        }
    }

    private void OnDestroy()
    {
        if (_uiGameObject != null)
        {
            Addressables.ReleaseInstance(_uiGameObject);
        }

        if (_currentTriggerer == this)
        {
            _currentTriggerer = null;
        }
    }
}
