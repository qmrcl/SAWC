using SAWC.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Modifiers
{
    [AddComponentMenu("SAWC/Modules/Extensions/Dash")]
    [DefaultExecutionOrder(15)]
    public sealed class Dash : CharacterModifierBase, IVelocityModifier
    {
        [Space(5)]
        [SerializeField] private InputActionReference _dashAction;

        [SerializeField, Min(1f)] private float _dashSpeed = 25f;
        [SerializeField, Min(0.01f)] private float _dashDuration = 0.2f;
        [SerializeField, Min(0f)] private float _cooldown = 1f;
        [SerializeField, Min(0f)] private float _inputBufferTime = 0.15f;

        private float _dashTimer;
        private float _cooldownEndTime;
        private float _bufferEndTime;
        private Vector3 _dashDirection;
        private bool _unbufferedDashRequested;
        private PlayerInput _playerInput;

        public bool IsDashing => _dashTimer > 0f;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out _playerInput);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_playerInput == null) ResolveDashAction()?.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Referenced actions can be shared; their map or PlayerInput owns disabling them.
            _dashTimer = 0f;
            _bufferEndTime = 0f;
            _unbufferedDashRequested = false;
            _dashDirection = Vector3.zero;
        }

        private InputAction ResolveDashAction()
        {
            var action = _dashAction != null ? _dashAction.action : null;
            if (action == null || _playerInput == null) return action;

            var actions = _playerInput.actions;
            return actions != null ? actions.FindAction(action.id) : null;
        }

        public void TriggerDash()
        {
            if (Time.time >= _cooldownEndTime)
            {
                _bufferEndTime = Time.time + _inputBufferTime;
                _unbufferedDashRequested = _inputBufferTime <= 0f;
            }
        }

        private void Update()
        {
            if (Controller == null || Controller.Input == null)
            {
                _unbufferedDashRequested = false;
                return;
            }

            var dashAction = ResolveDashAction();
            if (dashAction != null && dashAction.WasPressedThisFrame())
            {
                TriggerDash();
            }

            bool unbufferedDashRequested = _unbufferedDashRequested;
            _unbufferedDashRequested = false;

            if (IsDashing)
            {
                _dashTimer -= Time.deltaTime;
                return;
            }

            Vector3 moveDir = Controller.Input.WorldMoveDirection;

            if ((unbufferedDashRequested || Time.time < _bufferEndTime) && moveDir.sqrMagnitude > 0.01f)
            {
                _dashTimer = _dashDuration;
                _dashDirection = moveDir.normalized;
                _cooldownEndTime = Time.time + _cooldown;
                _bufferEndTime = 0f;
            }
        }

        public Vector3 ModifyVelocity(Vector3 currentVelocity, ref FrameContext ctx)
        {
            if (!IsDashing) return currentVelocity;

            return new Vector3(_dashDirection.x * _dashSpeed, 0f, _dashDirection.z * _dashSpeed);
        }
    }
}
