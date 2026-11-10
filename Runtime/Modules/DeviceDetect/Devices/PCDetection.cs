using UnityEngine;

namespace SAWC.Modules.Input.Detection
{
    [CreateAssetMenu(fileName = "PC_Detect", menuName = "SAWC/Modules/Device Detector/PC")]
    public class PCDetection : DeviceDetection
    {
        public override bool Detect()
        {
            return !Application.isMobilePlatform && !Application.isConsolePlatform;
        }
    }
}