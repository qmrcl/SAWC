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
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[5];

        private float _heightVelocity;

        internal CharacterPosture(CharacterSettings settings, CharacterController controller, Transform transform)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings), "CharacterSettings cannot be null.");
            _controller = controller ?? throw new ArgumentNullException(nameof(controller), "CharacterController cannot be null.");
            _transform = transform ?? throw new ArgumentNullException(nameof(transform), "Player Transform cannot be null.");

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
            _controller.height = height;
            _controller.center = new Vector3(0f, height * 0.5f, 0f);
        }

        private bool CanStandUp()
        {
            float distance = _settings.StandingHeight - _controller.height;
            if (distance <= 0f) return true;

            float radius = _controller.radius * StandUpRadiusBias;
            Vector3 rayStart = _transform.position + Vector3.up * _controller.height;

            int hitCount = Physics.SphereCastNonAlloc(
                rayStart, radius, Vector3.up, _hitBuffer, distance,
                ~0, QueryTriggerInteraction.Ignore
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