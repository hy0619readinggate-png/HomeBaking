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
	public class ParentChildLMSPopupPageMonthStudy : ParentChildLMSPopupPageMonthSubBase
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

            topPercentageTMP.text = "";
            childProgressbars.ForEach(progressbar => progressbar.gameObject.SetActive(false));
            totalProgressbars.ForEach(progressbar => progressbar.gameObject.SetActive(false));
        }
        protected override async UniTask onLoad(string date)
        {
            await base.onLoad(date);

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/monthly/{ChildData.ID}/learning-type?currentYearMonth={date.Substring(0, 6)}");
            if (result.Success)
            {
                var childLearningList = result.Data.Value<JArray>("childLearningList");
                var totalLearningList = result.Data.Value<JArray>("totalLearningList");
                var topPercentage = result.Data.Value<int>("topPercentage");

                int maxCount = 10;
                var childCounts = Enumerable.Repeat(0, 5).ToArray();
                childLearningList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var count = list.Value<int>("count");

                    while (maxCount < count) maxCount += 10;

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "030") idx = 3;
                    else if (code == "050") idx = 4;
                    else if (code == "060") idx = 4;

                    if (idx != -1)
                    {
                        childCounts[idx] += count;
                    }
                });

                var totalCounts = Enumerable.Repeat(0, 5).ToArray();
                totalLearningList.ForEach(list =>
                {
                    var code = list.Value<string>("learningTypeCode");
                    var count = list.Value<int>("count");

                    while (maxCount < count) maxCount += 10;

                    int idx = -1;
                    if (code == "010") idx = 0;
                    else if (code == "020") idx = 1;
                    else if (code == "040") idx = 2;
                    else if (code == "030") idx = 3;
                    else if (code == "050") idx = 4;
                    else if (code == "060") idx = 4;

                    if (idx != -1)
                    {
                        totalCounts[idx] += count;
                    }
                });

                topPercentageTMP.text = $"{topPercentage}%";
                for (int i = 0; i < 5; i++)
                {
                    childProgressbars[i].gameObject.SetActive(true);
                    childProgressbars[i].Max = maxCount;
                    childProgressbars[i].Value = childCounts[i];

                    totalProgressbars[i].gameObject.SetActive(true);
                    totalProgressbars[i].Max = maxCount;
                    totalProgressbars[i].Value = totalCounts[i];
                }
            }
            else
            {
                LOG.Warning(result.Data.Value<string>("message"), this);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text topPercentageTMP = null;
        [SerializeField] private LMSProgressbar2[] childProgressbars = null;
        [SerializeField] private LMSProgressbar2[] totalProgressbars = null;

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