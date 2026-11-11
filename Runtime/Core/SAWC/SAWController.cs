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
    [DefaultExecutionOrder(20)]
    public class SAWController : MonoBehaviour
    {
        public ICharacterState State => _stateTracker;
        public IInputProvider Input { get; private set; }
        public CharacterModifiers Modifiers { get; } = new CharacterModifiers();
        public CharacterSettingsData BaseSettings => _settings != null
            ? _settings.Data
            : (_defaultSettings ??= CharacterSettings.CreateDefaultData());

        [SerializeField] private CharacterSettings _settings;

        private CharacterSettingsData? _defaultSettings;

        private CharacterController _controller;
        private CharacterLocomotion _locomotion;
        private CharacterGravity _gravity;
        private CharacterPosture _posture;
        private CharacterRotation _rotation;
        private CharacterStateTracker _stateTracker = new CharacterStateTracker();

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (Input == null) Input = GetComponent<IInputProvider>();

            CharacterSettingsData initialSettings = BaseSettings;
            _locomotion = new CharacterLocomotion();
            _gravity = new CharacterGravity();
            _posture = new CharacterPosture(_controller, transform, ref initialSettings);
            _rotation = new CharacterRotation(transform);
        }

        private void Start()
        {
            _controller.Move(Vector3.zero);
            _stateTracker.Initialize(_controller.isGrounded);
        }

        private void Update()
        {
            if (Input == null) return;

            var context = new FrameContext
            {
                Settings = BaseSettings,
                MoveInput = Input.MoveInput,
                WorldMoveDirection = Input.WorldMoveDirection,
                WorldLookDirection = Input.WorldLookDirection,
                IsGrounded = _controller.isGrounded,
                JumpInput = Input.JumpHeld,
                SprintInput = Input.SprintHeld,
                CrouchInput = Input.CrouchHeld,
                CanStandUp = true,
                HitCeiling = (_controller.collisionFlags & CollisionFlags.Above) != 0,
                DeltaTime = Time.deltaTime
            };

            Modifiers.ProcessContext(ref context);

            context.CanStandUp &= _posture.CanStandUp(ref context.Settings);
            context.CrouchInput = _posture.CheckCrouchState(context.CrouchInput, _stateTracker.IsCrouching, context.CanStandUp, ref context.Settings);

            _locomotion.Tick(ref context);
            _gravity.Tick(ref context);
            _rotation.Tick(ref context);

            Vector3 finalMovement = _locomotion.CurrentHorizontalVelocity + Vector3.up * _gravity.VerticalVelocity;
            finalMovement = Modifiers.ProcessVelocity(finalMovement, ref context);

            _posture.Tick(context.CrouchInput, ref context.Settings);
            _controller.Move(finalMovement * context.DeltaTime);

            context.IsGrounded = _controller.isGrounded;
            context.GravityVerticalVelocity = _gravity.VerticalVelocity;

            _stateTracker.Tick(ref context, _controller.velocity, _locomotion.IsSprintingActive);
        }

        public void SetInputProvider(IInputProvider newInputProvider)
        {
            if (newInputProvider == null)
            {
                Debug.Log($"{nameof(SAWController)}. Input provider on '{gameObject.name}' was set to null. Ensure this was intentional.");
            }
            Input = newInputProvider;
        }
    }
}
