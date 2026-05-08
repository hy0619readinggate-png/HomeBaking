using System.Collections;
using UnityEngine;

namespace DoDoEng.Common
{
    public class AffMecanim : AffBase
    {
        // Properties
        public int Variation { get; set; }



        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (!string.IsNullOrEmpty(variationTriggerName))
                anim.SetInteger(variationTriggerName, Variation);

            anim.SetTrigger(startTriggerName);
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            anim.SetTrigger(abortTriggerName);
            yield return null;
        }



        // Fields
        private Animator anim = null;



        // Unity Inspectors
        [Header("¡Ú Config")]
        [SerializeField] private string startTriggerName = "startAff";
        [SerializeField] private string abortTriggerName = "abortAff";
        [SerializeField] private string variationTriggerName = "variation";

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            anim = affTargetGO.GetComponent<Animator>();
            if (anim == null)
                LOG.Error($"Animator must be exists!", this);
        }
    }
}