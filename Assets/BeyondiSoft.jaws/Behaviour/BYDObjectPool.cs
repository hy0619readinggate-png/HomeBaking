using beyondi.Util;
using UnityEngine;
using UnityEngine.Pool;

namespace beyondi.Behaviour
{
    public interface IBYDPooledObject<T> where T : MonoBehaviour
    {
        IObjectPool<T> Pool { get; set; }
    }

    public class BYDObjectPool<T> : MonoBehaviour where T : MonoBehaviour, IBYDPooledObject<T>
    {
        // Methods
        public T Get()
        {
            return objectPool.Get();
        }
        public void Release(T item)
        {
            objectPool.Release(item);
        }



        // Fields
        private IObjectPool<T> objectPool;

        // Functions
        private T createObject()
        {
            var instance = Instantiate(prefab, parentTR != null ? parentTR : transform);
            instance.Pool = objectPool;
            return instance;
        }
        private void onReleaseToPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }
        private void onGetFromPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }
        private void onDestroyPooledObject(T pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private T prefab = null;
        [SerializeField] private Transform parentTR = null;
        [Header("★ Config")]
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 20;
        [SerializeField] private int maxSize = 100;

        // Unity Messages
        protected virtual void Awake()
        {
            if (parentTR != null)
                parentTR.RemoveAllChildren();

            objectPool = new ObjectPool<T>(createObject,
                 onGetFromPool, onReleaseToPool, onDestroyPooledObject,
                 collectionCheck, defaultCapacity, maxSize);
        }
    }
}