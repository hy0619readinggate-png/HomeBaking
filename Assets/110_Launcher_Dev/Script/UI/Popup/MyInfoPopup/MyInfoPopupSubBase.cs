using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace DoDoEng.Launcher.UI
{
	public class MyInfoPopupSubBase : TabPage
	{
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching

        // Fields
        private string loadedDate = "";
        protected string category = "";
        protected CancellationTokenSource cancel = new CancellationTokenSource();

        // Functions
        private async UniTask loadPage()
        {
            LOG.Function(this, $"| loadedDate={loadedDate} | myInfoPopupPageToday.CurrentDate={myInfoPopupPageToday.CurrentDate}");

            //if (loadedDate != myInfoPopupPageToday.CurrentDate)
            //{
            loadedDate = myInfoPopupPageToday.CurrentDate;

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
                await LMS.One.CheckNew(thumbnail.LogSN);
            }
            thumbnail.IsNew = false;

            var result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/my-info/today/{category}?searchDate={loadedDate}");
            if (result.Success)
            {
                IsNew = result.Data.Value<bool>("isNew");
                OnUpdate?.Invoke(this);
            }
        }

        // Event Handlers
        private void myInfoPopupPageToday_onChangeDay(string date)
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
        [SerializeField] private MyInfoPopupPageToday myInfoPopupPageToday;

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

            myInfoPopupPageToday.OnChangeDay += myInfoPopupPageToday_onChangeDay;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            myInfoPopupPageToday.OnChangeDay -= myInfoPopupPageToday_onChangeDay;
        }
        private void OnDestroy()
        {
            cancel.Cancel();
        }
    }
}