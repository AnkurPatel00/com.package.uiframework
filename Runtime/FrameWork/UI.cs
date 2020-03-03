using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [DisallowMultipleComponent]
    public class UI : UIBase
    {
        public static UI _GlobalExclusiveUI = null;

        public UIButton _BackButton;
        protected List<UI> mChildUIs = new List<UI>();
        private UIManager mUIManager;
        [NonSerialized]
        private bool mInitialized;
        private bool mOnInitializeCalled;
        private Dictionary<string, HashSet<UIToggleButton>> mToggleButtonGroups = new Dictionary<string, HashSet<UIToggleButton>>();

        public System.Object pCustomData { get; set; }

        public Canvas pCanvas { get; private set; }

        public CanvasScaler pCanvasScaler { get; private set; }

        public float pScaleFactor
        {
            get { return pCanvas.scaleFactor; }
            set { pCanvas.scaleFactor = value; }
        }

        public List<UI> pChildUIs { get { return mChildUIs; } }

        /// <summary>
        /// UIs receive events by registering for events in this object. Listeners need not be 
        /// unregistered as long as the listener is a method on the same UI
        /// </summary>
        public UIEvents pEventReceiver { get; protected set; }

        public virtual void AddWidget(UIWidget widget)
        {
            if (widget.pParentWidget == null && widget.pParentUI != null)
                widget.pParentUI.RemoveWidget(widget, false, false);
            else if (widget.pParentWidget != null)
                widget.pParentWidget.RemoveWidget(widget, false, false);

            mChildWidgets.Add(widget);
            widget.transform.SetParent(_ProxyTransform);
            widget.pEventTarget = pEventReceiver;
            widget.Initialize(this, null);
        }

        /// <param name="widget">The widget to remove</param>
        /// <param name="destroy">Destroy the widget's game object after removing?</param>
        /// <param name="removeReferences">
        /// 	Remove references to the parent UI, event target etc and unparent the game object.
        ///		Setting this to false will leave the object with invalid references. 
        ///		However if you will be overwriting references immediately after this call, this
        ///		will save unnecessary processing
        /// </param>
        public virtual void RemoveWidget(UIWidget widget, bool destroy = false, bool removeReferences = true)
        {
            // This method must be kept in sync with ClearChildren() - any additional processing done there
            // while destroying a widget might have to be done here, and vice versa
            if (widget.pParentWidget == null && widget.pParentUI == this)
            {
                mChildWidgets.Remove(widget);

                if (destroy)
                    Destroy(widget.gameObject);
                else if (removeReferences)
                {
                    widget.transform.SetParent(null);
                    widget.pEventTarget = null;
                    widget.Initialize(null, null);
                }
            }
        }

        public int FindUIIndex(UI ui)
        {
            return mChildUIs.IndexOf(ui);
        }

        public UI GetUIAt(int index)
        {
            if (index < mChildUIs.Count)
                return mChildUIs[index];

            return null;
        }

        public void AddUI(UI ui)
        {
            if (ui.pParentUI != null)
                ui.pParentUI.RemoveUI(ui);

            mChildUIs.Add(ui);
            ui.transform.SetParent(_ProxyTransform);
            ui.pEventTarget = pEventReceiver;
            ui.Initialize(this);
            ui.TriggerOnInitializeRecursive();
        }

        public void RemoveUI(UI ui, bool destroy = false)
        {
            // This method must be kept in sync with ClearChildren() - any additional processing done there
            // while destroying a ui might have to be done here, and vice versa
            if (ui.pParentUI == this)
            {
                mChildUIs.Remove(ui);

                if (destroy)
                    GameObject.Destroy(ui.gameObject);
                else
                {
                    ui.transform.SetParent(null);
                    ui.pEventTarget = null;
                    ui.Initialize(null);
                }
            }
        }

        public virtual void ClearChildren()
        {
            // This method must be kept in sync with RemoveWidget() & RemoveUI() - any additional processing done there
            // while destroying a ui/widget might have to be done here, and vice versa
            foreach (UIWidget childWidget in mChildWidgets)
                Destroy(childWidget.gameObject);
            mChildWidgets.Clear();

            foreach (UI childUI in mChildUIs)
                Destroy(childUI.gameObject);
            mChildUIs.Clear();
        }

        public void RegisterToggleButtonInGroup(string groupName, UIToggleButton toggleButton)
        {
            HashSet<UIToggleButton> toggleButtonsInGroup;

            if (mToggleButtonGroups.TryGetValue(groupName, out toggleButtonsInGroup))
                toggleButtonsInGroup.Add(toggleButton);
            else
            {
                toggleButtonsInGroup = new HashSet<UIToggleButton>();
                toggleButtonsInGroup.Add(toggleButton);
                mToggleButtonGroups[groupName] = toggleButtonsInGroup;
            }
        }

        public void UnregisterToggleButtonInGroup(string groupName, UIToggleButton toggleButton)
        {
            HashSet<UIToggleButton> toggleButtonsInGroup;

            if (mToggleButtonGroups.TryGetValue(groupName, out toggleButtonsInGroup))
                toggleButtonsInGroup.Remove(toggleButton);
            else
                Debug.LogWarning("Toggle Button group name not found: " + groupName);
        }

        public void UncheckOtherToggleButtonsInGroup(string groupName, UIToggleButton otherThan)
        {
            HashSet<UIToggleButton> toggleButtonsInGroup;

            if (mToggleButtonGroups.TryGetValue(groupName, out toggleButtonsInGroup))
            {
                foreach (UIToggleButton toggleButton in toggleButtonsInGroup)
                {
                    if (toggleButton != otherThan)
                        toggleButton.pChecked = false;
                }
            }
        }

        public UIToggleButton GetCheckedToggleButtonInGroup(string groupName)
        {
            HashSet<UIToggleButton> toggleButtonsInGroup;

            if (mToggleButtonGroups.TryGetValue(groupName, out toggleButtonsInGroup))
            {
                foreach (UIToggleButton toggleButton in toggleButtonsInGroup)
                {
                    if (toggleButton.pChecked)
                        return toggleButton;
                }
            }

            return null;
        }

        #region Events
        /// <summary>
        /// This is called after this UI and the child UIs under this are all initialized. The child UIs+Widgets
        /// structure is guaranteed to be setup when this is called
        /// </summary>
        protected virtual void OnInitialize() { }

        protected virtual void OnAnimEnd(UIWidget widget, int animIdx) { }

        protected virtual void OnDrag(UIWidget widget, PointerEventData eventData) { }

        protected virtual void OnDrop(UIWidget widget, PointerEventData eventData) { }

        protected virtual void OnBeginDrag(UIWidget widget, PointerEventData eventData) { }

        protected virtual void OnEndDrag(UIWidget widget, PointerEventData eventData) { }

        protected virtual void OnClick(UIWidget widget, PointerEventData eventData) { }

        protected virtual void OnPress(UIWidget widget, bool isPressed, PointerEventData eventData) { }

        protected virtual void OnPressRepeated(UIWidget widget) { }

        protected virtual void OnHover(UIWidget widget, bool isHovering, PointerEventData eventData) { }

        protected virtual void OnEndEdit(UIEditBox editBox, string text) { }

        protected virtual void OnValueChanged(UIEditBox editBox, string text) { }

        protected virtual void OnCheckedChanged(UIToggleButton toggleButton, bool isChecked) { }

        /// <summary>
        ///	A widget in a menu has been selected.
        ///	<param name="widget">The widget that has been selected. Will be null if all widgets are deselected</param>
        ///	<param name="fromUI">The UI which sent the event. Note that this UI need not be the widget's parent UI</param>
        /// </summary>
        protected virtual void OnSelected(UIWidget widget, UI fromUI) { }
        #endregion

        /// <summary>
        /// Performs initial setup. Should be called only once per UI.
        /// Game code must not override this method. Use OnInitialize() instead
        /// </summary>
        protected virtual void Initialize(UI parentUI)
        {
            if (!mInitialized)
            {
                mInitialized = true;

                pRectTransform = transform as RectTransform;
                if (_ProxyTransform == null)
                    _ProxyTransform = pRectTransform;
                pCanvas = GetComponent<Canvas>();
                pCanvasScaler = GetComponent<CanvasScaler>();
                mUIManager = UIManager.pInstance;
                if (mUIManager == null)
                    Debug.LogError("No UIManager in the scene");

                pEventReceiver = new UIEvents();
                pEventReceiver.OnAnimEnd += OnAnimEnd;
                pEventReceiver.OnDrag += OnDrag;
                pEventReceiver.OnDrop += OnDrop;
                pEventReceiver.OnBeginDrag += OnBeginDrag;
                pEventReceiver.OnEndDrag += OnEndDrag;
                pEventReceiver.OnClick += OnClick;
                pEventReceiver.OnPress += OnPress;
                pEventReceiver.OnPressRepeated += OnPressRepeated;
                pEventReceiver.OnHover += OnHover;
                pEventReceiver.OnEndEdit += OnEndEdit;
                pEventReceiver.OnValueChanged += OnValueChanged;
                pEventReceiver.OnCheckedChanged += OnCheckedChanged;
                pEventReceiver.OnSelected += OnSelected;

                CacheWidgets(_ProxyTransform);
                mGraphicsStatesCached = true;
            }

            pParentUI = parentUI;

            // We can't call SetParamsFromPublicVariables() here since that would trigger 
            // a series of recursive calls
            mState = _State;
            mVisible = _Visible;
            if (parentUI != null)
            {
                mParentState = parentUI.pStateInHierarchy;
                mParentVisible = parentUI.pVisibleInHierarchy;
            }

            WidgetState previousStateInHierarchy = mStateInHierarchy;
            //bool previousVisibleInHierarchy = mVisibleInHierarchy;

            mStateInHierarchy = (mState < mParentState) ? mState : mParentState;
            mVisibleInHierarchy = mParentVisible ? mVisible : mParentVisible;

            OnStateInHierarchyChanged(previousStateInHierarchy, mStateInHierarchy);
            OnVisibleInHierarchyChanged(mVisibleInHierarchy);

            foreach (UIWidget childWidget in mChildWidgets)
            {
                childWidget.pEventTarget = pEventReceiver;
                childWidget.Initialize(this, null);
            }
            foreach (UI childUI in mChildUIs)
            {
                childUI.pEventTarget = pEventReceiver;
                childUI.Initialize(this);
            }
        }

        /// <summary>
        /// Recursively call OnInitalize() on child UIs first and then on self.
        /// This must be called only on the Root UI
        /// </summary>
        private void TriggerOnInitializeRecursive()
        {
            foreach (UI childUI in mChildUIs)
                childUI.TriggerOnInitializeRecursive();

            if (!mOnInitializeCalled)
            {
                mOnInitializeCalled = true;
                OnInitialize();
            }
        }

        /// <summary>
        /// Search for UIWidget, UI and Graphic components in child transforms and store references to them
        /// Also assign this UI's pEventReceiver as child widgets' and child UIs' pEventTarget
        /// </summary>
        protected void CacheWidgets(Transform cacheTransform)
        {
            int childCount = cacheTransform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform childTransform = cacheTransform.GetChild(i);

                UIWidget childWidget = childTransform.GetComponent<UIWidget>();
                if (childWidget != null)
                    mChildWidgets.Add(childWidget);
                else
                {
                    UI childUI = childTransform.GetComponent<UI>();
                    if (childUI != null)
                        mChildUIs.Add(childUI);
                    else
                    {
                        Graphic childGraphic = childTransform.GetComponent<Graphic>();
                        if (childGraphic != null && !(childGraphic is NonDrawableGraphic))
                        {
                            GraphicState graphicState = new GraphicState(childGraphic);
                            graphicState.pOriginallyEnabled = true;
                            mGraphicsStates.Add(graphicState);
                        }

                        if (childTransform.childCount > 0)
                            CacheWidgets(childTransform);
                    }
                }
            }
        }

        protected override void UpdateUnityComponents()
        {
            base.UpdateUnityComponents();

            pCanvas.enabled = mVisibleInHierarchy;
            if (pCanvasScaler != null)
                pCanvasScaler.enabled = mVisibleInHierarchy;
        }

        protected sealed override void UpdateStateInHierarchy()
        {
            WidgetState previousStateInHierarchy = mStateInHierarchy;
            base.UpdateStateInHierarchy();

            if (previousStateInHierarchy != mStateInHierarchy)
            {
                foreach (UI childUI in mChildUIs)
                    childUI.OnParentSetState(mStateInHierarchy);
            }
        }

        protected sealed override void UpdateVisibleInHierarchy()
        {
            bool previousVisibleInHierarchy = mVisibleInHierarchy;
            base.UpdateVisibleInHierarchy();

            if (previousVisibleInHierarchy != mVisibleInHierarchy)
            {
                foreach (UI childUI in mChildUIs)
                    childUI.OnParentSetVisible(mVisibleInHierarchy);
            }
        }

        protected override void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
        {
            UpdateUnityComponents();
        }

        protected override void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
        {
            UpdateUnityComponents();
        }

        protected virtual void Awake()
        {
            // Only the root UI calls CacheWidgets() in Awake()
            if (transform.parent == null || transform.parent.GetComponentInParent<UI>() == null)
            {
                Initialize(null);
                TriggerOnInitializeRecursive();
            }
        }

        public virtual void SetExclusive()
        {
            SetExclusive(new Color(0, 0, 0, 0.5f));
        }

        public void SetExclusive(Color color)
        {
            Image maskImage = GetComponent<Image>();
            if (maskImage == null)
                maskImage = gameObject.AddComponent<Image>();
            maskImage.sprite = null;
            maskImage.color = color;
            maskImage.raycastTarget = true;
            UIManager.pInstance.AddToExclusiveListOnTop(this);
        }

        public void RemoveExclusive()
        {
            Image maskImage = gameObject.GetComponent<Image>();
            if (maskImage != null)
                Destroy(maskImage);
            if (UIManager.pInstance != null)
                UIManager.pInstance.RemoveFromExclusiveList(this);
        }
    }
}
