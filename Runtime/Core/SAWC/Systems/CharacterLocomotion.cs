using UnityEngine;

namespace SAWC.Core
{
    internal sealed class CharacterLocomotion
    {
        private readonly CharacterSettings _settings;
        private readonly Transform _transform;
        private readonly Transform _cameraTransform;

        private Vector3 _currentHorizontalVelocity;
        private float _rotationVelocity;
        private float _lastCameraYRotation;

        internal Vector3 CurrentHorizontalVelocity => _currentHorizontalVelocity;

        internal CharacterLocomotion(CharacterSettings settings, Transform transform, Transform cameraTransform)
        {
            _settings = settings;
            _transform = transform;
            _cameraTransform = cameraTransform;
            _lastCameraYRotation = cameraTransform.eulerAngles.y;
        }

        internal void Tick(Vector2 moveInput, bool isSprinting, bool isGrounded, float deltaTime)
        {
            Vector3 inputDirection = GetInputDirection(moveInput);
            HandleRotation(inputDirection);

            float speed = CalculateSpeed(moveInput, isSprinting);
            Vector3 targetVelocity = inputDirection.normalized * speed;
            float smoothing = GetSmoothingRate(targetVelocity, isGrounded);

            _currentHorizontalVelocity = Vector3.MoveTowards(
                _currentHorizontalVelocity,
                targetVelocity,
                smoothing * deltaTime
            );
        }

        private Vector3 GetInputDirection(Vector2 moveInput)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
            Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up);

            if (camForward.sqrMagnitude < 0.001f || camRight.sqrMagnitude < 0.001f)
            {
                camForward = Quaternion.Euler(0f, _lastCameraYRotation, 0f) * Vector3.forward;
                camRight = Quaternion.Euler(0f, _lastCameraYRotation, 0f) * Vector3.right;
            }
            else
            {
                _lastCameraYRotation = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
                camForward.Normalize();
                camRight.Normalize();
            }

            return camRight * moveInput.x + camForward * moveInput.y;
        }

        private float CalculateSpeed(Vector2 moveInput, bool isSprinting)
        {
            bool canSprint = isSprinting && moveInput.y > 0.1f && _settings.CanSprint;
            return canSprint ? _settings.SprintSpeed : _settings.MoveSpeed;
        }

        private float GetSmoothingRate(Vector3 targetVelocity, bool isGrounded)
        {
            float rate = targetVelocity.sqrMagnitude > 0.01f ? _settings.Acceleration : _settings.Deceleration;
            if (!isGrounded) rate *= _settings.AirControlMultiplier;
            return rate;
        }

        private void HandleRotation(Vector3 inputDirection)
        {
            if (_settings.RotateWithMovement)
            {
                if (inputDirection.sqrMagnitude >= 0.01f)
                {
                    float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _rotationVelocity, _settings.MovementRotationSmoothTime);
                    _transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
            }
            else
            {
                float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, _cameraTransform.eulerAngles.y, ref _rotationVelocity, _settings.StrafeRotationSmoothTime);
                _transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }
    }
}