using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Modules.Input.Detection
{
    [CreateAssetMenu(fileName = "Gamepad_Detect", menuName = "SAWC/Modules/Device Detector/Gamepad")]
    public class GamepadDetection : DeviceDetection
    {
        public override bool Detect()
        {
            return Gamepad.all.Count > 0;
        }
    }
}