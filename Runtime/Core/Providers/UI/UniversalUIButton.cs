using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Universal UI Button")]
    public class UniversalUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Events")]
        public UnityEvent OnPointerDownEvent;
        public UnityEvent OnPointerUpEvent;

        private int? _pointerId;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || _pointerId.HasValue) return;
            _pointerId = eventData.pointerId;
            OnPointerDownEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId) return;
            Release();
        }

        private void OnDisable() => Release();

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) Release();
        }

        private void Release()
        {
            if (!_pointerId.HasValue) return;
            _pointerId = null;
            OnPointerUpEvent?.Invoke();
        }
    }
}
