using SAWC.Localization;
using UnityEngine;

namespace SAWC.Core.Input.Readers
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Mobile UI Reader")]
    public class MobileUIReader : BaseInputReader
    {
        [SerializeField, Loc] private BaseJoystick _joystick;

        private bool _uiJump;
        private bool _uiSprint;
        private bool _uiCrouch;

        public override Vector2 Move => _joystick?.JoystickDirection ?? Vector2.zero;
        public override bool Jump => _uiJump;
        public override bool Sprint => _uiSprint;
        public override bool Crouch => _uiCrouch;

        private void Awake()
        {
            if (_joystick == null)
                Debug.LogError($"{nameof(MobileUIReader)}. Joystick is not assigned on '{gameObject.name}'!", this);
        }

        public void SetUIJump(bool state) => _uiJump = state;
        public void SetUISprint(bool state) => _uiSprint = state;
        public void SetUICrouch(bool state) => _uiCrouch = state;
    }
}