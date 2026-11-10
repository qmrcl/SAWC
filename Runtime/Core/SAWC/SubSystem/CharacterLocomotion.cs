using UnityEngine;
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
            if (moveInput.sqrMagnitude <= settings.Thresholds.InputThresholdSq) return false;

            Vector2 dir = moveInput.normalized;
            var allowed = settings.Movement.AllowedSprintDirections;
            float dirThreshold = settings.Thresholds.SprintDirectionThreshold;

            if (dir.y > dirThreshold && (allowed & SprintAllowedDirections.Forward) != 0) return true;
            if (dir.y < -dirThreshold && (allowed & SprintAllowedDirections.Backward) != 0) return true;
            if (dir.x < -dirThreshold && (allowed & SprintAllowedDirections.Left) != 0) return true;
            if (dir.x > dirThreshold && (allowed & SprintAllowedDirections.Right) != 0) return true;

            return false;
        }

        private float CalculateSmoothingRate(Vector3 currentVelocity, Vector3 targetVelocity, bool isGrounded, ref CharacterSettingsData settings)
        {
            bool isBraking = targetVelocity.sqrMagnitude <= settings.Thresholds.InputThresholdSq
                          || Vector3.Dot(currentVelocity, targetVelocity) < 0f;

            float baseRate = isBraking
                ? EvaluateDeceleration(currentVelocity, ref settings)
                : EvaluateAcceleration(currentVelocity, targetVelocity, ref settings);

            return isGrounded ? baseRate : baseRate * settings.Jump.AirControlMultiplier;
        }

        private float EvaluateDeceleration(Vector3 currentVelocity, ref CharacterSettingsData settings)
        {
            float sprintSpeed = settings.Movement.SprintSpeed;
            float brakeRatio = sprintSpeed > 0.001f ? Mathf.Clamp01(currentVelocity.magnitude / sprintSpeed) : 0f;
            float curveMultiplier = settings.Movement.DecelerationCurve?.Evaluate(brakeRatio) ?? 1f;

            return settings.Movement.BaseDeceleration * curveMultiplier;
        }

        private float EvaluateAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, ref CharacterSettingsData settings)
        {
            float targetSpeedSq = targetVelocity.sqrMagnitude;
            if (targetSpeedSq < settings.Thresholds.VelocityThresholdSq)
                return settings.Movement.BaseAcceleration;

            float targetSpeed = Mathf.Sqrt(targetSpeedSq);
            Vector3 targetDir = targetVelocity / targetSpeed;

            float speedInTargetDirection = Vector3.Dot(currentVelocity, targetDir);
            float speedRatio = Mathf.Clamp01(speedInTargetDirection / targetSpeed);
            float curveMultiplier = settings.Movement.AccelerationCurve?.Evaluate(speedRatio) ?? 1f;

            return settings.Movement.BaseAcceleration * curveMultiplier;
        }
    }
}