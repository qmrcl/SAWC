using UnityEngine;
using UnityEngine.EventSystems;

namespace SAWC.Modules.Input.TouchControls
{
    public class TouchZone : MonoBehaviour, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private LookPad _lookPad;

        public void OnDrag(PointerEventData eventData)
        {
            _lookPad.ReceiveDelta(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _lookPad.ReceiveDelta(Vector2.zero);
        }
    }
}