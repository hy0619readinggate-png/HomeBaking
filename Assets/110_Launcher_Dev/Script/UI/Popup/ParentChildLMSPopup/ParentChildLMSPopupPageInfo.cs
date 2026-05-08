using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageInfo : ParentChildLMSPopupPageBase
    {
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching
        // Fields
        // Functions
        private async UniTask updateContents()
        {
            await coursePopupContentPageEdit.Init(ChildData.Course, ChildData.DayProgress);
            coursePopupContentPageEdit.SelectStage(ChildData.DayProgress.Value<int>("stageId"));
            coursePopupContentPageEdit.SelectDay(ChildData.DayProgress.Value<int>("day"));
            coursePopupContentPageEdit.SetInteract(false);
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

            updateContents().Forget();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}