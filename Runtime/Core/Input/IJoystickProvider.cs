using UnityEngine;

namespace SAWC.Core.Input
{
    public abstract class BaseJoystick : MonoBehaviour
    {
        public abstract Vector2 JoystickDirection { get; }
    }
}