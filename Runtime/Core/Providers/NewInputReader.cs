using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Core.Input.Readers
{
    [AddComponentMenu("SAWC/Core/Input/Readers/New Input Reader")]
    public class NewInputReader : BaseInputReader
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _sprintAction;
        [SerializeField] private InputActionReference _crouchAction;

        private PlayerInput _playerInput;

        private void Awake() => TryGetComponent(out _playerInput);

        public override Vector2 Move => ResolveAction(_moveAction)?.ReadValue<Vector2>() ?? Vector2.zero;
        public override bool Jump => ResolveAction(_jumpAction)?.IsPressed() ?? false;
        public override bool Sprint => ResolveAction(_sprintAction)?.IsPressed() ?? false;
        public override bool Crouch => ResolveAction(_crouchAction)?.IsPressed() ?? false;

        private InputAction ResolveAction(InputActionReference reference)
        {
            var action = reference != null ? reference.action : null;
            if (action == null || _playerInput == null) return action;

            // Additional PlayerInput instances own copies of the source asset.
            // Read this player's copy instead of another player's shared reference.
            var actions = _playerInput.actions;
            return actions != null ? actions.FindAction(action.id) : null;
        }
    }
}
