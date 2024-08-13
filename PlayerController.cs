using static Models;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController _charController;

    Ia_defInput _defInput;

    [Header("Refs")]
    public Transform CameraHolder;
    [SerializeField] Transform _feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel _playerSettings;

    [SerializeField] float _lookClampYMin = -70f;
    [SerializeField] float _lookClampYMax = 80f;
    [SerializeField] LayerMask _playerMask;
    [SerializeField] LayerMask _groundMask;


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

    [Header("Weapon")]
    [SerializeField] WpController _currentWp;


    float _camHeight;
    float _camHeightVelocity;

    Vector3 _cameraRotation;
    Vector3 _playerRotation;

    Vector3 _stanceCapsuleCenterVelocity;

    float _stanceCapsuleHeightVelocity;

    float _stanceMarginCheck = .05f;

    public bool IsSprinting;

    Vector3 _newMoveSpeed;
    Vector3 _newMoveSpeedVelocity;

    public float WpAnimSpeed;

    [HideInInspector]
    public Vector2 InputMove;
    [HideInInspector]
    public Vector2 InputLook;

    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;

    [Header("Aiming")]
    public bool IsAiming;

    #region - MonoBehaviour -

    void Awake()
    {
        _defInput = new Ia_defInput();

        _defInput.player.Move.performed += e => InputMove = e.ReadValue<Vector2>();
        _defInput.player.Look.performed += e => InputLook = e.ReadValue<Vector2>();
        _defInput.player.Jump.performed += e => Jump();
        _defInput.player.Crouch.performed += e => Crouch();
        _defInput.player.Crawl.performed += e => Crawl();
        _defInput.player.Sprint.performed += e => ToggleSprint();
        _defInput.player.SprintRelese.performed += e => StopSprint();

        _defInput.wp.fire2pressed.performed += e => AimPressed();
        _defInput.wp.fire2released.performed += e => AimReleased();

        _defInput.Enable();

        _cameraRotation = CameraHolder.localRotation.eulerAngles;
        _playerRotation = transform.localRotation.eulerAngles;

        _charController = GetComponent<CharacterController>();

        _camHeight = CameraHolder.localPosition.y;

        if (_currentWp)
        {
            _currentWp.Init(this);
        }
    }

    void Update()
    {
        SetIsGrounded();
        SetIsFalling();
        SetLook();
        SetMove();
        SetJump();
        SetStance();
        SetAim();
    }

    #endregion

    #region - Aiming -

    void AimPressed()
    {
        IsAiming = true;
    }

    void AimReleased()
    {
        IsAiming = false;
    }

    void SetAim()
    {
        if(!_currentWp) return;

        _currentWp.IsAiming = IsAiming;
    }

    #endregion

    #region - Falling / Grounded -

    void SetIsGrounded()
    {
        isGrounded = Physics.CheckSphere(_feetTransform.position, _charController.radius, _groundMask);
    }

    void SetIsFalling()
    {
        isFalling = !isGrounded && _charController.velocity.magnitude >= _playerSettings.IsFallingSpeed;
    }

    #endregion

    #region - Look / Move -
    void SetLook()
    {
        _playerRotation.y += _playerSettings.LookXSensitivity * (_playerSettings.LookXInverted ? -InputLook.x : InputLook.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_playerRotation);

        _cameraRotation.x += _playerSettings.LookYSensitivity * (_playerSettings.LookYInverted ? InputLook.y : -InputLook.y) * Time.deltaTime;
        _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, _lookClampYMin, _lookClampYMax);

        CameraHolder.localRotation = Quaternion.Euler(_cameraRotation);
    }

    void SetMove()
    {
        if (InputMove.y <= .2f)
        {
            IsSprinting = false;
        }

        float vertSpeed = _playerSettings.WalkForwardSpeed;
        float horSpeed = _playerSettings.WalkStrafeSpeed;

        if (IsSprinting)
        {
            vertSpeed = _playerSettings.RunForwardSpeed;
            horSpeed = _playerSettings.RunStrafeSpeed;
        }

        if (!isGrounded)
        {
            _playerSettings.SpeedModifier = _playerSettings.FallSpeedModifier;
        }
        else if (_stance == PlayerStance.Crouch)
        {
            _playerSettings.SpeedModifier = _playerSettings.CrouchSpeedModifier;
        }
        else if (_stance == PlayerStance.Crawl)
        {
            _playerSettings.SpeedModifier = _playerSettings.CrawlSpeedModifier;
        }
        else
        {
            _playerSettings.SpeedModifier = 1f;
        }

        WpAnimSpeed = _charController.velocity.magnitude / (_playerSettings.WalkForwardSpeed * _playerSettings.SpeedModifier);

        if (WpAnimSpeed > 1)
        {
            WpAnimSpeed = 1;
        }

        vertSpeed *= _playerSettings.SpeedModifier;
        horSpeed *= _playerSettings.SpeedModifier;

        _newMoveSpeed = Vector3.SmoothDamp(_newMoveSpeed, new Vector3(horSpeed * InputMove.x * Time.deltaTime, 0, vertSpeed * InputMove.y * Time.deltaTime),
            ref _newMoveSpeedVelocity,
                isGrounded ? _playerSettings.MoveSmoothing : _playerSettings.FallSmoothing);

        var moveSpeed = transform.TransformDirection(_newMoveSpeed);

        if (_playerGravity > _minGravity)
        {
            _playerGravity -= _gravity * Time.deltaTime;
        }

        if (_playerGravity < -.1f && isGrounded)
        {
            _playerGravity = -.1f;
        }

        moveSpeed.y = _playerGravity;
        moveSpeed += _jumpForce * Time.deltaTime;

        //_charController.Move(transform.TransformDirection(_newMoveSpeed));
        _charController.Move(moveSpeed);
    }

    #endregion

    #region - Jump -
    void SetJump()
    {
        _jumpForce = Vector3.SmoothDamp(_jumpForce, Vector3.zero, ref _jumpForceVelocity, _playerSettings.JumpFalloff);
    }

    void Jump()
    {
        if (!isGrounded) return;

        if (_stance == PlayerStance.Crawl)
        {
            if (StanceCheck(_standStance.StanceCollider.height))
            {
                return;
            }

            _stance = PlayerStance.Stand;
            return;
        }

        _jumpForce = Vector3.up * _playerSettings.JumpForce;
        _playerGravity = 0;
        _currentWp.TriggerJump();
    }

    #endregion

    #region - Stance -
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

        _camHeight = Mathf.SmoothDamp(CameraHolder.localPosition.y, currentStance.CameraHeight, ref _camHeightVelocity, _stanceSmooth);
        CameraHolder.localPosition = new Vector3(CameraHolder.localPosition.x, _camHeight, CameraHolder.localPosition.z);

        _charController.height = Mathf.SmoothDamp(_charController.height, currentStance.StanceCollider.height, ref _stanceCapsuleHeightVelocity, _stanceSmooth);
        _charController.center = Vector3.SmoothDamp(_charController.center, currentStance.StanceCollider.center, ref _stanceCapsuleCenterVelocity, _stanceSmooth);
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

    bool StanceCheck(float stanceCheckHeight)
    {
        Vector3 start = new Vector3(_feetTransform.position.x, _feetTransform.position.y + _charController.radius + _stanceMarginCheck + stanceCheckHeight, _feetTransform.position.z);
        Vector3 end = new Vector3(_feetTransform.position.x, _feetTransform.position.y - _charController.radius - _stanceMarginCheck + stanceCheckHeight, _feetTransform.position.z);

        return Physics.CheckCapsule(start, end, _charController.radius, _playerMask);
    }

    #endregion

    #region - Sprint -
    void ToggleSprint()
    {
        if (InputMove.y <= .2f)
        {
            IsSprinting = false;
            return;
        }

        IsSprinting = !IsSprinting;
    }

    void StopSprint()
    {
        if (!_playerSettings.IsHoldForSprint) return;

        IsSprinting = false;
    }

    #endregion

    #region - Gizmos -

    void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_feetTransform.position, _playerSettings.IsGroundedRadius);
    }

    #endregion
}
