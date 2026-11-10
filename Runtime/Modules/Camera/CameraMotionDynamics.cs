using SAWC.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace SAWC.Modules.CameraUtils
{
    [AddComponentMenu("SAWC/Modules/Camera/Motion Dynamics")]
    public class CameraMotionDynamics : CinemachineExtension
    {
        [SerializeField] private SAWController _controller;

        [Space(5)]
        [SerializeField] private float _tiltAngle = 2.5f;
        [SerializeField] private float _tiltSmoothTime = 0.15f;
        [SerializeField] private float _panAngle = 5f;
        [SerializeField] private float _panSmoothTime = 0.2f;
        [SerializeField] private float _maxStrafeSpeed = 5f;

        [Space(5)]
        [SerializeField] private float _forwardPitchAngle = 3f;
        [SerializeField] private float _forwardSmoothTime = 0.15f;
        [SerializeField] private float _maxForwardSpeed = 5f;

        [Space(5)]
        [SerializeField] private float _verticalPitchAngle = 5f;
        [SerializeField] private float _verticalSmoothTime = 0.15f;
        [SerializeField] private float _maxVerticalSpeed = 10f;

        private struct SmoothedAngle
        {
            public float Value;
            private float _velocity;

            public void MoveTo(float target, float smoothTime, float deltaTime)
            {
                if (deltaTime <= 0f)
                {
                    Value = target;
                    _velocity = 0f;
                    return;
                }
                Value = Mathf.SmoothDamp(Value, target, ref _velocity, smoothTime, Mathf.Infinity, deltaTime);
            }
        }

        private SmoothedAngle _tilt, _pan, _forwardPitch, _verticalPitch;

        private static float Norm(float speed, float max)
            => max > 0.001f ? Mathf.Clamp(speed / max, -1f, 1f) : 0f;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Aim) return;
            if (_controller == null || _controller.State == null) return;

            UpdateAngles(deltaTime);
            ApplyRotation(ref state);
        }

        private void UpdateAngles(float deltaTime)
        {
            Vector3 local = _controller.transform.InverseTransformDirection(_controller.State.Velocity);
            bool grounded = _controller.State.IsGrounded;

            float side = grounded ? Norm(local.x, _maxStrafeSpeed) : 0f;
            float fwd = grounded ? Norm(local.z, _maxForwardSpeed) : 0f;
            float vert = grounded ? 0f : Norm(local.y, _maxVerticalSpeed);

            _tilt.MoveTo(-side * _tiltAngle, _tiltSmoothTime, deltaTime);
            _pan.MoveTo(side * _panAngle, _panSmoothTime, deltaTime);
            _forwardPitch.MoveTo(fwd * _forwardPitchAngle, _forwardSmoothTime, deltaTime);
            _verticalPitch.MoveTo(-vert * _verticalPitchAngle, _verticalSmoothTime, deltaTime);
        }

        private void ApplyRotation(ref CameraState state)
        {
            float pitch = _forwardPitch.Value + _verticalPitch.Value;
            state.RawOrientation *= Quaternion.Euler(pitch, _pan.Value, _tilt.Value);
        }
    }
}