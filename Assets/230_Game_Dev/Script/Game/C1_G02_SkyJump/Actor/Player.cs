using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using System;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    // 다음의 내용들을 검토함 2023.10.25 by veramocor
    // https://github.com/codinginflow/2DPlatformerBeginner
    // https://gamedevbeginner.com/how-to-jump-in-unity-with-or-without-physics/
    // http://www.davetech.co.uk/gamedevplatformer

    public enum PlayerState { NA, Ground, Up, Down }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Player : BYDSingleton<Player>
    {
        // Properties
        public bool IsGround => isGround;
        public bool IsRespawning => isRespawing;
        public bool AutoJump
        {
            get => autoJump;
            set => autoJump = value;
        }
        public float MaxHeight => yMax;

        // Methods
        public void MoveTo(Vector2 ptScreen)
        {
            //LOG.Info($"MoveTo() | {ptScreen}", this);
            xTarget = Camera.main.ScreenToWorldPoint(ptScreen).x;
        }
        public void StopMove()
        {
            LOG.Info($"StopMove()", this);

            xTarget = null;
        }
        public void Kill()
        {
            LOG.Info($"Kill()", this);

            StartCoroutine(coDeath());
        }
        public void Respwan(Transform targetTR)
        {
            LOG.Info($"Respwan() | {targetTR.gameObject.name}", this);

            StartCoroutine(coRespwan(targetTR));
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            StartCoroutine(coCorrect());
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            StartCoroutine(coWrong());
        }
        public void BoostByItem()
        {
            LOG.Info($"BoostByItem()", this);

            StartCoroutine(coItemBoost());
        }
        public void LevelUp()
        {
            LOG.Info($"LevelUp()", this);

            StartCoroutine(coLevelBoost());
        }
        public void HappyOnTop()
        {
            LOG.Function(this);

            StartCoroutine(coHappyOnTop());
        }

        // Methods
        public void IncreaseMaxHeight(int floor)
        {
            yMax = Mathf.Max(yMax, transform.position.y + jumpHeight * floor);
        }

        // Methods
        public void Attach(Transform targetTR)
        {
            LOG.Info($"Attach() | {targetTR.gameObject.name}", this);

            transform.SetParent(targetTR);
            rb.velocity = Vector2.zero;
            xTarget = null;
        }
        public void Detach()
        {
            LOG.Info($"Detach()", this);

            transform.SetParent(null);
        }

        // Events
        public event Action OnCorrect;
        public event Action OnDeath;
        public event Action OnBoost;
        public event Action OnLevelUp;



        // Fields : caching
        private Rigidbody2D rb_ = null;
        private Rigidbody2D rb => rb_ ??= GetComponent<Rigidbody2D>();
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();

        // Fields
        private float jumpVelocity = 0;
        private float itemBoostVelocity = 0;
        private float answerBoostVelocity = 0;
        private float levelBoostVelocity = 0;

        // Fields
        private float? xTarget;
        private float yMax = 0;
        private float groundTime = 0;
        private PlayerState state = PlayerState.NA;
        private bool isGround = false;
        private bool isRespawing = false;
        private bool isFeedback = false;
        private bool boostWhenNextJump = false;
        private bool isJumpReadyAction = false;

        // Functions
        private void updateAnimation()
        {
            var newState = getAnimationState();
            if (newState == state)
                return;

            switch (newState)
            {
                case PlayerState.Ground:
                    millo.PlayAnimation(MilloAnimation.Jump_Land);
                    break;

                case PlayerState.Up:
                    millo.PlayAnimation(MilloAnimation.Jump_Start, MilloAnimation.Jump_Up);
                    break;

                case PlayerState.Down:
                    millo.PlayAnimationLoop(MilloAnimation.Jump_Down);
                    break;
            }
            state = newState;
        }
        private PlayerState getAnimationState()
        {
            if (rb.velocity.y > 0.1f)
                return PlayerState.Up;
            if (rb.velocity.y < -0.1f)
                return PlayerState.Down;

            if (state == PlayerState.Down && Mathf.Abs(rb.velocity.y) < 0.1f)
                return PlayerState.Ground;
            else return PlayerState.NA;
        }

        // Functions
        private bool isGrounded()
        {
            if (rb.velocity.y > 0.1f) return false;

            var pos = col.bounds.center;
            var size = col.bounds.size;
            var dir = Vector3.down;
            return Physics2D.BoxCast(pos, size, 0f, dir, groundCheckDistance, jumpLayer);
        }
        private void jump()
        {
            if (boostWhenNextJump)
            {
                boostWhenNextJump = false;
                StartCoroutine(coAnswerBoost());
            }
            else StartCoroutine(coJump());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private MilloAni millo = null;
        [SerializeField] private GameObject respawnFxGO = null;
        [SerializeField] private GameObject collectFxGO = null;
        [SerializeField] private Animator boostANIM = null;
        [Header("★ Bindings - vfx")]
        [SerializeField] private ParticleSystem boost1FX = null;
        [SerializeField] private ParticleSystem boost2FX = null;
        [SerializeField] private ParticleSystem popcornFX = null;
        [Header("★ Config - Horizontal")]
        [SerializeField] private float xSmoothTime = 0.3f;
        [SerializeField] private float xMoveThreshold = 0.5f;
        [Header("★ Config - Vertical")]
        [SerializeField] private bool autoJump = false;
        [SerializeField] private float jumpHeight = 6f;
        [SerializeField] private float itemBoostHeight = 8.5f;
        [SerializeField] private float answerBoostHeight = 17f;
        [SerializeField] private float levelBoostHeight = 50f;
        [SerializeField] private float jumpDelay = 0.8f;
        [SerializeField] private float groundCheckDistance = 1f;
        [SerializeField] private LayerMask jumpLayer;
        [Header("★ Config - Respawn")]
        [SerializeField] private float respawnDelay = 0.5f;
        [SerializeField] private float respawnOffsetY = 0.2f;
        [Header("★ Config - Sfx")]
        [SerializeField] private float correctSfxInterval = 0.7f;
        [SerializeField] private float wrongSfxDelay = 0.55f;
        [Header("★ Config - Jump")]
        [SerializeField] private float jumpReadyDuration = 0.8f;
        [Header("★ Config")]
        [SerializeField] private MilloAnimation[] correctAnimations = null;
        [SerializeField] private MilloAnimation[] wrongAnimations = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip jumpCLIP = null;
        [SerializeField] private AudioClip jumpBoostCLIP = null;
        [SerializeField] private AudioClip jumpBoostNarrCLIP = null;
        [SerializeField] private AudioClip respawnCLIP = null;
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip[] correctNarCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [SerializeField] private AudioClip fallCLIP = null;
        [SerializeField] private AudioClip levelUpSfxCLIP = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            respawnFxGO.SetActive(false);
            collectFxGO.SetActive(false);
            boostANIM.gameObject.SetActive(true);

            var m1 = boost1FX.main;
            m1.playOnAwake = false;
            m1.stopAction = ParticleSystemStopAction.None;
            boost1FX.Stop();
            boost1FX.gameObject.SetActive(true);

            var m2 = boost2FX.main;
            m2.playOnAwake = false;
            m2.stopAction = ParticleSystemStopAction.None;
            boost2FX.Stop();
            boost2FX.gameObject.SetActive(true);

            var m3 = popcornFX.main;
            m3.playOnAwake = true;
            m3.stopAction = ParticleSystemStopAction.Disable;
            popcornFX.gameObject.SetActive(false);

            xTarget = null;
            jumpVelocity = Mathf.Sqrt(jumpHeight * -2 * Physics2D.gravity.y * rb.gravityScale);
            itemBoostVelocity = Mathf.Sqrt(itemBoostHeight * -2 * Physics2D.gravity.y * rb.gravityScale);
            answerBoostVelocity = Mathf.Sqrt(answerBoostHeight * -2 * Physics2D.gravity.y * rb.gravityScale);
            levelBoostVelocity = Mathf.Sqrt(levelBoostHeight * -2 * Physics2D.gravity.y * rb.gravityScale);
        }
        private void Start()
        {
        }
        private void Update()
        {
            isGround = isGrounded();

            //if (Input.GetKeyDown(KeyCode.Space))
            //    jump();

            // 수평 움직임
            if (!isRespawing && !isFeedback && !isGround && xTarget != null &&
                Mathf.Abs(xTarget.Value - rb.position.x) >= xMoveThreshold)
            {
                var velocity = rb.velocity.x;
                var s = rb.position.x;
                var f = xTarget.Value;
                Mathf.SmoothDamp(s, f, ref velocity, xSmoothTime);

                rb.velocity = new Vector2(velocity, rb.velocity.y);

                millo.FlipX(rb.velocity.x > 0 ? false : true);
            }
            if (xTarget != null && Mathf.Abs(xTarget.Value - rb.position.x) < 0.1f)
                xTarget = null;
            if (xTarget == null)
                rb.velocity = new Vector2(0, rb.velocity.y);

            // 플랫폼에 도착하고, 일정 시간이 지나면, 자동 점프
            if (autoJump && isGround && Mathf.Abs(rb.velocity.y) < 0.1f && !isJumpReadyAction)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                groundTime += Time.deltaTime;
                if (groundTime > jumpDelay || boostWhenNextJump)
                    jump();
            }
            else groundTime = 0;

            // 애니메이션
            if (!isFeedback)
                updateAnimation();

            // 최대 높이 계산
            yMax = Mathf.Max(yMax, transform.position.y);
        }
        private void OnDrawGizmos()
        {
            var pos = col.bounds.center;
            var size = col.bounds.size;
            var dir = Vector3.down;

            var hit = Physics2D.BoxCast(pos, size, 0f, dir, groundCheckDistance, jumpLayer);
            if (hit.collider != null && rb.velocity.y < 0.1f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(pos, dir * hit.distance);
                Gizmos.DrawWireCube(pos + dir * hit.distance, col.bounds.size);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(pos, dir * groundCheckDistance);
                Gizmos.DrawWireCube(pos + dir * groundCheckDistance, col.bounds.size);
            }
        }

        // Unity Coroutine
        IEnumerator coDeath()
        {
            using (LOG.Coroutine($"coDeath()", this))
            {
                millo.gameObject.SetActive(false);
                autoJump = false;
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
                yield return new WaitForSeconds(0.5f);

                OnDeath?.Invoke();
            }
        }
        IEnumerator coRespwan(Transform targetTR)
        {
            using (LOG.Coroutine($"coRespwan()", this))
            {
                isRespawing = true;
                yield return null;

                autoJump = false;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                millo.gameObject.SetActive(false);
                yield return new WaitForSeconds(respawnDelay);

                var respwanPos = new Vector3(
                    targetTR.position.x,
                    targetTR.position.y + respawnOffsetY,
                    transform.position.z);
                transform.position = respwanPos;

                xTarget = null;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = Vector2.zero;

                AudioMGR.One.PlayEffect(respawnCLIP);
                respawnFxGO.SetActive(true);
                yield return new WaitForSeconds(0.5f);

                autoJump = true;
                millo.gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);

                isRespawing = false;
                yield return null;
            }
        }
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                isFeedback = true;
                yield return null;

                StartCoroutine(coCorrectSfx());
                autoJump = false;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;

                var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
                AudioMGR.One.PlayEffect(correctNarCLIP[idx]);
                yield return millo.PlayAnimationAndWait(correctAnimations[idx]);

                OnCorrect?.Invoke();
                yield return null;

                autoJump = true;
                boostWhenNextJump = true;
                xTarget = null;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = Vector2.zero;
                yield return null;

                isFeedback = false;
                yield return null;
            }
        }
        IEnumerator coCorrectSfx()
        {
            using (LOG.Coroutine($"coCorrectSfx()", this))
            {
                AudioMGR.One.PlayEffect(correctCLIP);
                yield return new WaitForSeconds(correctSfxInterval);
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                isFeedback = true;
                yield return null;

                StartCoroutine(coWrongSfx());
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;

                var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
                AudioMGR.One.PlayEffect(wrongCLIP);
                yield return millo.PlayAnimationAndWait(wrongAnimations[idx]);

                xTarget = null;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = Vector2.zero;
                yield return null;

                isFeedback = false;
                yield return null;
            }
        }
        IEnumerator coWrongSfx()
        {
            using (LOG.Coroutine($"coWrongSfx()", this))
            {
                yield return new WaitForSeconds(wrongSfxDelay);

                AudioMGR.One.PlayEffect(fallCLIP);
                yield return null;
            }
        }
        IEnumerator coItemBoost()
        {
            using (LOG.Coroutine($"coItemBoost()", this))
            {
                OnBoost?.Invoke();

                popcornFX.gameObject.SetActive(true);
                AudioMGR.One.PlayEffect(jumpBoostNarrCLIP);
                rb.velocity = new Vector2(rb.velocity.x, itemBoostVelocity);
                yield return null;

                yield return new WaitWhile(() => rb.velocity.y > 0.2f);
                yield return null;
            }
        }
        IEnumerator coAnswerBoost()
        {
            using (LOG.Coroutine($"coAnswerBoost()", this))
            {
                // #1674 점프 준비 동작
                // #2448 정답시 점프 준비동작 삭제
                isJumpReadyAction = true;
                yield return null;
                isJumpReadyAction = false;

                // 부스트
                AudioMGR.One.PlayEffect(jumpBoostCLIP);
                AudioMGR.One.PlayEffect(jumpBoostNarrCLIP);
                OnBoost?.Invoke();
                yield return null;

                boost1FX.Play();
                boost2FX.Play();

                collectFxGO.SetActive(true);
                rb.velocity = new Vector2(rb.velocity.x, answerBoostVelocity);
                boostANIM.SetTrigger("Show");
                yield return null;

                yield return new WaitWhile(() => rb.velocity.y > 0.2f);
                yield return null;

                boost1FX.Stop();
                boost2FX.Stop();
                boostANIM.SetTrigger("Hide");
                yield return null;
            }
        }
        IEnumerator coLevelBoost()
        {
            using (LOG.Coroutine($"coLevelBoost()", this))
            {
                OnLevelUp?.Invoke();
                yield return null;

                AudioMGR.One.PlayEffectLL(levelUpSfxCLIP);
                var tl = UIGameCommon.One.LevelUpTL;
                tl.time = 0;
                tl.Play();

                boost1FX.Play();
                boost2FX.Play();

                rb.velocity = new Vector2(rb.velocity.x, levelBoostVelocity);
                boostANIM.SetTrigger("Show");
                yield return null;

                yield return new WaitWhile(() => rb.velocity.y > 0.2f);
                yield return null;

                boost1FX.Stop();
                boost2FX.Stop();
                boostANIM.SetTrigger("Hide");
                yield return null;

                AudioMGR.One.StopEffectLL();
            }
        }
        IEnumerator coHappyOnTop()
        {
            using (LOG.Coroutine($"{nameof(coHappyOnTop)}", this))
            {
                yield return millo.PlayAnimationAndWait(MilloAnimation.Correct1);
                millo.PlayAnimationLoop(MilloAnimation.Idle1);
            }
        }

        // #1674 점프모션 추가
        IEnumerator coJump()
        {
            // #1674 점프 준비 동작
            isJumpReadyAction = true;
            yield return millo.PlayAnimationAndWait(MilloAnimation.Jump_Ready, false);
            millo.PlayAnimationLoop(MilloAnimation.Jump_ReadyHold);
            yield return new WaitForSeconds(jumpReadyDuration);

            isJumpReadyAction = false;

            AudioMGR.One.PlayEffect(jumpCLIP);
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);

        }
    }
}