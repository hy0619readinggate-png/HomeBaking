using beyondi.Util;
using ChartAndGraph;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
    public class ParentChildLMSPopupPageDayAll : ParentChildLMSPopupPageDaySubBase
    {
        // Fields : caching
        private ScrollRect scrollRect_;
        private ScrollRect scrollRect => scrollRect_ ??= GetComponent<ScrollRect>();

        // Event Handlers
        private void stamp_OnStamp(int numStamp)
        {
            LOG.Function(this, $"| numStamp={numStamp}");

            if (numStamp > 0)
                LMS.One.SetStamp(ChildData.ID, loadedDate, (LMS.StampType)(numStamp - 1)).Forget();
        }

        // Overrides
        protected override void onClear()
        {
            base.onClear();

            courseStageTMP.text = "";
            progressSlider.value = progressZeroValue;
            progressTMP.text = "";

            dayPannel1.HideAndClear();
            dayPannel2.HideAndClear();
            dayEmptyGO.SetActive(true);

            dayCountTMP.text = "";
            dayCountNum.ForEach(tmp => tmp.text = "");
            libraryCountTMP.text = "";
            libraryCountNum.ForEach(tmp => tmp.text = "");

            for (int i = 0; i < dayCountChart.DataSource.TotalCategories; i++)
            {
                dayCountChart.DataSource.SetValue(dayCountChart.DataSource.GetCategoryName(i), 0);
            }
            dayCountChart.Invalidate();
            for (int i = 0; i < libraryCountChart.DataSource.TotalCategories; i++)
            {
                libraryCountChart.DataSource.SetValue(libraryCountChart.DataSource.GetCategoryName(i), 0);
            }
            libraryCountChart.Invalidate();

            stamp.SetStamp(0);
            stamp.Interactable = false;
        }
        protected override async UniTask onLoad(CancellationToken cancellationToken, string date)
        {
            await base.onLoad(cancellationToken, date);

            praiseTMP.text = LocalizationMGR.One.GetText("MESSAGE_25", ChildData.NickName);

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/day/{ChildData.ID}/all?searchDate={date}");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (result.Success)
            {
                var praiseStamp = result.Data.Value<JObject>("praiseStamp");
                var isPraiseStamp = praiseStamp.Value<bool>("isPraiseStamp");
                var stampType = praiseStamp.Value<string>("stampType");


                var day = result.Data.Value<JObject>("day");
                var progressCount = day.Value<JObject>("progressCount");
                var totalCount = progressCount.Value<int>("totalCount");
                var completeCount = progressCount.Value<int>("completeCount");
                var dayLearningCompleteCount = day.Value<int>("learningCompleteCount");
                var days = day.Value<JArray>("days");
                var dayLearningTypeCompleteList = day.Value<JArray>("learningTypeCompleteList");

                var library = result.Data.Value<JObject>("library");
                var libraryLearningCompleteCount = library.Value<int>("learningCompleteCount");
                var libraryLearningTypeCompleteList = library.Value<JArray>("learningTypeCompleteList");

                var isAttendance = result.Data.Value<bool>("isAttendance");
                stamp.SetStamp(isPraiseStamp ? (int)Enum.Parse<LMS.StampType>(stampType) + 1 : 0, 0 < completeCount || 0 < libraryLearningCompleteCount);
                stamp.Interactable = ChildData.ParentID == UserData.One.Parent.LoginID;

                var completeRatio = totalCount == 0 ?
                                        progressZeroValue : Mathf.Lerp(progressZeroValue, 1, completeCount / (float)totalCount);

                var stageID = ChildData.DayProgress.Value<int>("stageId");
                courseStageTMP.text = $"Course {ChildData.Course} > Stage {stageID}";
                progressSlider.value = completeRatio;
                progressTMP.text = $"<color=#8bbcea>{completeCount}</color> /{totalCount}";

                var dayCount = days.Count();
                if (dayCount > 0) dayPannel1.ShowAndSetup(days[0]);
                if (dayCount > 1) dayPannel2.ShowAndSetup(days[1]);
                dayEmptyGO.SetActive(dayCount == 0);


                var dayCounts = Enumerable.Repeat(0, 5).ToArray();
                var dayRates = Enumerable.Repeat<double>(0, 5).ToArray();
                dayLearningTypeCompleteList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var count = list.Value<int>("count");
                    var percent = list.Value<double>("percent");

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "050") idx = 3;
                    else if (code == "030") idx = 4; // AIStudio는 해당 없음

                    if (idx != -1)
                    {
                        dayCounts[idx] += count;
                        dayRates[idx] = percent;
                    }
                });

                var libraryCounts = Enumerable.Repeat(0, 5).ToArray();
                var libraryRates = Enumerable.Repeat<double>(0, 5).ToArray();
                libraryLearningTypeCompleteList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var count = list.Value<int>("count");
                    var percent = list.Value<double>("percent");

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "030") idx = 3;
                    else if (code == "060") idx = 4;

                    if (idx != -1)
                    {
                        libraryCounts[idx] = count;
                        libraryRates[idx] = percent;
                    }
                });

                dayCountTMP.text = LocalizationMGR.One.GetText("WORD_110", dayLearningCompleteCount);
                for (int i = 0; i < dayCountChart.DataSource.TotalCategories; i++)
                {
                    dayCountChart.DataSource.SetValue(dayCountChart.DataSource.GetCategoryName(i), dayRates[i]);
                    dayCountNum[i].text = LocalizationMGR.One.GetText("WORD_122", dayCounts[i]);
                }
                dayCountChart.Invalidate();
                if (0 < dayLearningCompleteCount)
                    dayCountChart.GetComponent<PieAnimation>().Animate();

                libraryCountTMP.text = LocalizationMGR.One.GetText("WORD_110", libraryLearningCompleteCount);
                for (int i = 0; i < libraryCountChart.DataSource.TotalCategories; i++)
                {
                    libraryCountChart.DataSource.SetValue(libraryCountChart.DataSource.GetCategoryName(i), libraryRates[i]);
                    libraryCountNum[i].text = LocalizationMGR.One.GetText("WORD_122", libraryCounts[i]);
                    libraryCountRate[i].text = $"{libraryRates[i]}%";
                }
                libraryCountChart.Invalidate();
                if (0 < libraryLearningCompleteCount)
                    libraryCountChart.GetComponent<PieAnimation>().Animate();
            }
            else
            {
                LOG.Warning(result.Data.Value<string>("message"), this);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text courseStageTMP = null;
        [SerializeField] private Slider progressSlider = null;
        [SerializeField] private TMP_Text praiseTMP = null;
        [SerializeField] private TMP_Text progressTMP = null;
        [SerializeField] private ParentChildLMSPopupPageDayPannel dayPannel1 = null;
        [SerializeField] private ParentChildLMSPopupPageDayPannel dayPannel2 = null;
        [SerializeField] private GameObject dayEmptyGO = null;
        [SerializeField] private CanvasPieChart dayCountChart = null;
        [SerializeField] private TMP_Text dayCountTMP = null;
        [SerializeField] private TMP_Text[] dayCountNum = null;
        [SerializeField] private CanvasPieChart libraryCountChart = null;
        [SerializeField] private TMP_Text libraryCountTMP = null;
        [SerializeField] private TMP_Text[] libraryCountNum = null;
        [SerializeField] private TMP_Text[] libraryCountRate = null;
        [SerializeField] private LMSStampControl stamp = null;
        [Header("★ Config")]
        [SerializeField] private float progressZeroValue = 0.058f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            scrollRect.verticalNormalizedPosition = 1;

            stamp.OnStamp += stamp_OnStamp;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            stamp.OnStamp -= stamp_OnStamp;
        }
    }
}