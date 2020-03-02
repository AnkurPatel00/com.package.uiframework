using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    [RequireComponent(typeof(UIMenu))]
    public class ScrollableRect : ScrollRect 
    {
        public UIWidget _ScrollUpButton;
        public UIWidget _ScrollDownButton;
        public UIWidget _ScrollRightButton;
        public UIWidget _ScrollLeftButton;
        [Tooltip("The scroll step value in pixels")]
        public float _ScrollStep = 10f;

        private UIMenu mMenu;
		private bool mInitialized;
		private UIEvents mEventReceiver = new UIEvents();

		public void Initialize()
        {
			if (mInitialized)
				return;

			mInitialized = true;

			mMenu = GetComponent<UIMenu>();
			mEventReceiver.OnPressRepeated += OnWidgetPressRepeated;

			if (_ScrollUpButton != null)
			{
				_ScrollUpButton.pEventTarget = mEventReceiver;
				_ScrollUpButton.Initialize(mMenu, null);
			}
			if (_ScrollDownButton != null)
			{
				_ScrollDownButton.pEventTarget = mEventReceiver;
				_ScrollDownButton.Initialize(mMenu, null);
			}
			if (_ScrollRightButton != null)
			{
				_ScrollRightButton.pEventTarget = mEventReceiver;
				_ScrollRightButton.Initialize(mMenu, null);
			}
			if (_ScrollLeftButton != null)
			{
				_ScrollLeftButton.pEventTarget = mEventReceiver;
				_ScrollLeftButton.Initialize(mMenu, null);
			}
        }

        protected void OnWidgetPressRepeated(UIWidget widget)
        {
            if (widget == _ScrollUpButton && vertical)
                verticalNormalizedPosition += _ScrollStep / m_ContentBounds.size.y;
            else if (widget == _ScrollDownButton && vertical)
                verticalNormalizedPosition -= _ScrollStep / m_ContentBounds.size.y;
            else if (widget == _ScrollRightButton && horizontal)
                horizontalNormalizedPosition += _ScrollStep / m_ContentBounds.size.x;
            else if (widget == _ScrollLeftButton && horizontal)
                horizontalNormalizedPosition -= _ScrollStep / m_ContentBounds.size.x;
        }
	}
}
