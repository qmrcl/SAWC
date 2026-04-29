using UnityEngine;
using UnityEngine.UI;
using SAWC.Modules.Input.TouchControls;
using Unity.Cinemachine;

public class SensitivityManager : MonoBehaviour
{
    private const string SENS_KEY = "UserSensitivity";

    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private float _defaultSensitivity = 15f;

    [Header("Multipliers")]
    [SerializeField] private float _pcMultiplier = 0.05f;
    [SerializeField] private float _mobileMultiplier = 1f;

    [Header("References")]
    [SerializeField] private LookPad _lookPadMobile;
    [SerializeField] private CinemachineInputAxisController _pcAxis;

    private void OnEnable()
    {
        if (_sensitivitySlider != null)
        {
            float savedValue = PlayerPrefs.GetFloat(SENS_KEY, _defaultSensitivity);
            _sensitivitySlider.value = savedValue;

            ApplySensitivity(savedValue);

            _sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    private void OnDisable()
    {
        if (_sensitivitySlider != null)
        {
            _sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
    }

    private void OnSensitivityChanged(float value)
    {
        ApplySensitivity(value);
        PlayerPrefs.SetFloat(SENS_KEY, value);
        PlayerPrefs.Save();
    }

    private void ApplySensitivity(float value)
    {
        if (_lookPadMobile != null)
        {
            _lookPadMobile.SetSensitivity(value * _mobileMultiplier);
        }

        if (_pcAxis != null && _pcAxis.Controllers.Count >= 2)
        {
            float pcValue = value * _pcMultiplier;
            _pcAxis.Controllers[0].Input.Gain = pcValue;
            _pcAxis.Controllers[1].Input.Gain = -pcValue;
        }
    }
}