using System;
using SAWC.Core.Data;
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
        private Collider[] _obstacleBuffer = new Collider[8];

        internal CharacterPosture(CharacterController controller, Transform transform, ref CharacterSettingsData initialSettings)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));

            SetHeight(initialSettings.Crouch.StandingHeight);
        }

        internal bool CheckCrouchState(bool crouchInput, bool isCurrentlyCrouching, bool canStandUp, ref CharacterSettingsData settings)
        {
            if (isCurrentlyCrouching && !canStandUp)
            {
                return true;
            }

            return settings.Crouch.CanCrouch && crouchInput;
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
            float heightDifference = height - _controller.height;
            Vector3 previousCenter = _controller.center;

            _controller.height = height;
            _controller.center = new Vector3(
                previousCenter.x,
                previousCenter.y + heightDifference * 0.5f,
                previousCenter.z
            );
        }

        internal bool CanStandUp(ref CharacterSettingsData settings)
        {
            float distanceToStand = (settings.Crouch.StandingHeight - _controller.height)
                * Mathf.Abs(_transform.lossyScale.y);

            if (distanceToStand <= 0f) return true;

            return !HasCeilingObstacle(distanceToStand, ref settings);
        }

        private bool HasCeilingObstacle(float distanceToStand, ref CharacterSettingsData settings)
        {
            Vector3 scale = _transform.lossyScale;
            float worldRadius = _controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float radius = worldRadius * StandUpRadiusBias;
            float worldHeight = Mathf.Max(_controller.height * Mathf.Abs(scale.y), worldRadius * 2f);
            Vector3 currentCenterWorld = _transform.TransformPoint(_controller.center);

            Vector3 rayStart = currentCenterWorld + Vector3.up * (worldHeight * 0.5f - radius);
            float castDistance = distanceToStand + StandUpClearance;

            if (radius <= 0f) return false;

            // The swept volume also includes initial overlaps, unlike SphereCast.
            int count;
            while (true)
            {
                count = Physics.OverlapCapsuleNonAlloc(
                    rayStart, rayStart + Vector3.up * castDistance, radius, _obstacleBuffer,
                    settings.Crouch.EnvironmentMask, QueryTriggerInteraction.Ignore);

                if (count < _obstacleBuffer.Length) break;
                Array.Resize(ref _obstacleBuffer, _obstacleBuffer.Length * 2);
            }

            bool hasObstacle = false;
            for (int i = 0; i < count; i++)
            {
                Collider obstacle = _obstacleBuffer[i];
                _obstacleBuffer[i] = null;
                if (obstacle != null && !obstacle.transform.IsChildOf(_transform))
                    hasObstacle = true;
            }

            return hasObstacle;
        }
    }
}
