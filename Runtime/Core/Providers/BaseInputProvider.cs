using SAWC.Localization;
using System;
using UnityEngine;

namespace SAWC.Core.Input
{
    public abstract class BaseInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField, Loc] protected Transform _cameraTransform;

        private bool _jumpState;
        private bool _sprintState;
        private bool _crouchState;

        public Vector3 WorldMoveDirection { get; private set; }
        public Vector3 WorldLookDirection { get; private set; }

        public bool SprintHeld => _sprintState;
        public bool CrouchHeld => _crouchState;
        public bool JumpHeld => _jumpState;

        public abstract Vector2 MoveInput { get; }
        protected abstract bool GetJumpInput();
        protected abstract bool GetSprintInput();
        protected abstract bool GetCrouchInput();

        public event Action JumpStarted;
        public event Action JumpCanceled;
        public event Action SprintStarted;
        public event Action SprintCanceled;
        public event Action CrouchStarted;
        public event Action CrouchCanceled;

        protected virtual void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        protected virtual void Update()
        {
            ProcessAction(GetJumpInput(), ref _jumpState, JumpStarted, JumpCanceled);
            ProcessAction(GetSprintInput(), ref _sprintState, SprintStarted, SprintCanceled);
            ProcessAction(GetCrouchInput(), ref _crouchState, CrouchStarted, CrouchCanceled);
            CalculateWorldDirection();
        }

        private void CalculateWorldDirection()
        {
            Vector2 currentInput = MoveInput;

            if (_cameraTransform == null)
            {
                WorldMoveDirection = Vector3.zero;
                WorldLookDirection = transform.forward;
                return;
            }

            Vector3 camRight = _cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 camForward = Vector3.Cross(camRight, Vector3.up);
            WorldLookDirection = camForward;

            if (currentInput.sqrMagnitude < 0.001f)
            {
                WorldMoveDirection = Vector3.zero;
                return;
            }

            Vector3 direction = camRight * currentInput.x + camForward * currentInput.y;
            WorldMoveDirection = Vector3.ClampMagnitude(direction, 1f);
        }

        private void ProcessAction(bool isPressedNow, ref bool previousState, Action onStarted, Action onCanceled)
        {
            if (isPressedNow && !previousState)
            {
                previousState = true;
                onStarted?.Invoke();
            }
            else if (!isPressedNow && previousState)
            {
                previousState = false;
                onCanceled?.Invoke();
            }
        }
    }
}