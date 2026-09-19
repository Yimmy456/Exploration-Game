using UnityEngine;

public class PlayerInputScript : MonoBehaviour
{
    [SerializeField] float _movementSpeed = 5.0f;
    [SerializeField] float _rotationSpeed = 0.1f;
    [SerializeField] float _lookUp;
    [SerializeField] float _lookDown;
    [SerializeField] Camera _camera;

    [SerializeField] PlayerInputHandlerScript _handler;
    [SerializeField] CharacterController _controller;

    private Vector3 CurrentMovement = Vector3.zero;
    public float VerticalRotation { get; private set; }
    public float MovementSpeed { get { return _movementSpeed; } }
    public float RotationSpeed { get { return _rotationSpeed; } }
    public Camera Camera { get { return _camera; } }

    // NEW: exposes the already Inspector-wired Handler reference, so other
    // scripts (RelicTriggererScript) can reuse it instead of doing their
    // own hierarchy-dependent GetComponent/GetComponentInParent search.
    public PlayerInputHandlerScript Handler { get { return _handler; } }

    float _verticalRot;

    private void Update()
    {
        if (_handler == null || !_handler.enabled || _controller == null) { return; }

        HandleMovement();
        ApplyRotation();
    }

    void HandleMovement()
    {
        Vector3 _inputDirection = new Vector3(_handler.MovementInput.x, 0.0f, _handler.MovementInput.y);
        Vector3 _worldDirection = transform.TransformDirection(_inputDirection);
        CurrentMovement.x = _worldDirection.x * _movementSpeed;
        CurrentMovement.z = _worldDirection.z * _movementSpeed;
        _controller.Move(CurrentMovement * Time.deltaTime);
    }

    void ApplyRotation()
    {
        Vector3 _inputV3 = new Vector3(_handler.RotationInput.x, _handler.RotationInput.y, 0f);
        transform.Rotate(0f, _inputV3.x, 0f);
        _verticalRot = Mathf.Clamp(_verticalRot - _inputV3.y, _lookUp, _lookDown);
        _camera.transform.localRotation = Quaternion.Euler(_verticalRot, 0f, 0f);
    }
}