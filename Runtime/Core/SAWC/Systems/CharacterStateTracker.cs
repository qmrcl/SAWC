using SAWC.Core.Data;
using System;
using UnityEngine;

namespace SAWC.Core
{
    internal sealed class CharacterStateTracker : ICharacterState
    {
        private struct StateFlags
        {
            public bool IsMoving;
            public bool IsSprinting;
            public bool IsJumping;
            public bool IsFalling;
            public bool IsCrouching;
            public bool IsGrounded;
        }

        private StateFlags _flags;

        public bool IsMoving => _flags.IsMoving;
        public bool IsSprinting => _flags.IsSprinting;
        public bool IsJumping => _flags.IsJumping;
        public bool IsFalling => _flags.IsFalling;
        public bool IsCrouching => _flags.IsCrouching;
        public bool IsGrounded => _flags.IsGrounded;

        public CharacterSettingsData EffectiveSettings { get; private set; }
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

        private float _fallingTime;
        private float _airborneTime;
        private bool _fallEventFired;

        internal void Initialize(bool isGrounded)
        {
            _flags.IsGrounded = isGrounded;
            _flags.IsFalling = !isGrounded;
        }

        internal void Tick(ref FrameContext ctx, Vector3 intendedVelocity, bool sprintActive)
        {
            SaveFrameContext(ref ctx, intendedVelocity);

            StateFlags old = _flags;

            UpdateCurrentFlags(ref ctx, intendedVelocity, sprintActive, old);

            if (_flags.IsFalling)
            {
                _fallingTime += ctx.DeltaTime;
            }
            else
            {
                _fallingTime = 0f;
                _fallEventFired = false;
            }

            if (!_flags.IsGrounded)
            {
                _airborneTime += ctx.DeltaTime;
            }

            CheckTransitions(old, _flags);

            if (_flags.IsGrounded)
            {
                _airborneTime = 0f;
            }
        }

        private void SaveFrameContext(ref FrameContext ctx, Vector3 intendedVelocity)
        {
            EffectiveSettings = ctx.Settings;
            Velocity = intendedVelocity;
            IntendedMoveDirection = ctx.WorldMoveDirection;
            LookDirection = ctx.WorldLookDirection;
        }

        private void UpdateCurrentFlags(ref FrameContext ctx, Vector3 intendedVelocity, bool sprintActive, StateFlags old)
        {
            _flags.IsCrouching = ctx.CrouchInput;
            _flags.IsGrounded = ctx.IsGrounded;

            _flags.IsMoving = EvaluateMovementState(intendedVelocity, old.IsMoving, ref ctx.Settings);
            _flags.IsSprinting = _flags.IsMoving && sprintActive;

            CalculateAirFlags(intendedVelocity.y, ctx.GravityVerticalVelocity, old, ref ctx.Settings);
        }

        private bool EvaluateMovementState(Vector3 intendedVelocity, bool wasMoving, ref CharacterSettingsData settings)
        {
            float speedSq = new Vector3(intendedVelocity.x, 0f, intendedVelocity.z).sqrMagnitude;
            float moveThresholdSq = settings.Movement.MinMoveThreshold * settings.Movement.MinMoveThreshold;

            float currentMoveThreshold = wasMoving
                ? moveThresholdSq * settings.Thresholds.IdleTransitionMultiplier
                : moveThresholdSq;

            return speedSq > currentMoveThreshold;
        }

        private void CalculateAirFlags(float realVerticalVelocity, float gravityVerticalVelocity, StateFlags old, ref CharacterSettingsData settings)
        {
            if (_flags.IsGrounded)
            {
                _flags.IsJumping = false;
                _flags.IsFalling = false;
                return;
            }

            float upThreshold = settings.Thresholds.VerticalVelocityThreshold;
            float fallThreshold = settings.Thresholds.FallVelocityThreshold;

            _flags.IsJumping = gravityVerticalVelocity > upThreshold || (gravityVerticalVelocity >= fallThreshold && old.IsJumping);
            _flags.IsFalling = realVerticalVelocity < fallThreshold || (realVerticalVelocity <= upThreshold && old.IsFalling);
        }

        private void CheckTransitions(StateFlags old, StateFlags current)
        {
            ExecuteTransition(old.IsSprinting, current.IsSprinting, SprintStarted, SprintCanceled);
            ExecuteTransition(old.IsMoving, current.IsMoving, StartMoving, StopMoving);
            ExecuteTransition(old.IsCrouching, current.IsCrouching, CrouchStarted, CrouchCanceled);
            ExecuteTransition(old.IsJumping, current.IsJumping, JumpPerformed, null);

            float debounceLimit = EffectiveSettings.Thresholds.AirStateDebounceTime;

            if (_fallingTime >= debounceLimit && !_fallEventFired)
            {
                FallStarted?.Invoke();
                _fallEventFired = true;
            }

            if (!old.IsGrounded && current.IsGrounded)
            {
                if (_airborneTime >= debounceLimit)
                {
                    LandPerformed?.Invoke();
                }
            }
        }

        private void ExecuteTransition(bool oldVal, bool newVal, Action onBecomeTrue, Action onBecomeFalse)
        {
            if (oldVal == newVal) return;

            Action actionToInvoke = newVal ? onBecomeTrue : onBecomeFalse;
            actionToInvoke?.Invoke();
        }
    }
}