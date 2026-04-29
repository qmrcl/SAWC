using UnityEngine;
using System;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Input/Legacy Input Provider (Old)")]
    public class LegacyInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private string _horAxis = "Horizontal";
        [SerializeField] private string _verAxis = "Vertical";
        [SerializeField] private string _jumpButton = "Jump";
        [SerializeField] private string _sprintButton = "Fire3";

        [Header("Mobile Support")]
        [SerializeField] private GameObject _joystickObject;
        private IJoystickProvider _joystick;

        public Vector2 MoveInput 
        {
            get 
            {
                Vector2 joy = _joystick?.JoystickDirection ?? Vector2.zero;
                if (joy.sqrMagnitude > 0.01f) return joy;
                return new Vector2(Input.GetAxisRaw(_horAxis), Input.GetAxisRaw(_verAxis));
            }
        }
        
        public event Action JumpStarted;
        public event Action JumpCanceled;
        public event Action SprintStarted;
        public event Action SprintCanceled;

        private bool _isJumpPressed;
        private bool _isSprintPressed;

        private void Awake() => _joystick = _joystickObject?.GetComponent<IJoystickProvider>();

        private void Update()
        {
            HandleButton(_jumpButton, ref _isJumpPressed, JumpStarted, JumpCanceled);
            HandleButton(_sprintButton, ref _isSprintPressed, SprintStarted, SprintCanceled);
        }

        private void HandleButton(string buttonName, ref bool state, Action startEvent, Action cancelEvent)
        {
            try
            {
                if (Input.GetButtonDown(buttonName) && !state)
                {
                    state = true;
                    startEvent?.Invoke();
                }
                else if (Input.GetButtonUp(buttonName) && state)
                {
                    state = false;
                    cancelEvent?.Invoke();
                }
            }
            catch { }
        }

        public void UIJump(bool start) { if(start) JumpStarted?.Invoke(); else JumpCanceled?.Invoke(); }
        public void UISprint(bool start) { if(start) SprintStarted?.Invoke(); else SprintCanceled?.Invoke(); }
    }
}