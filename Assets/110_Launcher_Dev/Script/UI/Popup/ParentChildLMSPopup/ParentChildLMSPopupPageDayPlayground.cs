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
	public class ParentChildLMSPopupPageDayPlayground : ParentChildLMSPopupPageDaySubBase
    {
        // Definitions
        // Properties

        // Methods
        public ParentChildLMSPopupPageDayPlayground()
        {
            category = "playground";
        }

        // Events



        // Fields : caching
        private ScrollRect scrollRect_;
        private ScrollRect scrollRect => scrollRect_ ??= GetComponent<ScrollRect>();

        // Fields
        private List<LMSContentThumbnail> playedThumbnails = new List<LMSContentThumbnail>();
        private List<LMSPlaygroundSlot> playgroundSlots = new List<LMSPlaygroundSlot>();

        // Functions

        // Event Handlers
        private void thumbnail_onClick(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail} | contentIndex={thumbnail.ContentIndex}");

            checkNew(thumbnail).Forget();
            UILauncher.One.ContentPU.ShowPlayground(thumbnail);
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

            playedThumbnails.ForEach(thumbnail => thumbnail.OnClick -= thumbnail_onClick);
            playedThumbnails.Clear();
            playgroundSlots.Clear();

            foreach (var ch in playgroundRT.GetChildren())
            {
                if (ch.gameObject != noPlaygroundGO)
                    Destroy(ch.gameObject);
            }
            foreach (var ch in reviewRT.GetChildren())
            {
                if (ch.gameObject != noReviewGO)
                    Destroy(ch.gameObject);
            }
            foreach (var ch in slotRT.GetChildren())
                Destroy(ch.gameObject);

            courseTMP.text = $"Course";
            progressTMP.text = $"<color=#8bbcea>{0}</color> /{0}";
            progressSlider.value = 0;
        }
        protected override async UniTask onLoad(CancellationToken cancellationToken, string date)
        {
            await base.onLoad(cancellationToken, date);

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/day/{ChildData.ID}/playground?searchDate={date}");
            if (cancellationToken.IsCancellationRequested)
                return;
            if (result.Success)
            {
                var completeCount = result.Data.Value<int>("completeCount");
                var learningTime = result.Data.Value<int>("learningTime");
                var totalAvgCompleteCount = result.Data.Value<int>("totalAvgCompleteCount");
                var totalAvgLearningTime = result.Data.Value<int>("totalAvgLearningTime");
                IsNew = result.Data.Value<bool>("isNew");
                var watchList = result.Data.Value<JArray>("watchList");
                var playgroundListAll = result.Data.Value<JArray>("playgroundList");
                int numFirstSlot = playgroundListAll.ToList().FindIndex(list => list.Value<int>("courseId") == ChildData.Course) + 1;
                var playgroundList = playgroundListAll.Where(list => list.Value<int>("courseId") == ChildData.Course).ToArray();
                var currentSlot = playgroundList.ToList().FindIndex(list => !list.Value<bool>("isComplete"));
                if (currentSlot < 0) currentSlot = playgroundList.Length;

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

                courseTMP.text = $"Course {ChildData.Course}";
                progressTMP.text = $"<color=#8bbcea>{currentSlot}</color> /{playgroundList.Length}";
                progressSlider.value = (float)currentSlot / playgroundList.Length;

                int idxCurrentSlot = 0;
                for (int i = 0; i < playgroundList.Length; i++)
                {
                    var slotData = playgroundList[i];
                    var slot = Instantiate(i % 2 == 0 ? slot1PF : slot2PF, slotRT);
                    var stars = slotData.Value<int>("stars");
                    var isComplete = slotData.Value<bool>("isComplete");

                    if (isComplete && idxCurrentSlot < i)
                        idxCurrentSlot = i;
                    slot.Init(ChildData.Course, numFirstSlot + i, stars, isComplete);

                    playgroundSlots.Add(slot);
                }

                var currentSlotGO = playgroundSlots[idxCurrentSlot].gameObject;
                LayoutRebuilder.ForceRebuildLayoutImmediate(slotRT);
                float slotsSize = slotRT.sizeDelta.x;
                float screenSize = slotScroll.GetComponent<RectTransform>().sizeDelta.x;
                float slotX = currentSlotGO.GetComponent<RectTransform>().localPosition.x + currentSlotGO.transform.parent.GetComponent<RectTransform>().localPosition.x;
                float dest = (slotX - (screenSize / 2.0f)) / slotsSize;
                LOG.Info($"slotsSize={slotsSize}, screenSize={screenSize}, slotX ={slotX}, dest={dest}", this);
                slotScroll.horizontalScrollbar.value = dest;

                foreach (var studyData in watchList)
                {
                    var contentIndex = studyData.Value<int>("contentIndex");
                    var contentName = studyData.Value<string>("contentName");
                    var completeDatetime = studyData.Value<DateTime>("completeDatetime");
                    var learningTimeSec = studyData.Value<int>("learningTime");
                    var logSn = studyData.Value<int>("logSn");
                    var isNew2 = studyData.Value<bool>("isNew");

                    var course = (contentIndex / 1000000) % 10;
                    var type = (contentIndex / 1000) % 100;
                    var typeData = LibraryTableLoader.One.GameTable.TypeLists[course - 1][type - 1];

                    var contentIndexArr = contentIndex.ToString().ToCharArray();
                    var address = "";
                    var uiName = "";
                    if (contentIndexArr[0] == '5')
                    {
                        address = $"ReviewGame/Thumbnail/Course{contentIndexArr[1]}/{contentIndex}.png";

                        var reviewData = LibraryTableLoader.One.GameTable.ReviewLists[course - 1].ToList().Find(row => row.Index == contentIndex.ToString());
                        uiName = reviewData.GameName;
                    }
                    else if (contentIndexArr[0] == '6')
                    {
                        address = $"Game/Course{contentIndexArr[1]}/{contentIndex}.png";

                        var playgroundData = LibraryTableLoader.One.GameTable.PlaygroundLists[course - 1].ToList().Find(row => row.Index == contentIndex.ToString());
                        uiName = playgroundData.GameName;
                    }

                    contentName = $"{LocalizationMGR.One.Select(typeData.TitleKor, typeData.TitleEng, typeData.TitleVie)} : {uiName}";

                    Sprite sprite = null;
                    try
                    {
                        sprite = await DataLoader.One.LoadSprite(address);
                    }
                    catch (Exception ex) { LOG.Warning(ex.Message, this); }
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    LMSContentThumbnail thumbnail = null;
                    if (contentIndexArr[0] == '5')
                    {
                        thumbnail = Instantiate(thumbnailPF, reviewRT);
                        thumbnail.Init(contentName, sprite);
                    }
                    else if (contentIndexArr[0] == '6')
                    {
                        thumbnail = Instantiate(thumbnailPF, playgroundRT);
                        var slotName = playgroundListAll.ToList().Find(list => list.Value<int>("contentIndex") == contentIndex).Value<string>("slotName");
                        thumbnail.Init($"Slot {slotName}", sprite);
                    }

                    thumbnail.LogSN = logSn;
                    thumbnail.IsNew = isNew2;
                    thumbnail.ContentIndex = contentIndex;
                    thumbnail.CompleteDatetime = completeDatetime;
                    thumbnail.LearningTime = learningTimeSec;
                    thumbnail.OnClick += thumbnail_onClick;

                    playedThumbnails.Add(thumbnail);
                }

                noPlaygroundGO.SetActive(playgroundRT.childCount < 2);
                noReviewGO.SetActive(reviewRT.childCount < 2);
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
        [SerializeField] private RectTransform playgroundRT = null;
        [SerializeField] private RectTransform reviewRT = null;
        [SerializeField] private LMSContentThumbnail thumbnailPF = null;
        [SerializeField] private TMP_Text courseTMP = null;
        [SerializeField] private TMP_Text progressTMP = null;
        [SerializeField] private Slider progressSlider = null;
        [SerializeField] private RectTransform slotRT = null;
        [SerializeField] private LMSPlaygroundSlot slot1PF = null;
        [SerializeField] private LMSPlaygroundSlot slot2PF = null;
        [SerializeField] protected GameObject noPlaygroundGO;
        [SerializeField] protected GameObject noReviewGO;
        [SerializeField] private ScrollRect slotScroll;

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