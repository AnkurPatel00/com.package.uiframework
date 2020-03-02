using UnityEngine;
using UnityEngine.UI;
using System;

namespace TweenUtil
{
	public delegate float CustomCurve(float start, float end, float val);
	public delegate void OnAnimationCompleteCallback(object o);

	public enum TweenState
	{
		NONE,
		RUNNING,
		PAUSE,
		DONE
	}

	public abstract class Tweener
	{
		public delegate void AnimationCompleteEvent(Tweener tweener);
		public static event AnimationCompleteEvent pOnAnimationCompleted = null;
		private OnAnimationCompleteCallback pOnAnimationCompleteCallback = null;
		public GameObject pTweenObject { get; protected set; }
		public TweenState pState { get; set; }

		protected float mValue = 0;

		private AnimationCurve mAnimationCurve;
		private CustomCurve mCustomAnimationCurve = null;
		private int mLoopCount = 1;
		private int mCompletedLoopCount = 0;
		private float mDeltaMove = 0f;
		private float mDelay = 0f;
		private float mDelayCounter = 0;
		private bool mPingPong = false;
		private bool mDoInReverese = false;
		private bool mUseAnimationCurve = false;

		public void DoUpdate()
		{
			if (pTweenObject == null) 
			{
				pOnAnimationCompleted(this);
				return;
			}
				
			if (pState == TweenState.PAUSE || pState == TweenState.DONE)
				return;
			
			if (mDelayCounter < mDelay)
			{
				mDelayCounter += Time.deltaTime;
				return;
			}
				
			mValue = Mathf.MoveTowards(mValue, 1, Time.deltaTime * mDeltaMove);
			float value = mValue;
				
			if (mUseAnimationCurve && mAnimationCurve != null)
				value = mAnimationCurve.Evaluate(mValue);
			else if (mCustomAnimationCurve != null)
				value = mCustomAnimationCurve(0, 1, mValue);
			
			if (!mDoInReverese)
				DoAnim(value);
			else
				DoAnimReverse(value);

			if (mValue >= 1)
			{
				mDoInReverese = mPingPong ? !mDoInReverese : mDoInReverese;
				++mCompletedLoopCount;

				if (!(mCompletedLoopCount != (mPingPong ? mLoopCount != 1 ? 2 * mLoopCount : 2 : mLoopCount)))
				{
					pState = TweenState.DONE;
					pOnAnimationCompleted(this);
					
					if (pOnAnimationCompleteCallback != null)
						pOnAnimationCompleteCallback(null);
				}

				mValue = 0;
			}
		}

		public void SetData(float deltaMove, float delay, int loopCount, bool pingPong, bool useAnimationCurve ,AnimationCurve animationCurve, CustomCurve customAnimationCurve, OnAnimationCompleteCallback onAnimationCompleteCallback)
		{
			mDeltaMove = deltaMove;
			mDelay = delay;
			mLoopCount = loopCount == 0 ? 1 : loopCount;
			mPingPong = pingPong;
			mAnimationCurve = animationCurve;
			mCustomAnimationCurve = customAnimationCurve;
			pState = TweenState.RUNNING;
			useAnimationCurve = mUseAnimationCurve;
			pOnAnimationCompleteCallback = onAnimationCompleteCallback;
		}

		protected  float CustomLerp(float start, float end, float value)
		{
			return((1 - value) * start + value * end);
		}

		protected Vector2 CustomLerp(Vector2 start, Vector2 end, float value)
		{
			return new Vector2(CustomLerp(start.x, end.x, value), CustomLerp (start.y, end.y, value));
		}

		protected Vector3 CustomLerp(Vector3 start, Vector3 end, float value)
		{
			return new Vector3(CustomLerp(start.x, end.x, value), CustomLerp(start.y, end.y, value), CustomLerp(start.z, end.z, value));
		}

		protected Vector4 CustomLerp(Vector4 start, Vector4 end, float value)
		{
			return new Vector4(CustomLerp(start.x, end.x, value), CustomLerp(start.y, end.y, value), CustomLerp(start.z, end.z, value), CustomLerp(start.w, end.w, value));
		}

		protected abstract void DoAnim(float val);
		protected abstract void DoAnimReverse(float val);
	}

    public abstract class TweenerV4 : Tweener
    {
        protected Vector4 from;
        protected Vector4 to;

		public TweenerV4(GameObject tweenObject, Vector4 from, Vector4 to)
        {
			pTweenObject = tweenObject;
            this.from = from;
            this.to = to;
        }
    }

    public abstract class TweenerV3 : Tweener
    {
        protected Vector3 from;
        protected Vector3 to;

		public TweenerV3(GameObject tweenObject, Vector3 from, Vector3 to)
        {
			pTweenObject = tweenObject;
            this.from = from;
            this.to = to;
        }
    }

    public abstract class TweenerV2 : Tweener
    {
        protected Vector2 from;
        protected Vector2 to;

		public TweenerV2(GameObject tweenObject, Vector2 from, Vector2 to)
        {
			pTweenObject = tweenObject;
            this.from = from;
            this.to = to;
        }
    }

    public abstract class TweenerF : Tweener
    {
        protected float from;
        protected float to;

		public TweenerF(GameObject tweenObject, float from, float to)
        {
			pTweenObject = tweenObject;
            this.from = from;
            this.to = to;
        }
    }


    public class Move : TweenerV3
    {
		public Move(GameObject tweenObject, Vector3 from, Vector3 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
			pTweenObject.transform.SetPosition(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetPosition(CustomLerp(to, from, val));
        }
    }

    public class MoveLocal : TweenerV3
    {
		public MoveLocal(GameObject tweenObject, Vector3 from, Vector3 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalPosition(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetLocalPosition(CustomLerp(to, from, val));
        }
    }

    public class Scale : TweenerV3
    {
		public Scale(GameObject tweenObject, Vector3 from, Vector3 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalScale(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetLocalScale(CustomLerp(to, from, val));
        }
    }

    public class Rotate : TweenerV3
    {
		public Rotate(GameObject tweenObject, Vector3 from, Vector3 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetRotation(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetRotation(CustomLerp(to, from, val));
        }
    }

    public class RotateLocal : TweenerV3
    {
		public RotateLocal(GameObject tweenObject, Vector3 from, Vector3 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalRotation(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetLocalRotation(CustomLerp(to, from, val));
        }
    }

    public class Move2D : TweenerV2
    {
		public Move2D(GameObject tweenObject, Vector2 from, Vector2 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetPosition(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetPosition(CustomLerp(to, from, val));
        }
    }

    public class MoveLocal2D : TweenerV2
    {
		public MoveLocal2D(GameObject tweenObject, Vector2 from, Vector2 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalPosition(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetLocalPosition(CustomLerp(to, from, val));
        }
    }

    public class Scale2D : TweenerV2
    {
		public Scale2D(GameObject tweenObject, Vector2 from, Vector2 to) : base(tweenObject, from, to)
        {

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalScale(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            pTweenObject.transform.SetLocalScale(CustomLerp(to, from, val));
        }
    }

    public class Colour : TweenerV4
    {
        private Renderer mRenderer;

		public Colour(GameObject tweenObject, Renderer renderer, Vector4 from, Vector4 to) : base(tweenObject, from, to)
        {
            mRenderer = renderer;
        }

        protected override void DoAnim(float val)
        {
            mRenderer.SetColor(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            mRenderer.SetColor(CustomLerp(to, from, val));
        }
    }

    public class Alpha : TweenerF
    {
        private Renderer mRenderer;

		public Alpha(GameObject tweenObject, Renderer renderer, float from, float to) : base(tweenObject, from, to)
        {
            mRenderer = renderer;
        }

        protected override void DoAnim(float val)
        {
            mRenderer.SetAlpha(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
            mRenderer.SetAlpha(CustomLerp(to, from, val));
        }
    }

   	public class Count : TweenerF
    {
		private Text mUIText;  
		private TextMesh mTextMesh;

		public Count(GameObject tweenObject, Text uiText, float from, float to) : base(tweenObject, from, to)
        {
            mUIText = uiText;
        } 

		public Count(GameObject tweenObject, TextMesh textMesh, float from, float to) : base(tweenObject, from, to)
		{
			mTextMesh = textMesh;
		}

        protected override void DoAnim(float val)
        {
			int textValue = (int)CustomLerp(from, to, val);
			if (mUIText != null)
				mUIText.SetText(textValue.ToString());
			else if (mTextMesh != null)
				mTextMesh.SetText(textValue.ToString());
        }

        protected override void DoAnimReverse(float val)
        {
			int textValue = (int)CustomLerp(from, to, val);
			if (mUIText != null)
				mUIText.SetText(textValue.ToString());
			else if (mTextMesh != null)
				mTextMesh.SetText(textValue.ToString());
        }
    }

	public class TextMeshColor : TweenerV4
    {
        private TextMesh mTextMesh; 

		public TextMeshColor(GameObject tweenObject, TextMesh textMesh, Vector4 from, Vector4 to) : base(tweenObject, from, to)
        {
            mTextMesh = textMesh;
        } 

        protected override void DoAnim(float val)
        {
			mTextMesh.SetColor(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
			mTextMesh.SetColor(CustomLerp(to, from, val));
        }
    }

	public class GraphicColor : TweenerV4
	{
		private Graphic mUIGraphic;

		public GraphicColor(GameObject tweenObject, Graphic uiGraphic, Vector4 from, Vector4 to) : base(tweenObject, from, to)
		{
			mUIGraphic = uiGraphic;
		} 

        protected override void DoAnim(float val)
        {
			mUIGraphic.SetColor(CustomLerp(from, to, val));
        }

        protected override void DoAnimReverse(float val)
        {
			mUIGraphic.SetColor(CustomLerp(to, from, val));
        }
	}

    public class Shake : Tweener
    {
        Vector3 mOriginalPosition;
        float mShakeAmount;

		public Shake(GameObject tweenObject, float shakeAmount)
        {
			pTweenObject = tweenObject;
			mOriginalPosition = pTweenObject.transform.localPosition;
            mShakeAmount = shakeAmount;
        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalPosition(mOriginalPosition + UnityEngine.Random.insideUnitSphere * mShakeAmount);
            if (val == 1)
                pTweenObject.transform.localPosition = mOriginalPosition;
        }

        protected override void DoAnimReverse(float val)
        {
            throw new NotImplementedException();
        }
    }

    public class Shake2D : Tweener
    {
        Vector2 mOriginalPosition;
        float mShakeAmount;

		public Shake2D(GameObject tweenObject, float shakeAmount)
        {
			pTweenObject = tweenObject;
			mOriginalPosition = pTweenObject.transform.localPosition;
            mShakeAmount = shakeAmount;

        }

        protected override void DoAnim(float val)
        {
            pTweenObject.transform.SetLocalPosition(mOriginalPosition + UnityEngine.Random.insideUnitCircle * mShakeAmount);
            if (val == 1)
                pTweenObject.transform.localPosition = mOriginalPosition;
        }

        protected override void DoAnimReverse(float val)
        {
            throw new NotImplementedException();
        }
    }

    public class AnimatoinComp : Tweener
    {
        private Animation mAnim;
		private Animator mAnimator;
        private string mAnimName;

		public AnimatoinComp(GameObject tweenObject, Animation anim, string animName)
        {
			pTweenObject = tweenObject;
            mAnimName = animName;
            mAnim = anim;
            mAnim.Play(mAnimName);
        }

        protected override void DoAnim(float val)
        {
            mValue = mAnim[mAnimName].normalizedTime;
        }

        protected override void DoAnimReverse(float val)
        {
            throw new NotImplementedException();
        }
    }

	public class Timer : Tweener
	{
		public Timer()
		{
			GameObject timerObj = new GameObject("Timer");
			pTweenObject = timerObj;
		}

		protected override void DoAnim (float val)
		{
			if (val >= 1)
				GameObject.Destroy(pTweenObject);
		}

		protected override void DoAnimReverse (float val)
		{
			throw new NotImplementedException ();
		}
	}
}

