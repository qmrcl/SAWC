using UnityEngine;
using SAWC.Modules.Input.Detection; 

namespace SAWC.Modules.UI
{
    [AddComponentMenu("SAWC/Modules/Mobile UI Toggle")]
    public class MobileUIToggle : MonoBehaviour
    {
        [SerializeField] private Canvas _mobileControlsCanvas;
        [SerializeField] private DeviceDetector _detector;

        private void OnEnable()
        {
            if (_detector != null)
                _detector.DeviceDetected += HandleDeviceChange;
        }

        private void OnDisable()
        {
            if (_detector != null)
                _detector.DeviceDetected -= HandleDeviceChange;
        }

        private void HandleDeviceChange(InputDeviceType detectedType)
        {
            _mobileControlsCanvas.gameObject.SetActive(detectedType != InputDeviceType.PC && detectedType != InputDeviceType.Gamepad);
        }
    }
}