using UnityEngine;
using System;

namespace SAWC.Core
{
    internal sealed class CharacterPosture
    {
        private const float StandUpRadiusBias = 0.9f;
        private const float HeightSnapThreshold = 0.001f;

        private readonly CharacterSettings _settings;
        private readonly CharacterController _controller;
        private readonly Transform _transform;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

        private float _heightVelocity;

        internal CharacterPosture(CharacterSettings settings, CharacterController controller, Transform transform)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));

            SetHeight(_settings.StandingHeight);
        }

        internal bool CheckCrouchState(bool crouchInput)
        {
            if (!_settings.CanCrouch) return false;
            return crouchInput || !CanStandUp();
        }

        internal void Tick(bool isCrouching)
        {
            float targetHeight = isCrouching ? _settings.CrouchHeight : _settings.StandingHeight;

            if (Mathf.Abs(_controller.height - targetHeight) > HeightSnapThreshold)
            {
                SetHeight(Mathf.SmoothDamp(_controller.height, targetHeight, ref _heightVelocity, _settings.CrouchSmoothTime));
            }
        }

        private void SetHeight(float height)
        {
            float previousHeight = _controller.height;
            Vector3 previousCenter = _controller.center;
            float heightDifference = height - previousHeight;

            _controller.height = height;
            _controller.center = new Vector3(
                previousCenter.x,
                previousCenter.y + heightDifference * 0.5f,
                previousCenter.z
            );
        }

        private bool CanStandUp()
        {
            float distance = _settings.StandingHeight - _controller.height;
            if (distance <= 0f) return true;

            float radius = _controller.radius * StandUpRadiusBias;
            Vector3 rayStart = _transform.position + Vector3.up * _controller.height;

            int hitCount = Physics.SphereCastNonAlloc(
                rayStart, radius, Vector3.up, _hitBuffer, distance,
                _settings.EnvironmentMask, QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < hitCount; i++)
            {
                Transform hitTransform = _hitBuffer[i].transform;
                if (hitTransform != _transform && !hitTransform.IsChildOf(_transform))
                    return false;
            }

            return true;
        }
    }
}