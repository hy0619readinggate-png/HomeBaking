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
	public class ParentChildLMSPopupPageWeekAll : ParentChildLMSPopupPageWeekSubBase
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
            dayCountTMP.text = "";
            dayCountNum.ForEach(tmp => tmp.text = "");
            dayCountRate.ForEach(tmp => tmp.text = "");

            for (int i = 0; i < 5; i++)
            {
                dayCountChart.DataSource.SetValue(dayCountChart.DataSource.GetCategoryName(i), 0);
            }
            dayCountChart.Invalidate();
        }
        protected override async UniTask onLoad(string date)
        {
            await base.onLoad(date);

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/weekly/{ChildData.ID}/all?searchDate={date}");
            if (result.Success)
            {
                var totalAvgLearningTime = result.Data.Value<int>("totalAvgLearningTime");
                var learningTypeCompleteList = result.Data.Value<JArray>("learningTypeCompleteList");

                int completCount = 0;
                var dayCounts = Enumerable.Repeat(0, 5).ToArray();
                var dayRates = Enumerable.Repeat<double>(0, 5).ToArray();
                learningTypeCompleteList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var count = list.Value<int>("count");
                    var percent = list.Value<double>("percent");

                    completCount += count;

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "030") idx = 3;
                    else if (code == "050") idx = 4;
                    else if (code == "060") idx = 4;

                    if (idx != -1)
                    {
                        dayCounts[idx] += count;
                        dayRates[idx] += percent;
                    }
                });

                if (totalAvgLearningTime < 60)
                    avgLearningTimeTMP.text = LocalizationMGR.One.GetText("WORD_128", totalAvgLearningTime);
                else
                    avgLearningTimeTMP.text = LocalizationMGR.One.GetText("WORD_127", totalAvgLearningTime / 60);

                dayCountTMP.text = LocalizationMGR.One.GetText("WORD_110", completCount);
                for (int i = 0; i < 5; i++)
                {
                    dayCountChart.DataSource.SetValue(dayCountChart.DataSource.GetCategoryName(i), dayRates[i]);
                    dayCountNum[i].text = LocalizationMGR.One.GetText("WORD_122", dayCounts[i]);
                    dayCountRate[i].text = $"{dayRates[i]}%";
                }
                dayCountChart.Invalidate();
                if (0 < completCount)
                    dayCountChart.GetComponent<PieAnimation>().Animate();
            }
            else
            {
                LOG.Warning(result.Data.Value<string>("message"), this);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text avgLearningTimeTMP = null;
        [SerializeField] private CanvasPieChart dayCountChart = null;
        [SerializeField] private TMP_Text dayCountTMP = null;
        [SerializeField] private TMP_Text[] dayCountNum = null;
        [SerializeField] private TMP_Text[] dayCountRate = null;

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