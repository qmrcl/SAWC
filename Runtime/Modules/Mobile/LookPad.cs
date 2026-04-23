using UnityEngine;
using Unity.Cinemachine;
using System;

namespace SAWC.Modules.Input.TouchControls
{
    [AddComponentMenu("SAWC/Modules/Touchpad")]
    public class LookPad : InputAxisControllerBase<LookPad.TouchReader>
    {
        [SerializeField, Range(0.1f, 100f)] private float _sensitivity = 10f;

        private Vector2 _currentDelta;

        protected override void OnDisable()
        {
            base.OnDisable();
            _currentDelta = Vector2.zero;
        }

        private void Update()
        {
            UpdateControllers();
            _currentDelta = Vector2.zero;
        }

        public void ReceiveDelta(Vector2 delta)
        {
            _currentDelta = delta * _sensitivity;
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
                var controller = context as LookPad;
                if (controller == null) return 0;

                return _axisType == AxisType.Horizontal ? controller._currentDelta.x : -controller._currentDelta.y;
            }
        }
    }
}