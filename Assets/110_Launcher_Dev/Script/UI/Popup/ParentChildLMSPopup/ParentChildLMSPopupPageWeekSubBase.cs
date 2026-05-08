using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageWeekSubBase : TabPage
	{
        // Definitions

        // Properties
        public UserDataChild ChildData => parentChildLMSPopupPageWeek.ChildData;

        // Methods
        // Events



        // Fields : caching

        // Fields
        private string loadedDate = "";

        // Functions
        private async UniTask loadPage()
        {
            LOG.Function(this, $"| loadedDate={loadedDate} | parentChildLMSPopupPageWeek.CurrentDate={parentChildLMSPopupPageWeek.CurrentDate}");

            //if (loadedDate != parentChildLMSPopupPageWeek.CurrentDate)
            //{
                loadedDate = parentChildLMSPopupPageWeek.CurrentDate;

                await onLoad(loadedDate);
            //}
        }

        // Event Handlers
        private void parentChildLMSPopupPageWeek_onChangeDay(string date)
        {
            LOG.Function(this, $"| date={date}");

            if (gameObject.activeSelf)
            {
                loadPage().Forget();
            }
        }

        // Overrides
        protected virtual void onClear() {}
        protected virtual async UniTask onLoad(string date)
        {
            await UniTask.Yield();
            onClear();
        }



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private ParentChildLMSPopupPageWeek parentChildLMSPopupPageWeek;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            onClear();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            loadPage().Forget();

            parentChildLMSPopupPageWeek.OnChangeDay += parentChildLMSPopupPageWeek_onChangeDay;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            parentChildLMSPopupPageWeek.OnChangeDay -= parentChildLMSPopupPageWeek_onChangeDay;
        }
    }
}