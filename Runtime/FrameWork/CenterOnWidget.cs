using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
    [RequireComponent(typeof(UIMenu))]
    public class CenterOnWidget : MonoBehaviour // vertical snap is not fully implemented.
    {
        public RectTransform _CenterToCompare;
        public GridLayoutGroup _ProxyContent;
        public float _ScrollSpeed = 10f;

        protected RectTransform mProxyRectTransform;
        protected float[] mDistancesToCenter;
        protected bool mDragging = false;
        protected int mCurrentItemIndex = 0;
        protected int mChildCount = 0;
        protected float mDestinationPosition;

        public delegate void OnPageChange(int a);
        public static event OnPageChange mPageChangeCallBack;

        public void UpdateCache()
        {
            StartCoroutine(CacheValues());
        }

        private IEnumerator CacheValues()
        {
            yield return new WaitForSeconds(0.5f);

            if (_ProxyContent != null)
            {
                mProxyRectTransform = _ProxyContent.GetComponent<RectTransform>();
                mChildCount = _ProxyContent.transform.childCount;
                mDistancesToCenter = new float[mChildCount];
                UIWidget[] childRectTransform = new UIWidget[mChildCount];
                for (int i = 0; i < mChildCount; i++)
                {
                    childRectTransform[i] = _ProxyContent.transform.GetChild(i).GetComponent<UIWidget>();

                    if (_ProxyContent.startAxis == GridLayoutGroup.Axis.Horizontal)
                        mDistancesToCenter[i] = _ProxyContent.cellSize.x * i;//Mathf.Abs (_CenterToCompare.transform.localPosition.x - _ProxyContent.transform.GetChild (i).transform.localPosition.x);
                    else
                        mDistancesToCenter[i] = Mathf.Abs(_CenterToCompare.transform.localPosition.y - childRectTransform[i].transform.localPosition.y);
                }

                EndDrag();
            }
        }

        protected virtual void Update()
        {
            if (!mDragging)
                LerpToCenter(mDestinationPosition);
        }

        void LerpToCenter(float inDestinationPos)
        {
            if (mProxyRectTransform != null)
            {
                float newX = Mathf.Lerp(mProxyRectTransform.anchoredPosition.x, inDestinationPos, Time.deltaTime * _ScrollSpeed);
                mProxyRectTransform.anchoredPosition = new Vector2(newX, mProxyRectTransform.anchoredPosition.y);
                if (mPageChangeCallBack != null)
                    mPageChangeCallBack(mCurrentItemIndex);
            }
        }

        private Vector2 GetCenterPosition()
        {
            for (int i = 0; i < mChildCount; i++)
            {
                if (Mathf.Abs(mProxyRectTransform.anchoredPosition.x) < mDistancesToCenter[i])
                {
                    mCurrentItemIndex = i;
                    if (Mathf.Abs(mProxyRectTransform.anchoredPosition.x) > (mDistancesToCenter[mCurrentItemIndex] - (_ProxyContent.cellSize.x / 2)))
                        return new Vector2(-mDistancesToCenter[mCurrentItemIndex], mProxyRectTransform.anchoredPosition.y);
                    else
                    {
                        mCurrentItemIndex--;
                        if (mCurrentItemIndex < 0)
                            mCurrentItemIndex = 0;
                        return new Vector2(-mDistancesToCenter[mCurrentItemIndex], mProxyRectTransform.anchoredPosition.y);
                    }
                }
            }
            mCurrentItemIndex = mChildCount - 1;
            return new Vector2(-mDistancesToCenter[mCurrentItemIndex], mProxyRectTransform.anchoredPosition.y);
        }

        public virtual void StartDrag()
        {
            mDragging = true;
        }

        public virtual void EndDrag()
        {
            mDestinationPosition = GetCenterPosition().x;
            mDragging = false;
            if (mPageChangeCallBack != null)
                mPageChangeCallBack(mCurrentItemIndex);
        }
    }
}
