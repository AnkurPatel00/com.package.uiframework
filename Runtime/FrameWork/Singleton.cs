using UnityEngine;

namespace MonoUtility
{
    /// <summary>
    /// Singleton base class.
    /// Derive this class to make it Singleton.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        public bool _PersistentOnSceneChange = false;

        /// <summary>
        /// Returns the instance of this singleton.
        /// </summary>
        public static T pInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).ToString());
                        instance = obj.AddComponent<T>();
                        //Debug.LogError("An instance of " + typeof(T) + 
                        //   " is needed in the scene, but there is none.");
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (_PersistentOnSceneChange)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void Start()
        {
            T[] instance = FindObjectsOfType<T>();
            if (instance.Length > 1)
            {
                Debug.Log(gameObject.name + " has been destroyed because another object already has the same component.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (this == instance)
                instance = null;
        }
    }
}