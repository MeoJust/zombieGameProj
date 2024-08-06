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
    }

    [Serializable]
    public class CharStance
    {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }

    #endregion
}
