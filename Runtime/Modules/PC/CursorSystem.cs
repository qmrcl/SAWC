using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Modules.Utils
{
    [AddComponentMenu("SAWC/Modules/PC/CursorSystem")]
    public class CursorSystem : MonoBehaviour
    {
        [SerializeField] private bool _startLocked = true;
        [SerializeField] private CursorLockMode _defaultLockMode = CursorLockMode.Locked;

        [Space(5)]
        [SerializeField] private InputAction _toggleAction;

        private bool _isLocked;

        private void OnEnable()
        {
            _toggleAction.Enable();
            _toggleAction.performed += OnToggleInput;
        }

        private void OnDisable()
        {
            _toggleAction.performed -= OnToggleInput;
            _toggleAction.Disable();
        }

        private void Start()
        {
            SetState(_startLocked);
        }

        private void OnToggleInput(InputAction.CallbackContext context)
        {
            SetState(!_isLocked);
        }

        public void SetState(bool isLocked)
        {
            _isLocked = isLocked;
            Cursor.visible = !isLocked;
            Cursor.lockState = isLocked ? _defaultLockMode : CursorLockMode.None;
        }
    }
}