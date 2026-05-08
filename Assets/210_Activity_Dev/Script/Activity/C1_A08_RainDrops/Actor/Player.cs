using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A08
{
    [RequireComponent(typeof(Character))]
    public class Player : MonoBehaviour, ICompletable
    {
        // Definition
        public const int MAX_COUNT = 3;

        // Properties
        public static Player ActivePlayer { get; private set; }
        public bool IsValid => IsPlaying && !IsTaking;
        public bool IsPlaying { get; private set; } = false;
        public bool IsTaking { get; private set; } = false;
        public bool IsRemainOne => correctCount == MAX_COUNT - 1;

        // Methods
        public void StartPlay(AudioClip clip, Transform limitLeftTR, Transform limitRightTR)
        {
            LOG.Info($"StartPlay() ", this);

            wordCLIP = clip;
            correctCount = 0;
            IsPlaying = true;

            var leftZoneRT = limitLeftTR.GetComponent<RectTransform>();
            var rightZoneTR = limitRightTR.GetComponent<RectTransform>();

            limitLeft = UtilTransform.LocalToScreen(limitLeftTR.position, leftZoneRT, canvas).x;
            limitRight = UtilTransform.LocalToScreen(limitRightTR.position, rightZoneTR, canvas).x;

            updateFlower();
            updateAnimation(CharacterAnimation.Idle);
        }
        public void StopPlay()
        {
            LOG.Info($"StopPlay()", this);

            IsPlaying = false;
        }
        public void MoveTo(Vector2 ptScreen)
        {
            LOG.Info($"MoveTo()", this);

            var targetX = Mathf.Min(Mathf.Max(ptScreen.x, limitLeft), limitRight);
            var screenTargetPosition = new Vector2(targetX, ptScreen.y);

            targetPosition = UtilTransform.ScreenToLocal(screenTargetPosition, rt.parent as RectTransform, canvas);

        }
        public void TakeRaindrop(RainDrop raindrop)
        {
            LOG.Info($"TakeRaindrop() | {raindrop.gameObject.name}", this);

            if (raindrop.IsAnswer)
                StartCoroutine(coCorrect());
            else StartCoroutine(coWrong());
        }
        public void DoHappy(Transform happyZoneTR)
        {
            flipByHappyZone(happyZoneTR);
            updateAnimation(CharacterAnimation.Happy);
        }


        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private Character character_ = null;
        private Character character => character_ ??= GetComponent<Character>();

        // Fields
        private AudioClip wordCLIP;
        private Vector2 targetPosition;
        private CharacterAnimation currentAnimation = CharacterAnimation.Idle;
        private int correctCount = 0;
        private float limitRight = 0;
        private float limitLeft = 0;

        // Functions
        private void updateAnimation(CharacterAnimation ani)
        {
            if (currentAnimation != ani)
            {
                character.PlayAnimationLoop(ani);
                currentAnimation = ani;

                switch (ani)
                {
                    case CharacterAnimation.Forward:
                    case CharacterAnimation.Backward:
                        AudioMGR.One.PlayEffectLL(stepCLIP, true);
                        break;

                    default:
                        AudioMGR.One.StopEffectLL();
                        break;
                }
            }
        }
        private void updateFlower()
        {
            flowers.SetActiveOnly(correctCount);
        }
        private void flipByHappyZone(Transform happyZoneTR)
        {
            var ptScreen = UtilTransform.LocalToScreen(transform.position, rt, canvas);
            var zoneRT = happyZoneTR.GetComponent<RectTransform>();
            var rect = zoneRT.rect;
            var ptZone = UtilTransform.LocalToScreen(happyZoneTR.position, zoneRT, canvas);

            if (ptScreen.x < ptZone.x - rect.width / 2)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (ptScreen.x > ptZone.x + rect.width / 2)
                transform.localScale = new Vector3(1, 1, 1);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] flowers = null;
        [SerializeField] private GameObject vfxCorrect = null;
        [SerializeField] private GameObject vfxWrong = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip stepCLIP = null;
        [Header("★ Timing")]
        [SerializeField] private float waterDropDuration = 1f;
        [SerializeField] private float correctDuration = 1f;
        [SerializeField] private float wrongDuration = 1f;
        [Header("★ Config")]
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private float moveThreshold = 5; // pixel
        [ReadOnly][SerializeField] private Vector2 velocity = Vector2.zero;

        // Unity Messages
        private void Awake()
        {
            vfxCorrect.SetActive(false);
            vfxWrong.SetActive(false);

            targetPosition = rt.anchoredPosition;
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (!IsPlaying) return;

            if (!IsTaking)
            {
                if (Mathf.Abs(targetPosition.x - rt.anchoredPosition.x) >= moveThreshold)
                    updateAnimation(CharacterAnimation.Forward);
                else updateAnimation(CharacterAnimation.Idle);
            }

            if (Mathf.Abs(targetPosition.x - rt.anchoredPosition.x) >= moveThreshold)
            {
                var s = rt.anchoredPosition;
                var f = new Vector2(targetPosition.x, s.y);
                var position = Vector2.SmoothDamp(s, f, ref velocity, smoothTime);
                rt.anchoredPosition = position;
            }

            var localScaleX = velocity.x <= 0 ? 1 : -1;
            transform.localScale = new Vector3(localScaleX, 1, 1);
        }
        private void OnEnable()
        {
            if (ActivePlayer == null)
                ActivePlayer = this;
        }
        private void OnDisable()
        {
            if (ActivePlayer == this)
                ActivePlayer = null;
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                IsTaking = true;
                yield return null;

                
                var ani = UtilArray.ExtractOne(
                    new CharacterAnimation[] {
                            CharacterAnimation.Correct1,
                            CharacterAnimation.Correct2,
                            CharacterAnimation.Correct3 }
                    );
                updateAnimation(ani);
                yield return null;

                vfxCorrect.gameObject.SetActive(true);
                correctCount++;
                if (correctCount < MAX_COUNT)
                {
                    yield return new WaitForSeconds(waterDropDuration / 2);

                    updateFlower();
                    yield return new WaitForSeconds(waterDropDuration / 2);
                }
                else
                {
                    yield return new WaitForSeconds(correctDuration / 2);

                    updateFlower();
                    yield return new WaitForSeconds(correctDuration / 2);
                }

                AudioMGR.One.PlayNarration(wordCLIP);
                updateAnimation(CharacterAnimation.Idle);
                yield return null;

                IsTaking = false;
                vfxCorrect.gameObject.SetActive(false);
                yield return null;
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                ActivityProgress.One.Wrong();
                IsTaking = true;
                yield return null;

                var ani = UtilArray.ExtractOne(
                    new CharacterAnimation[] {
                            CharacterAnimation.Wrong1,
                            CharacterAnimation.Wrong2 }
                    );
                updateAnimation(ani);
                yield return null;

                vfxWrong.gameObject.SetActive(true);
                yield return new WaitForSeconds(wrongDuration / 2);

                correctCount--;
                correctCount = Mathf.Max(correctCount, 0);
                updateFlower();
                yield return new WaitForSeconds(wrongDuration / 2);

                updateAnimation(CharacterAnimation.Idle);
                yield return null;

                IsTaking = false;
                vfxWrong.gameObject.SetActive(false); ;
                yield return null;

                AudioMGR.One.PlayNarration(wordCLIP);
                yield return null;
            }
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => correctCount == MAX_COUNT && !IsTaking;
    }
}