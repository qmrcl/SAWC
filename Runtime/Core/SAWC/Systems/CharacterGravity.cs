namespace SAWC.Core
{
    internal sealed class CharacterGravity
    {
        private const float CeilingBounceVelocity = -1.5f;
        private const float JumpCooldownDuration = 0.1f;

        private float _verticalVelocity;
        private bool _wasJumpHeld;
        private bool _coyoteJumpConsumed;

        private float _timeSinceGrounded;
        private float _jumpBufferTimer;
        private float _jumpCooldownTimer;

        internal float VerticalVelocity => _verticalVelocity;

        internal void Tick(ref FrameContext ctx)
        {
            UpdateTimersAndBuffers(ref ctx);

            HandleCeilingCollision(ctx.HitCeiling);

            ApplyGravityForces(ref ctx);

            TryExecuteJump(ref ctx);

            _wasJumpHeld = ctx.JumpInput;
        }

        private void UpdateTimersAndBuffers(ref FrameContext ctx)
        {
            float deltaTime = ctx.DeltaTime;

            if (_jumpCooldownTimer > 0f) _jumpCooldownTimer -= deltaTime;
            if (_jumpBufferTimer > 0f) _jumpBufferTimer -= deltaTime;

            if (ctx.JumpInput && !_wasJumpHeld)
            {
                _jumpBufferTimer = ctx.Settings.Jump.JumpBufferTime;
            }

            if (ctx.IsGrounded && _verticalVelocity <= 0f)
            {
                _timeSinceGrounded = 0f;
                _coyoteJumpConsumed = false;
            }
            else
            {
                _timeSinceGrounded += deltaTime;
            }
        }

        private void HandleCeilingCollision(bool hitCeiling)
        {
            if (hitCeiling && _verticalVelocity > 0f)
            {
                _verticalVelocity = CeilingBounceVelocity;
            }
        }

        private void ApplyGravityForces(ref FrameContext ctx)
        {
            var physics = ctx.Settings.Physics;

            if (!physics.UseGravity)
            {
                if (ctx.IsGrounded && _verticalVelocity < 0f) _verticalVelocity = 0f;
                return;
            }

            if (ctx.IsGrounded && _verticalVelocity <= 0f)
            {
                _verticalVelocity = physics.GroundedGravity;
                return;
            }

            float multiplier = _verticalVelocity < 0f ? physics.FallMultiplier : 1f;
            _verticalVelocity += physics.Gravity * multiplier * ctx.DeltaTime;

            if (_verticalVelocity < physics.TerminalVelocity)
            {
                _verticalVelocity = physics.TerminalVelocity;
            }
        }

        private void TryExecuteJump(ref FrameContext ctx)
        {
            var jump = ctx.Settings.Jump;

            if (!jump.CanJump || !ctx.CanStandUp) return;
            if (ctx.CrouchInput && !ctx.Settings.Crouch.CanJumpWhileCrouching) return;

            bool canCoyoteJump = !_coyoteJumpConsumed && _timeSinceGrounded <= jump.CoyoteTime;
            bool hasJumpInput = _jumpBufferTimer > 0f || (jump.EnableAutoJump && ctx.JumpInput);

            if (hasJumpInput && canCoyoteJump && _jumpCooldownTimer <= 0f)
            {
                _verticalVelocity = jump.JumpForce;
                _jumpBufferTimer = 0f;
                _coyoteJumpConsumed = true;
                _jumpCooldownTimer = JumpCooldownDuration;
            }
        }
    }
}