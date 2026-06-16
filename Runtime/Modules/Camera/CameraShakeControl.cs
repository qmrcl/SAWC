using Unity.Cinemachine;
using UnityEngine;
using SAWC.Core;
using SAWC.Localization;

namespace SAWC.Modules.Camera
{
    [AddComponentMenu("SAWC/Modules/Camera Shake")]
    public class CameraShakeControl : MonoBehaviour
    {
        [SerializeField, Loc] private SAWController _controller;
        [SerializeField, Loc] private CinemachineBrain _brain;

        [Space(5)]
        [SerializeField, Loc] private float _smoothTime = 0.2f;
        [SerializeField, Loc] private float _maxShake = 5f;
        [SerializeField, Loc] private float _sprintFrequency = 3.5f;

        private CinemachineBasicMultiChannelPerlin _noise;
        private CinemachineBrainEvents _brainEvents;

        private float _currentAmplitude;
        private float _currentFrequency;
        private float _defaultFrequency;

        private float _ampVelocity;
        private float _freqVelocity;

        private void Awake()
        {
            if (_brain == null) return;
            _brain.TryGetComponent(out _brainEvents);
        }

        private void OnEnable()
        {
            _brainEvents?.CameraActivatedEvent.AddListener(OnCameraChanged);
            if (_brain != null) UpdateNoiseReference(_brain.ActiveVirtualCamera);
        }

        private void OnDisable()
        {
            _brainEvents?.CameraActivatedEvent.RemoveListener(OnCameraChanged);
        }

        private void Update()
        {
            if (_noise == null) return;

            bool onGround = _controller.State.IsGrounded;
    
            bool canShake = _controller.State.IsMoving && onGround;

            float targetAmplitude = canShake ? _maxShake : 0f;
    
            float targetFrequency = (canShake && _controller.State.IsSprinting) 
                ? _sprintFrequency 
                : _defaultFrequency;

            _currentAmplitude = Mathf.SmoothDamp(_currentAmplitude, targetAmplitude, ref _ampVelocity, _smoothTime);
            _currentFrequency = Mathf.SmoothDamp(_currentFrequency, targetFrequency, ref _freqVelocity, _smoothTime);

            _noise.AmplitudeGain = _currentAmplitude;
            _noise.FrequencyGain = _currentFrequency;
        }

        private void OnCameraChanged(ICinemachineMixer mixer, ICinemachineCamera newCam)
        {
            UpdateNoiseReference(newCam);
        }

        private void UpdateNoiseReference(ICinemachineCamera cam)
        {
            _noise = (cam as MonoBehaviour)?.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (_noise != null)
            {
                _defaultFrequency = _noise.FrequencyGain;
                _ampVelocity = 0f;
                _freqVelocity = 0f;

                _currentFrequency = _defaultFrequency;
            }
        }
    }
}