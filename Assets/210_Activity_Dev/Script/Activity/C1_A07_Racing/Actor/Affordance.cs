using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A07
{
    public class Affordance : AffMecanim
    {
        // Methods
        public void Run()
        {
            LOG.Info($"Run()", this);

            isStarted = true;
        }

        // Fields
        private bool isStarted = false;
        private bool isTouched = false;



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Update()
        {
            if (isStarted && !isTouched && (Input.touchCount > 0 || Input.GetMouseButtonDown(0)))
            {
                isTouched = true;

                gameObject.SetActive(false);
            }

        }
    }
}