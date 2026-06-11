using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    [SerializeField]
    float _speed = 5.0f;

    [SerializeField]
    Rigidbody _rb;

    [SerializeField]
    Camera _camera;

    InputSystem_Actions _inputSystem;

    Vector3 _direction = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents(true);
    }

    void Initialize()
    {
        SubscribeEvents();
    }

    void SubscribeEvents()
    {
        if (_inputSystem == null)
        {
            _inputSystem = new InputSystem_Actions();
        }

        _inputSystem.Enable();

        _inputSystem.Player.Move.performed += ctx => MoveFunction();
    }

    void UnsubscribeEvents(bool _dispose = false)
    {
        _inputSystem.Player.Move.performed -= ctx => MoveFunction();

        if (_dispose)
        {
            _inputSystem.Dispose();
        }
        else
        {
            _inputSystem.Disable();
        }
    }

    void MoveFunction()
    {

    }
}
