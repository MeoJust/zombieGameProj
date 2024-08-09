using UnityEngine;
using static Models;

public class WpController : MonoBehaviour
{
    PlayerController _playerController;

    [Header("Settings")]
    public WpSettingsModel WpSettings;

    bool _isInit;

    Vector3 _wpRotation;
    Vector3 _wpRotationVelocity;

    Vector3 _targetWpRotation;
    Vector3 _targetWpRotationVelocity;

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

        _targetWpRotation.y += WpSettings.SwayAmount * (WpSettings.IsSwayXInverted ? -_playerController.InputLook.x : _playerController.InputLook.x) * Time.deltaTime;
        _targetWpRotation.x += WpSettings.SwayAmount * (WpSettings.IsSwayYInverted ? _playerController.InputLook.y : -_playerController.InputLook.y) * Time.deltaTime;

        _targetWpRotation.x = Mathf.Clamp(_targetWpRotation.x, -WpSettings.SwayClampY, WpSettings.SwayClampY);
        _targetWpRotation.y = Mathf.Clamp(_targetWpRotation.y, -WpSettings.SwayClampX, WpSettings.SwayClampX);

        _targetWpRotation = Vector3.SmoothDamp(_targetWpRotation, Vector3.zero, ref _targetWpRotationVelocity, WpSettings.SwaySmoothReset);
        _wpRotation = Vector3.SmoothDamp(_wpRotation, _targetWpRotation, ref _wpRotationVelocity, WpSettings.SwaySmoothing);

        transform.localRotation = Quaternion.Euler(_wpRotation);
    }
}
