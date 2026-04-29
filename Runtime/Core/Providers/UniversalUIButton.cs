using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/UI/Universal UI Button")]
    public class UniversalUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Events")]
        public UnityEvent OnPointerDownEvent;
        public UnityEvent OnPointerUpEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke();
        }
    }
}