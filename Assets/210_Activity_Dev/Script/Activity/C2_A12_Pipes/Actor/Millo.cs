using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A12
{
    public class Millo : BYDSingleton<Millo>
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            ani.PlayAnimationLoopT2(MilloAnimation.Idle1, MilloAnimation.Idle2);

        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
            AudioMGR.One.PlayEffect(correctCLIP[idx]);
            ani.PlayAnimation(correctAnimations[idx]);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            ani.PlayAnimation(MilloAnimation.Wrong1);

            var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
            AudioMGR.One.PlayEffect(wrongCLIP[idx]);
            ani.PlayAnimation(wrongAnimations[idx]);
        }
        public void IdleWithButton()
        {
            LOG.Info($"IdleWithButton()", this);

            ani.PlayAnimationLoop(MilloAnimation.IdleWithButton);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private MilloAni ani = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private MilloAnimation[] correctAnimations = null;
        [SerializeField] private MilloAnimation[] wrongAnimations = null;


        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
        }
    }
}