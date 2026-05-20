using UnityEngine;
using System;

namespace SAWC.Core
{
    internal sealed class CharacterLocomotion
    {
        private readonly CharacterSettings _settings;
        private readonly Transform _transform;

        private Vector3 _currentHorizontalVelocity;
        private float _rotationVelocity;

        internal Vector3 CurrentHorizontalVelocity => _currentHorizontalVelocity;
        internal bool IsSprintingActive { get; private set; }

        internal CharacterLocomotion(CharacterSettings settings, Transform transform)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        internal void Tick(ref FrameContext ctx)
        {
            Vector3 inputDirection = ctx.WorldMoveDirection;

            HandleRotation(inputDirection, ctx.WorldLookDirection);

            float speed = CalculateSpeed(ctx.MoveInput, ctx.SprintInput, ctx.CrouchInput);
            Vector3 targetVelocity = inputDirection * speed;

            float smoothing = GetSmoothingRate(_currentHorizontalVelocity, targetVelocity, ctx.IsGrounded);

            _currentHorizontalVelocity = Vector3.MoveTowards(
                _currentHorizontalVelocity,
                targetVelocity,
                smoothing * ctx.DeltaTime
            );
        }

        private float CalculateSpeed(Vector2 moveInput, bool isSprinting, bool isCrouching)
        {
            if (isCrouching && _settings.CanCrouch)
            {
                IsSprintingActive = false;
                return _settings.CrouchSpeed;
            }

            bool hasInput = moveInput.sqrMagnitude > _settings.InputThresholdSq;
            bool validDirection = _settings.SprintOnlyForward ? moveInput.y > _settings.InputThreshold : hasInput;

            IsSprintingActive = isSprinting && validDirection && _settings.CanSprint;
            return IsSprintingActive ? _settings.SprintSpeed : _settings.MoveSpeed;
        }

        private float GetSmoothingRate(Vector3 currentVelocity, Vector3 targetVelocity, bool isGrounded)
        {
            if (targetVelocity.sqrMagnitude <= _settings.InputThresholdSq)
            {
                float brakeRatio = Mathf.Clamp01(currentVelocity.magnitude / _settings.SprintSpeed);
                float rate = _settings.BaseDeceleration * _settings.DecelerationCurve.Evaluate(brakeRatio);
                return isGrounded ? rate : rate * _settings.AirControlMultiplier;
            }

            Vector3 currentDir = currentVelocity.normalized;
            Vector3 targetDir = targetVelocity.normalized;
            float directionMatch = Vector3.Dot(currentDir, targetDir);

            if (directionMatch < 0f)
            {
                float brakeRatio = Mathf.Clamp01(currentVelocity.magnitude / _settings.SprintSpeed);
                float rate = _settings.BaseDeceleration * _settings.DecelerationCurve.Evaluate(brakeRatio);
                return isGrounded ? rate : rate * _settings.AirControlMultiplier;
            }

            float speedInTargetDirection = Vector3.Dot(currentVelocity, targetDir);
            float maxSpeed = targetVelocity.magnitude > 0.1f ? targetVelocity.magnitude : _settings.MoveSpeed;

            float speedRatio = Mathf.Clamp01(speedInTargetDirection / maxSpeed);
            float accelRate = _settings.BaseAcceleration * _settings.AccelerationCurve.Evaluate(speedRatio);

            return isGrounded ? accelRate : accelRate * _settings.AirControlMultiplier;
        }

        private void HandleRotation(Vector3 inputDirection, Vector3 lookDirection)
        {
            if (_settings.RotateWithMovement)
            {
                if (inputDirection.sqrMagnitude >= _settings.InputThresholdSq)
                {
                    float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _rotationVelocity, _settings.MovementRotationSmoothTime);
                    _transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
            }
            else
            {
                if (lookDirection.sqrMagnitude >= 0.001f)
                {
                    float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _rotationVelocity, _settings.StrafeRotationSmoothTime);
                    _transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
            }
        }
    }
}