using UnityEngine;
using UnityEngine.EventSystems;
namespace PocketMech {
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Knob;
        public Vector2 Value { get; private set; }
        int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData e) { if (pointer != int.MinValue) return; pointer = e.pointerId; OnDrag(e); }
        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var local);
            Value = Vector2.ClampMagnitude(local / 48, 1); Knob.anchoredPosition = Value * 48;
        }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) ResetStick(); }
        public void ResetStick() { pointer = int.MinValue; Value = Vector2.zero; if (Knob != null) Knob.anchoredPosition = Vector2.zero; }
        void OnDisable() => ResetStick();
    }
    public sealed class SafeArea : MonoBehaviour
    {
        Rect previous;
        void Update()
        {
            Rect area = Screen.safeArea; if (area == previous || Screen.width == 0 || Screen.height == 0) return; previous = area;
            var r = (RectTransform)transform; r.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height); r.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
        }
    }
}


