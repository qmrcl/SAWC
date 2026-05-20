using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Input
{
    [AddComponentMenu("SAWC/Core/Input/Input Action Provider (Fixed)")]
    public class InputActionInputProvider : BaseInputProvider
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _sprintAction;
        [SerializeField] private InputActionReference _crouchAction;

        protected override Vector2 GetRawMoveInput() =>
            _moveAction != null ? _moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        protected override bool GetRawJumpInput() =>
            _jumpAction != null && _jumpAction.action.IsPressed();

        protected override bool GetRawSprintInput() =>
            _sprintAction != null && _sprintAction.action.IsPressed();

        protected override bool GetRawCrouchInput() =>
            _crouchAction != null && _crouchAction.action.IsPressed();
    }
}