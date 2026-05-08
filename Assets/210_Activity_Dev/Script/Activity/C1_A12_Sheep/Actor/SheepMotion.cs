using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    public enum SheepMotionState
    {
        Stand,
        MoveL,
        MoveR
    }

    public class SheepMotion : MonoBehaviour
    {
        // Properties
        public bool SuppressMotion { get; set; } = false;
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        // Methods
        public void MoveTo(Vector2 position)
        {
            targetPosition = position;
        }
        public void StopMoving()
        {
            targetPosition = null;
        }
        public Coroutine MoveAndWait(Vector2 position)
        {
            LOG.Info($"MoveAndWait() | {position}", this);

            return StartCoroutine(coMoveTo(position));
        }



        // Fields
        private SheepMotionState currentState = SheepMotionState.Stand;
        private Vector2? targetPosition = null;
        private Vector2 velocity;

        // Functions
        private SheepMotionState getState(Vector2 velocity)
        {
            if (velocity.magnitude == 0)
                return SheepMotionState.Stand;
            else return velocity.x > 0 ? SheepMotionState.MoveR : SheepMotionState.MoveL;
        }
        private void updateAnimation(SheepMotionState nextState, bool force = false)
        {
            if (sheep == null) return;

            if (currentState != nextState || force)
            {
                switch (nextState)
                {
                    case SheepMotionState.Stand:
                        sheep.FlipX(currentState == SheepMotionState.MoveL);
                        sheep.PlayAnimationLoopT2(
                            SheepAnimation.Idle,
                            SheepAnimation.Idle2,
                            SheepAnimation.Idle3);
                        break;

                    case SheepMotionState.MoveR:
                        sheep.FlipX(true);
                        sheep.PlayAnimationLoop(SheepAnimation.WalkR);
                        break;

                    case SheepMotionState.MoveL:
                        sheep.FlipX(true);
                        sheep.PlayAnimationLoop(SheepAnimation.WalkL);
                        break;
                }

                if (walkCLIP != null)
                {
                    if (currentState == SheepMotionState.Stand)
                        AudioMGR.One.PlayEffectLL(walkCLIP, true);

                    if (currentState != SheepMotionState.Stand &&
                        nextState == SheepMotionState.Stand)
                        AudioMGR.One.StopEffectLL();
                }

                currentState = nextState;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SheepAni sheep = null;
        [Header("★ Config")]
        [SerializeField] private float moveSpeed = 3; // m/sec
        [SerializeField] private float moveThreshold = 0.2f;
        [SerializeField] private float arriveThreshold = 0.1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip walkCLIP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (targetPosition != null)
            {
                var target = targetPosition.Value;
                var displacement = target - (Vector2)transform.position;

                if (displacement.magnitude > moveThreshold)
                    velocity = displacement.normalized * moveSpeed;

                if (displacement.magnitude < arriveThreshold)
                    velocity = Vector2.zero;
            }
            else velocity = Vector2.zero;

            if (velocity != Vector2.zero)
                transform.position += (Vector3)velocity * Time.deltaTime;

            if (!SuppressMotion)
            {
                var state = getState(velocity);
                updateAnimation(state);
            }
        }

        // Unity Coroutine
        IEnumerator coMoveTo(Vector2 position)
        {
            using (LOG.Coroutine($"coMoveTo() | {position}", this))
            {
                targetPosition = position;

                while (true)
                {
                    // 다른 기능을 통해 목표지점을 해제할 경우 종료
                    if (targetPosition == null)
                    {
                        LOG.Warning($"targetPosition lost!", this);
                        break;
                    }

                    // 목표지점에 도착했을 경우 종료
                    var distance = Vector2.Distance(transform.position, targetPosition.Value);
                    if (distance < arriveThreshold)
                        break;

                    yield return null;
                }
            }
        }
    }
}