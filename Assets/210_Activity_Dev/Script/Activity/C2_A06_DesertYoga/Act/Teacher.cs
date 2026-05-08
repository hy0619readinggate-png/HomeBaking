using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C2_A06
{
    public class Teacher : MonoBehaviour
    {
        // Methods
        public void Setup(int pose)
        {
            LOG.Info($"Setup() | {pose}", this);

            this.poseNo = pose;

            this.StopCoroutineSafe(ref crIdle);

            var animName = poseNo == 1 ? TeacherAnimation.Idle1 : TeacherAnimation.Idle2;
            crIdle = StartCoroutine(coIdle(animName));
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            this.StopCoroutineSafe(ref crIdle);

            var animName = poseNo == 1 ? TeacherAnimation.Idle1 : TeacherAnimation.Idle2;
            crIdle = StartCoroutine(coIdle(animName));
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            this.StopCoroutineSafe(ref crIdle);

            var clip = UtilArray.ExtractOne(correctCLIP);
            AudioMGR.One.PlayEffect(clip);

            var animName = poseNo == 1 ? TeacherAnimation.Correct1 : TeacherAnimation.Correct2;
            anim.PlayAnimation(animName, false);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            this.StopCoroutineSafe(ref crIdle);

            var clip = UtilArray.ExtractOne(wrongCLIP);
            AudioMGR.One.PlayEffect(clip);

            var animName = poseNo == 1 ? TeacherAnimation.Wrong1 : TeacherAnimation.Wrong2;
            anim.PlayAnimation(animName, false);
        }
        public void Out()
        {
            LOG.Info($"Out()", this);

            this.StopCoroutineSafe(ref crIdle);

            var animName = poseNo == 1 ? TeacherAnimation.Out1 : TeacherAnimation.Out2;
            anim.PlayAnimation(animName, false);
        }



        // Fields
        private int poseNo = 0;
        private Coroutine crIdle = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TeacherAni anim = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float stumbleRatio = 0.2f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coIdle(TeacherAnimation animName)
        {
            using (LOG.Coroutine($"coIdle()", this))
            {
                while (true)
                {
                    yield return anim.PlayAnimationAndWait(animName, false);

                    var stumble = UtilRandom.RandomSuccess(stumbleRatio);
                    if (stumble)
                    {
                        var stumbleAnimName = poseNo == 1 ? TeacherAnimation.Wrong1 : TeacherAnimation.Wrong2;
                        yield return anim.PlayAnimationAndWait(stumbleAnimName, false);
                    }
                }
            }
        }
    }
}