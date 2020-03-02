using UnityEngine;

namespace InputUtil
{
    public class InputUtility : MonoBehaviour
    {
        public static int touchCount
        {
            get { return UnityEngine.Input.touchCount; }
        }
        public static Touch[] touches
        {
            get { return UnityEngine.Input.touches; }
        }
        public static Vector3 mousePosition
        {
            get { return UnityEngine.Input.mousePosition; }
        }

        public static Vector3? GetFingerOrMousePos(int fingerID)
        {
            if (touchCount > 0)
            {
                if (fingerID != -1)
                {
                    Touch? touch = GetTouchByFingerID(fingerID);
                    if (touch.HasValue)
                        return touch.Value.position;
                    else
                    {
                        Debug.LogError("Cannot get position of invalid finger ID " + fingerID);
                        return null;
                    }
                }
                else
                    return GetTouch(touchCount - 1).position;
            }
            return mousePosition;
        }
        public static Touch GetTouch(int index)
        {
            return UnityEngine.Input.GetTouch(index);
        }

        public static Touch? GetTouchByFingerID(int fingerID)
        {
            for (int touchIdx = 0; touchIdx < touches.Length; ++touchIdx)
            {
                Touch touch = touches[touchIdx];
                if (touch.fingerId == fingerID)
                    return touch;
            }
            return null;
        }
    }
}
