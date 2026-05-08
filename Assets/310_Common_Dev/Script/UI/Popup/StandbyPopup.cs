using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DoDoEng.Common
{
    public class StandbyPopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            return await showPopup();
        }
        public void HideNow()
        {
            gameObject.SetActive(false);
        }



        // Override
        protected override void onOpen()
        {
            base.onOpen();

            anim.SetTrigger("Show");
        }
        protected override void onClose(SimplePopupResult result)
        {
            base.onClose(result);

            //anim.SetTrigger("Hide");
            //DOVirtual.DelayedCall(1, () => gameObject.SetActive(false), false);
        }



        // Unity Inspectors
        [SerializeField] private Animator anim = null;
    }
}