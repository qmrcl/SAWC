using SAWC.Localization;
using UnityEngine;

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SAWC.Modules.Utils
{
    [AddComponentMenu("SAWC/Modules/CursorSystem")]
    public class CursorSystem : MonoBehaviour
    {
        [SerializeField, Loc] private bool _startLocked = true;
        [SerializeField, Loc] private CursorLockMode _defaultLockMode = CursorLockMode.Locked;

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
        [Space(5)]
        [SerializeField, Loc] private InputAction _toggleAction;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER && !(SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM)
        [Space(5)]
        [SerializeField] private string _toggleButton = "Cancel";
#endif

        private bool _isLocked;

        private void OnEnable()
        {
#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
            _toggleAction.Enable();
            _toggleAction.performed += OnToggleInput;
#endif
        }

        private void OnDisable()
        {
#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
            _toggleAction.Disable();
            _toggleAction.performed -= OnToggleInput;
#endif
        }

        private void Start()
        {
            SetState(_startLocked);
        }

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
        private void OnToggleInput(InputAction.CallbackContext context)
        {
            SetState(!_isLocked);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER && !(SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM)
        private void Update()
        {
            if (UnityEngine.Input.GetButtonDown(_toggleButton))
            {
                SetState(!_isLocked);
            }
        }
#endif

        public void SetState(bool isLocked)
        {
            _isLocked = isLocked;
            Cursor.visible = !isLocked;
            Cursor.lockState = isLocked ? _defaultLockMode : CursorLockMode.None;
        }
    }
}