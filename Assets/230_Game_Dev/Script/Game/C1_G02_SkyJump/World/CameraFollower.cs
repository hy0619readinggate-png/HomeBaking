using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class CameraFollower : MonoBehaviour
    {
        // Fields : caching
        private Transform cameraTR_ = null;
        private Transform cameraTR => cameraTR_ ??= Camera.main.transform;

        // Fields
        private float defaultOffset = 0;



        // Unity Messages
        private void Awake()
        {
            defaultOffset = cameraTR.position.y - transform.position.y;
        }
        private void Start()
        {
        }
        private void LateUpdate()
        {
            var offset = cameraTR.position.y - transform.position.y;
            if (offset > defaultOffset)
            {
                transform.position =
                    new Vector3(
                        transform.position.x,
                        cameraTR.position.y - defaultOffset,
                        transform.position.z);
            }
        }
    }
}