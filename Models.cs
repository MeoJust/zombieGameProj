using UnityEngine;
using System;

public static class Models
{
    #region - Player -

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("LookSettings")]
        public float LookXSensitivity;
        public float LookYSensitivity;

        public bool LookXInverted;
        public bool LookYInverted;

        [Header("MoveSettings")]
        public float WalkForwardSpeed;
        public float WalkBackwardSpeed;
        public float WalkStrafeSpeed;
    }

    #endregion
}
