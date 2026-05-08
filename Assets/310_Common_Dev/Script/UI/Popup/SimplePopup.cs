using Cysharp.Threading.Tasks;

namespace DoDoEng.Common
{
    public enum SimplePopupResult { NA, Yes, No, Next, Retry, Okay, Back };
    public class SimplePopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Info($"ShowPopup()", this);

            return await showPopup();
        }
    }
}