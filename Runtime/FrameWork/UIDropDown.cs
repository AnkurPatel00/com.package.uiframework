using UnityEngine.EventSystems;

namespace UIFrameWork
{
    public class UIDropDown : UI
    {
        public string _DefaultText;
        public UIWidget _DisplayWidget;
        public UIMenu _Menu;
        public bool _IsDropped;

        public bool pIsDropped
        {
            get { return _Menu.pVisible; }
            set
            {
                _IsDropped = value;
                _Menu.pVisible = value;
            }
        }

        public UIWidget pSelectedWidget
        {
            get { return _Menu.pSelectedWidget; }
            set { _Menu.pSelectedWidget = value; }
        }

        protected override void Initialize(UI parentUI)
        {
            base.Initialize(parentUI);

            _DisplayWidget.pText = _DefaultText;
        }

        protected override void OnClick(UIWidget widget, PointerEventData eventData)
        {
            if (widget == _DisplayWidget)
                pIsDropped = !pIsDropped;
            else if (widget.pParentUI == _Menu)
            {
                // Forward _Menu widget click to parent
                if (pEventTarget != null)
                    pEventTarget.TriggerOnClick(widget, eventData);
            }
        }

        protected override void OnSelected(UIWidget widget, UI fromUI)
        {
            base.OnSelected(widget, fromUI);

            if (widget == null)
                _DisplayWidget.pText = _DefaultText;
            else
            {
                _DisplayWidget.pText = widget.pText;
                if (widget.pSprite != null)
                    _DisplayWidget.pSprite = widget.pSprite;
                pIsDropped = false;
            }
            // Forward event to parent
            if (pEventTarget != null)
                pEventTarget.TriggerOnSelected(widget, this);
        }

        protected override void SetParamsFromPublicVariables()
        {
            base.SetParamsFromPublicVariables();

            if (pIsDropped != _IsDropped)
                pIsDropped = _IsDropped;
        }
    }
}