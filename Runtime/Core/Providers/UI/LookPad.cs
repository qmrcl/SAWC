using UnityEngine;
using UnityEngine.EventSystems;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Look Pad")]
    public class LookPad : MonoBehaviour, IDragHandler, IPointerUpHandler
    {
        public Vector2 Delta { get; private set; }

        private Vector2 _accumulatedDelta;

        public void OnDrag(PointerEventData eventData)
        {
            _accumulatedDelta += eventData.delta;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _accumulatedDelta = Vector2.zero;
            Delta = Vector2.zero;
        }

        private void Update()
        {
            Delta = _accumulatedDelta;
        }

        private void LateUpdate()
        {
            _accumulatedDelta = Vector2.zero;
        }

        private void OnDisable()
        {
            _accumulatedDelta = Vector2.zero;
            Delta = Vector2.zero;
        }
    }
}