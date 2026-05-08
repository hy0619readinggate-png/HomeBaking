using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class DownloadConfirmPopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(long downloadSize)
        {
            LOG.Info($"ShowPopup()", this);

            downloadSizeTXT.text = getSizeString(downloadSize);

            return await showPopup();
        }



        // Functions
        private static string getSizeString(long size)
        {
            if (size < 1024)
                return $"{size} B";
            else if (size < 1024 * 1024)
                return $"{size / 1024.0:F2} KB";
            else
                return $"{size / 1024.0 / 1024.0:F2} MB";
        }



        // Unity Inspectors
        [SerializeField] private Text downloadSizeTXT = null;
    }
}