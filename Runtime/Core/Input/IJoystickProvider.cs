using UnityEngine;

namespace SAWC.Core
{
    public interface IJoystickProvider
    {
        Vector2 JoystickDirection { get; }
    }
}