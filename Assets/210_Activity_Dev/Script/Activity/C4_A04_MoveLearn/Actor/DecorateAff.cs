using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C4_A04
{
    public class DecorateAff : AffBase
    {
        // Methods
        public void Init(Transform targetsParent, Transform nextTR)
        {
            LOG.Info($"Init()", this);

            targets = targetsParent.GetChildren().ToArray();
            this.nextTR = nextTR;
        }
        public void SetSelected(bool selected)
        {
            LOG.Info($"SetSelected() | {selected}", this);

            this.isSelected = selected;
        }



        // Fields
        private Transform[] targets = null;
        private Transform nextTR = null;

        // Fields
        private bool isSelected = false;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);

            if (isSelected)
                affTargetGO.transform.position = nextTR.position;
            else
            {
                var index = UtilArray.RandomOne(0, 3);
                affTargetGO.transform.position = targets[index].position;
            }

            affTargetGO.SetActive(true);
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            affTargetGO.SetActive(false);
            yield return null;
        }



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            affTargetGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}