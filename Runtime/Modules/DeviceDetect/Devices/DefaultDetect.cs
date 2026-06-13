using UnityEngine;

#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SAWC.Modules.Input.Detection
{
    [CreateAssetMenu(fileName = "DefaultDetect", menuName = "SAWC/Modules/Device Detector/PC")]
    public class DefaultDetect : DeviceDetectionStrategy
    {
        public override InputDeviceType Detect()
        {
            if (Application.isMobilePlatform)
                return InputDeviceType.Mobile;

            if (IsGamepadConnected())
                return InputDeviceType.Gamepad;

            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer => InputDeviceType.PC,
                RuntimePlatform.OSXPlayer => InputDeviceType.PC,
                RuntimePlatform.LinuxPlayer => InputDeviceType.PC,
                RuntimePlatform.WindowsEditor => InputDeviceType.PC,
                RuntimePlatform.OSXEditor => InputDeviceType.PC,
                _ => InputDeviceType.Unknown
            };
        }

        private bool IsGamepadConnected()
        {
#if SAWC_NEW_INPUT_AVAILABLE && ENABLE_INPUT_SYSTEM
            if (Gamepad.all.Count > 0)
                return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            string[] joysticks = UnityEngine.Input.GetJoystickNames();
            if (joysticks != null && joysticks.Length > 0)
            {
                for (int i = 0; i < joysticks.Length; i++)
                {
                    if (!string.IsNullOrEmpty(joysticks[i]))
                        return true;
                }
            }
#endif

            return false;
        }
    }
}