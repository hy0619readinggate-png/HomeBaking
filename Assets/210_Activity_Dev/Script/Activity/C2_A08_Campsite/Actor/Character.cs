using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A08
{
    public class Character : MonoBehaviour
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            ani.PlayAnimationLoop(CharacterAnimation.Idle);
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
            if (correctCLIP.Length != 0)
                AudioMGR.One.PlayEffect(correctCLIP[idx]);
            ani.PlayAnimation(correctAnimations[idx]);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
            if (wrongCLIP.Length != 0)
                AudioMGR.One.PlayEffect(wrongCLIP[idx]);
            ani.PlayAnimation(wrongAnimations[idx], false);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni ani = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private CharacterAnimation[] correctAnimations = null;
        [SerializeField] private CharacterAnimation[] wrongAnimations = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}