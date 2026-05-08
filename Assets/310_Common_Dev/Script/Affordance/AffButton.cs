using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(Button))]
    public class AffButton : AffBase
    {
        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (btn.interactable)
            {
                affTargetGO?.SetActive(true);
                yield return null;
            }
        }
        protected override IEnumerator onFinishAff()
        {
            if (btn.interactable)
            {
                affTargetGO?.SetActive(false);
                yield return null;
            }
        }



        // Fields : caching
        private Button btn => GetComponent<Button>();



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
            affTargetGO.SetActive(false);
        }
    }
}