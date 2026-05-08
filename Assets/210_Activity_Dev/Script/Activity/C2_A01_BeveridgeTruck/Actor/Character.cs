using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A01
{
    public class Character : MonoBehaviour
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            ani.PlayAnimationLoopT2(CharacterAnimation.Idle1, CharacterAnimation.Idle2);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
            AudioMGR.One.PlayEffect(wrongCLIP[idx]);
            ani.PlayAnimation(wrongAnimations[idx]);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni ani = null;
        [SerializeField] private CharacterAnimation[] wrongAnimations = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] wrongCLIP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}