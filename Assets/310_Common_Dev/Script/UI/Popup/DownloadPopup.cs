using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class DownloadPopup : PopupBase<SimplePopupResult>, IReportProgress
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Slider progressSLD = null;
        [SerializeField] private Text progressTXT = null;



        // Interface
        void IReportProgress.Begin()
        {
            progressSLD.value = 0;
            progressTXT.text = "0%";

            _ = showPopup();
        }
        void IReportProgress.End()
        {
            closePopup(SimplePopupResult.Okay);
        }
        void IReportProgress.ReportProgress(float progress)
        {
            progressSLD.value = progress;
            progressTXT.text = $"{progress * 100:f0}%";
        }
    }
}