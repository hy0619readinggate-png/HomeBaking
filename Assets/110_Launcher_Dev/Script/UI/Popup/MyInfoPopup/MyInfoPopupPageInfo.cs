using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Launcher.UI
{
	public class MyInfoPopupPageInfo : TabPage
    {
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching
        // Fields

        // Functions
        private async UniTask loadPage()
        {
            await LMS.One.LoadDayProgress();
            var stage = UserData.One.Child.DayProgress.Value<int>("stageId");
            var day = UserData.One.Child.DayProgress.Value<int>("day");

            await coursePopupContentPageEdit.Init(UserData.One.Child.Course, UserData.One.Child.DayProgress);
            coursePopupContentPageEdit.SelectStage(stage);
            coursePopupContentPageEdit.SelectDay(day);
            coursePopupContentPageEdit.SetInteract(false);

            var daysData = await LMS.One.LoadDayCurriculum(stage);
        }

        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CoursePopupContentPageEdit coursePopupContentPageEdit = null;

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

            loadPage().Forget();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}