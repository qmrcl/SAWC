using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using SAWC.Core;

namespace SAWC.Modules.Camera
{
    [AddComponentMenu("SAWC/Modules/Camera/ Shake")]
    public class CameraShakeControl : MonoBehaviour
    {
        [SerializeField] private SAWController _controller;
        [SerializeField] private CinemachineBrain _brain;

        [Space(5)]
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private float _maxShake = 5f;
        [SerializeField] private float _sprintFrequency = 3.5f;

        private readonly Dictionary<CinemachineBasicMultiChannelPerlin, float> _initialFrequencies = new();
        private CinemachineBasicMultiChannelPerlin _noise;
        private CinemachineBrainEvents _brainEvents;

        private float _currentAmplitude;
        private float _currentFrequency;
        private float _defaultFrequency;

        private float _ampVelocity;
        private float _freqVelocity;

        private void Awake()
        {
            if (_brain != null)
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
            if (_noise != null)
            {
                _noise.AmplitudeGain = 0f;
                _noise.FrequencyGain = _defaultFrequency;
            }
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            if (_noise == null || _controller == null || _controller.State == null) return;

            bool canShake = _controller.State.IsMoving && _controller.State.IsGrounded;
            float targetAmplitude = canShake ? _maxShake : 0f;
            float targetFrequency = (canShake && _controller.State.IsSprinting) ? _sprintFrequency : _defaultFrequency;

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
            if (_noise != null)
            {
                _noise.AmplitudeGain = 0f;
                _noise.FrequencyGain = _defaultFrequency;
            }

            _noise = (cam as MonoBehaviour)?.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (_noise != null)
            {
                if (!_initialFrequencies.TryGetValue(_noise, out _defaultFrequency))
                {
                    _defaultFrequency = _noise.FrequencyGain;
                    _initialFrequencies[_noise] = _defaultFrequency;
                }

                _ampVelocity = 0f;
                _freqVelocity = 0f;
                _currentAmplitude = 0f;
                _currentFrequency = _defaultFrequency;
            }
        }
    }
}
