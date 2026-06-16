using SAWC.Core;
using SAWC.Localization;
using Unity.Cinemachine;
using UnityEngine;

namespace SAWC.Modules.CameraUtils
{
    [AddComponentMenu("SAWC/Modules/Motion Dynamics")]
    public class CameraMotionDynamics : CinemachineExtension
    {
        [SerializeField, Loc] private SAWController _controller;

        [Space(5)]
        [SerializeField, Loc] private float _tiltAngle = 2.5f;
        [SerializeField, Loc] private float _tiltSmoothTime = 0.15f;
        [SerializeField, Loc] private float _panAngle = 5f;
        [SerializeField, Loc] private float _panSmoothTime = 0.2f;
        [SerializeField, Loc] private float _maxStrafeSpeed = 5f;

        [Space(5)]
        [SerializeField, Loc] private float _forwardPitchAngle = 3f;
        [SerializeField, Loc] private float _forwardSmoothTime = 0.15f;
        [SerializeField, Loc] private float _maxForwardSpeed = 5f;

        [Space(5)]
        [SerializeField, Loc] private float _verticalPitchAngle = 5f;
        [SerializeField, Loc] private float _verticalSmoothTime = 0.15f;
        [SerializeField, Loc] private float _maxVerticalSpeed = 10f;

        private struct SmoothedAngle
        {
            public float Value;
            private float _velocity;

            public void MoveTo(float target, float smoothTime)
                => Value = Mathf.SmoothDamp(Value, target, ref _velocity, smoothTime);
        }

        private SmoothedAngle _tilt, _pan, _forwardPitch, _verticalPitch;

        private float Norm(float speed, float max)
            => Mathf.Clamp(speed / max, -1f, 1f);

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Aim) return;
            if (_controller == null || _controller.State == null) return;

            UpdateAngles();
            ApplyRotation(ref state);
        }

        private void UpdateAngles()
        {
            Vector3 local = _controller.transform.InverseTransformDirection(_controller.State.Velocity);
            bool grounded = _controller.State.IsGrounded;

            float side = grounded ? Norm(local.x, _maxStrafeSpeed) : 0f;
            float fwd = grounded ? Norm(local.z, _maxForwardSpeed) : 0f;
            float vert = grounded ? 0f : Norm(local.y, _maxVerticalSpeed);

            _tilt.MoveTo(-side * _tiltAngle, _tiltSmoothTime);
            _pan.MoveTo(side * _panAngle, _panSmoothTime);
            _forwardPitch.MoveTo(fwd * _forwardPitchAngle, _forwardSmoothTime);
            _verticalPitch.MoveTo(-vert * _verticalPitchAngle, _verticalSmoothTime);
        }

        private void ApplyRotation(ref CameraState state)
        {
            float pitch = _forwardPitch.Value + _verticalPitch.Value;
            state.RawOrientation *= Quaternion.Euler(pitch, _pan.Value, _tilt.Value);
        }
    }
}