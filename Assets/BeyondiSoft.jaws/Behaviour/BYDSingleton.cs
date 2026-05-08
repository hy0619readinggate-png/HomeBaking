using UnityEngine;

namespace beyondi.Behaviour
{
    [DefaultExecutionOrder(-1000)]
    public class BYDSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Methods
        public static T One => one;



        // Fields
        private static T one = null;



        // Unity Messages
        protected virtual void Awake()
        {
            if (one == null)
                one = GetComponent<T>();
            else Debug.LogError($"Singleton must be unique!! {typeof(T)}");
        }
        protected virtual void OnDestroy()
        {
            if (one == this)
                one = null;
        }
    }
}