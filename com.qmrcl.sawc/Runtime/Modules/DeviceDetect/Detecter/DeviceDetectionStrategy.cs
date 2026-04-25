using UnityEngine;

namespace SAWC.Modules.Input.Detection
{
    public abstract class DeviceDetectionStrategy : ScriptableObject
    {
        public abstract InputDeviceType Detect();
    }
}