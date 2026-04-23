using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Modules.Input.Detection
{
    [CreateAssetMenu(fileName = "DefaultDetect", menuName = "SAWC/Device Detector/Strategies/Force PC")]
    public class DefaultDetect : DeviceDetectionStrategy
    {
        public override InputDeviceType Detect()
        {
            if (Application.isMobilePlatform)
                return InputDeviceType.Mobile;

            if (Gamepad.all.Count > 0)
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
    }
}