using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    public class UIToggleButton : UIButton
    {
        public bool _Checked;
        // TODO: Have to implement runtime changing of _GroupName & _Grouped
        public bool _Grouped;
        public string _GroupName;

        [Header("Unity Component References")]
        public Image _Checkmark;

        [Header("Effects")]
        public UIEffects _CheckedEffects;
        public UIEffects _CheckedDisabledEffects;

        private bool mChecked;

        public bool pChecked
        {
            get { return mChecked; }
            set
            {
                if (mChecked != value)
                {
                    _Checked = value;
                    mChecked = value;
                    if (_Checkmark != null)
                        _Checkmark.gameObject.SetActive(mChecked);
                    SelectEffect();
                    if (_Grouped && mChecked && pParentUI != null)
                        pParentUI.UncheckOtherToggleButtonsInGroup(_GroupName, this);
                    pEventTarget.TriggerOnCheckedChanged(this, mChecked);
                }
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!IsPointerValid(eventData) || IgnoreClick)
                return;
            if (pInteractableInHierarchy && !mIsDragging)
            {
                // If we're grouped, then we can't allow the user to uncheck this by clicking
                if (!_Grouped || !pChecked)
                    pChecked = !pChecked;
            }
            base.OnPointerClick(eventData);
        }

        public override void Initialize(UI parentUI, UIWidget parentWidget)
        {
            UI previousParentUI = pParentUI;

            base.Initialize(parentUI, parentWidget);

            if (previousParentUI != null && _Grouped)
                previousParentUI.UnregisterToggleButtonInGroup(_GroupName, this);
            if (parentUI != null && _Grouped)
                parentUI.RegisterToggleButtonInGroup(_GroupName, this);
        }

        protected override void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
        {
            base.OnStateInHierarchyChanged(previousStateInHierarchy, newStateInHierarchy);

            if (pChecked)
            {
                if ((previousStateInHierarchy == WidgetState.DISABLED && mStateInHierarchy != WidgetState.DISABLED) ||
                    (previousStateInHierarchy != WidgetState.DISABLED && mStateInHierarchy == WidgetState.DISABLED))
                {
                    // We're either changing from being DISABLED to somethig else, or becoming DISABLED. An effect change must be triggered
                    SelectEffect();
                }
            }
            else
                base.OnStateInHierarchyChanged(previousStateInHierarchy, newStateInHierarchy);
        }

        protected override void SetParamsFromPublicVariables()
        {
            base.SetParamsFromPublicVariables();

            pChecked = _Checked;
        }

        protected override void SelectEffect()
        {
            if (mStateInHierarchy == WidgetState.DISABLED && pChecked)
                ApplyEffect(_CheckedDisabledEffects);
            else if (pChecked)
                ApplyEffect(_CheckedEffects);
            else
                base.SelectEffect();
        }

        [ContextMenu("Fill Effect References")]
        protected override void FillAllEffectReferences()
        {
            base.FillAllEffectReferences();

            FillEffectReferences(_CheckedEffects);
            FillEffectReferences(_CheckedDisabledEffects);
        }

        private void OnDestroy()
        {
            if (pParentUI != null)
                pParentUI.UnregisterToggleButtonInGroup(_GroupName, this);
        }
    }
}
