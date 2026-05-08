using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class CloudDestroyer : MonoBehaviour
    {
        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var cloud = collision.GetComponent<Cloud>();
            if (cloud != null)
            {
                if (cloud.Booster != null)
                    cloud.Booster.Disappear();

                Destroy(cloud.gameObject);
            }
        }
    }
}