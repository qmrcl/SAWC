using UnityEngine;
using UnityEngine.EventSystems;

namespace SAWC.Modules.Input.TouchControls
{
    public class LookPad : MonoBehaviour, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TouchInputReceiver touchInputReceiver;

        public void OnDrag(PointerEventData eventData)
        {
            touchInputReceiver.ReceiveDelta(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            touchInputReceiver.ReceiveDelta(Vector2.zero);
        }
    }
}