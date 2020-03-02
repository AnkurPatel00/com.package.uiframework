using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utility;

namespace UIFrameWork
{
    public class UIMenu : UI
    {
        public UIWidget _Template;
        public MinMax _WidgetPoolCount;
        public RectTransform _ViewPortRectTransform;
        public bool pIsPageFlipEnabled { get { return (mPageFlip != null); } }
        public MaskableGraphic _SelectionHighlight;
        public MaskableGraphic _HoverHighlight;
        public bool _UpdateOnlyOnVisible;

        protected ScrollRect pScrollRect { get; private set; }
        protected LayoutGroup pLayoutGroup { get; private set; }

        [NonSerialized]
        private bool mInitialized;
        private GameObject mWidgetPoolParent;
        private UIWidget mSelectedWidget;
        private List<UIWidget> mWidgetPool = new List<UIWidget>();
        private PageFlip mPageFlip;
        private Vector2 mPrevScrollValue = Vector2.zero;
        private List<UIWidget> mCurrentVisibleWidgets = new List<UIWidget>();
        private List<UIWidget> mNewVisibleWidgets = new List<UIWidget>();
        private CenterOnWidget mCenterOnWidget;

        private const string POOLED_WIDGET_NAME = "PooledWidget";
        private const string WIDGET_POOL_PARENT_NAME = "PoolParent";
        private const string WIDGET_POOL_COUNT_ERROR = "_WidgetPoolCount.Max cannot be lesser than _WidgetPoolCount.Min";

        private bool pIsWidgetPoolingEnabled
        {
            get { return _WidgetPoolCount.Min > 0 || _WidgetPoolCount.Max > 0; }
        }

        public UIWidget pSelectedWidget
        {
            get { return mSelectedWidget; }
            set
            {
                mSelectedWidget = value;
                UpdateHighlightState(_SelectionHighlight, mSelectedWidget);
                if (pEventTarget != null)
                    pEventTarget.TriggerOnSelected(mSelectedWidget, this);
            }
        }

        public UIWidget AddWidget(string widgetName)
        {
            UIWidget widget = GetWidgetFromPool();
            if (widget == null)
            {
                if (_Template != null)
                    widget = _Template.Duplicate();
                else
                    Debug.LogError("Template item null in gameObject: " + gameObject);
            }

            widget.gameObject.SetActive(true);
            widget.name = widgetName;
            AddWidget(widget);
            widget.pVisible = true;
            widget.transform.localScale = _Template.transform.localScale;

            OnAddingWidget(widget);
            return widget;
        }

        protected virtual void OnAddingWidget(UIWidget widget)
        {
            if (pIsPageFlipEnabled)
                mPageFlip.UpdatePageCount();

            if (_UpdateOnlyOnVisible)
                StartCoroutine(UpdateWidgetsInViewWithDelay());

            if (mCenterOnWidget != null)
                mCenterOnWidget.UpdateCache();
        }

        public override void RemoveWidget(UIWidget widget, bool destroy = false, bool removeReferences = true)
        {
            if (pIsWidgetPoolingEnabled)
            {
                base.RemoveWidget(widget, false, true);
                AddWidgetToPool(widget);
            }
            else
                base.RemoveWidget(widget, destroy, removeReferences);

            if (widget == pSelectedWidget)
                pSelectedWidget = null;

            OnRemovingWidget();
        }

        protected virtual void OnRemovingWidget()
        {
            if (pIsPageFlipEnabled)
                mPageFlip.UpdatePageCount();

            if (_UpdateOnlyOnVisible)
                StartCoroutine(UpdateWidgetsInViewWithDelay());

            if (mCenterOnWidget != null)
                mCenterOnWidget.UpdateCache();
        }

        public override void ClearChildren()
        {
            if (pIsWidgetPoolingEnabled)
            {
                int widgetCount = pChildWidgets.Count;
                for (int i = widgetCount - 1; i >= 0; --i)
                {
                    UIWidget widget = pChildWidgets[i];
                    RemoveWidget(widget, false, false);
                }
            }

            base.ClearChildren();
            pSelectedWidget = null;
        }

        #region Events
        public virtual void UnloadWidget(UIWidget widget) { }

        public virtual void LoadWidget(UIWidget widget) { }
        #endregion

        protected override void Initialize(UI parentUI)
        {
            base.Initialize(parentUI);

            if (!mInitialized)
            {
                mInitialized = true;
                pScrollRect = GetComponent<ScrollRect>();
                //TODO: temporarily calling ScrollableRect initialize here to make sure the pEventReciever used in ScrollableRect is never null
                if (pScrollRect != null)
                {
                    ScrollableRect scrollableRect = pScrollRect as ScrollableRect;
                    if (scrollableRect != null)
                        scrollableRect.Initialize();
                }

                pLayoutGroup = _ProxyTransform.GetComponent<LayoutGroup>();

                // Adding another listener rather than overriding UI.OnClick() so that child classes of UIMenu 
                // don't need to call base.OnClick()
                pEventReceiver.OnClick += OnMenuWidgetClick;
                pEventReceiver.OnHover += OnMenuWidgetHover;
                pEventReceiver.OnBeginDrag += OnMenuWidgetBeginDrag;
                pEventReceiver.OnDrag += OnMenuWidgetDrag;
                pEventReceiver.OnEndDrag += OnMenuWidgetEndDrag;
                if (pScrollRect != null && _UpdateOnlyOnVisible)
                    pScrollRect.onValueChanged.AddListener(OnScrollValueChanged);

                if (_HoverHighlight != null)
                    _HoverHighlight.enabled = false;

                if (_SelectionHighlight != null)
                    _SelectionHighlight.enabled = false;

                if (pIsWidgetPoolingEnabled && _Template != null)
                    CreateWidgetPool();

                InitPageFlip();
                InitCenterOnWidget();
            }
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            if (mPrevScrollValue != value)
            {
                mPrevScrollValue = value;
                UpdateWidgetsInView();
            }
        }

        IEnumerator UpdateWidgetsInViewWithDelay()
        {
            yield return new WaitForEndOfFrame();
            UpdateWidgetsInView();
        }

        private void UpdateWidgetsInView()
        {
            GetWidgetsInView(mNewVisibleWidgets);
            if (mNewVisibleWidgets != null)
            {
                foreach (UIWidget widget in mCurrentVisibleWidgets)
                {
                    if (!mNewVisibleWidgets.Contains(widget))
                        UnloadWidget(widget);
                }

                foreach (UIWidget widget in mNewVisibleWidgets)
                {
                    if (!mCurrentVisibleWidgets.Contains(widget))
                        LoadWidget(widget);
                }

                // Swap the current and new lists
                List<UIWidget> tempList = mCurrentVisibleWidgets;
                mCurrentVisibleWidgets = mNewVisibleWidgets;
                mNewVisibleWidgets = tempList;
            }
            mNewVisibleWidgets.Clear();
        }

        /// <summary>
        /// Gets the widgets in view.
        /// </summary>
        /// <param name="widgetList">Widget list.</param>
        private void GetWidgetsInView(List<UIWidget> widgetList)
        {
            widgetList.Clear();
            if (pScrollRect == null || pScrollRect.viewport == null || pChildWidgets.Count == 0)
                return;

            for (int i = 0; i < pChildWidgets.Count; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(pScrollRect.viewport, pChildWidgets[i].GetScreenPosition()))
                    widgetList.Add(pChildWidgets[i]);
            }
        }

        /// <summary>
        /// Initialises the page flip if it is available
        /// </summary>
        private void InitPageFlip()
        {
            mPageFlip = GetComponent<PageFlip>();

            if (mPageFlip != null)
                mPageFlip.Initialize(pScrollRect);
        }

        private void InitCenterOnWidget()
        {
            mCenterOnWidget = GetComponent<CenterOnWidget>();
        }

        protected void OnMenuWidgetClick(UIWidget widget, PointerEventData eventData)
        {
            // Forward event to parent
            if (pEventTarget != null)
                pEventTarget.TriggerOnClick(widget, eventData);
            pSelectedWidget = widget;
            UpdateHighlightState(_SelectionHighlight, widget);
        }

        protected void OnMenuWidgetHover(UIWidget widget, bool hover, PointerEventData eventData)
        {
            UpdateHighlightState(_HoverHighlight, widget, hover);
        }

        protected void OnMenuWidgetBeginDrag(UIWidget widget, PointerEventData eventData)
        {
            // Forward event to ScrollRect to handle scrolling
            if (pScrollRect != null && pScrollRect.enabled)
                pScrollRect.OnBeginDrag(eventData);

            if (mCenterOnWidget != null)
                mCenterOnWidget.StartDrag();
        }

        protected void OnMenuWidgetDrag(UIWidget widget, PointerEventData eventData)
        {
            // Forward event to ScrollRect to handle scrolling
            if (pScrollRect != null && pScrollRect.enabled)
                pScrollRect.OnDrag(eventData);
        }

        protected void OnMenuWidgetEndDrag(UIWidget widget, PointerEventData eventData)
        {
            // Forward event to ScrollRect to handle scrolling
            if (pScrollRect != null && pScrollRect.enabled)
            {
                pScrollRect.OnEndDrag(eventData);

                if (pIsPageFlipEnabled)
                    mPageFlip.OnEndDrag(eventData);
            }

            if (mCenterOnWidget != null)
                mCenterOnWidget.EndDrag();
        }

        protected void CreateWidgetPool()
        {
            if (_WidgetPoolCount.Max < _WidgetPoolCount.Min)
            {
                Debug.LogError(WIDGET_POOL_COUNT_ERROR);
                return;
            }

            mWidgetPoolParent = new GameObject(WIDGET_POOL_PARENT_NAME);
            mWidgetPoolParent.transform.SetParent(transform);

            for (int i = 0; i < _WidgetPoolCount.Min; ++i)
            {
                UIWidget widgetToPool = _Template.Duplicate();
                AddWidgetToPool(widgetToPool);
            }
        }

        protected void AddWidgetToPool(UIWidget widget)
        {
            if (mWidgetPool.Count < _WidgetPoolCount.Max)
            {
                widget.pVisible = false;
                widget.transform.SetParent(mWidgetPoolParent.transform);
                widget.name = POOLED_WIDGET_NAME;
                mWidgetPool.Add(widget);
            }
            else
                Destroy(widget.gameObject);
        }

        protected UIWidget GetWidgetFromPool()
        {
            if (mWidgetPool.Count == 0)
                return null;

            UIWidget widget = mWidgetPool[mWidgetPool.Count - 1];
            mWidgetPool.RemoveAt(mWidgetPool.Count - 1);
            return widget;
        }

        protected int GetNumItemsPerPage()
        {
            GridLayoutGroup gridLayoutGroup = pLayoutGroup as GridLayoutGroup;
            if (gridLayoutGroup == null)
                return 0;
            Rect rect = RectTransformUtility.PixelAdjustRect(_ViewPortRectTransform, pCanvas);

            rect.width -= (gridLayoutGroup.padding.left + gridLayoutGroup.padding.right);
            rect.height -= (gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom);

            float canavasArea = rect.width * rect.height;
            float itemArea = (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x) * (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y);

            return Mathf.CeilToInt(canavasArea / itemArea);
        }

        void UpdateHighlightState(MaskableGraphic highlightGraphic, UIWidget widget, bool highlight = true)
        {
            if (highlightGraphic == null)
                return;

            if (widget != null && widget.pParentWidget == null)
            {
                if (highlight)
                {
                    highlightGraphic.rectTransform.SetParent(widget.pRectTransform);
                    highlightGraphic.rectTransform.anchoredPosition = Vector3.zero;
                    highlightGraphic.enabled = true;
                }
                else
                {
                    highlightGraphic.rectTransform.SetParent(pRectTransform);
                    highlightGraphic.enabled = false;
                }
            }
        }

        protected override void OnVisibleInHierarchyChanged(bool newVisible)
        {
            base.OnVisibleInHierarchyChanged(newVisible);
            if (pIsPageFlipEnabled)
                mPageFlip.OnMenuVisibleChanged(newVisible);

            if (_UpdateOnlyOnVisible && newVisible)
                UpdateWidgetsInView();
        }

        protected override void OnStateInHierarchyChanged(WidgetState previousState, WidgetState newState)
        {
            base.OnStateInHierarchyChanged(previousState, newState);
            if (pIsPageFlipEnabled)
                mPageFlip.OnMenuStateChanged(newState);
        }
    }
}
