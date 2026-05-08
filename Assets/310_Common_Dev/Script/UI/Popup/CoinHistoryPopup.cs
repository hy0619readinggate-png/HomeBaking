using beyondi.Util;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class CoinHistoryPopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            return await showPopup();
        }

        // Override
        protected override void onOpen()
        {
            base.onOpen();

            loadDay(DateTime.Today).Forget();
        }



        // Fields
        DateTime currentDT = DateTime.Today;

        // Functions
        private async UniTask loadDay(DateTime dt)
        {
            currentDT = dt;
            dayTMP.text = currentDT.ToString("MM\\/dd (ddd)", LocalizationMGR.One.Culture);

            nextDayBT.gameObject.SetActive(currentDT < DateTime.Today);

            incomeTMP.text = "";
            outcomeTMP.text = "";

            foreach (var ch in historyRT.GetChildren())
            {
                var line = ch.GetComponent<CoinHistoryLine>();
                if (line != null && line != lastHistoryLine)
                    Destroy(ch.gameObject);
            }
            lastHistoryLine.gameObject.SetActive(false);

            var data = await LMS.One.LoadRewardHistory(currentDT.ToString("yyyyMMdd"));

            incomeTMP.text = $"+{data.Value<int>("savedPoint")}";
            outcomeTMP.text = $"{data.Value<int>("usedPoint")}";

            var historyList = data.Value<JArray>("historyList");

            for (int i = 0; i < historyList.Count; i++)
            {
                //var issueDatetime = historyList[i].Value<DateTime>("issueDatetime");
                var policyName = historyList[i].Value<string>("policyName");
                var savedPoint = historyList[i].Value<int>("savedPoint");
                var usedPoint = historyList[i].Value<int>("usedPoint");

                if (i == historyList.Count - 1)
                {
                    lastHistoryLine.gameObject.SetActive(true);
                    lastHistoryLine.Init(policyName, savedPoint, usedPoint);
                    lastHistoryLine.transform.SetAsLastSibling();
                }
                else
                {
                    var line = Instantiate(historyLinePF, historyRT);
                    line.Init(policyName, savedPoint, usedPoint);
                }
            }
        }

        // Event Handlers
        private async UniTask calendarBT_onClick()
        {
            LOG.Function(this);

            var result = await SystemUI.One.CalendarPU.ShowPopup(currentDT);
            if (result == SimplePopupResult.Okay)
                await loadDay(SystemUI.One.CalendarPU.CurrentDT);
        }
        private void prevDayBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddDays(-1)).Forget();
        }
        private void nextDayBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddDays(1)).Forget();
        }



        // Unity Inspectors
        [SerializeField] private TMP_Text dayTMP = null;
        [SerializeField] private Button calendarBT = null;
        [SerializeField] private Button prevDayBT = null;
        [SerializeField] private Button nextDayBT = null;
        [SerializeField] private TMP_Text incomeTMP = null;
        [SerializeField] private TMP_Text outcomeTMP = null;
        [SerializeField] private RectTransform historyRT = null;
        [SerializeField] private CoinHistoryLine historyLinePF = null;
        [SerializeField] private CoinHistoryLine lastHistoryLine = null;

        // Unity Messages
        protected void Awake()
        {
            calendarBT.onClick.AddListener(() => calendarBT_onClick().Forget());
            prevDayBT.onClick.AddListener(() => prevDayBT_onClick());
            nextDayBT.onClick.AddListener(() => nextDayBT_onClick());
        }
    }
}