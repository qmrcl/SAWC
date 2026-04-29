using System;
using UnityEngine;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Core/SAW Controller")]
    [RequireComponent(typeof(CharacterController))]
    public class SAWController : MonoBehaviour
    {
        [SerializeField] private CharacterSettings _settings;
        
        public event Action JumpPerformed;
        public event Action LandPerformed;
        public event Action StartMoving;
        public event Action StopMoving;
        public event Action SprintStarted;
        public event Action SprintCanceled;

        private CharacterController _controller;
        private CharacterLocomotion _locomotion;
        private CharacterGravity _gravity;
        private CharacterStateTracker _stateTracker;
        private IInputProvider _input;

        private bool _isSprinting;

        public bool IsSprinting => _isSprinting;
        public bool IsMoving => _stateTracker?.IsMoving ?? false;
        public bool IsGrounded => _controller != null && _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.minMoveDistance = 0f;
            
            _input = GetComponent<IInputProvider>();

            var cam = Camera.main?.transform;
            if (cam == null || _settings == null) return;

            _locomotion = new CharacterLocomotion(_settings, transform, cam);
            _gravity = new CharacterGravity(_settings);
            _stateTracker = new CharacterStateTracker();

            _gravity.JumpPerformed += () => JumpPerformed?.Invoke();
        }

        private void Start()
        {
            if (_input == null)
            {
                Debug.LogError("IInputProvider not found on GameObject!", this);
                enabled = false;
                return;
            }

            _stateTracker.Initialize(_controller.isGrounded);
            _stateTracker.LandPerformed += () => LandPerformed?.Invoke();
            _stateTracker.StartMoving += () => StartMoving?.Invoke();
            _stateTracker.StopMoving += () => StopMoving?.Invoke();

            _input.JumpStarted += OnJumpStarted;
            _input.JumpCanceled += OnJumpCanceled;
            _input.SprintStarted += OnSprintStarted;
            _input.SprintCanceled += OnSprintCanceled;
        }

        private void OnDestroy()
        {
            if (_input == null) return;
            _input.JumpStarted -= OnJumpStarted;
            _input.JumpCanceled -= OnJumpCanceled;
            _input.SprintStarted -= OnSprintStarted;
            _input.SprintCanceled -= OnSprintCanceled;
        }

        private void Update()
        {
            bool grounded = _controller.isGrounded;

            _locomotion.Tick(_input.MoveInput, _isSprinting, grounded, Time.deltaTime);
            _gravity.Tick(grounded, Time.deltaTime);

            Vector3 finalMovement = _locomotion.CurrentHorizontalVelocity + new Vector3(0f, _gravity.VerticalVelocity, 0f);

            if (_controller.enabled)
                _controller.Move(finalMovement * Time.deltaTime);

            _stateTracker.Tick(_controller.isGrounded, _locomotion.CurrentHorizontalVelocity);
        }

        private void OnJumpStarted() => _gravity.SetJumpHeld(true);
        private void OnJumpCanceled() => _gravity.SetJumpHeld(false);
        
        private void OnSprintStarted() 
        { 
            _isSprinting = true; 
            SprintStarted?.Invoke(); 
        }
        
        private void OnSprintCanceled() 
        { 
            _isSprinting = false; 
            SprintCanceled?.Invoke(); 
        }
    }
}