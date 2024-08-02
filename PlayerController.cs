using static Models;
using UnityEngine;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    CharacterController _charController;

    Ia_defInput _defInput;

    [SerializeField] Vector2 _inputMove;
    [SerializeField] Vector2 _inputLook;

    [Header("Refs")]
    [SerializeField] Transform _cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel _playerSettings;

    [SerializeField] float _lookClampYMin = -70f;
    [SerializeField] float _lookClampYMax = 80f;

    Vector3 _cameraRotation;
    Vector3 _playerRotation;

    void Awake()
    {
        _defInput = new Ia_defInput();

        _defInput.player.Move.performed += e => _inputMove = e.ReadValue<Vector2>();
        _defInput.player.Look.performed += e => _inputLook = e.ReadValue<Vector2>();
        _defInput.Enable();

        _cameraRotation = _cameraHolder.localRotation.eulerAngles;
        _playerRotation = transform.localRotation.eulerAngles;

        _charController = GetComponent<CharacterController>();
    }

    void Update()
    {
        CalculateLook();
        CalculateMove();
    }

    void CalculateLook()
    {
        _playerRotation.y += _playerSettings.LookXSensitivity * (_playerSettings.LookXInverted ? -_inputLook.x : _inputLook.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_playerRotation);

        _cameraRotation.x += _playerSettings.LookYSensitivity * (_playerSettings.LookYInverted ? _inputLook.y : -_inputLook.y) * Time.deltaTime;
        _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, _lookClampYMin, _lookClampYMax);

        _cameraHolder.localRotation = Quaternion.Euler(_cameraRotation);
    }

    void CalculateMove()
    {
        float vertSpeed = _playerSettings.WalkForwardSpeed * _inputMove.y * Time.deltaTime;
        float horSpeed = _playerSettings.WalkStrafeSpeed * _inputMove.x * Time.deltaTime;

        Vector3 moveSpeed = new Vector3(horSpeed, 0, vertSpeed);

        _charController.Move(transform.TransformDirection(moveSpeed));
    }
}
