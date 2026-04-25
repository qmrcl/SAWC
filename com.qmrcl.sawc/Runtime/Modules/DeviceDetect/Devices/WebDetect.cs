using SAWC.Modules.Input.Detection;
using UnityEngine;

[CreateAssetMenu(fileName = "WebDetect", menuName = "SAWC/Device Detector/Web")]
public class WebDetect : DeviceDetectionStrategy
{
    public override InputDeviceType Detect()
    {
        return InputDeviceType.Unknown;
    }
}