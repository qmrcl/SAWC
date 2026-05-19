using UnityEngine;

namespace SAWC.Input
{
    public interface IJoystickProvider
    {
        Vector2 JoystickDirection { get; }
    }
}