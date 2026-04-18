using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAWC.Modules.Input.Detection
{
    [AddComponentMenu("SAWC/Modules/Device Detector")]
    public class DeviceDetector : MonoBehaviour
    {
        [SerializeField] private List<DeviceDetectionStrategy> _strategies;

        public event Action<InputDeviceType> DeviceDetected;

        private void Start()
        {
            DetectAndNotify();
        }

        public void DetectAndNotify()
        {
            InputDeviceType detected = GetCurrentDeviceType();
            DeviceDetected?.Invoke(detected);
        }

        private InputDeviceType GetCurrentDeviceType()
        {
            if (_strategies == null) return InputDeviceType.Unknown;

            foreach (var strategy in _strategies)
            {
                if (strategy == null) continue;

                InputDeviceType result = strategy.Detect();

                if (result != InputDeviceType.Unknown)
                {
                    return result;
                }
            }

            return InputDeviceType.Unknown;
        }
    }
}