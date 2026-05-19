using UnityEngine;
using UnityEngine.InputSystem;
using System;
using SAWC.Input;

namespace SAWC.Input
{
    [AddComponentMenu("SAWC/Core/Input/Input Action Provider (Fixed)")]
    public class InputActionInputProvider : MonoBehaviour, IInputProvider
    {
        [Header("Camera Reference")]
        [SerializeField] private Transform _cameraTransform;

        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _sprintAction;
        [SerializeField] private InputActionReference _crouchAction;

        [Header("Mobile Support")]
        [SerializeField] private GameObject _joystickObject;
        private IJoystickProvider _joystick;

        private bool _uiJumpHeld;
        private bool _uiSprintHeld;
        private bool _uiCrouchHeld;

        private bool _jumpState;
        private bool _sprintState;
        private bool _crouchState;

        private float _lastCameraYRotation;

        public Vector2 MoveInput
        {
            get
            {
                Vector2 joy = _joystick?.JoystickDirection ?? Vector2.zero;
                return joy.sqrMagnitude > 0.01f ? joy : _moveAction.action.ReadValue<Vector2>();
            }
        }

        public Vector3 WorldMoveDirection { get; private set; }
        public Vector3 WorldLookDirection { get; private set; }

        public bool SprintHeld => _sprintState;
        public bool CrouchHeld => _crouchState;

        public event Action JumpStarted;
        public event Action JumpCanceled;
        public event Action SprintStarted;
        public event Action SprintCanceled;
        public event Action CrouchStarted;
        public event Action CrouchCanceled;

        private void Awake()
        {
            _joystick = _joystickObject?.GetComponent<IJoystickProvider>();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_cameraTransform != null)
                _lastCameraYRotation = _cameraTransform.eulerAngles.y;
        }

        private void Update()
        {
            bool isJumpNow = (_jumpAction != null && _jumpAction.action.IsPressed()) || _uiJumpHeld;
            bool isSprintNow = (_sprintAction != null && _sprintAction.action.IsPressed()) || _uiSprintHeld;
            bool isCrouchNow = (_crouchAction != null && _crouchAction.action.IsPressed()) || _uiCrouchHeld;

            ProcessAction(isJumpNow, ref _jumpState, JumpStarted, JumpCanceled);
            ProcessAction(isSprintNow, ref _sprintState, SprintStarted, SprintCanceled);
            ProcessAction(isCrouchNow, ref _crouchState, CrouchStarted, CrouchCanceled);

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