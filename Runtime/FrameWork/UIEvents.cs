using UnityEngine.EventSystems;

namespace UIFrameWork
{
    public class UIEvents
    {
        public event System.Action<UIWidget, int> OnAnimEnd;
        public event System.Action<UIWidget, PointerEventData> OnDrag;
        public event System.Action<UIWidget, PointerEventData> OnDrop;
        public event System.Action<UIWidget, PointerEventData> OnBeginDrag;
        public event System.Action<UIWidget, PointerEventData> OnEndDrag;
        public event System.Action<UIWidget, PointerEventData> OnClick;
        public event System.Action<UIWidget, bool, PointerEventData> OnPress;
        public event System.Action<UIWidget> OnPressRepeated;
        public event System.Action<UIWidget, bool, PointerEventData> OnHover;
        public event System.Action<UIEditBox, string> OnEndEdit;
        public event System.Action<UIEditBox, string> OnValueChanged;
        public event System.Action<UIToggleButton, bool> OnCheckedChanged;
        public event System.Action<UIWidget, UI> OnSelected;

        public void TriggerOnAnimEnd(UIWidget widget, int animIndex)
        {
            if (OnAnimEnd != null)
                OnAnimEnd(widget, animIndex);
        }

        public void TriggerOnDrag(UIWidget widget, PointerEventData eventData)
        {
            if (OnDrag != null)
                OnDrag(widget, eventData);
        }

        public void TriggerOnDrop(UIWidget widget, PointerEventData eventData)
        {
            if (OnDrop != null)
                OnDrop(widget, eventData);
        }

        public void TriggerOnBeginDrag(UIWidget widget, PointerEventData eventData)
        {
            if (OnBeginDrag != null)
                OnBeginDrag(widget, eventData);
        }

        public void TriggerOnEndDrag(UIWidget widget, PointerEventData eventData)
        {
            if (OnEndDrag != null)
                OnEndDrag(widget, eventData);
        }

        public void TriggerOnClick(UIWidget widget, PointerEventData eventData)
        {
            if (OnClick != null)
                OnClick(widget, eventData);
        }

        public void TriggerOnPress(UIWidget widget, bool isPressed, PointerEventData eventData)
        {
            if (OnPress != null)
                OnPress(widget, isPressed, eventData);
        }

        public void TriggerOnPressRepeated(UIWidget widget)
        {
            if (OnPressRepeated != null)
                OnPressRepeated(widget);
        }

        public void TriggerOnHover(UIWidget widget, bool isHovering, PointerEventData eventData)
        {
            if (OnHover != null)
                OnHover(widget, isHovering, eventData);
        }

        public void TriggerOnEdit(UIEditBox editBox, string text)
        {
            if (OnEndEdit != null)
                OnEndEdit(editBox, text);
        }

        public void TriggerOnValueChanged(UIEditBox editBox, string text)
        {
            if (OnValueChanged != null)
                OnValueChanged(editBox, text);
        }

        public void TriggerOnCheckedChanged(UIToggleButton toggleButton, bool isChecked)
        {
            if (OnCheckedChanged != null)
                OnCheckedChanged(toggleButton, isChecked);
        }

        public void TriggerOnSelected(UIWidget widget, UI fromUI)
        {
            if (OnSelected != null)
                OnSelected(widget, fromUI);
        }
    }
}