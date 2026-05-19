using System;
using UnityEngine;

namespace SAWC.Core
{
    internal sealed class CharacterStateTracker : ICharacterState
    {
        public enum MovementState { Idle, Walking, Sprinting }
        public enum AirState { Grounded, Jumping, Falling }

        private MovementState _moveState = MovementState.Idle;
        private AirState _airState = AirState.Grounded;
        private bool _isCrouching = false;

        private CharacterSettings _settings;

        public bool IsMoving => _moveState != MovementState.Idle;
        public bool IsSprinting => _moveState == MovementState.Sprinting;
        public bool IsJumping => _airState == AirState.Jumping;
        public bool IsFalling => _airState == AirState.Falling;
        public bool IsCrouching => _isCrouching;
        public bool IsGrounded => _airState == AirState.Grounded;

        public Vector3 Velocity { get; private set; }
        public Vector3 IntendedMoveDirection { get; private set; }
        public Vector3 LookDirection { get; private set; }

        public event Action JumpPerformed;
        public event Action LandPerformed;
        public event Action FallStarted;
        public event Action StartMoving;
        public event Action StopMoving;
        public event Action SprintStarted;
        public event Action SprintCanceled;
        public event Action CrouchStarted;
        public event Action CrouchCanceled;

        internal void Initialize(bool isGrounded, CharacterSettings settings)
        {
            _settings = settings;
            _airState = isGrounded ? AirState.Grounded : AirState.Falling;
        }

        internal void Tick(ref FrameContext ctx, Vector3 intendedVelocity, bool sprintActive)
        {
            Velocity = intendedVelocity;
            IntendedMoveDirection = ctx.WorldMoveDirection;
            LookDirection = ctx.WorldLookDirection;

            UpdateAirState(ctx.IsGrounded, intendedVelocity.y);
            UpdateStance(ctx.CrouchInput);

            Vector3 horizontalVelocity = intendedVelocity;
            horizontalVelocity.y = 0;
            UpdateMovementState(horizontalVelocity.sqrMagnitude, sprintActive);
        }

        private void UpdateStance(bool crouchActive)
        {
            if (_isCrouching == crouchActive) return;

            _isCrouching = crouchActive;

            if (_isCrouching) CrouchStarted?.Invoke();
            else CrouchCanceled?.Invoke();
        }

        private void UpdateAirState(bool isGrounded, float verticalVelocity)
        {
            AirState nextState = _airState;

            if (isGrounded)
            {
                nextState = AirState.Grounded;
            }
            else if (verticalVelocity > _settings.VerticalVelocityThreshold)
            {
                nextState = AirState.Jumping;
            }
            else if (verticalVelocity < -_settings.VerticalVelocityThreshold)
            {
                nextState = AirState.Falling;
            }

            if (nextState == _airState) return;

            if (nextState == AirState.Grounded) LandPerformed?.Invoke();
            if (nextState == AirState.Jumping) JumpPerformed?.Invoke();
            if (nextState == AirState.Falling) FallStarted?.Invoke();

            _airState = nextState;
        }

        private void UpdateMovementState(float speedSq, bool sprintActive)
        {
            MovementState nextState = _moveState;
            float thresholdSq = _settings.MinMoveThreshold * _settings.MinMoveThreshold;

            if (speedSq > thresholdSq)
            {
                nextState = sprintActive ? MovementState.Sprinting : MovementState.Walking;
            }
            else if (speedSq <= thresholdSq * _settings.IdleTransitionMultiplier)
            {
                nextState = MovementState.Idle;
            }

            if (nextState == _moveState) return;

            if (_moveState == MovementState.Idle) StartMoving?.Invoke();
            if (_moveState == MovementState.Sprinting) SprintCanceled?.Invoke();

            if (nextState == MovementState.Idle) StopMoving?.Invoke();
            if (nextState == MovementState.Sprinting) SprintStarted?.Invoke();

            _moveState = nextState;
        }
    }
}