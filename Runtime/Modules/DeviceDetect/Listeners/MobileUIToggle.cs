using System.Collections.Generic;
using UnityEngine;
using SAWC.Modules.Input.Detection;

namespace SAWC.Modules.UI
{
    [AddComponentMenu("SAWC/Modules/Detector/Mobile UI Toggle")]
    public class MobileUIToggle : MonoBehaviour
    {
        [SerializeField] private Canvas _mobileControlsCanvas;
        [SerializeField] private DeviceDetector _detector;

        [Header("Hide Canvas for these devices:")]
        [SerializeField] private List<DeviceDetection> _hideOnDevices;

        private void Awake()
        {
            if (_mobileControlsCanvas == null)
            {
                Debug.LogError($"Mobile controls Canvas reference is null on '{gameObject.name}'!", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_detector == null) return;

            _detector.DeviceDetected += HandleDeviceChange;
            if (_detector.CurrentDevice != null)
                HandleDeviceChange(_detector.CurrentDevice);
        }

        private void OnDisable()
        {
            if (_detector != null)
                _detector.DeviceDetected -= HandleDeviceChange;
        }

        private void HandleDeviceChange(DeviceDetection detectedType)
        {
            if (detectedType == null || _mobileControlsCanvas == null) return;

            bool shouldHide = _hideOnDevices != null && _hideOnDevices.Contains(detectedType);
            _mobileControlsCanvas.gameObject.SetActive(!shouldHide);
        }
    }
}