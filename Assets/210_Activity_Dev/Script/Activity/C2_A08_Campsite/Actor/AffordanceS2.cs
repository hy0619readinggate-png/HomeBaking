using DoDoEng.Common;
using System.Collections;


namespace DoDoEng.Activity.C2_A08
{
    public class AffordanceS2 : AffDrag
    {
        // Overrides
        protected override IEnumerator onFinishAff()
        {
            base.onFinishAff();
            gameObject.SetActive(false);
            yield return null;
        }



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