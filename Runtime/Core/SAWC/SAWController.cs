using UnityEngine;
using SAWC.Modifiers;
using SAWC.Core.Input;
using SAWC.Core.Data;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Core/SAW Controller")]
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    [SelectionBase]
    public class SAWController : MonoBehaviour
    {
        public ICharacterState State => _stateTracker;
        public IInputProvider Input { get; private set; }

        public CharacterModifiers Modifiers { get; } = new CharacterModifiers();
        public CharacterSettingsData BaseSettings => _settings.Data;

        [SerializeField] private CharacterSettings _settings;

        private CharacterController _controller;
        private CharacterLocomotion _locomotion;
        private CharacterGravity _gravity;
        private CharacterPosture _posture;
        private CharacterRotation _rotation;
        private CharacterStateTracker _stateTracker = new CharacterStateTracker();

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            Input = GetComponent<IInputProvider>();

            if (_settings == null)
            {
                Debug.LogError($"[SAWController]. На {gameObject.name} не назначены Settings", this);
                enabled = false;
                return;
            }

            _locomotion = new CharacterLocomotion();
            _gravity = new CharacterGravity();
            _posture = new CharacterPosture(_controller, transform, ref _settings.Data);
            _rotation = new CharacterRotation(transform);
        }

        private void Start()
        {
            if (Input == null)
            {
                Debug.LogError($"[SAWController]. На {gameObject.name} Нет Input Provider", this);
                enabled = false;
                return;
            }
            _controller.Move(Vector3.zero);
            _stateTracker.Initialize(_controller.isGrounded);
        }

        private void Update()
        {
            var context = new FrameContext
            {
                Settings = _settings.Data,
                MoveInput = Input.MoveInput,
                WorldMoveDirection = Input.WorldMoveDirection,
                WorldLookDirection = Input.WorldLookDirection,
                IsGrounded = _controller.isGrounded,
                JumpInput = Input.JumpHeld,
                SprintInput = Input.SprintHeld,
                HitCeiling = (_controller.collisionFlags & CollisionFlags.Above) != 0,
                DeltaTime = Time.deltaTime
            };

            context.CanStandUp = _posture.CanStandUp(ref context.Settings);
            context.CrouchInput = _posture.CheckCrouchState(Input.CrouchHeld, _stateTracker.IsCrouching, context.CanStandUp, ref context.Settings);

            Modifiers.ProcessContext(ref context);

            _locomotion.Tick(ref context);
            _gravity.Tick(ref context);
            _rotation.Tick(ref context);

            Vector3 finalMovement = _locomotion.CurrentHorizontalVelocity + Vector3.up * _gravity.VerticalVelocity;

            finalMovement = Modifiers.ProcessVelocity(finalMovement, ref context);

            _posture.Tick(context.CrouchInput, ref context.Settings);

            _controller.Move(finalMovement * context.DeltaTime);

            context.IsGrounded = _controller.isGrounded;

            _stateTracker.Tick(ref context, _controller.velocity, _locomotion.IsSprintingActive);
        }
    }
}