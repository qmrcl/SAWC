using System;
using UnityEngine;
using UnityEngine.Audio;
using SAWC.Core;

namespace SAWC.Modules.Audio
{
    [Serializable]
    public struct LocomotionAudio
    {
        public AudioResource Walk;
        public AudioResource Sprint;
        public AudioResource Crouch;
        public AudioResource Land;
    }

    [Serializable]
    public struct SurfaceAudioConfig
    {
        public string SurfaceTag;
        public LocomotionAudio Audio;
    }

    [AddComponentMenu("SAWC/Modules/Audio/Player Audio")]
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Core Dependencies")]
        [SerializeField] private SAWController _controller;
        [SerializeField] private AudioMixerGroup _mixerGroup;

        [Header("Step Timing")]
        [SerializeField, Min(0.01f)] private float _walkInterval = 0.4f;
        [SerializeField, Min(0.01f)] private float _sprintInterval = 0.25f;
        [SerializeField, Min(0.01f)] private float _crouchInterval = 0.5f;

        [Header("Default Audio")]
        [SerializeField] private LocomotionAudio _defaultLocomotion;
        [SerializeField] private AudioResource _jump;
        [SerializeField] private AudioResource _crouchDown;
        [SerializeField] private AudioResource _crouchUp;

        [Header("Surface Detection")]
        [SerializeField, Range(0f, 1f)] private float _rayOriginOffset = 0.2f;
        [SerializeField, Range(0.1f, 2f)] private float _raycastDistance = 0.5f;
        [SerializeField] private LayerMask _surfaceLayerMask = ~0;
        [SerializeField] private SurfaceAudioConfig[] _surfaces;

        private AudioSource _stepSource;
        private AudioSource _actionSource;
        private CharacterController _characterController;
        private float _stepTimer;
        private RaycastHit[] _surfaceHits = new RaycastHit[8];

        private void Awake()
        {
            if (_controller == null && !TryGetComponent(out _controller))
            {
                Debug.LogError($"[{nameof(PlayerAudioController)}] SAWController is missing on '{name}'!", this);
                enabled = false;
                return;
            }

            _characterController = _controller.GetComponent<CharacterController>();
            _stepSource = CreateSource();
            _actionSource = CreateSource();
        }

        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = _mixerGroup;
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            return source;
        }

        private void OnEnable()
        {
            if (_controller == null) return;
            _controller.State.JumpPerformed += OnJump;
            _controller.State.LandPerformed += OnLand;
            _controller.State.CrouchStarted += OnCrouchDown;
            _controller.State.CrouchCanceled += OnCrouchUp;
        }

        private void OnDisable()
        {
            if (_controller == null) return;
            _controller.State.JumpPerformed -= OnJump;
            _controller.State.LandPerformed -= OnLand;
            _controller.State.CrouchStarted -= OnCrouchDown;
            _controller.State.CrouchCanceled -= OnCrouchUp;
        }

        // Опрос состояния идет строго в LateUpdate после того, как контроллер закончил движение
        private void LateUpdate()
        {
            if (!_controller.State.IsMoving || !_controller.State.IsGrounded)
            {
                _stepTimer = Mathf.Max(_stepTimer, 0.15f);
                return;
            }

            float currentInterval = GetCurrentInterval();

            if (_stepTimer > currentInterval)
                _stepTimer = currentInterval;

            _stepTimer -= Time.deltaTime;

            if (_stepTimer <= 0f)
            {
                _stepTimer = currentInterval;

                LocomotionAudio surfaceAudio = GetCurrentLocomotionAudio();
                PlayResource(_stepSource, GetStepResource(in surfaceAudio));
            }
        }

        private void PlayResource(AudioSource source, AudioResource resource)
        {
            if (resource == null || source == null) return;

            if (source.resource != resource)
                source.resource = resource;

            source.Play();
        }

        private float GetCurrentInterval()
        {
            if (_controller.State.IsCrouching) return _crouchInterval;
            if (_controller.State.IsSprinting) return _sprintInterval;
            return _walkInterval;
        }

        private AudioResource GetStepResource(in LocomotionAudio audio)
        {
            if (_controller.State.IsCrouching)
                return audio.Crouch != null ? audio.Crouch : _defaultLocomotion.Crouch;

            if (_controller.State.IsSprinting)
                return audio.Sprint != null ? audio.Sprint : _defaultLocomotion.Sprint;

            return audio.Walk != null ? audio.Walk : _defaultLocomotion.Walk;
        }

        private LocomotionAudio GetCurrentLocomotionAudio()
        {
            if (_surfaces == null || _surfaces.Length == 0)
                return _defaultLocomotion;

            if (_characterController == null || !_characterController.enabled)
                return _defaultLocomotion;

            Bounds bounds = _characterController.bounds;
            Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + _rayOriginOffset, bounds.center.z);
            float distance = _rayOriginOffset + _raycastDistance;

            Collider surface = FindSurface(origin, distance);
            if (surface != null)
            {
                string surfaceTag = surface.tag;
                for (int i = 0; i < _surfaces.Length; i++)
                {
                    if (surfaceTag == _surfaces[i].SurfaceTag)
                        return _surfaces[i].Audio;
                }
            }

            return _defaultLocomotion;
        }

        private Collider FindSurface(Vector3 origin, float distance)
        {
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, _surfaceHits, distance,
                _surfaceLayerMask, QueryTriggerInteraction.Ignore);
            while (hitCount == _surfaceHits.Length)
            {
                // A full buffer can omit the nearest external collider.
                Array.Resize(ref _surfaceHits, _surfaceHits.Length * 2);
                hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, _surfaceHits, distance,
                    _surfaceLayerMask, QueryTriggerInteraction.Ignore);
            }

            Collider closest = null;
            float closestDistance = float.PositiveInfinity;
            Transform character = _characterController.transform;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _surfaceHits[i];
                if (hit.distance >= closestDistance || hit.collider.transform.IsChildOf(character))
                    continue;

                closest = hit.collider;
                closestDistance = hit.distance;
            }
            return closest;
        }

        private void OnJump() => PlayResource(_actionSource, _jump);
        private void OnCrouchDown() => PlayResource(_actionSource, _crouchDown);
        private void OnCrouchUp() => PlayResource(_actionSource, _crouchUp);

        private void OnLand()
        {
            LocomotionAudio audio = GetCurrentLocomotionAudio();
            PlayResource(_actionSource, audio.Land != null ? audio.Land : _defaultLocomotion.Land);
        }
    }
}