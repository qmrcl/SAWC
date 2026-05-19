using UnityEngine;

namespace SAWC.Core
{
    public struct FrameContext
    {
        public Vector2 MoveInput;
        public Vector3 WorldMoveDirection;
        public Vector3 WorldLookDirection;

        public bool IsGrounded;
        public bool SprintInput;
        public bool CrouchInput;

        public float DeltaTime;
    }
}