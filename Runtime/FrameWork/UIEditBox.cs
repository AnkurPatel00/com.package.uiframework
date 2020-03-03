using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
    public class UIEditBox : UIWidget
    {
        // This must not be accessed from other classes.
        [SerializeField]
        private string _RegularExpression;

        [SerializeField]
        private bool _CheckRegularExpression = true;

        protected InputField mInputField;

        private bool mInitialized;

        public override string pText
        {
            get { return mInputField.text; }
            set { mInputField.text = value; }
        }

        public bool IsValidText()
        {
            return Regex.IsMatch(pText, _RegularExpression);
        }

        public override void Initialize(UI parentUI, UIWidget parentWidget)
        {
            base.Initialize(parentUI, parentWidget);

            if (!mInitialized)
            {
                mInitialized = true;
                mInputField = GetComponent<InputField>();
                _Text = mInputField.textComponent;
                mInputField.onEndEdit.AddListener(OnEndEdit);
                mInputField.onValueChanged.AddListener(OnValueChanged);

                if (!string.IsNullOrEmpty(_RegularExpression))
                    mInputField.onValidateInput = OnValidateInput;
            }
        }

        [ContextMenu("Fill Effect References")]
        protected override void FillAllEffectReferences()
        {
            base.FillAllEffectReferences();
        }

        protected void OnEndEdit(string text)
        {
            if (pEventTarget != null)
                pEventTarget.TriggerOnEdit(this, text);
        }

        protected char OnValidateInput(string oldValue, int index, char c)
        {
            if (_CheckRegularExpression && !Regex.IsMatch(oldValue + c, _RegularExpression))
                return default(char);

            return c;
        }

        protected void OnValueChanged(string text)
        {
            if (pEventTarget != null)
                pEventTarget.TriggerOnValueChanged(this, text);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mInputField != null)
            {
                if (string.IsNullOrEmpty(_RegularExpression))
                    mInputField.onValidateInput = null;
                else
                {
                    if (mInputField.onValidateInput == null)
                        mInputField.onValidateInput = OnValidateInput;
                }
            }
        }
#endif
    }
}