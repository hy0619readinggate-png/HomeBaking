using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A03
{
    public class DragAff : AffBase
    {
        // Methods
        public void Setup(int problemNO, int answerIDX)
        {
            LOG.Info($"Setup() | problemNO({problemNO}), AnswerIDX({answerIDX})", this);

            anims.ForEach(a => a.gameObject.SetActive(false));

            this.answerIDX = answerIDX;

            answerTrigger = problemNO switch
            {
                1 => "tire",
                2 => "door",
                3 => "engine",
                _ => "none"
            };
        }



        // Fields : caching
        private Animator[] anims_ = null;
        private Animator[] anims => anims_ ??= GetComponentsInChildren<Animator>(true);

        // Fields
        private Animator currentAnim = null;
        private string answerTrigger = "N/A";
        private int answerIDX = 0;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);

            anims.ForEach((i, a) => a.gameObject.SetActive(i == answerIDX));
            var currentAnim = anims[answerIDX];
            currentAnim?.SetTrigger(answerTrigger);
            yield return new WaitForSeconds(delay);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            currentAnim?.SetTrigger("idle");
            anims.ForEach(a => a.gameObject.SetActive(false));
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