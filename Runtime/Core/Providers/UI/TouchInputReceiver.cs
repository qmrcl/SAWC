using SAWC.Localization;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Touch Input Receiver")]
    public class TouchInputReceiver : InputAxisControllerBase<TouchInputReceiver.TouchReader>
    {
        [SerializeField, Loc] private LookPad _lookPad;

        [Space(5)]
        [SerializeField,Loc, Range(0.1f, 100f)] private float _sensitivity = 10f;

        private Vector2 CurrentSensitivityDelta => _lookPad != null ? _lookPad.Delta * _sensitivity : Vector2.zero;

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void Update()
        {
            UpdateControllers();
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