using System;
using TweenUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    [RequireComponent(typeof(UIMenu))]
    public class PageFlip : UIBehaviour, IEndDragHandler
    {
        public Action<int> OnPageCountUpdated;
        public Action<int> OnPageChange;

        public UIWidget _Previous;
        public UIWidget _Next;
        public float _SnapDuration = 0.5f;
        public int pTotalPage { get { return mTotalPages; } }
        public int pCurrentPage { get { return mCurrentPage; } }

        private bool mIsHorizontal; //Horizontal or vertical
        private RectTransform mViewport;
        private RectTransform mContent; //The content RectTransform is what is being manipulated to show the correct page content
        private Vector3 mContentInitialPosition;
        private GridLayoutGroup mGridLayoutGroup;
        private Vector2 mPageSize;
        private int mTotalPages;
        private int mCurrentPage;
        private bool mWidgetsInitialised;
        private bool mInitialized;
        private UIEvents mEventReceiver = new UIEvents();

        public void Initialize(ScrollRect scrollRect)
        {
            if (!mInitialized)
            {
                mInitialized = true;

                mEventReceiver.OnClick += OnWidgetClick;
            }

            mContent = scrollRect.content;

            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.inertia = false; //If intertia is enabled while switching to page after drag end, scrollRect's momentum conflicts with GoTo() tweener.

            if (scrollRect.horizontal && scrollRect.vertical)
                Debug.LogError("Page flip doesn't work as expected if scroll rect is movable in both horizontal and vertical");

            mIsHorizontal = scrollRect.horizontal;
            mGridLayoutGroup = gameObject.GetComponentInChildren<GridLayoutGroup>();
            mViewport = scrollRect.viewport;
        }

        protected override void Start()
        {
            base.Start();
            mContentInitialPosition = mContent.localPosition;
            if (mGridLayoutGroup == null)
                Debug.LogError("Cannot use page flip without a grid layout group!");
            else
            {
                mPageSize = RectTransformUtility.PixelAdjustRect(mViewport, GetComponent<Canvas>()).size;
                mCurrentPage = 1;
                UpdatePageCount();
            }
            InitializeWidgets();
        }

        protected void InitializeWidgets()
        {
            UIMenu menu = GetComponent<UIMenu>();

            if (_Next != null)
            {
                _Next.pEventTarget = mEventReceiver;
                _Next.Initialize(menu, null);
            }

            if (_Previous != null)
            {
                _Previous.pEventTarget = mEventReceiver;
                _Previous.Initialize(menu, null);
            }

            mWidgetsInitialised = true;
            UpdateArrowStatus();
        }

        private void OnWidgetClick(UIWidget widget, PointerEventData eventData)
        {
            if (widget == _Previous)
                GoTo(mCurrentPage - 1);
            else if (widget == _Next)
                GoTo(mCurrentPage + 1);
        }

        public void UpdatePageCount()
        {
            if (mGridLayoutGroup == null)
                return;

            if (mContent.childCount > 0)
            {
                if (mIsHorizontal)
                    mTotalPages = (int)System.Math.Ceiling((((mGridLayoutGroup.cellSize.x + mGridLayoutGroup.spacing.x) * mContent.childCount) / mGridLayoutGroup.constraintCount / mPageSize.x));
                else
                    mTotalPages = (int)System.Math.Ceiling((((mGridLayoutGroup.cellSize.y + mGridLayoutGroup.spacing.y) * mContent.childCount) / mGridLayoutGroup.constraintCount / mPageSize.y));
            }
            else
                mTotalPages = 1;

            if (OnPageCountUpdated != null)
                OnPageCountUpdated(mTotalPages);

            GoTo(Mathf.Clamp(mCurrentPage, 1, mTotalPages));
            UpdateArrowStatus();
        }

        public void GoTo(int pageNumber)
        {
            Vector3 finalPosition = mContent.localPosition;

            if (mIsHorizontal)
                finalPosition = mContentInitialPosition - new Vector3(mPageSize.x * (pageNumber - 1), 0, 0);
            else
                finalPosition = mContentInitialPosition + new Vector3(0, mPageSize.y * (pageNumber - 1), 0);

            TweenParam tweenParam = new TweenParam(_SnapDuration);
            Tween.MoveLocalTo(mContent.gameObject, mContent.localPosition, finalPosition, tweenParam);

            mCurrentPage = pageNumber;
            UpdateArrowStatus();

            if (OnPageChange != null)
                OnPageChange(mCurrentPage);
        }

        private void UpdateArrowStatus()
        {
            if (!mWidgetsInitialised || _Next == null || _Previous == null)
                return;

            _Next.pVisible = _Previous.pVisible = (mTotalPages > 1);
            _Next.pState = (mCurrentPage < mTotalPages) ? WidgetState.INTERACTIVE : WidgetState.DISABLED;
            _Previous.pState = (mCurrentPage > 1) ? WidgetState.INTERACTIVE : WidgetState.DISABLED;
        }

        /// <summary>
        /// This will be called from 2 places - 
        /// (1) UIMenu, when dragging over widgets
        /// (2) EventSystem, when dragging over the scroll area background
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            int pageNumber = 0;
            if (mIsHorizontal)
                pageNumber = Mathf.RoundToInt((mContentInitialPosition.x - mContent.transform.localPosition.x) / mPageSize.x);
            else
                pageNumber = Mathf.RoundToInt((mContent.transform.localPosition.y - mContentInitialPosition.y) / mPageSize.y);

            pageNumber = Mathf.Clamp(pageNumber, 0, mTotalPages - 1);
            GoTo(pageNumber + 1);
        }

        public void OnMenuVisibleChanged(bool newVisible)
        {
            if (_Next != null)
                _Next.OnParentSetVisible(newVisible);

            if (_Previous != null)
                _Previous.OnParentSetVisible(newVisible);
        }

        public void OnMenuStateChanged(WidgetState newState)
        {
            if (_Next != null)
                _Next.OnParentSetState(newState);

            if (_Previous != null)
                _Previous.OnParentSetState(newState);
        }
    }
}