using UnityEngine;
using System;

public static class Models
{
    #region - Player -

    public enum PlayerStance
    {
        Stand,
        Crouch,
        Crawl
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("LookSettings")]
        public float LookXSensitivity;
        public float LookYSensitivity;

        public bool LookXInverted;
        public bool LookYInverted;

        [Header("SprinSettings")]
        public bool IsHoldForSprint;
        public float MoveSmoothing;

        [Header("MoveRunSettings")]
        public float RunForwardSpeed;
        public float RunBackwardSpeed;
        public float RunStrafeSpeed;

        [Header("MoveWalkSettings")]
        public float WalkForwardSpeed;
        public float WalkBackwardSpeed;
        public float WalkStrafeSpeed;

        [Header("JumpSettings")]
        public float JumpForce;
        public float JumpFalloff;
        public float FallSmoothing;

        [Header("SpeedModifiers")]
        public float SpeedModifier = 1f;
        public float CrouchSpeedModifier = 1f;
        public float CrawlSpeedModifier = 1f;
        public float FallSpeedModifier = 1f;

        [Header("IsGrounded/Falling")]
        public float IsGroundedRadius;
        public float IsFallingSpeed;
    }

    [Serializable]
    public class CharStance
    {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }

    #endregion

    #region - Weapon -

    [Serializable]
    public class WpSettingsModel
    {
        [Header("WpSway")]
        public float SwayAmount;
        public float SwayZAmount;
        public bool IsSwayYInverted;
        public bool IsSwayXInverted;
        public float SwaySmoothing;
        public float SwaySmoothReset;
        public float SwayClampX;
        public float SwayClampY;

        [Header("WpMoveSway")]
        public float MoveSwayX;
        public float MoveSwayY;
        public bool IsMoveSwayYInverted;
        public bool IsMoveSwayXInverted;
        public float MoveSwaySmoothing;
    }

    #endregion
}
