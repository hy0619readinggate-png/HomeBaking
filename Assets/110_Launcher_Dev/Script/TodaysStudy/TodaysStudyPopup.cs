using beyondi.Util;
using System.Linq;
using System.Collections;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using DoDoEng.TodaysStudy.UI;

namespace DoDoEng.Launcher
{
    public class TodaysStudyPopup : MonoBehaviour
    {
        // Properties
        public int Stage => stage;
        public int Day => day;
        public bool AllCompleted
        {
            get
            {
                return todaysStudyItems.All(item => item.IsComplete);
            }
        }
        public bool Interactable
        {
            get
            {
                if (cg != null)
                    return cg.blocksRaycasts;
                else return true;
            }
            set
            {
                if (cg != null)
                    cg.blocksRaycasts = value;
                scrollSwipeControl.Interactable = value;
            }
        }
        public LibraryMovieTableLoaderResult MovieTableResult { get; set; }
        public LibraryEBookTableLoaderResult EBookTableResult { get; set; }
        public TodaysStudyItem CurrentTodaysItem => todaysStudyItems.Count > currentContentIdx ? todaysStudyItems[currentContentIdx] : null;

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public async UniTask Show(int course, int stage, int day = 1, int order = 1)
        {
            LOG.Info($"Show({course}, {stage}, {day}, {order})", this);

            this.course = course;
            this.stage = stage;
            this.day = day;
            currentContentIdx = order - 1;

            Activate(true);

            for (int i = 0; i < backgrounds.Length; i++)
            {
                var bg = backgrounds[i];
                bg.SetActive(i == course - 1);
            }

            removeAllItems();

            await Refresh();
        }
        public async UniTask Refresh()
        {
            daysData = await LMS.One.LoadDayCurriculum(stage);

            await loadDayCancelable(day, currentContentIdx + 1);
        }
        public void CompleteANIReady()
        {
            LOG.Info($"CompleteANIReady() | currentContentIdx={currentContentIdx} | todaysStudyItems={todaysStudyItems} | Count={todaysStudyItems.Count}", this);
            todaysStudyItems[currentContentIdx].CompleteANIReady();
        }
        public async UniTask CompleteANIPlay()
        {
            if (todaysStudyItems[currentContentIdx].IsComplete)
            {
                RunnerParam.TodaysCompleted = true;
                AudioMGR.One.PlayEffect(completeContentsCLIP);
                todaysStudyItems[currentContentIdx].CompleteANIStart();
                await UniTask.Delay(1500);
                for (int i = 0; i < todaysStudyItems.Count; i++)
                {
                    int idxNext = (currentContentIdx + 1 + i) % todaysStudyItems.Count;
                    if (!todaysStudyItems[idxNext].IsComplete)
                    {
                        currentContentIdx = idxNext;
                        scrollSwipeControl.JumpIndex(currentContentIdx);
                        await UniTask.Delay(500);
                        break;
                    }
                }
            }
        }
        public async UniTask<bool> DownloadAllStagesAssets()
        {
            if (daysData != null)
            {
                var idxList = new List<IndexBase>();
                foreach (var day in daysData)
                {
                    foreach (var content in day.Value<JArray>("learningIndexes"))
                    {
                        var contentIndex = content.Value<string>("contentIndex");
                        IndexBase index = null;
                        if (contentIndex.StartsWith("2"))
                        {
                            index = new EBookReadIndex(contentIndex);
                        }
                        else if (contentIndex.StartsWith("5"))
                        {
                            index = new GameIndex(contentIndex);
                        }
                        else if (contentIndex.StartsWith("1"))
                        {
                            index = new ActivityIndex(contentIndex);
                        }

                        if (index != null)
                        {
                            idxList.Add(index);
                        }
                    }
                }

                var size = await DataDownloader.One.GetDataDownloadSize(idxList);
                if (size > 0)
                {
                    var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                    if (result == SimplePopupResult.Yes)
                    {
                        cg.blocksRaycasts = false;
                        await DataDownloader.One.DownloadData(idxList, SystemUI.One.DownloadPU);
                        cg.blocksRaycasts = true;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return true;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private int course;
        private int stage;
        private int day;
        private int currentContentIdx;
        private JArray daysData;
        private List<TodaysStudyItem> todaysStudyItems = new List<TodaysStudyItem>();
        private List<TodaysStudyNaviIcon> todaysStudyNaviIcons = new List<TodaysStudyNaviIcon>();
        protected CancellationTokenSource cancel = new CancellationTokenSource();

        // Functions
        private void removeAllItems()
        {
            naviAreaRT.RemoveAllChildren();
            naviAreaRT.DetachChildren();
            todaysStudyNaviIcons.Clear();

            itemsAreaRT.RemoveAllChildren();
            itemsAreaRT.DetachChildren();
            todaysStudyItems.Clear();
        }
        private async UniTask loadDayCancelable(int day = 1, int order = 1)
        {
            cancel.Cancel();
            cancel = new CancellationTokenSource();

            await loadDay(day, order, cancel.Token);
        }
        private async UniTask loadDay(int day, int order, CancellationToken cancellationToken)
        {
            LOG.Info($"loadDay() | day={day} | order={order}", this);
            this.day = day;
            currentContentIdx = order - 1;

            dayTMP.text = $"Day {day}";
            prevDayBT.gameObject.SetActive(day > 1);
            nextDayBT.gameObject.SetActive(day < daysData.Count);
            var nextEnabled = day < daysData.Count && (!UserData.One.Child.DayLimit || daysData[day].Value<bool>("isComplete") || day + 1 == UserData.One.Child.DayProgress.Value<int>("day"));
            nextDayEnableGO.SetActive(nextEnabled);
            nextDayBT.interactable = nextEnabled;

            //scrollSwipeControl.Activate(false);

            removeAllItems();

            if (daysData != null)
            {
                if (daysData.Count > day - 1)
                {
                    var dayType = daysData[day - 1].Value<int>("dayType");
                    UITodaysStudy.One.VisibleReview = dayType == 1;
                    var learningIndexes = daysData[day - 1].Value<JArray>("learningIndexes");
                    var o = 0;
                    for (int i = 0; i < learningIndexes.Count; i++)
                    {
                        var learningIndexId = learningIndexes[i].Value<int>("learningIndexId");
                        var contentIndex = learningIndexes[i].Value<string>("contentIndex");
                        var isComplete = learningIndexes[i].Value<bool>("isComplete");
                        var isRead = learningIndexes[i].Value<bool>("isRead");
                        var isRecorded = learningIndexes[i].Value<bool>("isRecorded");
                        var isQuizDone = learningIndexes[i].Value<bool>("isQuizDone");

                        LOG.Info($"i={i} | learningIndexId={learningIndexId} | contentIndex={contentIndex} | isComplete={isComplete} | isRecorded={isRecorded} | isQuizDone={isQuizDone}", this);

                        if (contentIndex.Length == 8)
                        {
                            var naviIcon = Instantiate(naviIconPF, naviAreaRT).GetComponent<TodaysStudyNaviIcon>();
                            naviIcon.Init(isComplete || isRecorded, contentIndex);
                            todaysStudyNaviIcons.Add(naviIcon);

                            if (currentContentIdx == -1 && !isComplete) currentContentIdx = o;

                            var item = Instantiate(itemPF, itemsAreaRT).GetComponent<TodaysStudyItem>();
                            if (cancellationToken.IsCancellationRequested)
                                return;
                            item.Init(o++, TodaysStudyItem.TSItemState.Opened, contentIndex, learningIndexId, isComplete || isRecorded, isRead, isRecorded, isQuizDone);
                            item.OnClick += item_onClick;
                            todaysStudyItems.Add(item);
                        }
                    }
                }

                if (currentContentIdx == -1) currentContentIdx = 0;
                scrollSwipeControl.SetIndex(currentContentIdx);
                //scrollSwipeControl.Activate(true);

                for (int i = 0; i < todaysStudyItems.Count; i++)
                {
                    var item = todaysStudyItems[i];
                    Sprite sprite = null;
                    if (item.LearningType == TodaysStudyItem.TSLearningType.eBook)
                    {
                        try
                        {
                            var ebook = EBookTableResult.List.SingleOrDefault(c => c.Index == item.ContentIndex);
                            var address = $"eBook/Thumbnail/Category{ebook.MainCategory}/{item.ContentIndex}_w.png";
                            sprite = await DataLoader.One.LoadSprite(address);
                        }
                        catch { }
                    }
                    else if (item.LearningType == TodaysStudyItem.TSLearningType.Movie)
                    {
                        try
                        {
                            var movie = MovieTableResult.List.SingleOrDefault(c => c.Index == item.ContentIndex);
                            var category = "None";
                            if (movie.MainCategory > 0)
                                category = MovieTableResult.CategoryMain[movie.MainCategory - 1].Name;
                            var address = $"Movie/Thumbnail/Category{movie.MainCategory}/{item.ContentIndex}.png";
                            sprite = await DataLoader.One.LoadSprite(address);
                        }
                        catch { }
                    }
                    else if (item.LearningType == TodaysStudyItem.TSLearningType.Game)
                    {
                        try
                        {
                            var address = $"ReviewGame/Thumbnail/Course{course}/{item.ContentIndex}.png";
                            sprite = await DataLoader.One.LoadSprite(address);
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            var address = $"Activity/Thumbnail/Course{course}/{item.ContentIndex}.png";
                            sprite = await DataLoader.One.LoadSprite(address);
                        }
                        catch { }
                    }
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (sprite != null)
                        item.Thumbnail = sprite.texture;
                }
            }
        }

        // Event Handlers
        private void item_onClick(TodaysStudyItem item)
        {
            LOG.Info($"item_onClick() item: {item}", this);

            RunnerParam.TodaysStage = stage;
            RunnerParam.TodaysDay = day;
            RunnerParam.TodaysOrder = item.Order + 1;
            RunnerParam.TodaysCompleted = item.IsComplete;
            OnClickItem?.Invoke(item.Index, item.LearningIndexId);
        }
        private async UniTask dayBT_onClick()
        {
            LOG.Function(this);

            var changedCourse = await UITodaysStudy.One.CoursePU.ShowPopup(course, stage, day);
            if (changedCourse != -1)
            {
                if (!UserData.One.Child.DayLimit || !UITodaysStudy.One.CoursePU.Contents[changedCourse - 1].IsAllCompleted)
                {
                    await LMS.One.SaveLearningCourse(changedCourse);
                    UserData.One.Child.Course = changedCourse;
                }
                Show(changedCourse, UITodaysStudy.One.CoursePU.Stage, UITodaysStudy.One.CoursePU.Day).Forget();
            }
        }
        private void prevDayBT_onClick()
        {
            LOG.Info($"prevDayBT_onClick()", this);

            loadDayCancelable(--day).Forget();
        }
        private void nextDayBT_onClick()
        {
            LOG.Info($"nextDayBT_onClick()", this);

            loadDayCancelable(++day).Forget();
        }
        private void scrollSwipeControl_onBeginDrag()
        {
            LOG.Function(this);

            for (int i = 0; i < todaysStudyNaviIcons.Count; i++)
                todaysStudyItems[i].Select(false);
            selectedFX.SetActive(false);
        }
        private void scrollSwipeControl_onEndDrag()
        {
            LOG.Function(this);

            for (int i = 0; i < todaysStudyNaviIcons.Count; i++)
                todaysStudyItems[i].Select(i == currentContentIdx);

            selectedFX.SetActive(true);
        }
        private void scrollSwipeControl_onChangeIndex(int idx)
        {
            LOG.Function(this, $"| idx={idx}");

            currentContentIdx = idx;
            for (int i = 0; i < todaysStudyNaviIcons.Count; i++)
                todaysStudyItems[i].Select(i == idx);
            for (int i = 0; i < todaysStudyNaviIcons.Count; i++)
            {
                todaysStudyNaviIcons[i].IsSelected = i == idx;
            }
            selectedFX.SetActive(true);
        }


        // Events
        [HideInInspector]
        public event Action<IndexBase, int> OnClickItem;

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private RectTransform itemsAreaRT = null;
        [SerializeField] private TodaysStudyItem itemPF = null;
        [SerializeField] private RectTransform naviAreaRT = null;
        [SerializeField] private TodaysStudyNaviIcon naviIconPF = null;
        [SerializeField] private TMP_Text dayTMP = null;
        [SerializeField] private Button dayBT = null;
        [SerializeField] private Button prevDayBT = null;
        [SerializeField] private Button nextDayBT = null;
        [SerializeField] private GameObject nextDayEnableGO = null;
        [SerializeField] private ScrollSwipeControl scrollSwipeControl = null;
        [SerializeField] private GameObject[] backgrounds = null;
        [SerializeField] private GameObject selectedFX = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip completeContentsCLIP = null;

        // Unity Messages
        private void Awake()
        {
            LOG.Info($"Awake()", this);

            selectedFX.SetActive(false);

            dayBT.onClick.AddListener(() => dayBT_onClick().Forget());
            prevDayBT.onClick.AddListener(() => prevDayBT_onClick());
            nextDayBT.onClick.AddListener(() => nextDayBT_onClick());
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
            scrollSwipeControl.OnBeginDrag += scrollSwipeControl_onBeginDrag;
            scrollSwipeControl.OnEndDrag += scrollSwipeControl_onEndDrag;
            scrollSwipeControl.OnChangeIndex += scrollSwipeControl_onChangeIndex;
        }
        protected void OnDisable()
        {
            scrollSwipeControl.OnBeginDrag -= scrollSwipeControl_onBeginDrag;
            scrollSwipeControl.OnEndDrag -= scrollSwipeControl_onEndDrag;
            scrollSwipeControl.OnChangeIndex -= scrollSwipeControl_onChangeIndex;
        }
        private void OnDestroy()
        {
            cancel.Cancel();
        }
    }
}