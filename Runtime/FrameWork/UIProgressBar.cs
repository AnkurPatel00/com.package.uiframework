using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
	public class UIProgressBar : UIWidget
	{
		public enum FillDirection
		{
			LeftToRight,
			RightToLeft,
			BottomToTop,
			TopToBottom
		}

		public Image _FillImage;
		public FillDirection _FillDirection;

		public override void Initialize(UI parentUI, UIWidget parentWidget)
		{
			base.Initialize(parentUI, parentWidget);
			if (_FillImage != null && _FillImage.type != Image.Type.Filled)
			{
				Vector2 pivot = _FillImage.rectTransform.pivot;
				Vector2 offsetMin = _FillImage.rectTransform.offsetMin;
				Vector2 offsetMax = _FillImage.rectTransform.offsetMax;

				if (_FillDirection == FillDirection.LeftToRight)
					pivot.x = 0;
				else if (_FillDirection == FillDirection.RightToLeft)
					pivot.x = 1;
				else if (_FillDirection == FillDirection.BottomToTop)
					pivot.y = 0;
				else
					pivot.y = 1;

				_FillImage.rectTransform.pivot = pivot;
				_FillImage.rectTransform.offsetMin = offsetMin;
				_FillImage.rectTransform.offsetMax = offsetMax;
			}
		}

		/// <summary>
		/// Sets or gets the progress. Value should be between 0 and 1.
		/// </summary>
		public float pProgress
		{
			get
			{
				if (_FillImage.type != Image.Type.Filled)
				{
					if (_FillDirection == FillDirection.LeftToRight || _FillDirection == FillDirection.RightToLeft)
						return Mathf.Clamp(_FillImage.rectTransform.localScale.x, 0, 1);
					else
						return Mathf.Clamp(_FillImage.rectTransform.localScale.y, 0, 1);
				}

				return _FillImage.fillAmount;
			}
			set
			{
				if (_FillImage.type != Image.Type.Filled)
				{
					Vector3 localScale = _FillImage.rectTransform.localScale;
					localScale.x = Mathf.Clamp(value, 0, 1);

					if (_FillDirection == FillDirection.LeftToRight || _FillDirection == FillDirection.RightToLeft)
						localScale.x = Mathf.Clamp(value, 0, 1);
					else
						localScale.y = Mathf.Clamp(value, 0, 1);

					_FillImage.rectTransform.localScale = localScale;
				}
				else
					_FillImage.fillAmount = value;
			}
		}
	}
}