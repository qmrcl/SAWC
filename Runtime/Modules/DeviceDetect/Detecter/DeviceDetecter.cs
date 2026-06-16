using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAWC.Modules.Input.Detection
{
    [AddComponentMenu("SAWC/Modules/Device Detector")]
    public class DeviceDetector : MonoBehaviour
    {
        [SerializeField] private List<DeviceDetection> _detectors;

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
            if (_detectors == null || _detectors.Count == 0)
            {
                Debug.LogWarning($"Strategy list is empty on object '{gameObject.name}'!", this);
                return InputDeviceType.Unknown;
            }

            foreach (var strategy in _detectors)
            {
                if (strategy == null) continue;

                InputDeviceType result = strategy.Detect();
                if (result != InputDeviceType.Unknown) return result;
            }

            Debug.LogWarning($"Device type could not be determined on '{gameObject.name}'!", this);
            return InputDeviceType.Unknown;
        }
    }
}