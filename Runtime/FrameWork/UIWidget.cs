using MonoUtility.Input;
using System;
using TweenUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    // Analogous to Unity's Selectable. Augments or replaces Selectables.
    public class UIWidget : UIBase, IDropHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Vector3 _RotationPerSecond;

        [Header("Unity Component References")]
        public Text _Text;
        public Image _Background;
        public RawImage _RawImageBackground;

        [Header("Effects")]
        public UIEffects _ClickEffects;
        public UIEffects _DisabledEffects;
        public UIEffects _HoverEffects;
        public UIEffects _PressEffects;

        protected Selectable mSelectable;

        private NonDrawableGraphic mRaycastCaptureGraphic;
        [NonSerialized]
        private bool mInitialized;
        private UIAnim2D mAnim2D;
        private bool mIsHovering;
        private bool mIsPressed;
        protected bool mIsDragging;
        private bool mIsAttachedToPointer;
        private Vector2 mPointerAttachOffset;
        private int mFingerIDAttachedTo = -1;
        // Use this to cache the value of pCanReceiveInput
        private bool mPreviousCanReceiveInput;

        private UIEffects mCurrentEffect;
        private float mCurrentEffectStartTime;
        private UIEffects mCurrentOneTimeEffect;

        public UIAnim2D pAnim2D { get { return mAnim2D; } }

        public UIWidget pParentWidget { get; protected set; }

        public bool pIsAttachedToPointer { get { return mIsAttachedToPointer; } }

        public object pData { get; set; }

        public Color pColor
        {
            get { return _Background.color; }
            set
            {
                _Background.color = value;
                GraphicState graphicState = FindGraphicState(_Background);
                if (graphicState != null)
                    graphicState.pOriginalColor = value;
            }
        }

        public Color pTextureColor
        {
            get { return _RawImageBackground.color; }
            set
            {
                _RawImageBackground.color = value;
                GraphicState graphicState = FindGraphicState(_RawImageBackground);
                if (graphicState != null)
                    graphicState.pOriginalColor = value;
            }
        }

        /// <summary>
        /// Convenience method to directly set the alpha of the widget's color
        /// </summary>
        public float pAlpha
        {
            get { return _Background.color.a; }
            set
            {
                Color color = _Background.color;
                color.a = value;
                _Background.color = color;
                GraphicState graphicState = FindGraphicState(_Background);
                if (graphicState != null)
                    graphicState.pOriginalColor = color;
            }
        }

        public Sprite pSprite
        {
            get
            {
                if (_Background == null)
                    return null;
                else
                    return _Background.sprite;
            }
            set
            {
                _Background.sprite = value;
                GraphicState graphicState = FindGraphicState(_Background);
                if (graphicState != null)
                    graphicState.pOriginalSprite = value;
            }
        }

        public Texture pTexture
        {
            get { return _RawImageBackground.texture; }
            set { _RawImageBackground.texture = value; }
        }

        public virtual string pText
        {
            get { return _Text.text; }
            set { _Text.text = value; }
        }

        public Color pTextColor
        {
            get { return _Text.color; }
            set
            {
                _Text.color = value;
                GraphicState graphicState = FindGraphicState(_Text);
                if (graphicState != null)
                    graphicState.pOriginalColor = value;
            }
        }

        public bool pCanReceiveInput
        {
            get { return mRaycastCaptureGraphic.raycastTarget; }
            set { mRaycastCaptureGraphic.raycastTarget = value; }
        }

        public void SetAnim2D(UIAnim2D anim2D)
        {
            mAnim2D = anim2D;
        }

        public void Anim2DAnimEnded(int animIdx)
        {
            if (pEventTarget != null)
                pEventTarget.TriggerOnAnimEnd(this, animIdx);
        }

        public void AttachToPointer(int fingerID = -1)
        {
            AttachToPointer(Vector2.zero, fingerID);
        }

        public void AttachToPointer(Vector2 pointerAttachOffset, int fingerID = -1)
        {
            if (!mIsAttachedToPointer)
                mPreviousCanReceiveInput = pCanReceiveInput;
            mIsAttachedToPointer = true;
            mFingerIDAttachedTo = fingerID;
            mPointerAttachOffset = pointerAttachOffset;
            pCanReceiveInput = false;
        }

        public void DetachFromPointer()
        {
            if (mIsAttachedToPointer)
                pCanReceiveInput = mPreviousCanReceiveInput;
            mIsAttachedToPointer = false;
            mFingerIDAttachedTo = -1;
        }

        public void ResetEffects()
        {
            ApplyEffect(null);
        }

        public void Reset()
        {
            // TODO: Must be implemented
        }

        public void AddWidget(UIWidget widget)
        {
            if (widget.pParentWidget == null && widget.pParentUI != null)
                widget.pParentUI.RemoveWidget(widget, false, false);
            else if (widget.pParentWidget != null)
                widget.pParentWidget.RemoveWidget(widget, false, false);

            mChildWidgets.Add(widget);
            widget.transform.SetParent(_ProxyTransform);
            widget.pEventTarget = pEventTarget;
            widget.Initialize(pParentUI, this);
        }

        /// <param name="widget">The widget to remove</param>
        /// <param name="destroy">Destroy the widget's game object after removing?</param>
        /// <param name="removeReferences">
        /// Remove references to the parent UI, event target etc and unparent the game object.
        ///	Setting this to false will leave the object with invalid references. 
        ///	However if you will be overwriting references immediately after this call, this
        ///	will save unnecessary processing
        /// </param>
        public void RemoveWidget(UIWidget widget, bool destroy = false, bool removeReferences = true)
        {
            // This method must be kept in sync with ClearChildren() - any additional processing done there
            // while destroying a widget might have to be done here, and vice versa
            if (widget.pParentWidget == this)
            {
                mChildWidgets.Remove(widget);
                // We set pParentWidget to null here irrespective of removeReferences since it's an inexpensive call
                // and UI.AddWidget() will not be setting this
                widget.pParentWidget = null;

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

        public void ClearChildren()
        {
            // This method must be kept in sync with RemoveWidget() - any additional processing done there
            // while destroying a widget might have to be done here, and vice versa
            foreach (UIWidget childWidget in mChildWidgets)
                Destroy(childWidget.gameObject);
            mChildWidgets.Clear();
        }

        /// <summary>
        /// Create a duplicate of this widget
        /// </summary>
        /// <param name="autoAddToSameParent">
        ///	Should the duplicate widget be automatically added to the same parent as this widget?
        ///	Set this to false if you will be adding the duplicate widget to another widget/UI
        ///	immediately after this call
        /// </param>
        public UIWidget Duplicate(bool autoAddToSameParent = false)
        {
            GameObject newWidgetObj = Instantiate(gameObject, autoAddToSameParent ? transform.parent : null);
            UIWidget newWidget = newWidgetObj.GetComponent<UIWidget>();

            if (autoAddToSameParent)
            {
                if (pParentWidget == null)
                    pParentUI.AddWidget(newWidget);
                else
                    pParentWidget.AddWidget(newWidget);
            }
            else
            {
                newWidget.pEventTarget = null;
                newWidget.Initialize(null, null);
            }

            return newWidget;
        }

        /// <summary>
        /// This method will be called whenever the widget is being re-parented under a different widget or a UI.
        /// If you are overriding this, add checks to ensure that operations/methods that must not be performed multiple times
        /// are only executed once
        /// </summary>
        public virtual void Initialize(UI parentUI, UIWidget parentWidget)
        {
            if (!mInitialized)
            {
                mInitialized = true;

                pRectTransform = transform as RectTransform;
                if (_ProxyTransform == null)
                    _ProxyTransform = pRectTransform;
                mSelectable = GetComponent<Selectable>();

                // Clearing these lists since Initialize() could by called multiple times in the editor by FillEffectReferences()
                mChildWidgets.Clear();
                mGraphicsStates.Clear();

                CacheWidgets(transform);
                mGraphicsStatesCached = true;
            }

            pParentUI = parentUI;
            pParentWidget = parentWidget;

            // We can't call SetParamsFromPublicVariables() here since that would trigger 
            // a series of recursive calls
            mState = _State;
            mVisible = _Visible;
            if (parentWidget != null)
            {
                mParentState = parentWidget.pStateInHierarchy;
                mParentVisible = parentWidget.pVisibleInHierarchy;
            }
            else if (parentUI != null)
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
                childWidget.pEventTarget = pEventTarget;
                childWidget.Initialize(pParentUI, this);
            }
        }

        /// <summary>
        /// Search for UIWidget and Graphic components in child transforms and store references to them
        /// Also assign this widget's pEventTarget as child widgets' pEventTarget
        /// </summary>
        protected void CacheWidgets(Transform cacheTransform)
        {
            mRaycastCaptureGraphic = GetComponent<NonDrawableGraphic>();

            int childCount = cacheTransform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform childTransform = cacheTransform.GetChild(i);

                UIWidget childWidget = childTransform.GetComponent<UIWidget>();
                if (childWidget != null)
                    mChildWidgets.Add(childWidget);
                else
                {
                    Graphic childGraphic = childTransform.GetComponent<Graphic>();
                    if (childGraphic != null && !(childGraphic is NonDrawableGraphic))
                    {
                        // We need to cache the actual enabled/disabled state of the Graphic only if we are using a Unity Selectable
                        // since the Selectable will perform it's own Graphic enabling/disabling
                        RectTransform childGraphicTransfrom = childGraphic.rectTransform;
                        Image childGraphicImage = childGraphic as Image;

                        GraphicState graphicState = new GraphicState(childGraphic);
                        graphicState.pOriginallyEnabled = childGraphic.enabled || (mSelectable == null);
                        graphicState.pOriginalPosition = childGraphicTransfrom.localPosition;
                        graphicState.pOriginalScale = childGraphicTransfrom.localScale;
                        graphicState.pOriginalColor = childGraphic.color;
                        graphicState.pOriginalSprite = (childGraphicImage != null) ? childGraphicImage.sprite : null;
                        mGraphicsStates.Add(graphicState);
                    }

                    if (childTransform.childCount > 0)
                        CacheWidgets(childTransform);
                }
            }
        }

        protected override void UpdateUnityComponents()
        {
            base.UpdateUnityComponents();

            if (mSelectable != null)
            {
                mSelectable.enabled = mVisibleInHierarchy && (mStateInHierarchy == WidgetState.INTERACTIVE || mStateInHierarchy == WidgetState.DISABLED);
                mSelectable.interactable = mStateInHierarchy == WidgetState.INTERACTIVE;
            }
            // Enabling/Disabling the component rather than setting raycastTarget since game code can
            // modify raycastTarget through pCanReceiveInput and we don't want to interfere with that
            if (mRaycastCaptureGraphic != null)
                mRaycastCaptureGraphic.enabled = mVisibleInHierarchy;
        }

        protected override void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
        {
            UpdateUnityComponents();

            if ((previousStateInHierarchy == WidgetState.DISABLED && mStateInHierarchy != WidgetState.DISABLED) ||
                    (previousStateInHierarchy != WidgetState.DISABLED && mStateInHierarchy == WidgetState.DISABLED))
            {
                // We're either changing from being DISABLED to somethig else, or becoming DISABLED. An effect change must be triggered
                SelectEffect();
            }
        }

        protected override void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
        {
            UpdateUnityComponents();

            SelectEffect();
        }

        [ContextMenu("Fill Effect References")]
        protected virtual void FillAllEffectReferences()
        {
            Initialize(pParentUI, pParentWidget);
            FillEffectReferences(_ClickEffects);
            FillEffectReferences(_DisabledEffects);
            FillEffectReferences(_HoverEffects);
            FillEffectReferences(_PressEffects);
        }

        protected void FillEffectReferences(UIEffects effects)
        {
            effects._PositionEffect._ApplyTo = new UIEffects.PositionEffectData[mGraphicsStates.Count];
            effects._ScaleEffect._ApplyTo = new UIEffects.ScaleEffectData[mGraphicsStates.Count];
            effects._ColorEffect._ApplyTo = new UIEffects.ColorEffectData[mGraphicsStates.Count];
            effects._SpriteEffect._ApplyTo = new UIEffects.SpriteEffectData[mGraphicsStates.Count];

            for (int i = 0; i < mGraphicsStates.Count; ++i)
            {
                effects._PositionEffect._ApplyTo[i] = new UIEffects.PositionEffectData { _Widget = mGraphicsStates[i].pGraphic };
                effects._ScaleEffect._ApplyTo[i] = new UIEffects.ScaleEffectData { _Widget = mGraphicsStates[i].pGraphic };
                effects._ColorEffect._ApplyTo[i] = new UIEffects.ColorEffectData { _Widget = mGraphicsStates[i].pGraphic };
                effects._SpriteEffect._ApplyTo[i] = new UIEffects.SpriteEffectData { _Widget = mGraphicsStates[i].pGraphic };
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_RotationPerSecond != Vector3.zero)
                transform.Rotate(_RotationPerSecond * Time.deltaTime);

            if (mCurrentEffect != null && mCurrentEffectStartTime > 0 && mCurrentEffect._MaxDuration > 0 && ((Time.realtimeSinceStartup - mCurrentEffectStartTime) > mCurrentEffect._MaxDuration))
                ApplyEffect(null);

            if (mIsAttachedToPointer)
            {
                Vector2? pointerPosition = InputUtility.GetFingerOrMousePos(mFingerIDAttachedTo);
                if (pointerPosition.HasValue)
                    transform.position = ScreenToUIPoint(pointerPosition.Value) + mPointerAttachOffset;
            }

            if (mIsPressed && mIsHovering && pInteractableInHierarchy && pEventTarget != null)
                pEventTarget.TriggerOnPressRepeated(this);
        }

        public Vector2 ScreenToUIPoint(Vector3 pointerPosition)
        {
            Vector2 movePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pParentUI.pRectTransform, pointerPosition, pParentUI.pCanvas.worldCamera, out movePosition);
            return (Vector2)pParentUI.transform.TransformPoint(movePosition);
        }

        protected void ApplyEffect(UIEffects effect)
        {
            if (mCurrentEffect == effect)
                return;

            UIEffects previousEffect = mCurrentEffect;
            mCurrentEffect = effect;

            foreach (GraphicState graphicState in mGraphicsStates)
            {
                if (graphicState == null || graphicState.pGraphic == null) // when we delete the widget, togglebutton is calling ApplyEffect after deleting which causes null ref exception.
                    return;

                Tween.Stop(graphicState.pGraphic.gameObject);

                // The supported tween value types are - Position, Scale, Color, Sprite
                // We have to calculate the following in order to tween the values:
                // (1) Initial Value, (2) Duration, (3) EaseType and the (4) Final Value
                //
                // (1) The initial value of the tween will always be the current value of the Graphic (position, scale, color etc)
                // (2) If the current effect has specified the value type (has _UseEffect checked), the duration will be taken,
                //     or else the duration of the previous effect will be taken
                // (3) Same as above
                // (4) If the current effect has specified the value type (has _UseEffect checked), the value specified by the current effect will be used
                //     or it will be the value cached in Graphic States.

                Graphic graphic = graphicState.pGraphic;
                RectTransform graphicTransform = graphicState.pGraphic.rectTransform;
                MaskableGraphic maskableGraphic = graphicState.pGraphic as MaskableGraphic;
                Image graphicImage = graphicState.pGraphic as Image;

                Vector3 initialPosition = graphicTransform.localPosition;
                Vector3 initialScale = graphicTransform.localScale;
                Color initialColor = (maskableGraphic != null) ? maskableGraphic.color : Color.white;

                Vector3 finalPosition = graphicState.pOriginalPosition;
                Vector3 finalScale = graphicState.pOriginalScale;
                Color finalColor = graphicState.pOriginalColor;
                Sprite finalSprite = graphicState.pOriginalSprite;

                float positionDuration = 0;
                float scaleDuration = 0;
                float colorDuration = 0;

                EaseType positionEaseType = EaseType.Linear;
                EaseType scaleEaseType = EaseType.Linear;
                EaseType colorEaseType = EaseType.Linear;

                bool playPositionEffect = false;
                bool playScaleEffect = false;
                bool playColorEffect = false;
                bool playSpriteEffect = false;

                if (previousEffect != null)
                {
                    if (previousEffect._PositionEffect._UseEffect)
                    {
                        UIEffects.PositionEffectData positionEffectData = Array.Find(previousEffect._PositionEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (positionEffectData != null)
                        {
                            positionDuration = positionEffectData._Time;
                            positionEaseType = positionEffectData._PositionEffect;
                            playPositionEffect = true;
                        }
                    }
                    if (previousEffect._ScaleEffect._UseEffect)
                    {
                        UIEffects.ScaleEffectData scaleEffectData = Array.Find(previousEffect._ScaleEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (scaleEffectData != null)
                        {
                            scaleDuration = scaleEffectData._Time;
                            scaleEaseType = scaleEffectData._ScaleEffect;
                            playScaleEffect = true;
                        }
                    }
                    if (previousEffect._ColorEffect._UseEffect)
                    {
                        UIEffects.ColorEffectData colorEffectData = Array.Find(previousEffect._ColorEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (colorEffectData != null)
                        {
                            colorDuration = colorEffectData._Time;
                            colorEaseType = colorEffectData._ColorEffect;
                            playColorEffect = true;
                        }
                    }
                    if (previousEffect._SpriteEffect._UseEffect)
                        playSpriteEffect = true;
                }

                if (mCurrentEffect != null)
                {
                    mCurrentEffectStartTime = Time.realtimeSinceStartup;

                    if (mCurrentEffect._PositionEffect._UseEffect)
                    {
                        UIEffects.PositionEffectData positionEffectData = Array.Find(mCurrentEffect._PositionEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (positionEffectData != null)
                        {
                            finalPosition = positionEffectData._Offset;
                            positionDuration = positionEffectData._Time;
                            positionEaseType = positionEffectData._PositionEffect;
                            playPositionEffect = true;
                        }
                    }
                    if (mCurrentEffect._ScaleEffect._UseEffect)
                    {
                        UIEffects.ScaleEffectData scaleEffectData = Array.Find(mCurrentEffect._ScaleEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (scaleEffectData != null)
                        {
                            finalScale = scaleEffectData._Scale;
                            scaleDuration = scaleEffectData._Time;
                            scaleEaseType = scaleEffectData._ScaleEffect;
                            playScaleEffect = true;
                        }
                    }
                    if (mCurrentEffect._ColorEffect._UseEffect)
                    {
                        UIEffects.ColorEffectData colorEffectData = Array.Find(mCurrentEffect._ColorEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (colorEffectData != null)
                        {
                            finalColor = colorEffectData._Color;
                            colorDuration = colorEffectData._Time;
                            colorEaseType = colorEffectData._ColorEffect;
                            playColorEffect = true;
                        }
                    }
                    if (mCurrentEffect._SpriteEffect._UseEffect)
                    {
                        UIEffects.SpriteEffectData spriteEffectData = Array.Find(mCurrentEffect._SpriteEffect._ApplyTo, x => x._Widget == graphicState.pGraphic);
                        if (spriteEffectData != null)
                        {
                            finalSprite = spriteEffectData._Sprite;
                            playSpriteEffect = true;
                        }
                    }
                }

                if (playPositionEffect)
                    Tween.MoveLocalTo(graphic.gameObject, initialPosition, finalPosition, new TweenParam(positionDuration, positionEaseType));
                if (playScaleEffect)
                    Tween.ScaleTo(graphic.gameObject, initialScale, finalScale, new TweenParam(scaleDuration, scaleEaseType));
                if (playColorEffect)
                    Tween.ColorTo(graphic.gameObject, initialColor, finalColor, new TweenParam(colorDuration, colorEaseType));
                if (playSpriteEffect && graphicImage != null)
                    graphicImage.sprite = finalSprite;
            }
        }

        protected void PlayOneTimeEffect(UIEffects effect)
        {
            if (mCurrentOneTimeEffect != null)
            {
                mCurrentOneTimeEffect.PlayParticle(false);
                mCurrentOneTimeEffect.PlaySound(false);
            }
            if (effect != null)
            {
                effect.PlayParticle(true);
                effect.PlaySound(true);
            }
            mCurrentOneTimeEffect = effect;
        }

        protected void StopOneTimeEffectIfPlaying(UIEffects effect)
        {
            if (mCurrentOneTimeEffect == null || mCurrentOneTimeEffect != effect)
                return;

            mCurrentOneTimeEffect.PlayParticle(false);
            mCurrentOneTimeEffect.PlaySound(false);
            mCurrentOneTimeEffect = null;
        }

        /// <summary>
        /// This will be called whenever a state is changed which requires an effect to be played
        /// This method must take the different states into account and apply the correct effect.
        /// </summary>
        protected virtual void SelectEffect()
        {
            if (mStateInHierarchy == WidgetState.DISABLED)
                ApplyEffect(_DisabledEffects);
            else if (mIsPressed)
                ApplyEffect(_PressEffects);
            else if (mIsHovering)
                ApplyEffect(_HoverEffects);
            else
                ApplyEffect(null);
        }

        /// <summary>
        /// Allow pointer for left click
        /// Standalone should only take left mouse click to UIWidgets
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public bool IsPointerValid(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return false;

            return true;
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (!IsPointerValid(eventData))
                return;
            if (pInteractableInHierarchy && pEventTarget != null)
                pEventTarget.TriggerOnDrop(this, eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            //If multiple buttons are touched at the same time
            //this func will be called for the first PointerUp widget only
            if (!IsPointerValid(eventData) || IgnoreClick)
                return;

            if (pInteractableInHierarchy && !mIsDragging)
            {
                PlayOneTimeEffect(_ClickEffects);
                if (pEventTarget != null)
                {
                    IgnoreClick = true;
                    pEventTarget.TriggerOnClick(this, eventData);
                }
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!IsPointerValid(eventData))
                return;
            if (pInteractableInHierarchy)
            {
                mIsPressed = true;
                PlayOneTimeEffect(_PressEffects);
                SelectEffect();
                if (pEventTarget != null)
                {
                    IgnoreClick = false;
                    pEventTarget.TriggerOnPress(this, true, eventData);
                }
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!IsPointerValid(eventData))
                return;
            mIsPressed = false;
            StopOneTimeEffectIfPlaying(_PressEffects);
            SelectEffect();
            if (pInteractableInHierarchy)
            {
                if (pEventTarget != null)
                    pEventTarget.TriggerOnPress(this, false, eventData);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (pInteractableInHierarchy)
            {
                mIsHovering = true;
                PlayOneTimeEffect(_HoverEffects);
                SelectEffect();
                if (pEventTarget != null)
                    pEventTarget.TriggerOnHover(this, true, eventData);
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            mIsHovering = false;
            StopOneTimeEffectIfPlaying(_HoverEffects);
            SelectEffect();
            if (pInteractableInHierarchy)
            {
                if (pEventTarget != null)
                    pEventTarget.TriggerOnHover(this, false, eventData);
            }
        }

        public void SetDragging(bool isDragging)
        {
            mIsDragging = isDragging;
        }
    }
}
