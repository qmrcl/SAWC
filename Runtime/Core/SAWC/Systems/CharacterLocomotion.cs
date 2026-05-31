using UnityEngine;
using System;
using SAWC.Core.Data;

namespace SAWC.Core
{
    internal sealed class CharacterLocomotion
    {
        private Vector3 _currentHorizontalVelocity;

        internal Vector3 CurrentHorizontalVelocity => _currentHorizontalVelocity;
        internal bool IsSprintingActive { get; private set; }

        internal void Tick(ref FrameContext ctx)
        {
            float targetSpeed = EvaluateTargetSpeed(ref ctx);
            Vector3 targetVelocity = ctx.WorldMoveDirection * targetSpeed;

            if (ctx.Settings.Movement.UseInertia)
            {
                float smoothing = CalculateSmoothingRate(_currentHorizontalVelocity, targetVelocity, ctx.IsGrounded, ref ctx.Settings);

                _currentHorizontalVelocity = Vector3.MoveTowards(
                    _currentHorizontalVelocity,
                    targetVelocity,
                    smoothing * ctx.DeltaTime
                );
            }
            else
            {
                _currentHorizontalVelocity = targetVelocity;
            }
        }

        private float EvaluateTargetSpeed(ref FrameContext ctx)
        {
            var move = ctx.Settings.Movement;

            if (!move.CanMove)
            {
                IsSprintingActive = false;
                return 0f;
            }

            if (ctx.CrouchInput)
            {
                IsSprintingActive = false;
                return ctx.Settings.Crouch.CrouchSpeed;
            }

            IsSprintingActive = ctx.SprintInput && move.CanSprint && IsSprintDirectionAllowed(ctx.MoveInput, ref ctx.Settings);

            return IsSprintingActive ? move.SprintSpeed : move.MoveSpeed;
        }

        private bool IsSprintDirectionAllowed(Vector2 moveInput, ref CharacterSettingsData settings)
        {
            float threshold = settings.Thresholds.InputThreshold;
            if (moveInput.sqrMagnitude <= settings.Thresholds.InputThresholdSq) return false;

            var allowed = settings.Movement.AllowedSprintDirections;

            if (moveInput.y > threshold && (allowed & SprintAllowedDirections.Forward) == 0) return false;
            if (moveInput.y < -threshold && (allowed & SprintAllowedDirections.Backward) == 0) return false;
            if (moveInput.x < -threshold && (allowed & SprintAllowedDirections.Left) == 0) return false;
            if (moveInput.x > threshold && (allowed & SprintAllowedDirections.Right) == 0) return false;

            return true;
        }

        private float CalculateSmoothingRate(Vector3 currentVelocity, Vector3 targetVelocity, bool isGrounded, ref CharacterSettingsData settings)
        {
            bool isBraking = targetVelocity.sqrMagnitude <= settings.Thresholds.InputThresholdSq
                          || Vector3.Dot(currentVelocity.normalized, targetVelocity.normalized) < 0f;

            float baseRate = isBraking
                ? EvaluateDeceleration(currentVelocity, ref settings)
                : EvaluateAcceleration(currentVelocity, targetVelocity, ref settings);

            return isGrounded ? baseRate : baseRate * settings.Jump.AirControlMultiplier;
        }

        private float EvaluateDeceleration(Vector3 currentVelocity, ref CharacterSettingsData settings)
        {
            float sprintSpeed = settings.Movement.SprintSpeed;
            float brakeRatio = sprintSpeed > 0.001f ? Mathf.Clamp01(currentVelocity.magnitude / sprintSpeed) : 0f;

            return settings.Movement.BaseDeceleration * settings.Movement.DecelerationCurve.Evaluate(brakeRatio);
        }

        private float EvaluateAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, ref CharacterSettingsData settings)
        {
            float speedInTargetDirection = Vector3.Dot(currentVelocity, targetVelocity.normalized);
            float maxSpeed = targetVelocity.magnitude > 0.1f ? targetVelocity.magnitude : settings.Movement.MoveSpeed;

            float speedRatio = Mathf.Clamp01(speedInTargetDirection / maxSpeed);
            return settings.Movement.BaseAcceleration * settings.Movement.AccelerationCurve.Evaluate(speedRatio);
        }
    }
}