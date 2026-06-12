using UnityEngine;
using SAWC.Core;

namespace SAWC.Modifiers
{
    public sealed class DashMechanic : CharacterModifierBase, IVelocityModifier
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _key = KeyCode.LeftControl;

        [SerializeField] private float _dashSpeed = 25f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _cooldown = 1f;
        [SerializeField] private float _inputBufferTime = 0.15f;

        private float _dashTimer;
        private float _cooldownEndTime;
        private float _bufferEndTime;
        private Vector3 _dashDirection;

        private bool IsDashing => _dashTimer > 0f;

        protected override void Awake()
        {
            base.Awake();
            SetPriority(100);
        }

        private void Update()
        {
            if (Controller == null) return;

            if (Input.GetKeyDown(_key) && Time.time >= _cooldownEndTime)
            {
                _bufferEndTime = Time.time + _inputBufferTime;
            }

            if (IsDashing)
            {
                _dashTimer -= Time.deltaTime;
                return;
            }

            Vector3 moveDir = Controller.Input.WorldMoveDirection;
            bool hasMoveInput = moveDir.sqrMagnitude > 0.01f;

            if (Time.time < _bufferEndTime && hasMoveInput)
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

            return new Vector3(_dashDirection.x * _dashSpeed, currentVelocity.y, _dashDirection.z * _dashSpeed);
        }
    }
}