using UnityEngine;
using System;
using System.Collections.Generic;
using SAWC.Core;

namespace SAWC.Modules.Audio
{
    [AddComponentMenu("SAWC/Modules/Player Audio")]
    public class PlayerAudioController : MonoBehaviour
    {
        public enum PlaybackMode { Sequential, Shuffle, Random }

        [Serializable]
        public class AudioContainerSettings
        {
            [Header("Audio Clips")]
            public List<AudioClip> Clips = new List<AudioClip>();
            public PlaybackMode Playback = PlaybackMode.Random;

            [Header("Pace Settings")]
            [Tooltip("Интервал между звуками в секундах (для шагов)")]
            [Min(0.01f)] public float Interval = 0.4f;

            [Header("Volume Settings")]
            [Range(0f, 1f)] public float BaseVolume = 1f;
            [Range(0f, 0.5f)] public float VolumeRandomization = 0.05f;

            [Header("Pitch Settings")]
            [Range(0.1f, 3f)] public float BasePitch = 1f;
            [Range(0f, 0.5f)] public float PitchRandomization = 0.08f;

            [Header("Rules")]
            [Tooltip("Сколько прошлых клипов не повторять.")]
            [Min(0)] public int AvoidRepeatingLast = 1;

            private int _lastIndex = -1;
            private readonly List<int> _history = new List<int>();
            private readonly List<int> _shufflePool = new List<int>();
            private readonly List<int> _validIndicesBuffer = new List<int>();

            public void Play(AudioSource source)
            {
                if (Clips == null || Clips.Count == 0 || source == null) return;

                int index = GetNextIndex();
                if (index < 0 || index >= Clips.Count) return;

                float finalVolume = Mathf.Clamp01(BaseVolume + UnityEngine.Random.Range(-VolumeRandomization, VolumeRandomization));
                float finalPitch = Mathf.Max(0.1f, BasePitch + UnityEngine.Random.Range(-PitchRandomization, PitchRandomization));

                source.pitch = finalPitch;
                source.PlayOneShot(Clips[index], finalVolume);
            }

            private int GetNextIndex()
            {
                if (Clips.Count == 1) return 0;
                int index = -1;

                switch (Playback)
                {
                    case PlaybackMode.Sequential:
                        _lastIndex = (_lastIndex + 1) % Clips.Count;
                        index = _lastIndex;
                        break;

                    case PlaybackMode.Shuffle:
                        if (_shufflePool.Count == 0) PopulateShufflePool();
                        int poolIndex = UnityEngine.Random.Range(0, _shufflePool.Count);
                        index = _shufflePool[poolIndex];
                        _shufflePool.RemoveAt(poolIndex);
                        break;

                    case PlaybackMode.Random:
                        _validIndicesBuffer.Clear();
                        for (int i = 0; i < Clips.Count; i++)
                        {
                            if (!_history.Contains(i)) _validIndicesBuffer.Add(i);
                        }

                        if (_validIndicesBuffer.Count == 0)
                        {
                            _history.Clear();
                            index = UnityEngine.Random.Range(0, Clips.Count);
                        }
                        else
                        {
                            index = _validIndicesBuffer[UnityEngine.Random.Range(0, _validIndicesBuffer.Count)];
                        }
                        break;
                }

                UpdateHistory(index);
                return index;
            }

            private void PopulateShufflePool()
            {
                _shufflePool.Clear();
                for (int i = 0; i < Clips.Count; i++) _shufflePool.Add(i);
            }

            private void UpdateHistory(int idx)
            {
                int maxHistory = Mathf.Max(0, Clips.Count - 1);
                int allowedHistorySize = Mathf.Min(AvoidRepeatingLast, maxHistory);

                if (allowedHistorySize <= 0)
                {
                    _history.Clear();
                    return;
                }

                _history.Add(idx);
                while (_history.Count > allowedHistorySize)
                {
                    _history.RemoveAt(0);
                }
            }
        }

        [Header("References")]
        [SerializeField] private SAWController _controller;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _stepSource;
        [SerializeField] private AudioSource _actionSource;

        [Header("Footstep Containers")]
        [SerializeField] private AudioContainerSettings _walkStepSettings;
        [SerializeField] private AudioContainerSettings _sprintStepSettings;
        [SerializeField] private AudioContainerSettings _crouchStepSettings;

        [Header("Action Containers")]
        [SerializeField] private AudioContainerSettings _jumpSettings;
        [SerializeField] private AudioContainerSettings _landSettings;
        [SerializeField] private AudioContainerSettings _crouchDownSettings;
        [SerializeField] private AudioContainerSettings _crouchUpSettings;

        [Header("Anti-Spam Settings")]
        [SerializeField, Range(0f, 1f)] private float _antiSpamFactor = 0.75f;

        private float _stepTimer;
        private float _antiSpamCooldown;
        private bool _wasMovingLastFrame;

        private void Awake()
        {
            if (_controller == null) Debug.LogError($"[{nameof(PlayerAudioController)}] Контроллер не найден.", this);
            if (_stepSource == null || _actionSource == null) Debug.LogError($"[{nameof(PlayerAudioController)}] Не назначено оба AudioSource!", this);
        }

        private void OnEnable()
        {
            if (_controller == null) return;
            _controller.State.JumpPerformed += OnJumpPerformed;
            _controller.State.LandPerformed += OnLandPerformed;
            _controller.State.CrouchStarted += OnCrouchStarted;
            _controller.State.CrouchCanceled += OnCrouchCanceled;
        }

        private void OnDisable()
        {
            if (_controller == null) return;
            _controller.State.JumpPerformed -= OnJumpPerformed;
            _controller.State.LandPerformed -= OnLandPerformed;
            _controller.State.CrouchStarted -= OnCrouchStarted;
            _controller.State.CrouchCanceled -= OnCrouchCanceled;
        }

        private void Update()
        {
            if (_controller == null || _stepSource == null) return;

            if (_antiSpamCooldown > 0f)
            {
                _antiSpamCooldown -= Time.deltaTime;
            }

            bool isMovingAndGrounded = _controller.State.IsMoving && _controller.State.IsGrounded;

            if (isMovingAndGrounded)
            {
                AudioContainerSettings currentStepSettings = GetCurrentStepSettings();
                _stepTimer += Time.deltaTime;

                if (_stepTimer >= currentStepSettings.Interval || (!_wasMovingLastFrame && _antiSpamCooldown <= 0f))
                {
                    currentStepSettings.Play(_stepSource);
                    _stepTimer = 0f;
                    _antiSpamCooldown = currentStepSettings.Interval * _antiSpamFactor;
                }
            }
            else
            {
                _stepTimer = 0f;
            }

            _wasMovingLastFrame = isMovingAndGrounded;
        }

        private AudioContainerSettings GetCurrentStepSettings()
        {
            if (_controller.State.IsCrouching) return _crouchStepSettings;
            if (_controller.State.IsSprinting) return _sprintStepSettings;
            return _walkStepSettings;
        }

        private void OnJumpPerformed() => _jumpSettings.Play(_actionSource);
        private void OnLandPerformed() => _landSettings.Play(_actionSource);
        private void OnCrouchStarted() => _crouchDownSettings.Play(_actionSource);
        private void OnCrouchCanceled() => _crouchUpSettings.Play(_actionSource);
    }
}