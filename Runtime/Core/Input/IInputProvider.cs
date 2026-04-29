using UnityEngine;
using System;

namespace SAWC.Core
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        event Action JumpStarted;
        event Action JumpCanceled;
        event Action SprintStarted;
        event Action SprintCanceled;
    }
}