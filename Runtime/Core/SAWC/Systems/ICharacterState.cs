using System;
using UnityEngine;

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

        Vector3 Velocity { get; }

        Vector3 IntendedMoveDirection { get; }
        Vector3 LookDirection { get; }

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