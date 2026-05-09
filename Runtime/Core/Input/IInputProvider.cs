using System;
using UnityEngine;

namespace SAWC.Core
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        event Action JumpStarted;
        event Action JumpCanceled;
        event Action SprintStarted;
        event Action SprintCanceled;
        
        event Action CrouchStarted;
        event Action CrouchCanceled;
    }
}