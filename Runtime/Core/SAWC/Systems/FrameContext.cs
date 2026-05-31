using SAWC.Core.Data;
using UnityEngine;

namespace SAWC.Core
{
    public struct FrameContext
    {
        public CharacterSettingsData Settings;

        public Vector2 MoveInput;
        public Vector3 WorldMoveDirection;
        public Vector3 WorldLookDirection;

        public bool IsGrounded;
        public bool SprintInput;
        public bool CrouchInput;
        public bool JumpInput;
        public bool CanStandUp;
        public bool HitCeiling;

        public float DeltaTime;
    }
}