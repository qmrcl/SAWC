using System;
using UnityEngine;

namespace SAWC.Core.Input
{
    public abstract class BaseInputProvider : MonoBehaviour, IInputProvider
    {
        [Header("Camera Reference")]
        [SerializeField] protected Transform _cameraTransform;

        private bool _jumpState;
        private bool _sprintState;
        private bool _crouchState;
        private float _lastCameraYRotation;

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
            if (_cameraTransform != null)
                _lastCameraYRotation = _cameraTransform.eulerAngles.y;
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

            Vector3 camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
            Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up);
            WorldLookDirection = camForward.normalized;

            if (camForward.sqrMagnitude < 0.001f || camRight.sqrMagnitude < 0.001f)
            {
                camForward = Quaternion.Euler(0f, _lastCameraYRotation, 0f) * Vector3.forward;
                camRight = Quaternion.Euler(0f, _lastCameraYRotation, 0f) * Vector3.right;
            }
            else
            {
                _lastCameraYRotation = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
                camForward.Normalize();
                camRight.Normalize();
            }

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