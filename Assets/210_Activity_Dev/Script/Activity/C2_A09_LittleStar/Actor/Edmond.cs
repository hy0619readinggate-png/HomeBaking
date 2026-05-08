using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A09
{
    public class Edmond : MonoBehaviour
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            if (!isIdle)
                anim.PlayAnimationLoop(EdmondAnimation.Idle1);
        }
        public void OutOfControl()
        {
            LOG.Info($"OutOfControl()", this);

            isIdle = false;
            anim.AbortAnimation();
            this.StopCoroutineSafe(ref crIdle);
        }



        // Fields
        private bool isIdle = false;
        private Coroutine crIdle = null;

        // Functions
        private bool isIdleAnimating => anim.IsAnimationPlaying(EdmondAnimation.Idle1) &&
                                        anim.IsAnimationPlaying(EdmondAnimation.Idle2);



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private EdmondAni anim = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}