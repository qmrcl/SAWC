using UnityEngine;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Core/SAW Controller")]
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class SAWController : MonoBehaviour
    {
        public ICharacterState State => _stateTracker;

        [SerializeField] private CharacterSettings _settings;

        private CharacterController _controller;
        private CharacterLocomotion _locomotion;
        private CharacterGravity _gravity;
        private CharacterPosture _posture;
        private CharacterStateTracker _stateTracker = new CharacterStateTracker();
        
        private IInputProvider _input;

        private bool _sprintInput;
        private bool _crouchInput;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.minMoveDistance = 0f;
            _input = GetComponent<IInputProvider>();

            var cam = Camera.main?.transform;
            if (cam == null || _settings == null) return;

            _locomotion = new CharacterLocomotion(_settings, transform, cam);
            _gravity = new CharacterGravity(_settings);
            _posture = new CharacterPosture(_settings, _controller, transform);
        }

        private void Start()
        {
            if (_input == null)
            {
                Debug.LogError("SAWController: IInputProvider not found on GameObject!", this);
                enabled = false;
                return;
            }

            _stateTracker.Initialize(_controller.isGrounded);
        }

        private void OnEnable()
        {
            if (_input == null) return;

            _input.JumpStarted    += OnJumpStarted;
            _input.JumpCanceled   += OnJumpCanceled;
            _input.SprintStarted  += OnSprintStarted;
            _input.SprintCanceled += OnSprintCanceled;
            _input.CrouchStarted  += OnCrouchStarted;
            _input.CrouchCanceled += OnCrouchCanceled;
        }

        private void OnDisable()
        {
            if (_input == null) return;

            _input.JumpStarted    -= OnJumpStarted;
            _input.JumpCanceled   -= OnJumpCanceled;
            _input.SprintStarted  -= OnSprintStarted;
            _input.SprintCanceled -= OnSprintCanceled;
            _input.CrouchStarted  -= OnCrouchStarted;
            _input.CrouchCanceled -= OnCrouchCanceled;
        }

        private void Update()
        {
            if (_controller == null || !_controller.enabled) return;

            var context = new FrameContext
            {
                MoveInput = _input.MoveInput,
                IsGrounded = _controller.isGrounded,
                SprintInput = _sprintInput,
                CrouchInput = _posture.CheckCrouchState(_crouchInput),
                DeltaTime = Time.deltaTime
            };

            _locomotion.Tick(ref context);
            _gravity.Tick(ref context);

            Vector3 finalMovement = _locomotion.CurrentHorizontalVelocity + Vector3.up * _gravity.VerticalVelocity;
            _controller.Move(finalMovement * context.DeltaTime);

            _stateTracker.Tick(ref context, _controller.velocity, _locomotion.IsSprintingActive, _settings.MinMoveThreshold);
            
            _posture.Tick(_stateTracker.IsCrouching);
        }

        private void OnJumpStarted()    => _gravity.SetJumpHeld(true);
        private void OnJumpCanceled()   => _gravity.SetJumpHeld(false);
        private void OnSprintStarted()  => _sprintInput = true;
        private void OnSprintCanceled() => _sprintInput = false;
        private void OnCrouchStarted()  => _crouchInput = true;
        private void OnCrouchCanceled() => _crouchInput = false;
    }
}