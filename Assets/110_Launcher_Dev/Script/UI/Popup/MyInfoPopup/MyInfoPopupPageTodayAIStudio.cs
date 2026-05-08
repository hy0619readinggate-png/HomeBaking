using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;

namespace DoDoEng.Launcher.UI
{
	public class MyInfoPopupPageTodayAIStudio : MyInfoPopupSubBase
    {
        // Definitions
        // Properties

        // Methods
        public MyInfoPopupPageTodayAIStudio()
        {
            category = "ai-studio";
        }

        // Events



        // Fields : caching
        private ScrollRect scrollRect_;
        private ScrollRect scrollRect => scrollRect_ ??= GetComponent<ScrollRect>();

        // Fields
        private List<LMSContentThumbnail> recordedThumbnails = new List<LMSContentThumbnail>();

        // Functions

        // Event Handlers
        private void thumbnail_onClick(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail} | contentIndex={thumbnail.ContentIndex}");

            checkNew(thumbnail).Forget();
            UILauncher.One.ContentPU.ShowAIStudio(thumbnail);
        }

        // Overrides
        protected override void onClear()
        {
            base.onClear();

            countTMP.text = "";
            timeTMP.text = "";
            aVGcountTMP.text = "";
            aVGtimeTMP.text = "";
            countSlider.value = 0;
            timeSlider.value = 0;
            aVGcountSlider.value = 0;
            aVGtimeSlider.value = 0;

            recordedThumbnails.ForEach(thumbnail => thumbnail.OnClick -= thumbnail_onClick);
            recordedThumbnails.Clear();

            foreach (var ch in recordedRT.GetChildren())
            {
                if (ch.gameObject != noUserContentGO)
                    Destroy(ch.gameObject);
            }
        }
        protected override async UniTask onLoad(CancellationToken cancellationToken, string date)
        {
            await base.onLoad(cancellationToken, date);

            var result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/my-info/today/ai-studio?searchDate={date}");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (result.Success)
            {
                var completeCount = result.Data.Value<int>("completeCount");
                var learningTime = result.Data.Value<int>("learningTime");
                var totalAvgCompleteCount = result.Data.Value<int>("totalAvgCompleteCount");
                var totalAvgLearningTime = result.Data.Value<int>("totalAvgLearningTime");
                IsNew = result.Data.Value<bool>("isNew");
                var userContentList = result.Data.Value<JArray>("userContentList");

                var learningTimeTS = TimeSpan.FromSeconds(learningTime);
                var totalAvgLearningTimeTS = TimeSpan.FromSeconds(totalAvgLearningTime);
                var time = (int)Math.Floor(learningTimeTS.TotalMinutes);
                var sec = (int)Math.Floor(learningTimeTS.TotalSeconds);
                var avgTime = (int)Math.Floor(totalAvgLearningTimeTS.TotalMinutes);
                var avgSec = (int)Math.Floor(totalAvgLearningTimeTS.TotalSeconds);
                var countMax = (((Math.Max(completeCount, totalAvgCompleteCount) - 1) / 10) + 1) * 10f;
                var timeMax = (((Math.Max(time, avgTime) - 1) / 60) + 1) * 60f;

                countTMP.text = LocalizationMGR.One.GetText("WORD_122", completeCount);
                if (time > 0)
                    timeTMP.text = LocalizationMGR.One.GetText("WORD_127", time);
                else
                    timeTMP.text = LocalizationMGR.One.GetText("WORD_128", sec);
                aVGcountTMP.text = LocalizationMGR.One.GetText("WORD_122", totalAvgCompleteCount);
                if (avgTime > 0)
                    aVGtimeTMP.text = LocalizationMGR.One.GetText("WORD_127", avgTime);
                else
                    aVGtimeTMP.text = LocalizationMGR.One.GetText("WORD_128", avgSec);

                countSlider.value = completeCount / countMax;
                timeSlider.value = time / timeMax;
                aVGcountSlider.value = totalAvgCompleteCount / countMax;
                aVGtimeSlider.value = avgTime / timeMax;

                noUserContentGO.SetActive(userContentList.Count == 0);
                foreach (var recordData in userContentList)
                {
                    var thumbnail = Instantiate(thumbnailPF, recordedRT);
                    var contentIndex = recordData.Value<int>("contentIndex");
                    var contentName = recordData.Value<string>("contentName");
                    var completeDatetime = recordData.Value<DateTime>("completeDatetime");
                    var learningTimeSec = recordData.Value<int>("learningTime");
                    var recordType = recordData.Value<int>("recordType");
                    var logSn = recordData.Value<int>("logSn");
                    var isNew2 = recordData.Value<bool>("isNew");

                    var address = $"AIStudio/Thumbnail/{contentIndex}.png";
                    var sprite = await DataLoader.One.LoadSprite(address);
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    thumbnail.Init(contentName, sprite, true, recordType + 1);
                    thumbnail.LogSN = logSn;
                    thumbnail.IsNew = isNew2;
                    thumbnail.ContentIndex = contentIndex;
                    thumbnail.CompleteDatetime = completeDatetime;
                    thumbnail.LearningTime = learningTimeSec;
                    thumbnail.OnClick += thumbnail_onClick;

                    recordedThumbnails.Add(thumbnail);
                }
            }
            else
            {
                LOG.Warning(result.Data.Value<string>("message"), this);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text countTMP = null;
        [SerializeField] private TMP_Text timeTMP = null;
        [SerializeField] private TMP_Text aVGcountTMP = null;
        [SerializeField] private TMP_Text aVGtimeTMP = null;
        [SerializeField] private Slider countSlider = null;
        [SerializeField] private Slider timeSlider = null;
        [SerializeField] private Slider aVGcountSlider = null;
        [SerializeField] private Slider aVGtimeSlider = null;
        [SerializeField] private RectTransform recordedRT = null;
        [SerializeField] private LMSContentThumbnail thumbnailPF = null;
        [SerializeField] protected GameObject noUserContentGO;

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
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // Unity Coroutine
    }
}