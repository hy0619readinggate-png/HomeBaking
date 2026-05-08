using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageMonthSubBase : TabPage
	{
        // Definitions

        // Properties
        public UserDataChild ChildData => parentChildLMSPopupPageMonth.ChildData;

        // Methods
        // Events



        // Fields : caching

        // Fields
        private string loadedDate = "";

        // Functions
        private async UniTask loadPage()
        {
            LOG.Function(this, $"| loadedDate={loadedDate} | parentChildLMSPopupPageMonth.CurrentDate={parentChildLMSPopupPageMonth.CurrentDate}");

            //if (loadedDate != parentChildLMSPopupPageMonth.CurrentDate)
            //{
                loadedDate = parentChildLMSPopupPageMonth.CurrentDate;

                await onLoad(loadedDate);
            //}
        }

        // Event Handlers
        private void parentChildLMSPopupPageMonth_onChangeDay(string date)
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
        [SerializeField] private ParentChildLMSPopupPageMonth parentChildLMSPopupPageMonth;

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

            parentChildLMSPopupPageMonth.OnChangeDay += parentChildLMSPopupPageMonth_onChangeDay;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            parentChildLMSPopupPageMonth.OnChangeDay -= parentChildLMSPopupPageMonth_onChangeDay;
        }
    }
}