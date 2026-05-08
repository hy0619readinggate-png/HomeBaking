using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public enum CharacterState
    {
        Stand,
        MoveL,
        MoveR
    }

    public class CharacterMotion : MonoBehaviour
    {
        // Properties
        public bool IsMoving => targetPosition != null;

        // Methods
        public void SetCharacter(CharacterAni character)
        {
            LOG.Info($"SetCharacter() | {character.gameObject.name}", this);

            characterAni = character;
            updateAnimation(currentState, true);
        }
        public void TeleportTo(Vector2 position)
        {
            transform.position = position;

            var leader = GetComponent<CharacterLeader>();
            if (leader != null)
                leader.ResetWayPoints();
        }
        public void MoveTo(Vector2 position)
        {
            playWalkEffect();
            targetPosition = position;
        }
        public void StopMoving()
        {
            LOG.Info($"StopMoving()", this);

            stopWalkEffect();
            targetPosition = null;
        }
        public Coroutine MoveAndWait(Vector2 position, bool forceForward = false)
        {
            LOG.Info($"MoveAndWait() | {position}", this);

            playWalkEffect();
            return StartCoroutine(coMoveTo(position, forceForward));
        }
        public void ChangeSpeed(bool highSpeed)
        {
            LOG.Info($"ChangeSpeed() | {highSpeed}", this);

            moveSpeed = highSpeed ? moveHighSpeed : moveNormalSpeed;
        }



        // Fields
        private CharacterState currentState = CharacterState.Stand;
        private Vector2? targetPosition = null;
        private Vector2 velocity;
        private bool isMoving = false;


        // Functions
        private CharacterState getState(Vector2 velocity)
        {
            if (velocity.magnitude == 0 || velocity.x == 0)
                return CharacterState.Stand;
            else return velocity.x > 0 ? CharacterState.MoveR : CharacterState.MoveL;
        }
        private void updateAnimation(CharacterState state, bool force = false)
        {
            if (characterAni == null) return;

            if (currentState != state || force)
            {
                switch (state)
                {
                    case CharacterState.Stand:
                        characterAni.FlipX(currentState != CharacterState.MoveL);
                        characterAni.PlayAnimationLoopT2(
                            CharacterAnimation.Idle,
                            CharacterAnimation.Idle2);
                        break;

                    case CharacterState.MoveR:
                        characterAni.FlipX(true);
                        characterAni.PlayAnimationLoop(CharacterAnimation.WalkRight);
                        break;

                    case CharacterState.MoveL:
                        characterAni.FlipX(true);
                        characterAni.PlayAnimationLoop(CharacterAnimation.WalkLeft);
                        break;
                }
                currentState = state;
            }
        }
        private void playWalkEffect()
        {
            if (!isMoving)
            {
                AudioMGR.One.PlayEffectLL(walkCLIP, true);
                isMoving = true;
            }

        }
        private void stopWalkEffect()
        {
            if (isMoving)
            {
                AudioMGR.One.StopEffectLL();
                isMoving = false;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni characterAni = null;
        [Header("★ Config")]
        [SerializeField] private float moveSpeed = 3; // m/sec
        [SerializeField] private float moveNormalSpeed = 3f; // m/sec
        [SerializeField] private float moveHighSpeed = 6f; // m/sec
        [SerializeField] private float moveThreshold = 0.2f;
        [SerializeField] private float arriveThreshold = 0.1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip walkCLIP = null;

        // Unity Messages
        private void Awake()
        {
            moveSpeed = moveNormalSpeed;
        }
        private void Start()
        {
        }
        private void FixedUpdate()
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

            transform.position += (Vector3)velocity * Time.fixedDeltaTime;

            var state = getState(velocity);
            updateAnimation(state);
        }

        // Unity Coroutine
        IEnumerator coMoveTo(Vector2 position, bool forceForward = false)
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
                        stopWalkEffect();
                        targetPosition = null;

                        if (forceForward)
                        {
                            updateAnimation(CharacterState.Stand);
                            characterAni.FlipX(true);
                        }
                        break;
                    }

                    yield return null;
                }
            }
        }
    }
}