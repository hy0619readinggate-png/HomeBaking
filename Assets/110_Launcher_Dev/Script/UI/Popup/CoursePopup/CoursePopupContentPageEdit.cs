using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
	public class CoursePopupContentPageEdit : MonoBehaviour
	{
        // Definitions
        // Properties
        public int Stage => stage;
        public int Day => day;
        public bool IsAllCompleted => isAllCompleted;

        // Methods
        public async UniTask Init(int course, JObject progressData, List<JArray> curriculumDatas = null)
        {
            LOG.Function(this, $"| course={course}");

            this.course = course;
            this.progressData = progressData;
            this.curriculumDatas = curriculumDatas;
            stage = 0;

            var courseData = CourseTableLoader.One.Table.CourseList.SingleOrDefault(List => List.Course == course);
            stageLists = CourseTableLoader.One.Table.StageList.Where(list => list.Course == course).ToArray();

            if (courseNumTMP != null) courseNumTMP.text = course.ToString();
            if (courseNameTMP != null)
            {
                courseNameTMP.text = LocalizationMGR.One.Select(courseData.TitleKor, courseData.TitleEng, courseData.TitleVie);
            }

            stageInfoTMP.text = "";
            stageExplainTMP.text = "";
            if (dayInfoTMP != null)
                dayInfoTMP.text = "";

            for (int i = 0; i < stageToggles.Length; i++)
            {
                stageToggles[i].Activate(i < stageLists.Length);
                if (i < stageArrows.Length)
                    stageArrows[i].SetActive(i < stageLists.Length - 1);

                if (i < stageLists.Length)
                {
                    stageToggles[i].Num = stageLists[i].Stage;
                    stageToggles[i].IsFirstStage = i == 0;
                }
                else
                    stageToggles[i].Num = 0;

                if (stageToggles[i].Slider != null)
                    stageToggles[i].Slider.value = 0;
            }

            foreach (var ch in keywordsRT.GetChildren())
                Destroy(ch.gameObject);

            var keywords = stageLists[0].Keywords.Split(",");
            keywords.ForEach(keyword =>
            {
                var sticker = Instantiate(keywordPF[stageLists[0].Course - 1], keywordsRT);
                sticker.Text = keyword;
            });

            delayedRebuildLayout().Forget();

            if (dayToggles.Length > 0)
            {
                for (int i = 0; i < dayToggles.Length; i++)
                {
                    dayToggles[i].gameObject.SetActive(true);
                    dayToggles[i].Num = i + 1;
                    dayToggles[i].IsReview = false;
                    dayToggles[i].IsComplete = false;
                    dayToggles[i].Final = false;
                }
                await loadAllStageProgress();
            }
        }
        public void Activate(bool active)
        {
            LOG.Function(this, $"{active}");

            gameObject.SetActive(active);
        }
        public void SelectStage(int stage)
        {
            LOG.Function(this, $"| stage={stage}");

            if (stageLists != null && this.stage != stage)
            {
                if (stage == 0) stage = stageLists[0].Stage;

                this.stage = stage;

                var toggle = stageToggles.ToList().FirstOrDefault(t => t.Num == stage);
                toggle.Select(true);
                currentStageRT = toggle.GetComponent<RectTransform>();

                var stageTable = stageLists.ToList().SingleOrDefault(list => list.Stage == stage);
                if (dayToggles.Length > 0)
                    stageInfoTMP.text = "0%";
                else
                    stageInfoTMP.text = LocalizationMGR.One.GetText("WORD_122", stageTable.DayCount);
                stageExplainTMP.text = LocalizationMGR.One.Select(stageTable.InfoKor, stageTable.InfoEng, stageTable.InfoVie);

                for (int i = 0; i < dayToggles.Length; i++)
                {
                    if (i < stageTable.DayCount)
                    {
                        dayToggles[i].gameObject.SetActive(true);
                        dayToggles[i].Num = i + 1;
                        dayToggles[i].IsReview = false;
                        dayToggles[i].IsComplete = false;
                        dayToggles[i].Final = i == stageTable.DayCount - 1;
                    }
                    else
                        dayToggles[i].gameObject.SetActive(false);
                }

                if (dayToggles.Length > 0)
                {
                    dayScroll.horizontalScrollbar.value = 0;
                    if (startBT != null)
                    {
                        if (stage == progressData.Value<int>("stageId"))
                            SelectDay(progressData.Value<int>("day"));
                        else
                            SelectDay(1);
                    }
                    updateDays(stage);
                }
            }
        }
        public void SelectDay(int day)
        {
            LOG.Function(this, $"day={day}");
            if (dayToggles.Length > 0)
            {
                if (dayInfoTMP != null)
                {
                    dayInfoTMP.text = LocalizationMGR.One.GetText("MESSAGE_22", UserData.One.Child.NickName);
                }

                if (day == 0) day = 1;
                dayToggles[day - 1].Select();
            }
        }
        public void SetInteract(bool interact)
        {
            stageToggles.ForEach(toggle => toggle.SetInteract(interact));
            dayToggles.ForEach(toggle => toggle.SetInteract(interact));
        }

        // Events
        public Action<int, int> OnClickStart;



        // Fields : caching

        // Fields
        private int course;
        private int stage = 0;
        private int day = 1;
        private StageList[] stageLists;
        private RectTransform currentStageRT;
        private List<JArray> stageDatas;
        private List<JArray> curriculumDatas;
        private JObject progressData;
        private bool isAllCompleted;

        // Functions
        private async UniTask loadAllStageProgress()
        {
            LOG.Function(this, $"| course={course}");

            stageDatas = new List<JArray>();
            isAllCompleted = true;
            for (int i = 0; i < stageLists.Length; i++)
            {
                JArray daysData;
                if (curriculumDatas != null)
                    daysData = curriculumDatas[stageLists[i].Stage - 1];
                else
                    daysData = await LMS.One.LoadDayCurriculum(stageLists[i].Stage);
                stageDatas.Add(daysData);
                if (!stageToggles[i])
                    break;
                if (daysData != null)
                {
                    int completeCount = 0;
                    for (int j = 0; j < daysData.Count; j++)
                    {
                        var isComplete = daysData[j].Value<bool>("isComplete");
                        if (isComplete) completeCount++;
                        else isAllCompleted = false;
                    }

                    if (stageToggles[i].gameObject.activeSelf && stageToggles[i].Slider != null)
                    {
                        stageToggles[i].Slider.value = completeCount / (float)daysData.Count;
                    }
                }
            }

            LOG.Info($"course={course}, isAllCompleted={isAllCompleted}", this);
        }
        private void updateDays(int stage)
        {
            LOG.Function(this, $"| stage={stage}");

            var findIdx = stageLists.ToList().FindIndex(d => d.Stage == stage);
            if (findIdx != -1 && findIdx < stageDatas.Count)
            {
                var daysData = stageDatas[findIdx];
                int completeCount = 0;
                int idxCurrent = 0;
                for (int i = 0; i < daysData.Count; i++)
                {
                    var dayType = daysData[i].Value<int>("dayType");
                    var isComplete = daysData[i].Value<bool>("isComplete");
                    if (isComplete) completeCount++;
                    dayToggles[i].IsReview = dayType == 1;
                    dayToggles[i].IsComplete = isComplete;
                    var isCurrent = (stage == progressData.Value<int>("stageId") && i + 1 == progressData.Value<int>("day"));
                    dayToggles[i].Current = isCurrent;
                    if ((isComplete || isCurrent) && idxCurrent < i) idxCurrent = i;
                    dayToggles[i].SetInteract(true);
                }

                var currentSlotGO = dayToggles[idxCurrent].gameObject;
                LayoutRebuilder.ForceRebuildLayoutImmediate(dayRT);
                float slotsSize = dayRT.sizeDelta.x;
                float screenSize = dayScroll.GetComponent<RectTransform>().sizeDelta.x;
                float slotX = currentSlotGO.GetComponent<RectTransform>().localPosition.x + currentSlotGO.transform.parent.GetComponent<RectTransform>().localPosition.x;
                float dest = (slotX - (screenSize / 2.0f)) / slotsSize;
                LOG.Info($"slotsSize={slotsSize}, screenSize={screenSize}, slotX ={slotX}, dest={dest}", this);
                dayScroll.horizontalScrollbar.value = dest;

                var currentStageToggle = stageToggles.SingleOrDefault(toggle => toggle.gameObject.activeSelf && toggle.Num == stage && toggle.Slider != null);
                if (currentStageToggle != null)
                {
                    currentStageToggle.Slider.value = completeCount / (float)daysData.Count;
                }
                if (dayToggles.Length > 0)
                    stageInfoTMP.text = $"{(int)(completeCount / (float)daysData.Count * 100)}%";
            }

            updateStartBT();
        }
        private async UniTask delayedRebuildLayout()
        {
            await UniTask.Yield();
            if (keywordsRT.gameObject.activeInHierarchy)
                keywordsRT.GetComponent<MonoBehaviour>().RebuildLayout();
        }
        private void updateStartBT()
        {
            if (startBT != null)
            {
                LOG.Function(this, $"{UserData.One.Child.Course}, {progressData.Value<int>("stageId")}, {progressData.Value<int>("day")}");
                if ((stage <= progressData.Value<int>("stageId") && day <= progressData.Value<int>("day")) ||
                    dayToggles[day - 1].IsComplete || !UserData.One.Child.DayLimit)
                {
                    dayToggles[0].GetComponent<Toggle>().group.allowSwitchOff = false;
                    startBT.interactable = true;
                }
                else
                {
                    dayToggles[0].GetComponent<Toggle>().group.allowSwitchOff = true;
                    dayToggles.ForEach(toggle => toggle.DeSelect());
                    startBT.interactable = false;
                }
            }
        }

        // Event Handlers
        private void editStageTGL_onSelect(int num)
        {
            LOG.Function(this, $"| num ={num}");

            SelectStage(num);
        }
        private void editDayTGL_onSelect(int num)
        {
            day = num;

            updateStartBT();
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private Button startBT = null;
        [SerializeField] private TMP_Text courseNumTMP = null;
        [SerializeField] private TMP_Text courseNameTMP = null;
        [SerializeField] private CoursePopupContentStage[] stageToggles = null;
		[SerializeField] private GameObject[] stageArrows = null;
        [SerializeField] private CoursePopupContentDay[] dayToggles = null;
        [SerializeField] private TMP_Text stageInfoTMP = null;
        [SerializeField] private TMP_Text stageExplainTMP = null;
        [SerializeField] private TMP_Text dayInfoTMP = null;
        [SerializeField] private RectTransform keywordsRT = null;
        [SerializeField] private CoursePopupContentKeyword[] keywordPF = null;
        [SerializeField] private RectTransform stageArrowRT = null;
        [SerializeField] private RectTransform dayRT = null;
        [SerializeField] private ScrollRect dayScroll = null;

        // Unity Messages
        private void Awake()
		{
            if (startBT != null) startBT.onClick.AddListener(() => OnClickStart?.Invoke(stage, day));
        }
		private void Start()
		{
        }
        private void OnEnable()
        {
            stageToggles.ForEach(stage => stage.OnSelect += editStageTGL_onSelect);
            dayToggles.ForEach(day => day.OnSelect += editDayTGL_onSelect);
        }
        private void OnDisable()
        {
            stageToggles.ForEach(stage => stage.OnSelect -= editStageTGL_onSelect);
            dayToggles.ForEach(day => day.OnSelect -= editDayTGL_onSelect);
        }
        private void Update()
        {
            if (stageArrowRT && currentStageRT)
            {
                stageArrowRT.position = new Vector2(currentStageRT.position.x, stageArrowRT.position.y);
            }
        }

        // Unity Coroutine
    }
}