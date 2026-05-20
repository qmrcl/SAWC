using UnityEngine;
using UnityEngine.EventSystems;
using SAWC.Input;

namespace SAWC.Core
{
    [AddComponentMenu("SAWC/Core/Input/UI/Universal Joystick")]
    [RequireComponent(typeof(RectTransform))]
    public class UniversalJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IJoystickProvider
    {
        [Header("Components")]
        [SerializeField] private RectTransform _handle;

        [Header("Settings")]
        [SerializeField] private float _deadZone = 0.1f;
        [SerializeField] private float _handleLimit = 1f;

        private RectTransform _container;

        public Vector2 JoystickDirection { get; private set; }

        private void Awake()
        {
            _container = GetComponent<RectTransform>();

            if (_handle == null)
            {
                Debug.LogError($"[UniversalJoystick] На объекте {name} не назначена ссылка на Handle! Уволю.");
                enabled = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out var pos))
            {
                float radius = _container.rect.width / 2f;
                if (radius == 0) return;

                Vector2 rawDirection = pos / radius;
                Vector2 clampedDirection = Vector2.ClampMagnitude(rawDirection, 1f);

                _handle.anchoredPosition = clampedDirection * radius * _handleLimit;

                JoystickDirection = (clampedDirection.magnitude > _deadZone) ? clampedDirection : Vector2.zero;
            }
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetJoystick();
        }

        private void OnDisable()
        {
            ResetJoystick();
        }

        private void ResetJoystick()
        {
            JoystickDirection = Vector2.zero;
            if (_handle != null)
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