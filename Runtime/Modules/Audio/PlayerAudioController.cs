using UnityEngine;
using System;
using System.Collections.Generic;
using SAWC.Core;
using SAWC.Localization;

namespace SAWC.Modules.Audio
{
    [AddComponentMenu("SAWC/Modules/Player Audio")]
    public class PlayerAudioController : MonoBehaviour
    {
        public enum PlaybackMode { Sequential, Shuffle, Random }

        [Serializable]
        public class AudioContainerSettings
        {
            [Loc] public List<AudioClip> Clips = new List<AudioClip>();
            [Loc] public PlaybackMode Playback = PlaybackMode.Random;

            [Space(5)]
            [Min(0.01f)] public float Interval = 0.4f;

            [Space(5)]
            [Range(0f, 1f),Loc]   public float BaseVolume = 1f;
            [Range(0f, 0.5f),Loc] public float VolumeRandomization = 0.05f;

            [Space(5)]
            [Range(0.1f, 3f),Loc] public float BasePitch = 1f;
            [Range(0f, 0.5f),Loc] public float PitchRandomization = 0.08f;

            [Space(5)]
            [Min(0),Loc] public int AvoidRepeatingLast = 1;

            private int _lastIndex = -1;

            private readonly HashSet<int> _history = new HashSet<int>();
            private readonly List<int> _historyOrder = new List<int>();
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

            public void ResetState()
            {
                _lastIndex = -1;
                _history.Clear();
                _historyOrder.Clear();
                _shufflePool.Clear();
                _validIndicesBuffer.Clear();
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
                            _historyOrder.Clear();
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
                    _historyOrder.Clear();
                    return;
                }

                if (!_history.Contains(idx))
                {
                    _history.Add(idx);
                    _historyOrder.Add(idx);
                }

                while (_historyOrder.Count > allowedHistorySize)
                {
                    int oldest = _historyOrder[0];
                    _historyOrder.RemoveAt(0);
                    _history.Remove(oldest);
                }
            }
        }

        [SerializeField, Loc] private SAWController _controller;

        [Space(5)]
        [SerializeField, Loc] private AudioSource _stepSource;
        [SerializeField, Loc] private AudioSource _actionSource;

        [Space(5)]
        [SerializeField,Loc] private AudioContainerSettings _walkStepSettings;
        [SerializeField,Loc] private AudioContainerSettings _sprintStepSettings;
        [SerializeField,Loc] private AudioContainerSettings _crouchStepSettings;

        [Space(5)]
        [SerializeField,Loc] private AudioContainerSettings _jumpSettings;
        [SerializeField,Loc] private AudioContainerSettings _landSettings;
        [SerializeField,Loc] private AudioContainerSettings _crouchDownSettings;
        [SerializeField,Loc] private AudioContainerSettings _crouchUpSettings;

        [Space(5)]
        [SerializeField, Loc, Range(0f, 1f)] private float _antiSpamFactor = 0.75f;
        [SerializeField, Loc, Range(0f, 1f)] private float _actionCooldown = 0.1f;

        private float _stepTimer;
        private float _antiSpamCooldown;
        private float _actionCooldownTimer;
        private bool _wasMovingLastFrame;

        private void Awake()
        {
            if (_controller == null)
                Debug.LogError($"[{nameof(PlayerAudioController)}] Controller reference is null on '{gameObject.name}'!", this);

            if (_stepSource == null)
                Debug.LogError($"[{nameof(PlayerAudioController)}] Step AudioSource is not assigned on '{gameObject.name}'!", this);

            if (_actionSource == null)
                Debug.LogError($"[{nameof(PlayerAudioController)}] Action AudioSource is not assigned on '{gameObject.name}'!", this);

            _walkStepSettings?.ResetState();
            _sprintStepSettings?.ResetState();
            _crouchStepSettings?.ResetState();
            _jumpSettings?.ResetState();
            _landSettings?.ResetState();
            _crouchDownSettings?.ResetState();
            _crouchUpSettings?.ResetState();
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

            if (_actionCooldownTimer > 0f)
            {
                _actionCooldownTimer -= Time.deltaTime;
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
            if (_controller.State.IsCrouching && _crouchStepSettings != null)
                return _crouchStepSettings;

            if (_controller.State.IsSprinting && _sprintStepSettings != null)
                return _sprintStepSettings;

            return _walkStepSettings;
        }

        private void OnJumpPerformed()
        {
            if (_actionCooldownTimer <= 0f && _jumpSettings != null)
            {
                _jumpSettings.Play(_actionSource);
                _actionCooldownTimer = _actionCooldown;
            }
        }

        private void OnLandPerformed()
        {
            if (_actionCooldownTimer <= 0f && _landSettings != null)
            {
                _landSettings.Play(_actionSource);
                _actionCooldownTimer = _actionCooldown;
            }
        }

        private void OnCrouchStarted()
        {
            if (_actionCooldownTimer <= 0f && _crouchDownSettings != null)
            {
                _crouchDownSettings.Play(_actionSource);
                _actionCooldownTimer = _actionCooldown;
            }
        }

        private void OnCrouchCanceled()
        {
            if (_actionCooldownTimer <= 0f && _crouchUpSettings != null)
            {
                _crouchUpSettings.Play(_actionSource);
                _actionCooldownTimer = _actionCooldown;
            }
        }
    }
}