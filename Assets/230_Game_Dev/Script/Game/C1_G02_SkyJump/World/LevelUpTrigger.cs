using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class LevelUpTrigger : MonoBehaviour
    {
        // Fields : caching
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();



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
            {
                col.enabled = false;
                player.LevelUp();
            }
        }
    }
}