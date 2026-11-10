using UnityEngine;

namespace SAWC.Modules.Input.Detection
{
    [CreateAssetMenu(fileName = "Mobile_Detect", menuName = "SAWC/Modules/Device Detector/Mobile")]
    public class MobileDetection : DeviceDetection
    {
        public override bool Detect()
        {
            return Application.isMobilePlatform;
        }
    }
}