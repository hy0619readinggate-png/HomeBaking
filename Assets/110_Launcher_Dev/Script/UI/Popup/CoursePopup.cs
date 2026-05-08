using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DoDoEng.Launcher.UI
{
    public enum CoursePopupType { New, Edit };
    public class CoursePopup : PopupBase<int>
	{
        // Definitions
        // Properties
        public int Stage => currentStage;
        public int Day => currentDay;
        public CoursePopupContent[] Contents => contents;

        // Methods
        public async UniTask Init(CoursePopupType type, CourseTableLoaderResult courseTable)
        {
            this.type = type;
            this.courseTable = courseTable;

            curriculumDatas = new List<JArray>();
            if (type == CoursePopupType.Edit)
            {
                var allCurriculumData = await LMS.One.LoadDayCurriculum();
                if (allCurriculumData != null)
                {
                    for (int i = 0; i < CourseTableLoader.One.Table.StageList.Length; i++)
                    {
                        curriculumDatas.Add(new JArray());
                    }
                    for (int i = 0; i < allCurriculumData.Count; i++)
                    {
                        var dayData = allCurriculumData[i];
                        var stageId = dayData.Value<int>("stageId");
                        curriculumDatas[stageId - 1].Add(dayData);
                    }
                }
            }

            for (int i = 0; i < contents.Length; i++)
            {
                await contents[i].Init(type, courseTable, i, curriculumDatas);
            }
        }
        public async UniTask<int> ShowPopup(int currentCourse, int currentStage = 0, int currentDay = 0)
        {
            LOG.Function(this, $"{currentCourse} | {currentStage} | {currentDay}");

            AudioMGR.One.PlayEffect(popupCLIP);

            if (currentStage == 0)
                currentStage = courseTable.StageList.ToList().Find(stage => stage.Course == currentCourse).Stage;
            this.currentStage = currentStage;
            if (currentDay == 0) currentDay = 1;
            this.currentDay = currentDay;

            idxCurrentTab = currentCourse - 1;
            progressCourse = idxCurrentTab;
            progressStage = currentStage;
            progressDay = currentDay;

            return await showPopup();
        }

        // Events



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private CourseTableLoaderResult courseTable;
        private int idxCurrentTab = 0;
        private int currentStage = 0;
        private int currentDay = 0;
        private int progressCourse = 0;
        private int progressStage = 0;
        private int progressDay = 0;
        private CoursePopupType type;
        private List<JArray> curriculumDatas;

        // Functions
        private void selectTab(int idxTab, int stage = 0, int day = 0)
        {
            this.idxCurrentTab = idxTab;

            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].Activate(i == idxCurrentTab);
                contents[i].Activate(i == idxCurrentTab);
            }
            contents[idxCurrentTab].SelectStage(stage);
            contents[idxCurrentTab].SelectDay(day);
        }

        // Event Handlers
        private void tab_onClick(int idxTab)
        {
            LOG.Function(this, $"{idxTab}");

            var stage = 0;
            var day = 0;
            if (idxTab == progressCourse)
            {
                stage = progressStage;
                day = progressDay;
            }
            selectTab(idxTab, stage, day);
        }
        private void content_onClickStart(int stage, int day)
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;

            currentStage = stage;
            currentDay = day;
            DOVirtual.DelayedCall(buttonDelay, () => CloseWithResult(idxCurrentTab + 1));
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            if (UserData.One.Child.HasSignedIn)
            {
                var courseLearningHistory = UserData.One.Child.DayProgress.Value<JArray>("courseLearningHistory");
                for (int i = 0; i < tabs.Length; i++)
                {
                    var tab = tabs[i];
                    tab.SetComplete(courseLearningHistory[i].Value<bool>("isComplete"));
                }
            }
            selectTab(idxCurrentTab, currentStage, currentDay);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private CoursePopupTab[] tabs = null;
		[SerializeField] private CoursePopupContent[] contents = null;
		[SerializeField] private Button closeBT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            for (int i = 0; i < tabs.Length; i++)
            {
                var tab = tabs[i];
                tab.Init(i);
            }

            closeBT.onClick.AddListener(() => {
                cg.blocksRaycasts = false;
                CloseWithResult(-1);
                });
        }
		private void Start()
		{
		   
		}
        protected void OnEnable()
        {
            tabs.ForEach(tab => tab.OnClick += tab_onClick);
            
            if (type == CoursePopupType.Edit)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    tabs[i].Interactable(i + 1 == UserData.One.Child.Course || !UserData.One.Child.DayLimit || contents[i].IsAllCompleted);
                }
            }
            contents.ForEach(content => content.OnClickStart += content_onClickStart);

            cg.blocksRaycasts = true;
        }
        protected void OnDisable()
        {
            tabs.ForEach(tab => tab.OnClick -= tab_onClick);
            contents.ForEach(content => content.OnClickStart -= content_onClickStart);
        }

        // Unity Coroutine
    }
}