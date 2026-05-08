using UnityEngine;

namespace DoDoEng.Game.C4_G02
{
    public class WallCollider : MonoBehaviour
    {
        // Unity Messages
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null)
            {
                Destroy(collision.attachedRigidbody.gameObject);
            }
        }
    }
}