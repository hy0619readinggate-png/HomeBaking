using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DoDoEng.Common
{
    public class PushPopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(string title, string message)
        {
            LOG.Info($"ShowPopup() | {title}, {message}", this);

            titleTXT.text = title;
            messageTXT.text = message;

            return await showPopup();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI titleTXT = null;
        [SerializeField] private TextMeshProUGUI messageTXT = null;
    }
}