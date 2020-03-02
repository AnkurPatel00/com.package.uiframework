using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TweenUtil
{
	public class Tween
	{
	#region Move

		public static void MoveTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new Move(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}
			
		public static void MoveBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new Move(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void MoveLocalTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new MoveLocal(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void MoveLocalBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new MoveLocal(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void MoveTo(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
		{
			Tweener tweener = new Move2D(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void MoveBy(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
		{
			Tweener tweener = new Move2D(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}
			
		public static void MoveLocalTo(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
		{
			Tweener tweener = new MoveLocal2D(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void MoveLocalBy(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
		{
			Tweener tweener = new MoveLocal2D(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

	#endregion

	#region Scale

		public static void ScaleTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new Scale(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void ScaleBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new Scale(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void ScaleTo(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
		{
			Tweener tweener = new Scale2D(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void ScaleBy(GameObject tweenObject, Vector2 from, Vector2 to, TweenParam tweenParam)
		{
			Tweener tweener = new Scale2D(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}
	#endregion

	#region Rotate

		public static void RotateTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new Rotate(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void RotateBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new Rotate(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void RotateLocalTo(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new RotateLocal(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, true);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

		public static void RotateLocalBy(GameObject tweenObject, Vector3 from, Vector3 to, TweenParam tweenParam)
		{
			Tweener tweener = new RotateLocal(tweenObject, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, false);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}

	#endregion

	#region Color

		public static void ColorTo(GameObject tweenObject, Color from, Color to, TweenParam tweenParam)
		{
			Color(tweenObject, from, to, tweenParam, true);
		}

		public static void ColorBy(GameObject tweenObject, Color from, Color to, TweenParam tweenParam)
		{
			Color(tweenObject, from, to, tweenParam, false);
		}

		private static void Color(GameObject tweenObject, Color from, Color to, TweenParam tweenParam, bool isTimeDependent)
	    {
			Tweener tweener = null;
	    
			if(tweenObject.GetComponent<Renderer>() != null)
				tweener = new Colour(tweenObject, tweenObject.GetComponent<Renderer>(), from, to);
			else if (tweenObject.GetComponent<TextMesh>() != null)
				tweener = new TextMeshColor(tweenObject, tweenObject.GetComponent<TextMesh>(), from, to);
			else if (tweenObject.GetComponent<Graphic>() != null)
				tweener = new GraphicColor(tweenObject, tweenObject.GetComponent<Graphic>(), from, to);
			else
	        {
				Debug.Log("================  Color Component Not Available ================= "+ tweenObject.name);
	            return;
	        }
	        
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDependent);
			SetTweenParam(tweener, deltaMove, tweenParam);
	    }

	#endregion

	#region Alpha

		public static void AlphaTo(GameObject tweenObject, float from, float to, TweenParam tweenParam)
		{
			Alpha(tweenObject, from, to, tweenParam, true);
		}

		public static void AlphaBy(GameObject tweenObject, float from, float to, TweenParam tweenParam)
		{
			Alpha(tweenObject, from, to, tweenParam, false);
		}

		private static void Alpha(GameObject tweenObject, float from, float to, TweenParam tweenParam, bool isTimeDependent)
	    {
			Renderer rend = tweenObject.GetComponent<Renderer>();
	        if (rend == null)
	        {
				Debug.Log("================  Renderer Component Not Available ================= "+ tweenObject.name);
	            return;
	        }

			Tweener tweener = new Alpha(tweenObject, rend, from, to);
			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDependent);
			SetTweenParam(tweener, deltaMove, tweenParam);
	    }
	#endregion

	#region Text Counter

		public static void TextCounterTo(GameObject tweenObject, float from, float to, TweenParam tweenParam)
		{
			TextCounter(tweenObject, from, to, tweenParam, true);
		}

		public static void TextCounterBy(GameObject tweenObject, float from, float to, TweenParam tweenParam)
		{
			TextCounter(tweenObject, from, to, tweenParam, false);
		}

		private static void TextCounter(GameObject tweenObject, float from, float to, TweenParam tweenParam, bool isTimeDependent)
		{
			Tweener tweener = null;
			if (tweenObject.GetComponent<Text>() != null) 
				tweener = new Count(tweenObject, tweenObject.GetComponent<Text>(), from, to);
			else if (tweenObject.GetComponent<TextMesh>()!= null)
				tweener = new Count(tweenObject, tweenObject.GetComponent<TextMesh>(), from, to);
			else
			{
				Debug.Log("================  Text Component Not Available ================= "+ tweenObject.name);
				return;
			}

			float deltaMove = GetDeltaMove(from, to, tweenParam._DurationOrSpeed, isTimeDependent);
			SetTweenParam(tweener, deltaMove, tweenParam);
		}
	#endregion

	#region Shake

		public static void Shake(GameObject tweenObject, float duration, float shakeAmount, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		{
			Tweener tweener = new Shake(tweenObject, shakeAmount);
			float deltaMove = 1 / duration;
			tweener.SetData(deltaMove, 0, 1, false, false, null, null, onAnimationCompleteCallback);
			TweenUpdater.pTweeners.Add(tweener);
		}

		public static void Shake2D(GameObject tweenObject, float duration, float shakeAmount, OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		{
			Tweener tweener = new Shake2D(tweenObject, shakeAmount);
			float deltaMove = 1 / duration;
			tweener.SetData(deltaMove, 0, 1, false, false, null, null ,onAnimationCompleteCallback);
			TweenUpdater.pTweeners.Add(tweener);
		}
			
	#endregion

		public static void PlayAnim(GameObject tweenObject, string name , OnAnimationCompleteCallback onAnimationCompleteCallback = null)
		{
			Animation anim = tweenObject.GetComponent<Animation>();
			if (anim == null)
			{
				Debug.Log("Aniamtion component has not been attached to this game object: " + tweenObject.name);
				return;
			}
			Tweener tweener = new AnimatoinComp(tweenObject, anim, name);
			TweenUpdater.pTweeners.Add(tweener);
		}

		public static void Timer(float delay, OnAnimationCompleteCallback onDelayReachesCallback)
		{
			Tweener tweener = new Timer();
			float deltaMove = 1 / delay;
			tweener.SetData(deltaMove, 0, 1, false, false, null, null ,onDelayReachesCallback);
			TweenUpdater.pTweeners.Add(tweener);

		}


	#region Helper

		public static CustomCurve GetCustomCurve(EaseType easeType)
		{
			switch (easeType)
			{
			case EaseType.Linear: return EaseCurve.Linear;
			case EaseType.Spring: return EaseCurve.Spring;
			case EaseType.EaseInQuad: return EaseCurve.EaseInQuad;
			case EaseType.EaseOutQuad: return EaseCurve.EaseOutQuad;
			case EaseType.EaseInOutQuad: return EaseCurve.EaseInOutQuad;
			case EaseType.EaseInCubic: return EaseCurve.EaseInCubic;
			case EaseType.EaseOutCubic: return EaseCurve.EaseOutCubic;
			case EaseType.EaseInOutCubic: return EaseCurve.EaseInOutCubic;
			case EaseType.EaseInQuart: return EaseCurve.EaseInQuart;
			case EaseType.EaseOutQuart: return EaseCurve.EaseOutQuart;
			case EaseType.EaseInOutQuart: return EaseCurve.EaseInOutQuart;
			case EaseType.EaseInQuint: return EaseCurve.EaseInQuint;
			case EaseType.EaseOutQuint: return EaseCurve.EaseOutQuint;
			case EaseType.EaseInOutQuint: return EaseCurve.EaseInOutQuint;
			case EaseType.EaseInSine: return EaseCurve.EaseInSine;
			case EaseType.EaseOutSine: return EaseCurve.EaseOutSine;
			case EaseType.EaseInOutSine: return EaseCurve.EaseInOutSine;
			case EaseType.EaseInExpo: return EaseCurve.EaseInExpo;
			case EaseType.EaseOutExpo: return EaseCurve.EaseOutExpo;
			case EaseType.EaseInOutExpo: return EaseCurve.EaseInOutExpo;
			case EaseType.EaseInCirc: return EaseCurve.EaseInCirc;
			case EaseType.EaseOutCirc: return EaseCurve.EaseOutCirc;
			case EaseType.EaseInOutCirc: return EaseCurve.EaseInOutCirc;
			case EaseType.EaseInBounce: return EaseCurve.EaseInBounce;
			case EaseType.EaseOutBounce: return EaseCurve.EaseOutBounce;
			case EaseType.EaseInOutBounce: return EaseCurve.EaseInOutBounce;
			case EaseType.EaseInBack: return EaseCurve.EaseInBack;
			case EaseType.EaseOutBack: return EaseCurve.EaseOutBack;
			case EaseType.EaseInOutBack: return EaseCurve.EaseInOutBack;
			case EaseType.EaseInElastic: return EaseCurve.EaseInElastic;
			case EaseType.EaseOutElastic: return EaseCurve.EaseOutElastic;
			case EaseType.EaseInOutElastic: return EaseCurve.EaseInOutElastic;
			default: return EaseCurve.Linear;

			}
		}

		private static float GetDeltaMove(Vector2 from, Vector2 to, float factor ,bool isTimeDepndent)
		{
			return factor = (isTimeDepndent ? 1 / factor : factor / Vector2.Distance(from, to));
		}

		private static float GetDeltaMove(Vector3 from, Vector3 to, float factor ,bool isTimeDepndent)
		{
			return factor = (isTimeDepndent ? 1 / factor : factor / Vector3.Distance(from, to));
		}

		private static float GetDeltaMove(Vector4 from, Vector4 to, float factor ,bool isTimeDepndent)
		{
			return factor = (isTimeDepndent ? 1 / factor : factor / Vector4.Distance(from, to));
		}

		private static float GetDeltaMove(float from, float to, float factor ,bool isTimeDepndent)
		{
			return factor = (isTimeDepndent ? 1 / factor : factor / Mathf.Abs(from - to));
		}

		private static void SetTweenParam(Tweener tweener, float deltaMove, TweenParam tweenParam)
		{
			tweener.SetData(deltaMove, tweenParam._Delay, tweenParam._LoopCount, tweenParam._PingPong, tweenParam._UseAnimationCurve, tweenParam._AnimationCurve, tweenParam.pCustomAnimationCurve, tweenParam.pOnTweenCompleteCallback);
			TweenUpdater.pTweeners.Add(tweener);
		}

	#endregion

		public static void Pause(GameObject tweenObject, bool state)
		{
			List<Tweener> tweeners = TweenUpdater.pTweeners.FindAll(elememt => elememt.pTweenObject.GetInstanceID() == tweenObject.GetInstanceID());
			for (int i = 0; i < tweeners.Count; ++i)
				tweeners[i].pState = state ? TweenState.PAUSE : TweenState.RUNNING;
		}

        public static bool IsRunning(GameObject tweenObject)
        {
            List<Tweener> tweeners = TweenUpdater.pTweeners.FindAll(elememt => elememt.pTweenObject.GetInstanceID() == tweenObject.GetInstanceID());
            for (int i = 0; i < tweeners.Count; ++i)
            {
                if (tweeners[i].pState == TweenState.RUNNING)
                    return true;
            }
            return false;
        }

        public static void Stop(GameObject tweenObject)
		{
			List<Tweener> tweeners = TweenUpdater.pTweeners.FindAll(elememt => elememt.pTweenObject.GetInstanceID() == tweenObject.GetInstanceID());
			for (int i = 0; i < tweeners.Count; ++i)
				TweenUpdater.pTweeners.Remove(tweeners[i]);

			tweeners.Clear();
		}

		public static void PauseAll(bool state)
		{
			TweenUpdater.pInstance.enabled = !state;
		}
	}
}
