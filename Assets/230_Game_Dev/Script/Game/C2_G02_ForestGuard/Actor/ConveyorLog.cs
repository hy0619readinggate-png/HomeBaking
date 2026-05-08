using beyondi.Util;
using DG.Tweening;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public class ConveyorLog : MonoBehaviour
    {
        // Methods
        public void TurnOn()
        {
            //LOG.Info($"TurnOn()", this);

            var delay = initialDelay.RandomValue();
            DOVirtual.DelayedCall(delay, () =>
            {
                logANIM.SetBool("On", true);
                shakeTweener = transform.DOShakePosition(1, shakeStrength,
                                                            shakeVibrato,
                                                            shakeRandomness,
                                                            shakeSnapping,
                                                            shakeFadeOut,
                                                            shakeRandomnessMode)
                                         .SetLoops(-1);
            });
        }
        public void TurnOff()
        {
            //LOG.Info($"TurnOff()", this);

            if (shakeTweener != null)
            {
                shakeTweener.Kill();
                shakeTweener = null;
            }

            logANIM.SetBool("On", false);
        }
        public void Damage()
        {
            logANIM.SetTrigger("Hit");
        }



        // Fields
        private Tweener shakeTweener;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator logANIM = null;
        [Header("★ Config")]
        [SerializeField] private Range initialDelay = new Range(0, 1);
        [SerializeField] private float shakeStrength = 1f;
        [SerializeField] private int shakeVibrato = 5;
        [SerializeField] private float shakeRandomness = 90f;
        [SerializeField] private bool shakeSnapping = false;
        [SerializeField] private bool shakeFadeOut = false;
        [SerializeField] private ShakeRandomnessMode shakeRandomnessMode = ShakeRandomnessMode.Harmonic;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}