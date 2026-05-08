using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C3_A09
{
    public enum CustomerAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("walk")] Walk,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("wrong")] Wrong,
    }

    public class CustomerAni : AnimationSpineUI<CustomerAnimation>
    {
        // Methods
        public IEnumerator Correct()
        {
            LOG.Info($"Correct()", this);
            var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
            AudioMGR.One.PlayEffect(correctCLIP[idx]);
            yield return PlayAnimationAndWait(correctAnimations[idx]);
        }
        public IEnumerator Wrong()
        {
            LOG.Info($"Wrong()", this);

            var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
            AudioMGR.One.PlayEffect(wrongCLIP[idx]);
            yield return PlayAnimationAndWait(wrongAnimations[idx]);
        }



        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("¡Ú Config")]
        [SerializeField] private CustomerAnimation[] correctAnimations = null;
        [SerializeField] private CustomerAnimation[] wrongAnimations = null;
    }
}