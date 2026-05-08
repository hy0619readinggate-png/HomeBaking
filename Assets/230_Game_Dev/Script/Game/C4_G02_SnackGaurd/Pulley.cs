using UnityEngine;

namespace DoDoEng.Game.C4_G02
{
    public class Pulley : MonoBehaviour
    {
        // Fields
        private Vector3 initialScale;



        // Functions
       private void updateTransformScale()
        {
            var distance = Vector3.Distance(startPos.position, targetPos.position);
            rope.transform.localScale = new Vector3(initialScale.x, distance/4, initialScale.z);
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject rope = null;
        [SerializeField] private Transform startPos = null;
        [SerializeField] private Transform targetPos = null;

        // Unity Messages
        private void Awake()
        {
            initialScale = rope.transform.localScale;
        }
        private void Update()
        {
            if(targetPos.hasChanged)
                updateTransformScale();
        }
    }
}