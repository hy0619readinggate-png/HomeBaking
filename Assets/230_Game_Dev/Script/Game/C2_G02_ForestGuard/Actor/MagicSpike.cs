using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public class MagicSpike : MonoBehaviour
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
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            var go = collision.attachedRigidbody?.gameObject;
            if (go == null) return;

            var monster = go.GetComponent<Monster>();
            if (monster != null)
                monster.BlowAway();
        }
    }
}