using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C2_A11
{
    [RequireComponent(typeof(Animator))]
    public class Tori : MonoBehaviour
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()s", this);

            anim.SetTrigger("Idle");

            tori.PlayAnimationLoop(ToriAnimation.Idle);
        }
        public IEnumerator Correct()
        {
            LOG.Info($"Correct()", this);

            var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
            AudioMGR.One.PlayEffect(correctCLIP[idx]);
            yield return tori.PlayAnimationAndWait(correctAnimations[idx]);
        }
        public IEnumerator Wrong()
        {
            LOG.Info($"Wrong()", this);

            var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
            AudioMGR.One.PlayEffect(wrongCLIP[idx]);
            yield return tori.PlayAnimationAndWait(wrongAnimations[idx]);
        }
        public Coroutine StartCross(Transform[] positions, ProblemData pData)
        {
            LOG.Info($"StartCross()", this);

            crCross = StartCoroutine(coCross(positions, pData.PhonicsCLIP, pData.WordCLIP));
            return crCross;
        }
        public void StopCross(Transform tr)
        {
            LOG.Info($"StopCross()", this);

            this.StopCoroutineSafe(ref crCross);

            DOTween.Kill(rt);

            transform.position = tr.position;
            transform.SetParent(tr);
        }
        public void OutroIdle()
        {
            LOG.Info($"OutroIdle()", this);

            anim.SetTrigger("OutroIdle");
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private Coroutine crCross = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ToriAni tori = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip jumpClip = null;
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] public float jumpPower = 1f;
        [SerializeField] public float jumpDuration = 1f;
        [SerializeField] public float jumpPrevDelay = 0.1f;
        [SerializeField] public float jumpPostDelay = 0.4f;
        [SerializeField] private ToriAnimation[] correctAnimations = null;
        [SerializeField] private ToriAnimation[] wrongAnimations = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCross(Transform[] positions, AudioClip phoneticCLIP, AudioClip WordClip)
        {
            using (LOG.Coroutine($"coCross()", this))
            {
                yield return coJumpTo(positions[0], phoneticCLIP);
                yield return coJumpTo(positions[1], phoneticCLIP);
                yield return coJumpTo(positions[2], WordClip);
                yield return coJumpTo(positions[3]);
            }
        }
        IEnumerator coJumpTo(Transform tr, AudioClip clip = null)
        {
            using (LOG.Coroutine($"coJumpTo()", this))
            {
                var isJumpComplete = false;

                tori.PlayAnimation(ToriAnimation.Jump);
                yield return new WaitForSeconds(jumpPrevDelay);

                AudioMGR.One.PlayEffect(jumpClip);
                rt.DOJump(tr.position, jumpPower, 1, jumpDuration - jumpPrevDelay - jumpPostDelay)
                .OnComplete(() =>
                {
                    transform.SetParent(tr);
                    isJumpComplete = true;
                });

                if (clip != null)
                    yield return AudioMGR.One.PlayNarrationAndWait(clip);
                yield return new WaitUntil(() => isJumpComplete);
            }
        }
    }
}