using System.Collections;

namespace DoDoEng.Common
{
    public class AffGameObject : AffBase
    {
        // Overrides
        protected override IEnumerator onStartAff()
        {
            affTargetGO?.SetActive(true);
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO?.SetActive(false);
            yield return null;
        }



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
            affTargetGO.SetActive(false);
        }
    }
}