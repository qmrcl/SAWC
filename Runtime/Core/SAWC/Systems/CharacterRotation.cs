using UnityEngine;
using SAWC.Core.Data;

namespace SAWC.Core
{
    internal sealed class CharacterRotation
    {
        private readonly Transform _transform;
        private float _rotationVelocity;

        internal CharacterRotation(Transform transform)
        {
            _transform = transform ?? throw new System.ArgumentNullException(nameof(transform));
        }

        internal void Tick(ref FrameContext ctx)
        {
            Vector3 targetDirection;
            float smoothTime;

            if (ctx.Settings.Rotation.RotateWithMovement)
            {
                if (ctx.WorldMoveDirection.sqrMagnitude < ctx.Settings.Thresholds.InputThresholdSq) return;
                targetDirection = ctx.WorldMoveDirection;
                smoothTime = ctx.Settings.Rotation.MovementRotationSmoothTime;
            }
            else
            {
                if (ctx.WorldLookDirection.sqrMagnitude < 0.001f) return;
                targetDirection = ctx.WorldLookDirection;
                smoothTime = ctx.Settings.Rotation.StrafeRotationSmoothTime;
            }

            if (targetDirection.sqrMagnitude < 0.001f) return;

            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _rotationVelocity, smoothTime);
            _transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}