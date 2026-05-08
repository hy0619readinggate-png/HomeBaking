using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C2_A06
{
    [RequireComponent(typeof(CharacterAni))]
    public class Character : MonoBehaviour
    {
        // Properties
        public int CharacterID => characterID;

        // Methods
        public void Setup(int pose)
        {
            LOG.Info($"Setup() | {pose}", this);

            this.pose = pose;
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            anim.PlayAnimationLoopT2(CharacterAnimation.Idle1, CharacterAnimation.Idle2);
        }
        public void StartCorrect()
        {
            LOG.Info($"StartCorrect()", this);

            crCorrect = StartCoroutine(coCorrect());
        }
        public void FinishCorrect()
        {
            LOG.Info($"FinishCorrect()", this);

            this.StopCoroutineSafe(ref crCorrect);
        }
        public void CorrectIdle()
        {
            LOG.Info($"CorrectIdle()", this);

            
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            var clip = UtilArray.ExtractOne(wrongCLIP);
            AudioMGR.One.PlayEffect(clip);

            var animName = pose == 1 ? CharacterAnimation.Wrong1 : CharacterAnimation.Wrong2;
            anim.PlayAnimationLoop(animName);
        }
        public void Out()
        {
            LOG.Info($"Out()", this);

            var animName = pose == 1 ? CharacterAnimation.Out1 : CharacterAnimation.Out2;
            anim.PlayAnimation(animName, false);
        }
        public void Drag()
        {
            LOG.Info($"Drag()", this);

            anim.PlayAnimationLoop(CharacterAnimation.Drag);
        }



        // Fields : caching
        private CharacterAni anim_ = null;
        private CharacterAni anim => anim_ ??= GetComponent<CharacterAni>();

        // Fields
        private int pose = 0;
        private Coroutine crCorrect = null;



        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("¡Ú Config")]
        [SerializeField] private int characterID;

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
                var clip = UtilArray.ExtractOne(correctCLIP);
                AudioMGR.One.PlayEffect(clip);
                yield return null;

                var animName = pose == 1 ? CharacterAnimation.CorrectReady1 : CharacterAnimation.CorrectReady2;
                yield return anim.PlayAnimationAndWait(animName, false);

                var animName1 = pose == 1 ? CharacterAnimation.Correct1 : CharacterAnimation.Correct2;
                yield return anim.PlayAnimationAndWait(animName1, false);

                var animName2 = pose == 1 ? CharacterAnimation.CorrectIdle1 : CharacterAnimation.CorrectIdle2;
                anim.PlayAnimationLoopT2(animName1, animName2);
            }

        }
    }
}