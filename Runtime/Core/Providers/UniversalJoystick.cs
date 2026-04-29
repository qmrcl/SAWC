using UnityEngine;
using UnityEngine.EventSystems;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/UI/Universal Joystick")]
    [RequireComponent(typeof(RectTransform))]
    public class UniversalJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IJoystickProvider
    {
        [Header("Components")]
        [SerializeField] private RectTransform _handle;

        [Header("Settings")]
        [SerializeField] private float _deadZone = 0.1f;
        [SerializeField] private float _handleLimit = 1f;

        private RectTransform _container;
        private float _radius;

        public Vector2 JoystickDirection { get; private set; }

        private void Awake()
        {
            _container = GetComponent<RectTransform>();
            
            if (_handle == null)
            {
                Debug.LogError($"[UniversalJoystick] На объекте {name} не назначена ссылка на Handle! Работать не буду.");
                enabled = false;
                return;
            }

            _radius = _container.sizeDelta.x / 2f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out var pos))
            {
                Vector2 direction = pos / _radius;

                JoystickDirection = (direction.magnitude > _deadZone) ? direction : Vector2.zero;
                
                JoystickDirection = Vector2.ClampMagnitude(JoystickDirection, 1f);

                _handle.anchoredPosition = JoystickDirection * _radius * _handleLimit;
            }
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            JoystickDirection = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }

        private void OnDisable()
        {
            JoystickDirection = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }
        
        private void OnValidate()
        {
            if (_handle == null && transform.childCount > 0)
            {
                _handle = transform.GetChild(0) as RectTransform;
            }
        }
    }
}