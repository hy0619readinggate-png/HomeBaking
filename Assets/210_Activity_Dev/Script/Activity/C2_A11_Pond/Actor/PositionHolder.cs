using UnityEngine;

namespace DoDoEng.Activity.C2_A11
{
    public class PositionHolder : MonoBehaviour
    {
        // Fields
        private Vector3 interval = Vector3.zero;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform targetTR = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
            interval = targetTR.position - transform.position;
        }
        private void Update()
        {
            transform.position = targetTR.position - interval;
        }
    }
}