using UnityEngine;
using SAWC.Core; 

namespace SAWC.Modules.Audio
{
    [AddComponentMenu("SAWC/Modules/Player Audio")]
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Refrences")]
        [SerializeField] private SAWController _controller;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _step;
        [SerializeField] private AudioSource _jump;
        [SerializeField] private AudioSource _land;

        private void Awake()
        {
            if (_controller == null) Debug.LogError("SAWController не привязан", this);
            if (_step != null) _step.loop = true;
        }

        private void OnEnable() => SubscribeToEvents();
        private void OnDisable() => UnsubscribeFromEvents();

        private void SubscribeToEvents()
        {
            if (_controller == null) return;
            _controller.JumpPerformed += OnJumpPerformed;
            _controller.LandPerformed += OnLandPerformed;
        }

        private void UnsubscribeFromEvents()
        {
            if (_controller == null) return;
            _controller.JumpPerformed -= OnJumpPerformed;
            _controller.LandPerformed -= OnLandPerformed;
        }

        private void Update()
        {
            if (_controller == null || _step == null) return;

            if (_controller.IsMoving && _controller.IsGrounded)
            {
                if (!_step.isPlaying) PlayContainer(_step);
            }
            else
            {
                if (_step.isPlaying) _step.Stop();
            }
        }

        private void OnJumpPerformed()
        {
            PlayContainer(_jump);
        }

        private void OnLandPerformed()
        {
            PlayContainer(_land);
        }

        private void PlayContainer(AudioSource source)
        {
            if (source != null && source.enabled)
            {
                source.Play();
            }
        }
    }
}