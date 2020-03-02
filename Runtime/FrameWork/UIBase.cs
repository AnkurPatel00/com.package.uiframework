using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
    public enum WidgetState
    {
        DISABLED,
        NOT_INTERACTIVE,
        INTERACTIVE
    }

    /// <summary
    /// The base class for UIs and UIWidgets
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        // Used to store the original enabled state of a Graphic before disabling it
        [System.Serializable]
        public class GraphicState
        {
            public Graphic pGraphic { get; private set; }
            public bool pOriginallyEnabled { get; set; }
            public Vector3 pOriginalPosition { get; set; }
            public Color pOriginalColor { get; set; }
            public Vector3 pOriginalScale { get; set; }
            public Sprite pOriginalSprite { get; set; }

            public GraphicState(Graphic graphic)
            {
                pGraphic = graphic;
            }
        }

        [Tooltip("This is the transform on which all child elements are parented to")]
        public RectTransform _ProxyTransform;
        /// <summary>
        /// This must not be accessed from code. Use pVisible instead.
        /// </summary>
        [SerializeField]
        protected bool _Visible = true;
        /// <summary>
        /// This must not be accessed from code. Use pState instead.
        /// </summary>
        [SerializeField]
        protected WidgetState _State = WidgetState.INTERACTIVE;

        protected List<UIWidget> mChildWidgets = new List<UIWidget>();

        protected List<GraphicState> mGraphicsStates = new List<GraphicState>();
        protected bool mGraphicsStatesCached;

        protected bool mParentVisible = true;
        protected WidgetState mParentState = WidgetState.INTERACTIVE;
        protected bool mVisible = true;
        protected WidgetState mState = WidgetState.INTERACTIVE;
        protected bool mVisibleInHierarchy = true;
        protected WidgetState mStateInHierarchy = WidgetState.INTERACTIVE;
        protected static bool IgnoreClick = true;
        public RectTransform pRectTransform { get; protected set; }
        /// <summary>
        /// Convenience function to get/set the local position
        /// </summary>
        public Vector3 pLocalPosition
        {
            // TODO: Is this required?
            get { return pRectTransform.localPosition; }
            set { pRectTransform.localPosition = value; }
        }

        /// <summary>
        /// Convenience function to get/set the anchored position
        /// </summary>
        public Vector3 pAnchoredPosition
        {
            get { return pRectTransform.anchoredPosition; }
            set { pRectTransform.anchoredPosition = value; }
        }

        /// <summary>
        /// Convenience function to get/set the world position
        /// </summary>
        public Vector3 pPosition
        {
            get { return pRectTransform.position; }
            set { pRectTransform.position = value; }
        }

        public Vector3 pLocalScale
        {
            get { return pRectTransform.localScale; }
            set { pRectTransform.localScale = value; }
        }

        public Vector2 pSize
        {
            get { return pRectTransform.sizeDelta; }
            set { pRectTransform.sizeDelta = value; }
        }

        /// <summary>
        /// Widgets and UIs trigger events on the event target
        /// </summary>
        public UIEvents pEventTarget { get; set; }

        public UI pParentUI { get; protected set; }

        public List<UIWidget> pChildWidgets { get { return mChildWidgets; } }

        public bool pInteractable { get { return mState == WidgetState.INTERACTIVE && mVisible; } }

        public bool pInteractableInHierarchy { get { return mStateInHierarchy == WidgetState.INTERACTIVE && mVisibleInHierarchy; } }

        /// <summary>
        /// The effective visibility of this element considering its parent hierarchy.
        /// Returns true if the parents of this element and itself are all visible
        /// </summary>
        public bool pVisibleInHierarchy { get { return mVisibleInHierarchy; } }

        /// <summary>
        /// The effective state of this element considering its parent hierarchy.
        /// Returns the most restrictive state among itself and its parents states with
        /// DISABLED being the most restrictive and INTERACTIVE being the least restrictive
        /// </summary>
        public WidgetState pStateInHierarchy { get { return mStateInHierarchy; } }

        /// <summary>
        /// Sets/Gets the visibility of this element.
        /// Do not override this to if you only need to be 'informed' of visibility changes. Use OnVisibleChanged() instead
        /// </summary>
        public virtual bool pVisible
        {
            get { return mVisible; }
            set
            {
                _Visible = value;
                if (mVisible != value)
                {
                    mVisible = value;
                    UpdateVisibleInHierarchy();
                    OnVisibleChanged(mVisible);
                }
            }
        }

        /// <summary>
        /// Sets/Gets the state of this element.
        /// Do not override this to if you only need to be 'informed' of state changes. Use OnStateChanged() instead
        /// </summary>
        public virtual WidgetState pState
        {
            get { return mState; }
            set
            {
                _State = value;
                if (mState != value)
                {
                    WidgetState previousState = mState;
                    mState = value;
                    OnStateChanged(previousState, mState);
                    UpdateStateInHierarchy();
                }
            }
        }

        public void OnParentSetVisible(bool parentVisible)
        {
            if (mParentVisible != parentVisible)
            {
                mParentVisible = parentVisible;
                UpdateVisibleInHierarchy();
            }
        }

        public void OnParentSetState(WidgetState parentState)
        {
            if (mParentState != parentState)
            {
                mParentState = parentState;
                UpdateStateInHierarchy();
            }
        }

        public Vector2 GetScreenPosition()
        {
            // TODO: This might not work for all cases. 
            return pPosition;
        }

        public UIWidget FindWidget(string widgetName, bool recursive = true)
        {
            foreach (UIWidget widget in mChildWidgets)
            {
                if (widget.name == widgetName)
                    return widget;
                else if (recursive)
                {
                    UIWidget childWidget = widget.FindWidget(widgetName, recursive);
                    if (childWidget != null)
                        return childWidget;
                }
            }
            return null;
        }

        public int FindWidgetIndex(UIWidget widget)
        {
            return pChildWidgets.IndexOf(widget);
        }

        public UIWidget GetWidgetAt(int index)
        {
            if (index < pChildWidgets.Count)
                return pChildWidgets[index];

            return null;
        }

        protected GraphicState FindGraphicState(Graphic graphic)
        {
            return mGraphicsStates.Find(x => x.pGraphic == graphic);
        }

        #region events
        protected virtual void OnStateChanged(WidgetState previousState, WidgetState newState) { }

        protected virtual void OnVisibleChanged(bool newVisible) { }

        protected virtual void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy) { }

        protected virtual void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy) { }
        #endregion

        /// <summary>
        /// Sets mVisibleInHierarchy and propagates it to children.
        /// Calls the OnVisibleInHierarchyChanged() event
        /// </summary>
        protected virtual void UpdateVisibleInHierarchy()
        {
            bool previousVisibleInHierarchy = mVisibleInHierarchy;
            mVisibleInHierarchy = mParentVisible ? mVisible : mParentVisible;
            if (previousVisibleInHierarchy != mVisibleInHierarchy)
            {
                foreach (UIWidget childWidget in mChildWidgets)
                    childWidget.OnParentSetVisible(mVisibleInHierarchy);

                OnVisibleInHierarchyChanged(mVisibleInHierarchy);
            }
        }

        /// <summary>
        /// Sets mStateInHierarchy and propagates it to children.
        /// Calls the OnStateInHierarchyChanged() event
        /// </summary>
        protected virtual void UpdateStateInHierarchy()
        {
            WidgetState previousStateInHierarchy = mStateInHierarchy;
            mStateInHierarchy = (mState < mParentState) ? mState : mParentState;
            if (previousStateInHierarchy != mStateInHierarchy)
            {
                foreach (UIWidget childWidget in mChildWidgets)
                    childWidget.OnParentSetState(mStateInHierarchy);

                OnStateInHierarchyChanged(previousStateInHierarchy, mStateInHierarchy);
            }
        }

        protected virtual void Start()
        {
            SetParamsFromPublicVariables();
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            SetParamsFromPublicVariables();
#endif
        }

        protected virtual void SetParamsFromPublicVariables()
        {
            if (_State != pState)
                pState = _State;
            if (_Visible != pVisible)
                pVisible = _Visible;
        }

        /// <summary>
        /// Called when the state or the visibility changes. This method can be overriden to 
        /// change the state of the associated Unity components
        /// </summary>
        protected virtual void UpdateUnityComponents()
        {
            if (!mVisibleInHierarchy)
            {
                if (mGraphicsStates != null)
                {
                    foreach (GraphicState graphicState in mGraphicsStates)
                    {
                        // When we're making the widget invisible, we cache the enabled state of all the Graphic components
                        if (graphicState != null && graphicState.pGraphic != null)
                        {
                            if (!mGraphicsStatesCached)
                                graphicState.pOriginallyEnabled = graphicState.pGraphic.enabled;

                            graphicState.pGraphic.enabled = false;
                        }
                    }
                }
                else
                    Debug.LogWarning("mGraphicsState found null in" + gameObject.name);

                mGraphicsStatesCached = true;
            }
            else if (mVisibleInHierarchy && mGraphicsStatesCached)
            {
                if (mGraphicsStates != null)
                {
                    foreach (GraphicState graphicState in mGraphicsStates)
                    {
                        if (graphicState != null && graphicState.pGraphic != null)
                            graphicState.pGraphic.enabled = graphicState.pOriginallyEnabled;
                    }
                }
                else
                    Debug.LogWarning("mGraphicsState found null in" + gameObject.name);
                mGraphicsStatesCached = false;
            }
        }
    }
}
