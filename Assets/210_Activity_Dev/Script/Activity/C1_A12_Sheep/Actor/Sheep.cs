using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    public enum SheepState
    {
        Idle,
        Move
    }

    [RequireComponent(typeof(SheepMotion))]
    [RequireComponent(typeof(Collider2D))]
    public class Sheep : MonoBehaviour
    {
        // Properties
        public bool IsFollowing => isFollowing;
        public bool IsFree => !isFollowing && !isFeedback;

        // Methods
        public Coroutine DoCorrect(Barn barn)
        {
            LOG.Info($"DoCorrect() | {barn.gameObject.name}", this);

            unfollow();
            return StartCoroutine(coCorrect(barn));
        }
        public Coroutine DoWrong(Barn barn)
        {
            LOG.Info($"DoWrong() | {barn.gameObject.name}", this);

            ActivityProgress.One.Wrong();

            unfollow();
            return StartCoroutine(coWrong(barn));
        }



        // Fields : caching
        private SheepMGR mgr_ = null;
        private SheepMGR mgr => mgr_ ??= GetComponentInParent<SheepMGR>();
        private SheepMotion motion_ = null;
        private SheepMotion motion => motion_ ??= GetComponent<SheepMotion>();
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();

        // Fields
        private Transform followTR = null;
        private bool isFollowing = false;
        private bool isFeedback = false;
        private Coroutine crActAlong = null;
        private int originalSortingOrder;

        // Functions
        private void wander()
        {
            crActAlong = StartCoroutine(coActAlong());
        }
        private void follow(float moveSpeed)
        {
            LOG.Info($"follow() | {moveSpeed}", this);

            var tr = Papa.One.TakeSeat();
            isFollowing = true;
            StartCoroutine(coFollow(tr, moveSpeed));
        }
        private void unfollow()
        {
            LOG.Info($"unfollow() | {followTR.gameObject.name}", this);

            sheep.SorderOrder = originalSortingOrder;
            motion.MoveSpeed = defaultSpeed * speedRatio;

            Papa.One.ReleaseSeat(followTR);
            followTR = null;
            isFollowing = false;
        }

        // Functions
        private void playSheepSfx()
        {
            var clip = UtilRandom.RandomSuccess(0.5f) ?
                correct1CLIP :
                correct2CLIP;

            AudioMGR.One.PlayEffect(clip);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SheepAni sheep = null;
        [SerializeField] private ParticleSystem followFX = null;
        [Header("★ Config")]
        [SerializeField] private Range idleDuration = new Range(3, 6);
        [SerializeField] private float moveRatio = 0.5f;        // 이동할 확률
        [SerializeField] private float maxMoveDistance = 2f;    // 최대 이동 거리
        [SerializeField] private float defaultSpeed = 1f;       // 기본 이동 속도
        [SerializeField] private float speedRatio = 1f;         // 이동 속도 비율
        [SerializeField] private float enterScale = 0.8f;       // 입장시 스케일
        [SerializeField] private int enterSortingOrder = 40;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correct1CLIP;
        [SerializeField] private AudioClip correct2CLIP;
        [SerializeField] private AudioClip wrongCLIP;

        // Unity Messages
        private void Awake()
        {
            originalSortingOrder = sheep.SorderOrder;
            motion.MoveSpeed = defaultSpeed * speedRatio;

            var main = followFX.main;
            main.stopAction = ParticleSystemStopAction.Disable;
            main.playOnAwake = true;
            followFX.gameObject.SetActive(false);
        }
        private void Start()
        {
            var flip = UtilRandom.RandomSuccess(0.5f);
            sheep.FlipX(flip);
            wander();
        }
        private void Update()
        {
            if (followTR != null)
                motion.MoveTo(followTR.position);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var papa = collision.GetComponent<Papa>();
            if (papa != null && !isFollowing && !isFeedback)
                follow(papa.MoveSpeed * speedRatio);
        }

        // Unity Coroutine
        IEnumerator coActAlong()
        {
            using (LOG.Coroutine($"coActAlong()", this))
            {
                while (true)
                {
                    yield return new WaitForSeconds(idleDuration.RandomValue());
                    yield return null;

                    if (UtilRandom.RandomSuccess(moveRatio))
                    {
                        var pos = mgr.GetMovingPosition(transform.position, maxMoveDistance);
                        if (pos != null)
                            yield return motion.MoveAndWait(pos.Value);
                    }
                }
            }
        }
        IEnumerator coCorrect(Barn barn)
        {
            using (LOG.Coroutine($"coCorrect() | {barn.gameObject.name}", this))
            {
                isFeedback = true;
                col.enabled = false;

                yield return motion.MoveAndWait(barn.EntranceTR.position);
                yield return null;

                motion.SuppressMotion = true;
                yield return null;

                Papa.One.DoCorrect();
                AudioMGR.One.PlayEffect(correct1CLIP);
                yield return sheep.PlayAnimationAndWait(SheepAnimation.Correct);
                yield return null;

                barn.DoSuccess();
                AudioMGR.One.PlayEffect(correct2CLIP);
                sheep.FlipX(true);
                yield return sheep.PlayAnimationAndWait(
                    barn.ID == 1
                    ? SheepAnimation.CorrectL
                    : SheepAnimation.CorrectR);

                sheep.SorderOrder = enterSortingOrder;
                sheep.transform.localScale = Vector3.one * enterScale;
                sheep.PlayAnimationLoopT2(
                    SheepAnimation.Idle,
                    SheepAnimation.Idle2,
                    SheepAnimation.Idle3);

                //motion.SuppressMotion = false;
                //Destroy(gameObject);
            }
        }
        IEnumerator coWrong(Barn barn)
        {
            using (LOG.Coroutine($"coWrong() | {barn.gameObject.name}", this))
            {
                isFeedback = true;
                col.enabled = false;

                yield return motion.MoveAndWait(barn.EntranceTR.position);
                yield return null;

                motion.SuppressMotion = true;
                yield return null;

                Papa.One.DoWrong();
                barn.DoWrong();
                yield return null;

                AudioMGR.One.PlayEffect(wrongCLIP);
                sheep.FlipX(true);
                yield return sheep.PlayAnimationAndWait(
                    barn.ID == 1
                    ? SheepAnimation.WrongL
                    : SheepAnimation.WrongR);

                motion.SuppressMotion = false;
                isFeedback = false;
            }

            StartCoroutine(coRespawn());
        }
        IEnumerator coRespawn()
        {
            using (LOG.Coroutine($"coRespawn()", this))
            {
                followFX.gameObject.SetActive(false);
                playSheepSfx();

                var pos = mgr.GetRespawnPosition(out var startPos);
                transform.position = startPos;
                yield return motion.MoveAndWait(pos);

                col.enabled = true;
            }

            wander();
        }
        IEnumerator coFollow(Transform tr, float moveSpeed)
        {
            using (LOG.Coroutine($"coFollow() | {tr.gameObject.name} {moveSpeed}", this))
            {
                this.StopCoroutineSafe(ref crActAlong);

                motion.MoveSpeed = 0;
                yield return null;

                followFX.gameObject.SetActive(true);
                playSheepSfx();
                sheep.PlayAnimation(SheepAnimation.Correct);
                yield return new WaitForSeconds(1f);

                sheep.SorderOrder += 10;
                motion.MoveSpeed = moveSpeed;
                followTR = tr;
                yield return null;
            }
        }
    }
}