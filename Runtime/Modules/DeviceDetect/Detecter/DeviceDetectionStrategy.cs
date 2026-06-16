using UnityEngine;

namespace SAWC.Modules.Input.Detection
{
    public abstract class DeviceDetection : ScriptableObject
    {
        public abstract InputDeviceType Detect();
    }
}