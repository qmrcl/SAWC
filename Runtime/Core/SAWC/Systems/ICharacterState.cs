using System;

namespace SAWC.Core
{
    public interface ICharacterState
    {
        bool IsMoving { get; }
        bool IsSprinting { get; }
        bool IsJumping { get; }
        bool IsFalling { get; }
        bool IsCrouching { get; }
        bool IsGrounded { get; }

        event Action JumpPerformed;
        event Action LandPerformed;
        event Action FallStarted;
        event Action StartMoving;
        event Action StopMoving;
        event Action SprintStarted;
        event Action SprintCanceled;
        event Action CrouchStarted;
        event Action CrouchCanceled;
    }
}