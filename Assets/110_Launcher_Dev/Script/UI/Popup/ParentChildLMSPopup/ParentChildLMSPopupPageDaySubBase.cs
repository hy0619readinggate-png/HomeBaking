using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageDaySubBase : TabPage
	{
        // Definitions

        // Properties
        public UserDataChild ChildData => parentChildLMSPopupPageDay.ChildData;

        // Methods
        // Events



        // Fields : caching

        // Fields
        protected string loadedDate = "";
        protected string category = "";
        protected CancellationTokenSource cancel = new CancellationTokenSource();

        // Functions
        private async UniTask loadPage()
        {
            LOG.Function(this, $"| loadedDate={loadedDate} | parentChildLMSPopupPageDay.CurrentDate={parentChildLMSPopupPageDay.CurrentDate}");

            //if (loadedDate != parentChildLMSPopupPageDay.CurrentDate)
            //{
            loadedDate = parentChildLMSPopupPageDay.CurrentDate;

            cancel.Cancel();
            cancel = new CancellationTokenSource();
            await onLoad(cancel.Token, loadedDate);

            OnUpdate?.Invoke(this);
            //}
        }
        protected async UniTask checkNew(LMSContentThumbnail thumbnail)
        {
            if (thumbnail.LogSN != 0)
            {
                if (await LMS.One.CheckNew(UserData.One.Child.ID, thumbnail.LogSN))
                    thumbnail.IsNew = false;
            }

            var result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/child-learning/day/{ChildData.ID}/{category}?searchDate={loadedDate}");
            if (result.Success)
            {
                IsNew = result.Data.Value<bool>("isNew");
                OnUpdate?.Invoke(this);
            }
        }

        // Event Handlers
        private void parentChildLMSPopupPageDay_onChangeDay(string date)
        {
            LOG.Function(this, $"| date={date}");

            if (gameObject.activeSelf)
            {
                loadPage().Forget();
            }
        }

        // Overrides
        protected override void onLoad()
        {
            base.onLoad();

            loadPage().Forget();
        }
        protected virtual void onClear() {}
        protected virtual async UniTask onLoad(CancellationToken cancellationToken, string date)
        {
            await UniTask.Yield();
            onClear();
        }



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private ParentChildLMSPopupPageDay parentChildLMSPopupPageDay;

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

            parentChildLMSPopupPageDay.OnChangeDay += parentChildLMSPopupPageDay_onChangeDay;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            parentChildLMSPopupPageDay.OnChangeDay -= parentChildLMSPopupPageDay_onChangeDay;
        }
    }
}