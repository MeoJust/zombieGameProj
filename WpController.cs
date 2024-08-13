using System;
using TMPro;
using UnityEngine;
using static Models;

public class WpController : MonoBehaviour
{
    PlayerController _playerController;

    [Header("Refs")]
    [SerializeField] Animator _wpAnimator;

    [Header("Settings")]
    public WpSettingsModel WpSettings;

    bool _isInit;

    Vector3 _wpRotation;
    Vector3 _wpRotationVelocity;

    Vector3 _targetWpRotation;
    Vector3 _targetWpRotationVelocity;

    Vector3 _wpMoveRotation;
    Vector3 _wpMoveRotationVelocity;

    Vector3 _targetWpMoveRotation;
    Vector3 _targetWpMoveRotationVelocity;

    bool _isGroundedTrigger;
    float _fallDelay = 0;

    [Header("WpSwayIdle")]
    [SerializeField] Transform _wpSwayGO;
    [SerializeField] float _swayAmountA = 1f;
    [SerializeField] float _swayAmountB = 2f;
    [SerializeField] float _swayScale = 600f;
    [SerializeField] float _swayLerpSpeed = 14f;
    float _swayTime;
    Vector3 _swayPosition;

    [Header("Sights")]
    [SerializeField] Transform _sightsGO;
    [SerializeField] float _sightOffset;
    [SerializeField] float _aimTime;

    Vector3 _wpSwayPosition;
    Vector3 _wpSwayPositionVelocity;

    [HideInInspector]
    public bool IsAiming;

    void Start()
    {
        _wpRotation = transform.localRotation.eulerAngles;
    }

    public void Init(PlayerController playerController)
    {
        _playerController = playerController;
        _isInit = true;
    }

    void Update()
    {
        if (!_isInit) return;

        SetWpRotation();
        SetWpAnim();
        SetWpSway();
        SetAim();
    }

    void SetAim()
    {
        var targetPos = transform.position;

        if (IsAiming)
        {
            targetPos = _playerController.CameraHolder.transform.position + (_wpSwayGO.position - _sightsGO.position) + (_playerController.CameraHolder.transform.forward * _sightOffset);
        }

        _wpSwayPosition = _wpSwayGO.transform.position;
        _wpSwayPosition = Vector3.SmoothDamp(_wpSwayPosition, targetPos, ref _wpSwayPositionVelocity, _aimTime);
        _wpSwayGO.transform.position = _wpSwayPosition;
    }

    public void TriggerJump()
    {
        _isGroundedTrigger = false;
        _wpAnimator.SetTrigger("jump");
    }

    void SetWpRotation()
    {
        // _wpAnimator.speed = _playerController.WpAnimSpeed;

        _targetWpRotation.y += WpSettings.SwayAmount * (WpSettings.IsSwayXInverted ? -_playerController.InputLook.x : _playerController.InputLook.x) * Time.deltaTime;
        _targetWpRotation.x += WpSettings.SwayAmount * (WpSettings.IsSwayYInverted ? _playerController.InputLook.y : -_playerController.InputLook.y) * Time.deltaTime;

        _targetWpRotation.x = Mathf.Clamp(_targetWpRotation.x, -WpSettings.SwayClampY, WpSettings.SwayClampY);
        _targetWpRotation.y = Mathf.Clamp(_targetWpRotation.y, -WpSettings.SwayClampX, WpSettings.SwayClampX);
        _targetWpRotation.z = _targetWpRotation.y * WpSettings.SwayZAmount;

        _targetWpRotation = Vector3.SmoothDamp(_targetWpRotation, Vector3.zero, ref _targetWpRotationVelocity, WpSettings.SwaySmoothReset);
        _wpRotation = Vector3.SmoothDamp(_wpRotation, _targetWpRotation, ref _wpRotationVelocity, WpSettings.SwaySmoothing);

        _targetWpMoveRotation.z = WpSettings.MoveSwayX * (WpSettings.IsMoveSwayXInverted ? -_playerController.InputMove.x : _playerController.InputMove.x);
        _targetWpMoveRotation.x = WpSettings.MoveSwayY * (WpSettings.IsMoveSwayYInverted ? -_playerController.InputMove.y : _playerController.InputMove.y);

        _targetWpMoveRotation = Vector3.SmoothDamp(_targetWpMoveRotation, Vector3.zero, ref _targetWpMoveRotationVelocity, WpSettings.MoveSwaySmoothing);
        _wpMoveRotation = Vector3.SmoothDamp(_wpMoveRotation, _targetWpMoveRotation, ref _wpMoveRotationVelocity, WpSettings.MoveSwaySmoothing);

        transform.localRotation = Quaternion.Euler(_wpRotation + _wpMoveRotation);
    }

    void SetWpAnim()
    {
        if (_isGroundedTrigger)
        {
            _fallDelay = 0;
        }
        else
        {
            _fallDelay += Time.deltaTime;
        }

        if (_playerController.isGrounded && !_isGroundedTrigger && _fallDelay > 0.15f)
        {
            _wpAnimator.SetTrigger("landed");
            _isGroundedTrigger = true;
        }
        else if (!_playerController.isGrounded && _isGroundedTrigger)
        {
            _wpAnimator.SetTrigger("fly");
            _isGroundedTrigger = false;
        }

        _wpAnimator.SetBool("isSprinting", _playerController.IsSprinting);
        _wpAnimator.SetFloat("wpAnimSpeed", _playerController.WpAnimSpeed);
    }

    void SetWpSway()
    {
        var targetPos = LissajousCurve(_swayTime, _swayAmountA, _swayAmountB) / _swayScale;

        _swayPosition = Vector3.Lerp(_swayPosition, targetPos, Time.smoothDeltaTime * _swayLerpSpeed);
        _swayTime += Time.deltaTime;

        if (_swayTime > 2 * Mathf.PI)
        {
            _swayTime = 0;
        }

        //_wpSwayGO.localPosition = _swayPosition;
    }

    Vector3 LissajousCurve(float time, float a, float b)
    {
        return new Vector3(Mathf.Sin(time), a * Mathf.Sin(b * time + Mathf.PI));
    }
}
