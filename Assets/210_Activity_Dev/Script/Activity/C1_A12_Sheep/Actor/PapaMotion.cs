using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    public enum PapaMotionState
    {
        Stand,
        MoveL,
        MoveR
    }

    public class PapaMotion : MonoBehaviour
    {
        // Properties
        public float MoveSpeed => moveSpeed;

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
        public void DoCorrect()
        {
            LOG.Info($"DoCorrect()", this);

            if (!isPlayFeedback)
                StartCoroutine(coCorrect());
        }
        public void DoWrong()
        {
            LOG.Info($"DoWrong()", this);

            if (!isPlayFeedback)
                StartCoroutine(coWrong());
        }



        // Fields
        private PapaMotionState currentState = PapaMotionState.Stand;
        private Vector2? targetPosition = null;
        private Vector2 velocity;
        private bool isPlayFeedback = false;

        // Functions
        private PapaMotionState getState(Vector2 velocity)
        {
            if (velocity.magnitude == 0)
                return PapaMotionState.Stand;
            else return velocity.x > 0 ? PapaMotionState.MoveR : PapaMotionState.MoveL;
        }
        private void updateAnimation(PapaMotionState nextState, bool force = false)
        {
            if (papa == null) return;

            if (currentState != nextState || force)
            {
                switch (nextState)
                {
                    case PapaMotionState.Stand:
                        if (currentState == PapaMotionState.MoveR)
                            papa.PlayAnimation(PapaAnimation.WalkR_toIdle, PapaAnimation.Idle);
                        else papa.PlayAnimationLoop(PapaAnimation.Idle);
                        break;

                    case PapaMotionState.MoveR:
                        papa.PlayAnimationLoop(PapaAnimation.WalkR);
                        break;

                    case PapaMotionState.MoveL:
                        papa.PlayAnimationLoop(PapaAnimation.WalkL);
                        break;
                }

                if (currentState == PapaMotionState.Stand && !isPlayFeedback)
                    AudioMGR.One.PlayEffectLL(walkCLIP, true);

                if (currentState != PapaMotionState.Stand &&
                nextState == PapaMotionState.Stand)
                    AudioMGR.One.StopEffectLL();

                currentState = nextState;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PapaAni papa = null;
        [SerializeField] private Transform boundLB = null;
        [SerializeField] private Transform boundRT = null;
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
            if (isPlayFeedback) return;

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
            {
                transform.position += (Vector3)velocity * Time.deltaTime;
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, boundLB.position.x, boundRT.position.x),
                    Mathf.Clamp(transform.position.y, boundLB.position.y, boundRT.position.y),
                    transform.position.z);
            }

            var state = getState(velocity);
            updateAnimation(state);
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
                        break;

                    // 목표지점에 도착했을 경우 종료
                    var distance = Vector2.Distance(transform.position, targetPosition.Value);
                    if (distance < moveThreshold)
                    {
                        targetPosition = null;
                        break;
                    }

                    yield return null;
                }
            }
        }
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                isPlayFeedback = true;
                yield return null;

                updateAnimation(PapaMotionState.Stand, true);

                yield return papa.Correct();
                yield return null;

                updateAnimation(PapaMotionState.Stand, true);
                isPlayFeedback = false;
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                isPlayFeedback = true;
                yield return null;

                updateAnimation(PapaMotionState.Stand, true);

                yield return papa.Wrong();
                yield return null;

                updateAnimation(PapaMotionState.Stand, true);
                isPlayFeedback = false;
            }
        }
    }
}