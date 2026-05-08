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
	public class ParentChildLMSPopupPageMonthArea : ParentChildLMSPopupPageMonthSubBase
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

            totalGauges.ForEach(gauge => gauge.gameObject.SetActive(false));
            categories.ForEach(category => category.gameObject.SetActive(false));
        }
        protected override async UniTask onLoad(string date)
        {
            await base.onLoad(date);

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/monthly/{ChildData.ID}/language-type?currentYearMonth={date.Substring(0, 6)}");
            if (result.Success)
            {
                var languageTypeList = result.Data.Value<JArray>("languageTypeList");

                int accumulate = 0;
                for (int i = 0; i < languageTypeList.Count; i++)
                {
                    var code = languageTypeList[i].Value<string>("langTypeCode");
                    var name = languageTypeList[i].Value<string>("langTypeName");
                    var count = languageTypeList[i].Value<int>("count");
                    var percent = languageTypeList[i].Value<double>("percent");
                    int percentInt = (int)Math.Round(percent, 0, MidpointRounding.AwayFromZero);

                    if (i < totalGauges.Length)
                    {
                        totalGauges[i].gameObject.SetActive(true);
                        totalGauges[i].flexibleWidth = (float)percentInt;
                        var image = totalGauges[i].GetComponent<Image>();
                        if (0 < i && i < languageTypeList.Count - 1)
                            image.sprite = null;
                        else if (i == languageTypeList.Count - 1)
                        {
                            image.sprite = totalGauges[totalGauges.Length - 1].GetComponent<Image>().sprite;
                            image.type = Image.Type.Sliced;
                        }
                    }

                    if (i < categories.Length)
                    {
                        categories[i].gameObject.SetActive(true);
                        categories[i].Set(name, accumulate, percentInt);
                    }

                    accumulate += percentInt;
                }
            }
            else
            {
                LOG.Warning(result.Data.Value<string>("message"), this);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private LayoutElement[] totalGauges = null;
        [SerializeField] private LMSLearningCategory[] categories = null;

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