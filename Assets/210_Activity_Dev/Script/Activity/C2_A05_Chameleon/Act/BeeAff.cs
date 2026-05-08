using DoDoEng.Common;
using System.Collections;

namespace DoDoEng.Activity.C2_A05
{
    public class BeeAff : AffGameObject
    {
        // Methods
        public void Init(BeeMGR beeMGR)
        {
            LOG.Info($"Init(", this);

            this.beeMGR = beeMGR;
        }



        // Fields
        private BeeMGR beeMGR;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (!affTargetGO.activeSelf)
            {
                var bee = beeMGR.AnswerBee;
                transform.position = bee.transform.position;

                yield return base.onStartAff();

            }
            yield return null;
        }



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
    }
}