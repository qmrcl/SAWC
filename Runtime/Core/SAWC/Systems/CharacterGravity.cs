using System;

namespace SAWC.Core
{
    internal sealed class CharacterGravity
    {
        private readonly CharacterSettings _settings;

        private float _verticalVelocity;
        private bool _jumpHeld;

        private float _timeSinceGrounded;
        private float _jumpBufferTimer;
        private bool _coyoteJumpConsumed;

        internal float VerticalVelocity => _verticalVelocity;

        internal CharacterGravity(CharacterSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        internal void SetJumpHeld(bool held)
        {
            _jumpHeld = held;
            if (held) _jumpBufferTimer = _settings.JumpBufferTime;
        }

        internal void Tick(ref FrameContext ctx)
        {
            if (ctx.IsGrounded)
            {
                _timeSinceGrounded = 0f;
                _coyoteJumpConsumed = false;
            }
            else
            {
                _timeSinceGrounded += ctx.DeltaTime;
            }

            if (_jumpBufferTimer > 0f) _jumpBufferTimer -= ctx.DeltaTime;

            ApplyGravity(ctx.IsGrounded, ctx.DeltaTime);
            HandleJump();
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

        private void HandleJump()
        {
            if (!_settings.CanJump) return;

            bool canCoyoteJump = !_coyoteJumpConsumed && _timeSinceGrounded <= _settings.CoyoteTime;
            bool hasJumpInput = _jumpBufferTimer > 0f || (_settings.EnableAutoJump && _jumpHeld);

            if (hasJumpInput && canCoyoteJump)
            {
                _verticalVelocity = _settings.JumpForce;
                _jumpBufferTimer = 0f;

                _coyoteJumpConsumed = true;
            }
        }
    }
}