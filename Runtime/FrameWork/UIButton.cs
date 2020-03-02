using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
	public class UIButton : UIWidget
	{
		[Header("Unity Component References")]
		public MaskableGraphic _IconGraphic;

		public Sprite pIconSprite
		{
			get { return (_IconGraphic as Image).sprite; }
			set
			{
				(_IconGraphic as Image).sprite = value;
				GraphicState graphicState = FindGraphicState(_IconGraphic);
				if (graphicState != null)
					graphicState.pOriginalSprite = value;
			}
		}

		public Texture pIconTexture
		{
			get { return (_IconGraphic as RawImage).texture; }
			set { (_IconGraphic as RawImage).texture = value; }
		}

		[ContextMenu("Fill Effect References")]
		protected override void FillAllEffectReferences()
		{
			base.FillAllEffectReferences();
		}

	}
}

