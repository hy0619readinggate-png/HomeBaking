using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoDoEng.Activity.C2_A08
{
    public class AffordanceS1 : AffBase
    {
        // Methods
        public void Setup(int answerIDX)
        {
            LOG.Info($"Setup() | {answerIDX}", this);

            this.answerIDX = answerIDX;
        }



        // Fields : caching
        private IEnumerable<GameObject> targets_ = null;
        private IEnumerable<GameObject> targets => targets_ ??= transform.GetChildrenAsGameObject();

        // Fields
        private int answerIDX = 0;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            targets.ForEach((i, t) => t.SetActive(answerIDX == i));

            yield return null;
        }

        protected override IEnumerator onFinishAff()
        {
            targets.ForEach(t => t.SetActive(false));
            yield return null;
        }



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            targets.ForEach(t => t.SetActive(false));
        }
        private void Start()
        {
        }
    }
}