using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using ChartAndGraph;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
	public class MyInfoPopupPageTodayAll : MyInfoPopupSubBase
    {
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers

        // Overrides
        protected override void onClear()
        {
            base.onClear();

            todayStudyTimeTMP.text = "";
            totalStudyCountTMP.text = "";
            totalStudyCountNum.ForEach(tmp => tmp.text = "");
            totalStudyTimeTMP.text = "";
            totalStudyTimeNum.ForEach(tmp => tmp.text = "");

            for (int i = 0; i < totalStudyCountChart.DataSource.TotalCategories; i++)
            {
                totalStudyCountChart.DataSource.SetValue(totalStudyCountChart.DataSource.GetCategoryName(i), 0);
            }
            totalStudyCountChart.Invalidate();
            for (int i = 0; i < totalStudyTimeChart.DataSource.TotalCategories; i++)
            {
                totalStudyTimeChart.DataSource.SetValue(totalStudyTimeChart.DataSource.GetCategoryName(i), 0);
            }
            totalStudyTimeChart.Invalidate();


            // From Parent LMS
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
        }
        protected override async UniTask onLoad(CancellationToken cancellationToken, string date)
        {
            await base.onLoad(cancellationToken, date);

            var result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/my-info/today/all?searchDate={date}");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (result.Success)
            {
                var praiseStamp = result.Data.Value<JObject>("praiseStamp");
                var isPraiseStamp = praiseStamp.Value<bool>("isPraiseStamp");
                var stampType = praiseStamp.Value<string>("stampType");

                var learningTypeCount = result.Data.Value<JObject>("learningTypeCount");
                var totalCompleteCount = learningTypeCount.Value<int>("totalCompleteCount");
                var completeCountList = learningTypeCount.Value<JArray>("completeCountList");
                var learningTypeTime = result.Data.Value<JObject>("learningTypeTime");
                var totalCompleteTime = learningTypeTime.Value<int>("totalCompleteTime");
                var completeTimeList = learningTypeTime.Value<JArray>("completeTimeList");

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

                stamp.SetStamp(isPraiseStamp ? (int)Enum.Parse<LMS.StampType>(stampType) + 1 : 0);
                stampTipGO.SetActive(isAttendance && !isPraiseStamp);

                var counts = Enumerable.Repeat(0, 5).ToArray();
                var countRates = Enumerable.Repeat<double>(0, 5).ToArray();
                completeCountList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var count = list.Value<int>("count");
                    var percent = list.Value<double>("percent");

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "050" || code == "060") idx = 3;
                    else if (code == "030") idx = 4;

                    if (idx != -1)
                    {
                        counts[idx] += count;
                        countRates[idx] = percent;
                    }
                });

                var times = Enumerable.Repeat<int>(0, 5).ToArray();
                var timeRates = Enumerable.Repeat<double>(0, 5).ToArray();
                completeTimeList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var time = list.Value<int>("time");
                    var percent = list.Value<double>("percent");

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "060") idx = 3;
                    else if (code == "030") idx = 4;

                    if (idx != -1)
                    {
                        times[idx] = time;
                        timeRates[idx] = percent;
                    }
                });

                var totalTS = TimeSpan.FromSeconds(totalCompleteTime);
                todayStudyTimeTMP.text = $"{totalTS.Days * 24 + totalTS.Hours}:{totalTS.ToString("mm")}:{totalTS.ToString("ss")}";

                totalStudyCountTMP.text = LocalizationMGR.One.GetText("WORD_122", totalCompleteCount);
                for (int i = 0; i < totalStudyCountChart.DataSource.TotalCategories; i++)
                {
                    totalStudyCountChart.DataSource.SetValue(totalStudyCountChart.DataSource.GetCategoryName(i), countRates[i]);
                    totalStudyCountNum[i].text = LocalizationMGR.One.GetText("WORD_122", counts[i]);
                }
                totalStudyCountChart.Invalidate();

                if (totalTS.TotalSeconds >= 60)
                    totalStudyTimeTMP.text = LocalizationMGR.One.GetText("WORD_127", Math.Floor(totalTS.TotalMinutes));
                else
                    totalStudyTimeTMP.text = LocalizationMGR.One.GetText("WORD_128", Math.Floor(totalTS.TotalSeconds));
                for (int i = 0; i < totalStudyTimeChart.DataSource.TotalCategories; i++)
                {
                    totalStudyTimeChart.DataSource.SetValue(totalStudyTimeChart.DataSource.GetCategoryName(i), timeRates[i]);
                    if (times[i] >= 60)
                        totalStudyTimeNum[i].text = LocalizationMGR.One.GetText("WORD_127", times[i] / 60);
                    else
                        totalStudyTimeNum[i].text = LocalizationMGR.One.GetText("WORD_128", times[i]);
                }
                totalStudyTimeChart.Invalidate();

                if (0 < totalCompleteCount)
                {
                    totalStudyCountANI.SetTrigger("Go");
                    totalStudyCountChart.GetComponent<PieAnimation>().Animate();
                }
                if (0 < totalCompleteTime)
                {
                    totalStudyTimeANI.SetTrigger("Go");
                    totalStudyTimeChart.GetComponent<PieAnimation>().Animate();
                }

                // From Parent LMS
                var completeRatio = totalCount == 0 ?
                                        progressZeroValue : Mathf.Lerp(progressZeroValue, 1, completeCount / (float)totalCount);

                var stageID = UserData.One.Child.DayProgress.Value<int>("stageId");
                courseStageTMP.text = $"Course {UserData.One.Child.Course} > Stage {stageID}";
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
        [SerializeField] private TMP_Text todayStudyTimeTMP = null;
        [SerializeField] private TMP_Text totalStudyCountTMP = null;
        [SerializeField] private TMP_Text[] totalStudyCountNum = null;
        [SerializeField] private Animator totalStudyCountANI = null;
        [SerializeField] private CanvasPieChart totalStudyCountChart = null;
        [SerializeField] private TMP_Text totalStudyTimeTMP = null;
        [SerializeField] private TMP_Text[] totalStudyTimeNum = null;
        [SerializeField] private Animator totalStudyTimeANI = null;
        [SerializeField] private CanvasPieChart totalStudyTimeChart = null;
        [SerializeField] private LMSStampControl stamp = null;
        [SerializeField] private GameObject stampTipGO = null;

        [SerializeField] private TMP_Text courseStageTMP = null;
        [SerializeField] private Slider progressSlider = null;
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
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // Unity Coroutine
    }
}