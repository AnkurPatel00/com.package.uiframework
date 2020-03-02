using System.Collections.Generic;
using MonoUtility;

namespace TweenUtil
{
	public class TweenUpdater : Singleton<TweenUpdater>
	{
		private List<Tweener> mTweeners = new List<Tweener>();

		public static List<Tweener> pTweeners
		{
			get 
			{
				return pInstance.mTweeners;
			}
		}
	  
		protected override void Awake ()
		{
			base.Awake ();
			Tweener.pOnAnimationCompleted += OnAnimDone;
		}

	    void Update()
	    {
	        for (int i = 0; i < mTweeners.Count; ++i)
	            mTweeners[i].DoUpdate();
	    }

		private void OnAnimDone(Tweener tweener)
	    {
	        mTweeners.Remove(tweener);
	    }
	}
}
