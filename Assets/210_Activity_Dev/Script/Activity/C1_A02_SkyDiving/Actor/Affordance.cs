using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A02
{
    public class Affordance : AffBase
    {
        // Methods
        public void Setup(int answerIDX)
        {
            LOG.Info($"Setup() | {answerIDX}", this);

            this.answerIDX = answerIDX;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponentInChildren<Animator>(true);

        // Fields
        private int answerIDX = 0;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);
            
            var answerTrigger = answerIDX switch
            {
                0 => "affor_L",
                1 => "affor_M",
                2 => "affor_R",
                _ => "none"
            };

            anim?.SetTrigger(answerTrigger);

            yield return new WaitForSeconds(delay);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            anim?.SetTrigger("none");
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