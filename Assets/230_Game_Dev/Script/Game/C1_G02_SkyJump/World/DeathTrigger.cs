using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class DeathTrigger : MonoBehaviour
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
            var player = collision.GetComponent<Player>();
            if (player != null)
                player.Kill();
        }
    }
}