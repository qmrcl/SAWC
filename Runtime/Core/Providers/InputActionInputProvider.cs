using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Input/Input Action Provider (New)")]
    public class InputActionInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _sprintAction;

        [Header("Mobile Support")]
        [SerializeField] private GameObject _joystickObject;
        private IJoystickProvider _joystick;

        public Vector2 MoveInput 
        {
            get 
            {
                Vector2 joy = _joystick?.JoystickDirection ?? Vector2.zero;
                return joy.sqrMagnitude > 0.01f ? joy : _moveAction.action.ReadValue<Vector2>();
            }
        }

        public event Action JumpStarted;
        public event Action JumpCanceled;
        public event Action SprintStarted;
        public event Action SprintCanceled;

        private void Awake() => _joystick = _joystickObject?.GetComponent<IJoystickProvider>();

        private void OnEnable()
        {
            _jumpAction.action.started += OnJumpStarted;
            _jumpAction.action.canceled += OnJumpCanceled;
            _sprintAction.action.started += OnSprintStarted;
            _sprintAction.action.canceled += OnSprintCanceled;
        }

        private void OnDisable()
        {
            _jumpAction.action.started -= OnJumpStarted;
            _jumpAction.action.canceled -= OnJumpCanceled;
            _sprintAction.action.started -= OnSprintStarted;
            _sprintAction.action.canceled -= OnSprintCanceled;
        }

        private void OnJumpStarted(InputAction.CallbackContext ctx) => JumpStarted?.Invoke();
        private void OnJumpCanceled(InputAction.CallbackContext ctx) => JumpCanceled?.Invoke();
        private void OnSprintStarted(InputAction.CallbackContext ctx) => SprintStarted?.Invoke();
        private void OnSprintCanceled(InputAction.CallbackContext ctx) => SprintCanceled?.Invoke();

        public void UIJump(bool start) { if(start) JumpStarted?.Invoke(); else JumpCanceled?.Invoke(); }
        public void UISprint(bool start) { if(start) SprintStarted?.Invoke(); else SprintCanceled?.Invoke(); }
    }
}