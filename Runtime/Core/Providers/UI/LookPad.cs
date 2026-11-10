using UnityEngine;
using UnityEngine.EventSystems;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Look Pad")]
    public class LookPad : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Vector2 _accumulatedDelta;
        private int? _pointerId;

        public Vector2 Delta => _accumulatedDelta;

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || _pointerId != eventData.pointerId) return;
            _accumulatedDelta += eventData.delta;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isActiveAndEnabled && !_pointerId.HasValue)
                _pointerId = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerId == eventData.pointerId)
                _pointerId = null;
        }

        private void LateUpdate()
        {
            _accumulatedDelta = Vector2.zero;
        }

        private void OnDisable()
        {
            _pointerId = null;
            _accumulatedDelta = Vector2.zero;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) OnDisable();
        }
    }
}
