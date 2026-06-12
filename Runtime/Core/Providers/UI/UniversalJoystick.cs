using UnityEngine;
using UnityEngine.EventSystems;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Readers/UI/Universal Joystick")]
    [RequireComponent(typeof(RectTransform))]
    public class UniversalJoystick : BaseJoystick, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform _handle;

        [Header("Settings")]
        [SerializeField] private float _deadZone = 0.1f;
        [SerializeField] private float _handleLimit = 1f;

        private RectTransform _container;
        private Canvas _parentCanvas;
        private Vector2 _currentDirection;

        public override Vector2 JoystickDirection => _currentDirection;

        private void Awake()
        {
            _container = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();

            if (_handle == null)
            {
                Debug.LogError($"Missing Handle reference on object '{name}'!", this);
                enabled = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Camera cam = null;
            if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = eventData.pressEventCamera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, cam, out var pos))
            {
                float radius = _container.rect.width / 2f;

                if (radius <= 0.001f)
                {
                    _currentDirection = Vector2.zero;
                    return;
                }

                Vector2 rawDirection = pos / radius;
                Vector2 clampedDirection = Vector2.ClampMagnitude(rawDirection, 1f);
                _handle.anchoredPosition = clampedDirection * radius * _handleLimit;
                _currentDirection = (clampedDirection.magnitude > _deadZone) ? clampedDirection : Vector2.zero;
            }
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
        public void OnPointerUp(PointerEventData eventData) => ResetJoystick();
        private void OnDisable() => ResetJoystick();

        private void ResetJoystick()
        {
            _currentDirection = Vector2.zero;
            if (_handle != null) _handle.anchoredPosition = Vector2.zero;
        }

        private void OnValidate()
        {
            if (_handle == null && transform.childCount > 0)
                _handle = transform.GetChild(0) as RectTransform;
        }
    }
}