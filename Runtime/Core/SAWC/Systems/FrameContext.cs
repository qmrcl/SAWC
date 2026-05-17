using UnityEngine;

namespace SAWC.Core
{
    public struct FrameContext
    {
        public Vector2 MoveInput;

        public bool IsGrounded;
        public bool SprintInput;
        public bool CrouchInput;

        public float DeltaTime;
    }
}