using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C2_A07
{
    public class Edmond : MonoBehaviour
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            edmond.PlayAnimationLoopT2(EdmondAnimation.Idle1, EdmondAnimation.Idle2);
        }
        public IEnumerator Correct()
        {
            LOG.Info($"Correct()", this);

            var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
            AudioMGR.One.PlayEffect(correctCLIP[idx]);

            yield return edmond.PlayAnimationAndWait(correctAnimations[idx]);
        }
        public IEnumerator Wrong()
        {
            LOG.Info($"Wrong()", this);

            var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
            AudioMGR.One.PlayEffect(wrongCLIP[idx]);

            yield return edmond.PlayAnimationAndWait(wrongAnimations[idx]);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private EdmondAni edmond = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private EdmondAnimation[] correctAnimations = null;
        [SerializeField] private EdmondAnimation[] wrongAnimations = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}