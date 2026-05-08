using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Popcorn : MonoBehaviour
    {
        // Properties
        public bool IsCollected { get; protected set; } = false;

        // Methods
        public void Fire(float speed, float angle, float gravityScale)
        {
            LOG.Info($"Fire() | {speed} {angle} {gravityScale}", this);

            gameObject.SetActive(true);
            startFly(speed, angle, gravityScale);
            onFired();
        }
        public void StartAff()
        {
            LOG.Info($"StartAff()", this);

            pauseFly();
            onStartAff();
        }
        public void StopAff()
        {
            LOG.Info($"StopAff()", this);

            resumeFly();
            onStopAff();
        }



        // Virtual
        protected virtual void onFired() { }
        protected virtual void onStartAff() { }
        protected virtual void onStopAff() { }



        // Fields : caching
        protected RectTransform rt_ = null;
        protected RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        protected Rigidbody2D rb_ = null;
        protected Rigidbody2D rb => rb_ ??= GetComponent<Rigidbody2D>();

        // Fields
        private Coroutine crCheckOnFloor;
        private Vector3 pausedVelocity;
        private float pausedAngularV;

        // Functions
        private void startFly(float speed, float angle, float gravityScale)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = gravityScale;
            rb.velocity = new Vector2(
                Mathf.Cos((angle + 90) * Mathf.Deg2Rad),
                Mathf.Sin((angle + 90) * Mathf.Deg2Rad)) * speed;

            crCheckOnFloor = StartCoroutine(coCheckOnFloor());
        }
        private void pauseFly()
        {
            pausedVelocity = rb.velocity;
            pausedAngularV = rb.angularVelocity;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = 0;
        }
        private void resumeFly()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = pausedVelocity;
            rb.angularVelocity = pausedAngularV;
        }
        protected void stopFly()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector3.zero;

            StopCoroutine(crCheckOnFloor);
        }



        // Unity Messages
        protected virtual void Awake()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector3.zero;
        }

        // Unity Coroutine
        IEnumerator coCheckOnFloor()
        {
            using (LOG.Coroutine($"coCheckOnFloor()", this))
            {
                var startingY = rt.anchoredPosition.y;
                while (rt.anchoredPosition.y > startingY - 100)
                    yield return null;

                stopFly();
                gameObject.SetActive(false);

            }
        }
    }
}