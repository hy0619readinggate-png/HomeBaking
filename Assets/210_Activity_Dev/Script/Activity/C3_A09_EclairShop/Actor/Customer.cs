using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C3_A09
{
    public class Customer : BYDSingleton<Customer>
    {
        // Methods
        public void Setup(int id)
        {
            LOG.Info($"Setup() | {id}", this);

            this.id = id;
            customers.SetActiveOnly(id - 1);
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            idle();
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            this.StopCoroutineSafe(ref crFeedback);

            crFeedback = StartCoroutine(coCorrect());
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            this.StopCoroutineSafe(ref crFeedback);

            crFeedback = StartCoroutine(coWrong());
        }



        // Fields 
        private int id;
        private Coroutine crFeedback = null;

        // Functions
        private CustomerAni activeCustomer => customers[id - 1];
        private void idle()
        {
            activeCustomer.PlayAnimationLoopT2(CustomerAnimation.Idle1, CustomerAnimation.Idle2);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CustomerAni[] customers = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                yield return activeCustomer.Correct();

                idle();
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                
                yield return activeCustomer.Wrong();

                idle();
            }
        }
    }
}