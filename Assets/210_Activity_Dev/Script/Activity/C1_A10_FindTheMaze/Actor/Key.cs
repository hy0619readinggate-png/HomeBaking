using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class Key : MonoBehaviour
    {
        // Properties
        public bool IsAvaliable { get; private set; }

        // Methods
        public void Reset()
        {
            LOG.Info($"Show()", this);

            transform.position = originPosition;
            effGO.SetActive(false);

            IsAvaliable = true;
        }
        public void Get()
        {
            LOG.Info($"Get()", this);

            IsAvaliable = false;
        }



        // Fields
        private Vector3 originPosition;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject effGO = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
            originPosition = transform.position;
            effGO.SetActive(false);
        }
    }
}