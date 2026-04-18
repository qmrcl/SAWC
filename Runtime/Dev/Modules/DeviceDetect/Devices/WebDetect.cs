using SAWC.Modules.Input.Detection;
using UnityEngine;

[CreateAssetMenu(fileName = "WebDetect", menuName = "YG/Detection Strategy")]
public class WebDetect : DeviceDetectionStrategy
{
    public override InputDeviceType Detect()
    {
        return InputDeviceType.Unknown;
    }
}