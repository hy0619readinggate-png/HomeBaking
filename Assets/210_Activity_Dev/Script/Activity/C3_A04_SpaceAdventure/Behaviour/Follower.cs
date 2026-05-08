using beyondi.Util;
using DoDoEng.Common;
using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public class Follower : MonoBehaviour
    {
        // Methods
        public void Follow(Leader leader, float distance, float zOffset)
        {
            LOG.Info($"Follow() {leader.gameObject.name} {distance}", this);

            this.leader = leader;
            this.distance = distance;

            transform.SetLocalZ(zOffset);
        }
        public void Unfollow()
        {
            this.leader = null;
            this.distance = 0;

            transform.SetLocalZ(0);
        }



        // Fields : caching
        private Rigidbody2D rb_ = null;
        private Rigidbody2D rb => rb_ ??= GetComponent<Rigidbody2D>();

        // Fields
        private Leader leader = null;
        private bool isFollowing = false;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField][Range(0, 1)] private float followSpeed = 0.1f;
        [Header("★ DEV")]
        [SerializeField][ReadOnly] private float distance = 1;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void FixedUpdate()
        {
            if (leader != null)
            {
                var pos = leader.GetPosition(distance);

                if (isFollowing && pos == null)
                    isFollowing = false;
                else if (!isFollowing && pos != null)
                    isFollowing = true;

                if (isFollowing)
                {
                    if (pos != null)
                        rb.MovePosition(Vector2.Lerp(transform.position, pos.Value, followSpeed));
                }
            }
        }
    }
}