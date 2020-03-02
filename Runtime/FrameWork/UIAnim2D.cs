using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
    [Serializable]
    public class UIAnimSpriteInfo
    {
        public Sprite _Sprite;
        public float _Time;
    }

    [Serializable]
    public class UIAnimInfo
    {
        public string _Name;
        public UIAnimSpriteInfo[] _SpriteInfo;
        public float _Length;
        // If loop count is set to less than zero it will play in loop.
        public int _LoopCount;
        public Image _Image;
        public string _ClipName;

        private Sprite mOriginalSprite;

        private int mCachedLoopCount;
        public int pLoopCount
        {
            get { return mCachedLoopCount; }
            set { mCachedLoopCount = value; }
        }

        public void CacheOriginalSprite()
        {
            if (_Image != null)
                mOriginalSprite = _Image.sprite;
        }

        public void SetOriginalSprite()
        {
            if (_Image != null)
                _Image.sprite = mOriginalSprite;
        }
    }

    public class UIAnim2D : MonoBehaviour
    {
        public UIAnimInfo[] _Anims;
        public int _StartUpIndex = -1;

        private bool mPlaying = false;
        private float mLastTime = 0;
        private float mNextDuration = 0;
        private int mLoopCount = 0;
        private UIWidget mWidget = null;

        public event System.Action<UIAnim2D> OnUpdate;

        public UIAnimInfo pCurrentAnimInfo { get; private set; }

        public int pSpriteIndex { get; private set; }

        public bool mIsPlaying
        {
            get { return mPlaying; }
        }

        void Awake()
        {
            mWidget = gameObject.GetComponent<UIWidget>();

            if (mWidget != null)
                mWidget.SetAnim2D(this);

            if (_Anims != null && _Anims.Length > 0)
            {
                foreach (UIAnimInfo anim in _Anims)
                    anim.CacheOriginalSprite();
            }

            if (_StartUpIndex >= 0)
                Play(_StartUpIndex);
        }

        void Update()
        {
            if (mPlaying)
            {
                //TODO: we have to take care when both length and sprite time is not set.
                if (Time.realtimeSinceStartup - mLastTime > mNextDuration)
                {
                    if (pSpriteIndex >= pCurrentAnimInfo._SpriteInfo.Length)
                    {
                        pSpriteIndex = 0;

                        // If we have reached the end of the loop then stop the animation
                        if (mLoopCount == pCurrentAnimInfo.pLoopCount)
                            Stop();
                        else
                        {
                            mLoopCount++;
                            UpdateSprite();
                        }
                    }
                    else
                        UpdateSprite();
                }
            }
        }

        //Processing the next sprite
        void UpdateSprite()
        {
            if (pCurrentAnimInfo._Image == null || pCurrentAnimInfo._SpriteInfo.Length == 0)
                return;

            pCurrentAnimInfo._Image.sprite = pCurrentAnimInfo._SpriteInfo[pSpriteIndex]._Sprite;
            mLastTime = Time.realtimeSinceStartup;

            if (pCurrentAnimInfo._Length <= 0)
                mNextDuration = pCurrentAnimInfo._SpriteInfo[pSpriteIndex]._Time;

            if (OnUpdate != null)
                OnUpdate(this);

            pSpriteIndex++;
        }

        public void Play(string animName, int loopCount)
        {
            int animIdx = GetIndex(animName);

            Play(animIdx, loopCount);
        }

        public void Play(int idx, int loopCount)
        {
            if (idx >= 0 && idx < _Anims.Length)
            {
                _Anims[idx].pLoopCount = loopCount;
                Play(idx);
            }
        }

        public void Play(string animName)
        {
            int animIdx = GetIndex(animName);

            if (animIdx >= 0)
                Play(animIdx, _Anims[animIdx]._LoopCount);
        }

        public void Play(int animIdx, bool inPlayAtRandomSprites = false)
        {
            if (_Anims == null || _Anims.Length <= animIdx)
                return;

            pCurrentAnimInfo = _Anims[animIdx];
            _Anims[animIdx].pLoopCount = _Anims[animIdx]._LoopCount;
            mPlaying = true;
            if (!inPlayAtRandomSprites)
                pSpriteIndex = 0;
            else
                pSpriteIndex = UnityEngine.Random.Range(0, pCurrentAnimInfo._SpriteInfo.Length);
            mLoopCount = 0;
            mLastTime = 0;

            if (pCurrentAnimInfo._Length > 0)
                mNextDuration = pCurrentAnimInfo._Length / pCurrentAnimInfo._SpriteInfo.Length;
            else if (pCurrentAnimInfo._SpriteInfo.Length > 0)
                mNextDuration = pCurrentAnimInfo._SpriteInfo[0]._Time;

            UpdateSprite();
            PlayLegacyAnim(_Anims[animIdx]._ClipName);
        }

        //Stops only if current playing anim is same as the animName that needs to be stopped.
        public void Stop(string animName)
        {
            if (mPlaying && pCurrentAnimInfo != null && (pCurrentAnimInfo._Name == animName))
                Stop();
        }

        //Stops whichever anim is playing. Resetting the Play bool and other required anim parameters.
        public void Stop()
        {
            mPlaying = false;

            if (mWidget != null && pCurrentAnimInfo != null)
            {
                pCurrentAnimInfo.SetOriginalSprite();
                mWidget.Anim2DAnimEnded(GetIndex(pCurrentAnimInfo._Name));
            }
        }

        //Find the anim by name in _Anim array and return its respective index. It will returns -1, if the anim is not found.
        public int GetIndex(string animName)
        {
            if (_Anims != null && _Anims.Length > 0)
            {
                for (int idx = 0; idx < _Anims.Length; idx++)
                {
                    if (_Anims[idx]._Name == animName)
                        return idx;
                }
            }

            return -1;
        }

        void PlayLegacyAnim(string clipName)
        {
            if (clipName == null)
                return;

            Animation animation = GetComponent<Animation>();
            if (animation != null)
            {
                if (animation.GetClip(clipName) != null)
                    animation.Play(clipName);
            }
        }
    }
}