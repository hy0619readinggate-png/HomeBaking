using System;
using UnityEngine;


namespace DoDoEng.Game.C4_G02
{
    public class Bullet : MonoBehaviour
    {

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator _Animator = null;
        [Header("★ Config")]
        [SerializeField, Range(1, 10)] private float _BulletRotationSpeed = 1f;
        [SerializeField, Range(1, 20)] private float _BulletSpeed = 1f;

        // Unity Messages
        private void Start()
        {
            _Animator.speed = _BulletRotationSpeed;
        }
        private void Update()
        {
            transform.position += Vector3.left * _BulletSpeed * Time.deltaTime;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null)
            {
                var bubble = collision.attachedRigidbody.GetComponent<Bubble>();
                if (bubble != null)
                    Destroy(this.gameObject);
            }
        }
    }

}