using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using System;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterMotion))]
    [RequireComponent(typeof(CharacterLeader))]
    public class Dodo : BYDSingleton<Dodo>
    {
        // Definitions
        public int FriendCount => friends;

        // Properties
        public Vector3 Position => transform.position;
        public float Distance => friendDistances[friends];

        // Methods
        public void TeleportTo(Vector3 position) => motion.TeleportTo(position);
        public void MoveTo(Vector3 position) => motion.MoveTo(position);
        public void StopMoving() => motion.StopMoving();
        public void ChangeSpeed(bool boost) => motion.ChangeSpeed(boost);
        public Coroutine MoveAndWait(Vector2 position, bool forceForward = false) => motion.MoveAndWait(position, forceForward);
        public void ResetWayPoints() => leader.ResetWayPoints();
        public void EnableRecordPath(bool enable) => leader.EnableRecordPath(enable);
        public void EnableMotion(bool enable) => motion.enabled = enable;

        // Methods
        public void AddFriend(Friend friend)
        {
            LOG.Info($"AddFriend() | {friend.gameObject.name}", this);

            var dist = friendDistances[friends];
            friend.SetFollow(leader, dist);
            friends++;
        }
        public Vector2 GetNextFriendPosition()
        {
            var dist = friendDistances[friends];
            var position = leader.GetPosition(dist);

            if (position == null)
            {
                LOG.Error("No Position for new friends", this);
                return Vector2.zero;
            }

            return position.Value;
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            characterAni.PlayAnimationLoop(CharacterAnimation.Idle);
        }
        public void IdleForward()
        {
            LOG.Info($"IdleForward()", this);

            characterAni.PlayAnimationLoop(CharacterAnimation.Idle);
            characterAni.FlipX(true);
        }
        public void Direction(bool right = true)
        {
            LOG.Info($"Direction()", this);

            characterAni.FlipX(right);
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            var ani = UtilArray.ExtractOne(correctAnimations);
            characterAni.PlayAnimationLoop(ani);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            var ani = UtilArray.ExtractOne(wrongAnimations);
            characterAni.PlayAnimation(ani);
        }
        public void Join()
        {
            LOG.Info($"Join()", this);

            characterAni.PlayAnimation(CharacterAnimation.Join);
        }
        public void Gogo()
        {
            LOG.Info($"Gogo()", this);

            characterAni.PlayAnimation(CharacterAnimation.Gogo);
        }
        public void JustLook(Vector2 position)
        {
            LOG.Info($"JustLook()", this);

            var isRight = position.x >= transform.position.x;

            characterAni.PlayAnimationLoop(isRight
                                            ? CharacterAnimation.IdleR
                                            : CharacterAnimation.IdleL);
        }
        public void GetKey()
        {
            LOG.Info($"GetKey()", this);

            characterAni.PlayAnimation(CharacterAnimation.Key);
        }

        // Events
        public event Action<Key> OnGetKey;
        public event Action OnTryExit;



        // Fields : caching
        private CharacterMotion motion_ = null;
        private CharacterMotion motion => motion_ ??= GetComponent<CharacterMotion>();
        private CharacterLeader leader_ = null;
        private CharacterLeader leader => leader_ ??= GetComponent<CharacterLeader>();

        // Fields
        private int friends = 0;



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private CharacterAni characterAni = null;
        [Header("¡Ú Config")]
        [SerializeField] private float[] friendDistances = null;
        [SerializeField] private CharacterAnimation[] correctAnimations = null;
        [SerializeField] private CharacterAnimation[] wrongAnimations = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision}", this);

            var key = collision.GetComponent<Key>();
            if (key != null && key.IsAvaliable)
                OnGetKey?.Invoke(key);

            if (collision.name == "ExitTrigger")
                OnTryExit?.Invoke();
        }
    }
}