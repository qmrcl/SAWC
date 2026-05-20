using SAWC.Input;
using UnityEngine;
using SAWC.Pipeline;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Core/SAW Controller")]
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    [SelectionBase]
    public class SAWController : MonoBehaviour
    {
        public ICharacterState State => _stateTracker;
        public CharacterPipeline Pipeline { get; } = new CharacterPipeline();

        [SerializeField] private CharacterSettings _settings;

        private CharacterController _controller;
        private CharacterLocomotion _locomotion;
        private CharacterGravity _gravity;
        private CharacterPosture _posture;
        private CharacterStateTracker _stateTracker = new CharacterStateTracker();

        private IInputProvider _input;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.minMoveDistance = 0f;
            _input = GetComponent<IInputProvider>();

            if (_settings == null)
            {
                Debug.LogError($"[SAWController] На {gameObject.name} не назначены Settings");
                enabled = false;
                return;
            }

            _locomotion = new CharacterLocomotion(_settings, transform);
            _gravity = new CharacterGravity(_settings);
            _posture = new CharacterPosture(_settings, _controller, transform);
        }

        private void Start()
        {
            if (_input == null)
            {
                Debug.LogError($"[SAWController] Нет Input Provider.");
                enabled = false;
                return;
            }
            _controller.Move(Vector3.zero);
            _stateTracker.Initialize(_controller.isGrounded, _settings);
        }

        private void OnEnable()
        {
            if (_input == null) return;
            _input.JumpStarted += OnJumpStarted;
            _input.JumpCanceled += OnJumpCanceled;
        }

        private void OnDisable()
        {
            if (_input == null) return;
            _input.JumpStarted -= OnJumpStarted;
            _input.JumpCanceled -= OnJumpCanceled;
        }

        private void Update()
        {
            var context = new FrameContext
            {
                MoveInput = _input.MoveInput,
                WorldMoveDirection = _input.WorldMoveDirection,
                WorldLookDirection = _input.WorldLookDirection,
                IsGrounded = _controller.isGrounded,
                SprintInput = _input.SprintHeld,
                CrouchInput = _posture.CheckCrouchState(_input.CrouchHeld),
                DeltaTime = Time.deltaTime
            };

            Pipeline.ProcessContext(ref context);

            _locomotion.Tick(ref context);
            _gravity.Tick(ref context);

            Vector3 finalMovement = _locomotion.CurrentHorizontalVelocity + Vector3.up * _gravity.VerticalVelocity;

            finalMovement = Pipeline.ProcessVelocity(finalMovement, ref context);

            _controller.Move(finalMovement * context.DeltaTime);

            _stateTracker.Tick(ref context, finalMovement, _locomotion.IsSprintingActive);
            _posture.Tick(_stateTracker.IsCrouching);
        }

        private void OnJumpStarted() => _gravity.SetJumpHeld(true);
        private void OnJumpCanceled() => _gravity.SetJumpHeld(false);
    }
}