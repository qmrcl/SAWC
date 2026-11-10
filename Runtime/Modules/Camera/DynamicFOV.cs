using UnityEngine;
using Unity.Cinemachine;
using SAWC.Core;

namespace SAWC.Modules.CameraUtils
{
    [AddComponentMenu("SAWC/Modules/Camera/Dynamic FOV")]
    public class DynamicFOV : MonoBehaviour
    {
        [SerializeField] private SAWController _controller;
        [SerializeField] private CinemachineCamera _cinemachineCam;

        [Space(5)]
        [SerializeField] private float _speedMultiplier = 1.5f;
        [SerializeField] private float _maxFovOffset = 15f;
        [SerializeField] private float _smoothTime = 0.15f;

        private float _baseFov;
        private float _currentFov;
        private float _fovVelocity;
        private bool _hasBaseFov;
        
        private void OnDisable()
        {
            if (!_hasBaseFov || _cinemachineCam == null) return;
        
            _currentFov = _baseFov;
            _fovVelocity = 0f;
        
            var lens = _cinemachineCam.Lens;
            lens.FieldOfView = _baseFov;
            _cinemachineCam.Lens = lens;
        }

        private void Awake()
        {
            if (_cinemachineCam == null)
                TryGetComponent(out _cinemachineCam);

            if (_controller == null || _cinemachineCam == null)
            {
                Debug.LogError($"Required references are null or missing on '{gameObject.name}'!", this);
                enabled = false;
                return;
            }

            _baseFov = _cinemachineCam.Lens.FieldOfView;
            _currentFov = _baseFov;
            _hasBaseFov = true;
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            if (!_hasBaseFov || _cinemachineCam == null || _controller == null || _controller.State == null) return;

            Vector3 localVel = _controller.transform.InverseTransformDirection(_controller.State.Velocity);
            float targetFov = _baseFov;

            if (localVel.z > 0.1f && _controller.State.IsMoving && _controller.State.IsGrounded)
            {
                float fovOffset = localVel.z * _speedMultiplier;
                fovOffset = Mathf.Clamp(fovOffset, 0f, _maxFovOffset);
                targetFov += fovOffset;
            }

            _currentFov = Mathf.SmoothDamp(_currentFov, targetFov, ref _fovVelocity, _smoothTime);

            var lens = _cinemachineCam.Lens;
            if (Mathf.Abs(lens.FieldOfView - _currentFov) > 0.01f)
            {
                lens.FieldOfView = _currentFov;
                _cinemachineCam.Lens = lens;
            }
        }
        
        
    }
}
