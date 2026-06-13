using UnityEngine;

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SAWC.Core.Input.Readers
{
    [AddComponentMenu("SAWC/Core/Input/Readers/New Input Reader")]
    public class NewInputReader : BaseInputReader
    {
#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _sprintAction;
        [SerializeField] private InputActionReference _crouchAction;

        public override Vector2 Move => _moveAction != null ? _moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        public override bool Jump => _jumpAction != null && _jumpAction.action.IsPressed();
        public override bool Sprint => _sprintAction != null && _sprintAction.action.IsPressed();
        public override bool Crouch => _crouchAction != null && _crouchAction.action.IsPressed();
#else
        public override Vector2 Move => Vector2.zero;
        public override bool Jump => false;
        public override bool Sprint => false;
        public override bool Crouch => false;
#endif
    }
}