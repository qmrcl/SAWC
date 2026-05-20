using UnityEngine;
using System;

namespace SAWC.Input
{
    public abstract class BaseInputProvider : MonoBehaviour, IInputProvider
    {
        [Header("Camera Reference")]
        [SerializeField] protected Transform _cameraTransform;

        [Header("Mobile Support")]
        [SerializeField] protected GameObject _joystickObject;
        protected IJoystickProvider _joystick;

        private bool _uiJumpHeld;
        private bool _uiSprintHeld;
        private bool _uiCrouchHeld;

        private bool _jumpState;
        private bool _sprintState;
        private bool _crouchState;

        private float _lastCameraYRotation;

        public Vector3 WorldMoveDirection { get; private set; }
        public Vector3 WorldLookDirection { get; private set; }

        public bool SprintHeld => _sprintState;
        public bool CrouchHeld => _crouchState;

        public Vector2 MoveInput
        {
            get
            {
                Vector2 joy = _joystick?.JoystickDirection ?? Vector2.zero;
                return joy.sqrMagnitude > 0.01f ? joy : GetRawMoveInput();
            }
        }

        public event Action JumpStarted;
        public event Action JumpCanceled;
        public event Action SprintStarted;
        public event Action SprintCanceled;
        public event Action CrouchStarted;
        public event Action CrouchCanceled;

        protected virtual void Awake()
        {
            _joystick = _joystickObject?.GetComponent<IJoystickProvider>();

            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (_cameraTransform != null)
                _lastCameraYRotation = _cameraTransform.eulerAngles.y;
        }

        protected virtual void Update()
        {
            bool isJumpNow = GetRawJumpInput() || _uiJumpHeld;
            bool isSprintNow = GetRawSprintInput() || _uiSprintHeld;
            bool isCrouchNow = GetRawCrouchInput() || _uiCrouchHeld;

            ProcessAction(isJumpNow, ref _jumpState, JumpStarted, JumpCanceled);
            ProcessAction(isSprintNow, ref _sprintState, SprintStarted, SprintCanceled);
            ProcessAction(isCrouchNow, ref _crouchState, CrouchStarted, CrouchCanceled);

            CalculateWorldDirection();
        }

        protected abstract Vector2 GetRawMoveInput();
        protected abstract bool GetRawJumpInput();
        protected abstract bool GetRawSprintInput();
        protected abstract bool GetRawCrouchInput();

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

            WorldMoveDirection = (camRight * currentInput.x + camForward * currentInput.y).normalized;
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

        public void UIJump(bool state) => _uiJumpHeld = state;
        public void UISprint(bool state) => _uiSprintHeld = state;
        public void UICrouch(bool state) => _uiCrouchHeld = state;
    }
}