using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SAWC.Modules.Input.Detection
{
    [AddComponentMenu("SAWC/Modules/Detector/Device Detector")]
    public class DeviceDetector : MonoBehaviour
    {
        [SerializeField] private List<DeviceDetection> _detectors;

        public DeviceDetection CurrentDevice { get; private set; }
        public event Action<DeviceDetection> DeviceDetected;

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnInputDeviceChanged;
            EvaluateDevice();
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnInputDeviceChanged;
        }

        private void OnInputDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (change is InputDeviceChange.Added or InputDeviceChange.Removed or InputDeviceChange.Reconnected or InputDeviceChange.Disconnected)
            {
                EvaluateDevice();
            }
        }

        public void EvaluateDevice()
        {
            DeviceDetection detected = GetCurrentDeviceType();
            if (detected == CurrentDevice && CurrentDevice != null) return;

            CurrentDevice = detected;
            DeviceDetected?.Invoke(CurrentDevice);
        }

        private DeviceDetection GetCurrentDeviceType()
        {
            if (_detectors == null || _detectors.Count == 0)
            {
                Debug.LogWarning($"Strategy list is empty on object '{gameObject.name}'!", this);
                return null;
            }

            for (int i = 0; i < _detectors.Count; i++)
            {
                var strategy = _detectors[i];
                if (strategy != null && strategy.Detect())
                    return strategy;
            }

            Debug.LogWarning($"Device type could not be determined on '{gameObject.name}'!", this);
            return null;
        }
    }
}