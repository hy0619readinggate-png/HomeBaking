using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class Affordance : AffBase
    {
        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponentInChildren<Animator>(true);

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);

            var seq = UtilArray.Random(1, 2);
            anim?.SetTrigger($"startAff{seq[0]}");
            yield return new WaitForSeconds(delay);

            anim?.SetTrigger($"startAff{seq[1]}");
            yield return new WaitForSeconds(delay);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            anim?.SetTrigger("abortAff");
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int delay = 2;

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