using UnityEngine;

namespace SAWC.Core.Input.Readers
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Mobile UI Reader")]
    public class MobileUIReader : BaseInputReader
    {
        [SerializeField] private BaseJoystick _joystick;
        [SerializeField] private bool _sprintByJoystick = true;

        private bool _jump;
        private bool _sprint;
        private bool _crouchToggled;

        public override Vector2 Move
        {
            get
            {
                if (_joystick == null || _joystick.HandleLimit <= 0f) return Vector2.zero;
                return Vector2.ClampMagnitude(_joystick.JoystickDirection / _joystick.HandleLimit, 1f);
            }
        }

        public override bool Jump => _jump;

        public override bool Sprint
        {
            get
            {
                if (_sprint) return true;

                if (_sprintByJoystick && _joystick != null && _joystick.HandleLimit > 0.05f)
                {
                    float maxLimit = _joystick.HandleLimit;
                    float threshold = maxLimit - 0.05f;
                    return _joystick.JoystickDirection.sqrMagnitude >= threshold * threshold;
                }

                return false;
            }
        }

        public override bool Crouch => _crouchToggled && !Sprint;

        private void Awake()
        {
            if (_joystick == null)
                Debug.LogError($"{nameof(MobileUIReader)}. Joystick is not assigned on '{gameObject.name}'!", this);
        }

        private void OnDisable()
        {
            _jump = false;
            _sprint = false;
            _crouchToggled = false;
        }

        public void SetJump(bool state) => _jump = state;

        public void SetSprint(bool state)
        {
            _sprint = state;
            if (state) _crouchToggled = false;
        }

        public void SetCrouch(bool state)
        {
            if (state) _crouchToggled = !_crouchToggled;
        }
    }
}
