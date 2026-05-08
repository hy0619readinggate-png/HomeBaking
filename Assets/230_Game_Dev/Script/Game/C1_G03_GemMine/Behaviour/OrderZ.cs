using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    [ExecuteInEditMode]
    public class OrderZ : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private bool sortAtUpdate = false;

        // Unity Messages
        private void Awake()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (sortAtUpdate)
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
        }
    }
}