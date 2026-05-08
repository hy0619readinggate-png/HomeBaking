using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C2_A05
{
    [RequireComponent(typeof(ChameleonAni))]
    public class Chameleon : MonoBehaviour
    {
        // Methods
        public void Eat(int id)
        {
            LOG.Info($"Eat() | {id}", this);

            var aniName = id switch
            {
                1 => ChameleonAnimation.Hit1,
                2 => ChameleonAnimation.Hit2,
                3 => ChameleonAnimation.Hit3,
                4 => ChameleonAnimation.Hit4,
                5 => ChameleonAnimation.Hit5,
                _ => ChameleonAnimation.Idle
            };

            anim.PlayAnimation(aniName);
        }
        public Coroutine Correct()
        {
            LOG.Info($"Correct()", this);

            return StartCoroutine(coCorrect());
        }
        public Coroutine Wrong()
        {
            LOG.Info($"Wrong()", this);

            return StartCoroutine(coWrong());
        }



        // Fields : caching
        private ChameleonAni anim_ = null;
        private ChameleonAni anim => anim_ ??= GetComponent<ChameleonAni>();



        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [Header("¡Ú Config")]
        [SerializeField] private ChameleonAnimation[] correctAnimations = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
                AudioMGR.One.PlayEffect(correctCLIP[idx]);
                yield return anim.PlayAnimationAndWait(correctAnimations[idx]);
                yield return null;
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                AudioMGR.One.PlayEffect(wrongCLIP);

                yield return anim.PlayAnimationAndWait(ChameleonAnimation.Wrong);
                yield return null;
            }
        }
    }
}