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
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageWeekContents : ParentChildLMSPopupPageWeekSubBase
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

            avgLearningTimeTMP.text = "";

            leftNums.ForEach(num => num.text = "");
            dateActiveNums.ForEach(num => num.gameObject.SetActive(false));
            dateDeactiveNums.ForEach(num => num.gameObject.SetActive(false));
            progressbars.ForEach(progressbar => progressbar.gameObject.SetActive(false));
            avgSlider.gameObject.SetActive(false);
        }
        protected override async UniTask onLoad(string date)
        {
            await base.onLoad(date);

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/weekly/{ChildData.ID}/{apiName}?searchDate={date}");
            if (result.Success)
            {
                var avgLearningTime = result.Data.Value<int>("avgLearningTime");
                var totalAvgCompleteCount = result.Data.Value<int>("totalAvgCompleteCount");
                var dayCompleteCountList = result.Data.Value<JArray>("dayCompleteCountList");

                int maxCount = 20;
                var dayCounts = Enumerable.Repeat(0, 7).ToArray();
                var dateTime = DateTime.Parse($"{date.Substring(0, 4)}-{date.Substring(4, 2)}-{date.Substring(6, 2)}");
                var firstDay = dateTime.FirstDayOfWeek(DayOfWeek.Sunday);
                dayCompleteCountList.ForEach(list =>
                {
                    var dayDate = list.Value<string>("date");
                    var dayCount = list.Value<int>("count");

                    while (maxCount < dayCount) maxCount += 20;
                    while (maxCount < totalAvgCompleteCount) maxCount += 20;

                    var dayDateTime = DateTime.Parse(dayDate);
                    var ts = dayDateTime - firstDay;

                    dayCounts[ts.Days] = dayCount;
                });

                if (avgLearningTime < 60)
                    avgLearningTimeTMP.text = LocalizationMGR.One.GetText("WORD_128", avgLearningTime);
                else
                    avgLearningTimeTMP.text = LocalizationMGR.One.GetText("WORD_127", avgLearningTime / 60);

                int gap = maxCount / leftNums.Length;
                for (int i = 0; i < leftNums.Length; i++)
                {
                    leftNums[i].text = LocalizationMGR.One.GetText("WORD_122", (i + 1) * gap);
                }

                for (int i = 0; i < 7; i++)
                {
                    dateDeactiveNums[i].gameObject.SetActive(dayCounts[i] == 0);
                    dateActiveNums[i].gameObject.SetActive(dayCounts[i] > 0);
                    dateDeactiveNums[i].text = firstDay.AddDays(i).ToString("MM\\/d\nddd", LocalizationMGR.One.Culture);
                    dateActiveNums[i].text = dateDeactiveNums[i].text;
                    progressbars[i].gameObject.SetActive(dayCounts[i] > 0);
                    progressbars[i].Max = maxCount;
                    progressbars[i].Value = dayCounts[i];
                    progressbars[i].IsToday = firstDay.AddDays(i).ToString("yyyyMMdd") == DateTime.Today.ToString("yyyyMMdd");
                }

                avgSlider.value = totalAvgCompleteCount / (float)maxCount;
                avgSlider.gameObject.SetActive(true);
                avgCountTMP.text = LocalizationMGR.One.GetText("WORD_122", totalAvgCompleteCount);
            }
            else
            {
                LOG.Warning(result.Data.Value<string>("message"), this);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text avgLearningTimeTMP = null;
        [SerializeField] private TMP_Text[] leftNums = null;
        [SerializeField] private TMP_Text[] dateDeactiveNums = null;
        [SerializeField] private TMP_Text[] dateActiveNums = null;
        [SerializeField] private LMSProgressbar[] progressbars = null;
        [SerializeField] private Slider avgSlider = null;
        [SerializeField] private TMP_Text avgCountTMP = null;
        [Header("★ Config")]
        [SerializeField] private string apiName = "movies";

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