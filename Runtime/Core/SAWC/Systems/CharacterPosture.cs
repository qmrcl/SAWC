using SAWC.Core.Data;
using System;
using UnityEngine;

namespace SAWC.Core
{
    internal sealed class CharacterPosture
    {
        private const float StandUpRadiusBias = 0.9f;
        private const float HeightSnapThreshold = 0.01f;
        private const float StandUpClearance = 0.05f;

        private readonly CharacterController _controller;
        private readonly Transform _transform;

        internal CharacterPosture(CharacterController controller, Transform transform, ref CharacterSettingsData initialSettings)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));

            SetHeight(initialSettings.Crouch.StandingHeight);
        }

        internal bool CheckCrouchState(bool crouchInput, bool isCurrentlyCrouching, bool canStandUp, ref CharacterSettingsData settings)
        {
            if (!settings.Crouch.CanCrouch) return false;

            if (isCurrentlyCrouching && !crouchInput)
            {
                return !canStandUp;
            }

            return crouchInput;
        }

        internal void Tick(bool isCrouching, ref CharacterSettingsData settings)
        {
            float targetHeight = isCrouching ? settings.Crouch.CrouchHeight : settings.Crouch.StandingHeight;

            if (Mathf.Abs(_controller.height - targetHeight) > HeightSnapThreshold)
            {
                SetHeight(targetHeight);
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

            _controller.Move(Vector3.zero);
        }

        internal bool CanStandUp(ref CharacterSettingsData settings)
        {
            float distanceToStand = settings.Crouch.StandingHeight - _controller.height;

            if (distanceToStand <= StandUpClearance) return true;

            return !HasCeilingObstacle(distanceToStand, ref settings);
        }

        private bool HasCeilingObstacle(float distanceToStand, ref CharacterSettingsData settings)
        {
            float radius = _controller.radius * StandUpRadiusBias;
            Vector3 currentCenterWorld = _transform.position + _controller.center;
            Vector3 rayStart = currentCenterWorld + Vector3.up * (_controller.height * 0.5f - radius);

            float castDistance = distanceToStand - StandUpClearance;

            if (castDistance <= 0f) return false;

            return Physics.SphereCast(
                rayStart, radius, Vector3.up, out _, castDistance,
                settings.Crouch.EnvironmentMask, QueryTriggerInteraction.Ignore
            );
        }
    }
}