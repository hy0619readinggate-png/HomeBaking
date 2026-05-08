using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DoDoEng.Launcher.UI
{
	public class CoursePopupContent : MonoBehaviour
	{
        // Definitions
        // Properties
        public int Stage => stage;
        public int Day => day;
        public bool IsAllCompleted => currentPage.IsAllCompleted;

        // Methods
        public async UniTask Init(CoursePopupType type, CourseTableLoaderResult courseTable, int idxCourse, List<JArray> curriculumDatas = null)
        {
            newPage.Activate(type == CoursePopupType.New);
            editPage.Activate(type == CoursePopupType.Edit);

            if (type == CoursePopupType.New)
                currentPage = newPage;
            else
                currentPage = editPage;

            var data = courseTable.CourseList[idxCourse];
            await currentPage.Init(data.Course, UserData.One.Child.DayProgress, curriculumDatas);

            courseInfoTMP.text = LocalizationMGR.One.Select(data.InfoKor, data.InfoEng, data.InfoVie);
        }
        public void Activate(bool active)
        {
            LOG.Function(this, $"{active}");

            gameObject.SetActive(active);
        }
        public void SelectStage(int stage)
        {
            LOG.Function(this, $"| stage={stage}");

            currentPage.SelectStage(stage);
        }
        public void SelectDay(int day)
        {
            LOG.Function(this, $"| day={day}");

            if (currentPage == editPage)
                currentPage.SelectDay(day);
        }

        // Events
        public Action<int, int> OnClickStart;



        // Fields : caching

        // Fields
        private int stage = 1;
        private int day = 1;
        private CoursePopupContentPageEdit currentPage;

        // Functions

        // Event Handlers
        private void page_onClickStart(int stage, int day)
        {
            OnClickStart?.Invoke(stage, day);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private TMP_Text courseInfoTMP = null;
        [SerializeField] private CoursePopupContentPageEdit newPage = null;
        [SerializeField] private CoursePopupContentPageEdit editPage = null;
        [SerializeField] private ScrollRect thumbnailScroll = null;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
		}
        private void OnEnable()
        {
            newPage.OnClickStart += page_onClickStart;
            editPage.OnClickStart += page_onClickStart;

            thumbnailScroll.horizontalNormalizedPosition = 0;
        }
        private void OnDisable()
        {
            newPage.OnClickStart -= page_onClickStart;
            editPage.OnClickStart -= page_onClickStart;
        }

        // Unity Coroutine
    }
}