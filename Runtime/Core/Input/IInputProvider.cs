using System;
using UnityEngine;

namespace SAWC.Core.Input
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        Vector3 WorldMoveDirection { get; }
        Vector3 WorldLookDirection { get; }

        bool SprintHeld { get; }
        bool CrouchHeld { get; }
        bool JumpHeld { get; }

        event Action JumpStarted;
        event Action JumpCanceled;

        event Action SprintStarted;
        event Action SprintCanceled;
        event Action CrouchStarted;
        event Action CrouchCanceled;
    }
}