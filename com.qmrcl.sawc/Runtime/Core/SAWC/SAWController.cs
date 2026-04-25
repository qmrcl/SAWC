using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private Vector2 _moveInput;
        private bool _isSprinting;

        public bool IsSprinting => _isSprinting;
        public bool IsMoving => _stateTracker?.IsMoving ?? false;
        public bool IsGrounded => _controller != null && _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.minMoveDistance = 0f;

            var cam = Camera.main?.transform;
            if (cam == null) return;

            _locomotion = new CharacterLocomotion(_settings, transform, cam);
            _gravity = new CharacterGravity(_settings);
            _stateTracker = new CharacterStateTracker();

            _gravity.JumpPerformed += () => JumpPerformed?.Invoke();
        }

        private void Start()
        {
            if (Camera.main == null)
            {
                Debug.LogError("Нет камеры с тегом MainCamera.", this);
                enabled = false;
                return;
            }

            if (_settings == null)
            {
                Debug.LogError("CharacterSettings не назначен.", this);
                enabled = false;
                return;
            }

            _stateTracker.Initialize(_controller.isGrounded);
            _stateTracker.LandPerformed += () => LandPerformed?.Invoke();
            _stateTracker.StartMoving += () => StartMoving?.Invoke();
            _stateTracker.StopMoving += () => StopMoving?.Invoke();

            InputReader.Actions.Player.Move.performed += OnMove;
            InputReader.Actions.Player.Move.canceled += OnMove;
            InputReader.Actions.Player.Jump.started += OnJump;
            InputReader.Actions.Player.Jump.canceled += OnJump;
            InputReader.Actions.Player.Sprint.started += OnSprint;
            InputReader.Actions.Player.Sprint.canceled += OnSprint;
        }

        private void OnDestroy()
        {
            InputReader.Actions.Player.Move.performed -= OnMove;
            InputReader.Actions.Player.Move.canceled -= OnMove;
            InputReader.Actions.Player.Jump.started -= OnJump;
            InputReader.Actions.Player.Jump.canceled -= OnJump;
            InputReader.Actions.Player.Sprint.started -= OnSprint;
            InputReader.Actions.Player.Sprint.canceled -= OnSprint;
        }

        private void Update()
        {
            bool grounded = _controller.isGrounded;

            _locomotion.Tick(_moveInput, _isSprinting, grounded, Time.deltaTime);
            _gravity.Tick(grounded, Time.deltaTime);

            Vector3 finalMovement = _locomotion.CurrentHorizontalVelocity + new Vector3(0f, _gravity.VerticalVelocity, 0f);

            if (_controller.enabled)
                _controller.Move(finalMovement * Time.deltaTime);

            _stateTracker.Tick(_controller.isGrounded, _locomotion.CurrentHorizontalVelocity);
        }

        private void OnMove(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started) _gravity.SetJumpHeld(true);
            if (ctx.canceled) _gravity.SetJumpHeld(false);
        }
        private void OnSprint(InputAction.CallbackContext ctx)
        {
            if (ctx.started) { _isSprinting = true; SprintStarted?.Invoke(); }
            if (ctx.canceled) { _isSprinting = false; SprintCanceled?.Invoke(); }
        }
    }
}