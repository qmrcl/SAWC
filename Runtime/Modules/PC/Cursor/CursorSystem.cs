using UnityEngine;

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SAWC.Modules.Utils
{
    [AddComponentMenu("SAWC/Modules/CursorSystem")]
    public class CursorSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _startLocked = true;
        [SerializeField] private CursorLockMode _defaultLockMode = CursorLockMode.Locked;

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
        [Header("Input Actions (New System)")]
        [SerializeField] private InputAction _toggleAction;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        [Header("Input Settings (Legacy System)")]
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

#if ENABLE_LEGACY_INPUT_MANAGER
        private void Update()
        {
#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
#else
            if (UnityEngine.Input.GetButtonDown(_toggleButton))
            {
                SetState(!_isLocked);
            }
#endif
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