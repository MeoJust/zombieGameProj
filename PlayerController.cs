using static Models;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController _charController;

    Ia_defInput _defInput;



    [Header("Refs")]
    [SerializeField] Transform _cameraHolder;
    [SerializeField] Transform _feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel _playerSettings;

    [SerializeField] float _lookClampYMin = -70f;
    [SerializeField] float _lookClampYMax = 80f;
    [SerializeField] LayerMask _playerMask;


    [Header("Gravity")]
    [SerializeField] float _gravity = -9.81f;
    [SerializeField] float _minGravity = -9.81f;
    float _playerGravity = -9.81f;

    [SerializeField] Vector3 _jumpForce;
    [SerializeField] Vector3 _jumpForceVelocity;

    [Header("Stance")]
    [SerializeField] PlayerStance _stance;
    [SerializeField] float _stanceSmooth = 5f;
    [SerializeField] float _satnceCheckModifier = .5f;
    [SerializeField] CharStance _standStance;
    [SerializeField] CharStance _crouchStance;
    [SerializeField] CharStance _crawlStance;

    Vector2 _inputMove;
    Vector2 _inputLook;

    float _camHeight;
    float _camHeightVelocity;

    Vector3 _cameraRotation;
    Vector3 _playerRotation;

    Vector3 _stanceCapsuleCenterVelocity;

    float _stanceCapsuleHeightVelocity;

    float _stanceMarginCheck = .05f;

    bool _isSprinting;

    Vector3 _newMoveSpeed;
    Vector3 _newMoveSpeedVelocity;

    void Awake()
    {
        _defInput = new Ia_defInput();

        _defInput.player.Move.performed += e => _inputMove = e.ReadValue<Vector2>();
        _defInput.player.Look.performed += e => _inputLook = e.ReadValue<Vector2>();
        _defInput.player.Jump.performed += e => Jump();
        _defInput.player.Crouch.performed += e => Crouch();
        _defInput.player.Crawl.performed += e => Crawl();
        _defInput.player.Sprint.performed += e => ToggleSprint();
        _defInput.player.SprintRelese.performed += e => StopSprint();
        _defInput.Enable();

        _cameraRotation = _cameraHolder.localRotation.eulerAngles;
        _playerRotation = transform.localRotation.eulerAngles;

        _charController = GetComponent<CharacterController>();

        _camHeight = _cameraHolder.localPosition.y;
    }

    void Update()
    {
        SetLook();
        SetMove();
        SetJump();
        SetStance();
    }

    void SetLook()
    {
        _playerRotation.y += _playerSettings.LookXSensitivity * (_playerSettings.LookXInverted ? -_inputLook.x : _inputLook.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_playerRotation);

        _cameraRotation.x += _playerSettings.LookYSensitivity * (_playerSettings.LookYInverted ? _inputLook.y : -_inputLook.y) * Time.deltaTime;
        _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, _lookClampYMin, _lookClampYMax);

        _cameraHolder.localRotation = Quaternion.Euler(_cameraRotation);
    }

    void SetMove()
    {
        if (_inputMove.y <= .2f)
        {
            _isSprinting = false;
        }

        float vertSpeed = _playerSettings.WalkForwardSpeed;
        float horSpeed = _playerSettings.WalkStrafeSpeed;

        if (_isSprinting)
        {
            vertSpeed = _playerSettings.RunForwardSpeed;
            horSpeed = _playerSettings.RunStrafeSpeed;
        }

        _newMoveSpeed = Vector3.SmoothDamp(_newMoveSpeed, new Vector3(horSpeed * _inputMove.x * Time.deltaTime, 0, vertSpeed * _inputMove.y * Time.deltaTime), ref _newMoveSpeedVelocity, _playerSettings.MoveSmoothing);
        var moveSpeed = transform.TransformDirection(_newMoveSpeed);

        if (_playerGravity > _minGravity)
        {
            _playerGravity -= _gravity * Time.deltaTime;
        }

        if (_playerGravity < -.1f && _charController.isGrounded)
        {
            _playerGravity = -.1f;
        }

        moveSpeed.y = _playerGravity;
        moveSpeed += _jumpForce * Time.deltaTime;

        //_charController.Move(transform.TransformDirection(_newMoveSpeed));
        _charController.Move(moveSpeed);
    }

    void SetJump()
    {
        _jumpForce = Vector3.SmoothDamp(_jumpForce, Vector3.zero, ref _jumpForceVelocity, _playerSettings.JumpFalloff);
    }

    void SetStance()
    {
        var currentStance = _standStance;

        if (_stance == PlayerStance.Crouch)
        {
            currentStance = _crouchStance;
        }
        else if (_stance == PlayerStance.Crawl)
        {
            currentStance = _crawlStance;
        }

        _camHeight = Mathf.SmoothDamp(_cameraHolder.localPosition.y, currentStance.CameraHeight, ref _camHeightVelocity, _stanceSmooth);
        _cameraHolder.localPosition = new Vector3(_cameraHolder.localPosition.x, _camHeight, _cameraHolder.localPosition.z);

        _charController.height = Mathf.SmoothDamp(_charController.height, currentStance.StanceCollider.height, ref _stanceCapsuleHeightVelocity, _stanceSmooth);
        _charController.center = Vector3.SmoothDamp(_charController.center, currentStance.StanceCollider.center, ref _stanceCapsuleCenterVelocity, _stanceSmooth);
    }

    void Jump()
    {
        if (!_charController.isGrounded) return;

        if (_stance == PlayerStance.Crawl)
        {
            _stance = PlayerStance.Stand;
            return;
        }

        _jumpForce = Vector3.up * _playerSettings.JumpForce;
        _playerGravity = 0;
    }

    void Crouch()
    {
        if (_stance == PlayerStance.Crouch)
        {
            if (StanceCheck(_standStance.StanceCollider.height - _satnceCheckModifier))
            {
                return;
            }
            _stance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(_crouchStance.StanceCollider.height - _satnceCheckModifier))
        {
            return;
        }

        _stance = PlayerStance.Crouch;
    }

    void Crawl()
    {
        _stance = PlayerStance.Crawl;
    }

    void ToggleSprint()
    {
        if (_inputMove.y <= .2f)
        {
            _isSprinting = false;
            return;
        }

        _isSprinting = !_isSprinting;
    }

    void StopSprint()
    {
        if (!_playerSettings.IsHoldForSprint) return;

        _isSprinting = false;
    }

    bool StanceCheck(float stanceCheckHeight)
    {
        Vector3 start = new Vector3(_feetTransform.position.x, _feetTransform.position.y + _charController.radius + _stanceMarginCheck + stanceCheckHeight, _feetTransform.position.z);
        Vector3 end = new Vector3(_feetTransform.position.x, _feetTransform.position.y - _charController.radius - _stanceMarginCheck + stanceCheckHeight, _feetTransform.position.z);

        return Physics.CheckCapsule(start, end, _charController.radius, _playerMask);
    }
}
