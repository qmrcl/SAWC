using System;
using Unity.Cinemachine;
using UnityEngine;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Touch Input Receiver")]
    [DefaultExecutionOrder(-100)]
    public class TouchInputReceiver : InputAxisControllerBase<TouchInputReceiver.TouchReader>
    {
        [SerializeField] private LookPad _lookPad;

        [Space(5)]
        [SerializeField, Range(0.1f, 100f)] private float _sensitivity = 1f / 6f;

        private Vector2 CurrentSensitivityDelta
        {
            get
            {
                float deltaTime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                // Pointer delta is already a displacement. Cancel the axis driver's
                // integration so sensitivity is expressed in degrees per pixel.
                return _lookPad != null && deltaTime > 0f
                    ? _lookPad.Delta * (_sensitivity / deltaTime)
                    : Vector2.zero;
            }
        }

        // EventSystem dispatches drag events in Update. Read them before LookPad's
        // LateUpdate clears the shared delta and before CinemachineBrain updates.
        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                UpdateControllers();
            }
        }

        public void SetSensitivity(float newSensitivity)
        {
            _sensitivity = Mathf.Clamp(newSensitivity, 0.1f, 100f);
        }

        [Serializable]
        public class TouchReader : IInputAxisReader
        {
            [SerializeField] private AxisType _axisType;
            public enum AxisType { Horizontal, Vertical }

            public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
            {
                var controller = context as TouchInputReceiver;
                if (controller == null) return 0;

                return _axisType == AxisType.Horizontal
                    ? controller.CurrentSensitivityDelta.x
                    : -controller.CurrentSensitivityDelta.y;
            }
        }
    }
}
