using System;

namespace SAWC.Core
{
    internal sealed class CharacterGravity
    {
        private readonly CharacterSettings _settings;

        private float _verticalVelocity;
        private bool _jumpHeld;

        internal float VerticalVelocity => _verticalVelocity;
        internal event Action JumpPerformed;

        internal CharacterGravity(CharacterSettings settings)
        {
            _settings = settings;
        }

        internal void SetJumpHeld(bool held) => _jumpHeld = held;

        internal void Tick(bool isGrounded, float deltaTime)
        {
            ApplyGravity(isGrounded, deltaTime);
            HandleJump(isGrounded);
        }

        private void ApplyGravity(bool isGrounded, float deltaTime)
        {
            if (isGrounded)
            {
                if (_verticalVelocity < 0f) _verticalVelocity = _settings.GroundedGravity;
                return;
            }

            float multiplier = _verticalVelocity < 0f ? _settings.FallMultiplier : 1f;
            _verticalVelocity += _settings.Gravity * multiplier * deltaTime;

            if (_verticalVelocity < _settings.TerminalVelocity)
                _verticalVelocity = _settings.TerminalVelocity;
        }

        private void HandleJump(bool isGrounded)
        {
            if (!_settings.CanJump) return;

            if (_jumpHeld && isGrounded)
            {
                _verticalVelocity = _settings.JumpForce;
                JumpPerformed?.Invoke();
                if (!_settings.EnableAutoJump) _jumpHeld = false;
            }
        }
    }
}