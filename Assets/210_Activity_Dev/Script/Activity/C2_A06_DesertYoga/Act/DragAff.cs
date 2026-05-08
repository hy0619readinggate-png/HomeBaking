using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Activity.C2_A06
{
    public class DragAff : AffBase
    {
        // Methods
        public void Setup(List<int[]> list)
        {
            LOG.Info($"Setup() | {list.Count}", this);

            this.list = list;
        }



        // Fields : caching
        private Animator[] anims_ = null;
        private Animator[] anims => anims_ ??= GetComponentsInChildren<Animator>(true);

        // Fields
        private Animator currentAnim = null;
        private string answerTrigger = "N/A";
        private List<int[]> list = null;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);

            var tempList = new List<int[]>();
            foreach (var t in list)
            {
                if (t[1] != -1)
                    tempList.Add(t);
            }
            var temp = UtilArray.ExtractOne(tempList.ToArray());
            if (temp != null)
            {
                currentAnim = anims[temp[0]];
                answerTrigger = temp[1] switch
                {
                    0 => "left",
                    1 => "right",
                    _ => "none"
                };

            }
            currentAnim?.SetTrigger(answerTrigger);
            yield return new WaitForSeconds(delay);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            currentAnim?.SetTrigger("idle");
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