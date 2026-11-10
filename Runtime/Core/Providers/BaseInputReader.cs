using UnityEngine;

namespace SAWC.Core.Input.Readers
{
    public abstract class BaseInputReader : MonoBehaviour
    {
        public abstract Vector2 Move { get; }
        public abstract bool Jump { get; }
        public abstract bool Sprint { get; }
        public abstract bool Crouch { get; }
    }
}