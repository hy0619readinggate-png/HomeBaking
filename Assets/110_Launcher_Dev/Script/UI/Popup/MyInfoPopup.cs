using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
    public class MyInfoPopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties

        // Methods
        public void Init(RewardTableLoaderResult rewardTable)
        {
            coinPage.Init(rewardTable);
        }
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            await LMS.One.LoadDayProgress();

            nickTMP.text = UserData.One.Child.NickName;
            photo.SetPhoto(UserData.One.Child.Photo);
            tabControl.Select(0);

            return await showPopup();
        }
        public async UniTask ShowPopupJump()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            await LMS.One.LoadDayProgress();

            nickTMP.text = UserData.One.Child.NickName;
            tabControl.Select(0);
            todayPage.Jump(UserData.One.LastReportPage, UserData.One.LastReportDate);

            showPopup().Forget();
        }
        //public void StopLoading()
        //{
        //    todayPage.StopLoading();
        //}

        // Events
        [HideInInspector] public event Action OnUpdate;
        [HideInInspector] public event Action OnSignOut;
        [HideInInspector] public event Action OnChangeSettings;
        [HideInInspector] public event Action OnRestart;
        [HideInInspector] public event Action OnInit;
        [HideInInspector] public event Action OnAlarm;
        [HideInInspector] public event Action OnChangePhoto;



        // Fields : caching
        // Fields
        // Functions

        // Event Handlers
        private void photo_OnChangePhoto(Texture2D photo)
        {
            LMS.One.SavePhotoChild(photo).Forget();
            OnChangePhoto?.Invoke();
        }
        private void preferencesPage_onUpdate()
        {
            LOG.Function(this);

            OnUpdate?.Invoke();
        }
        private void preferencesPage_onSignOut()
        {
            LOG.Function(this);

            OnSignOut?.Invoke();
        }
        private void preferencesPage_onChangeSettings()
        {
            OnChangeSettings?.Invoke();
        }
        private void preferencesPage_onRestart()
        {
            OnRestart?.Invoke();
        }
        private void preferencesPage_onInit()
        {
            OnInit?.Invoke();
        }
        private void preferencesPage_onAlarm()
        {
            LOG.Function(this);

            OnAlarm?.Invoke();
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text nickTMP = null;
        [SerializeField] private ProfilePhoto photo = null;
        [SerializeField] private Button signOutBT = null;
        [SerializeField] private TabControl tabControl = null;
        [SerializeField] private MyInfoPopupPageCoin coinPage = null;
        [SerializeField] private MyInfoPopupPageToday todayPage = null;
        [SerializeField] private MyInfoPopupPagePreferences preferencesPage = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            signOutBT.onClick.AddListener(() => OnSignOut?.Invoke());
        }
		private void Start()
		{
        }
        protected void OnEnable()
        {
            photo.OnChangePhoto += photo_OnChangePhoto;

            preferencesPage.OnUpdateApp += preferencesPage_onUpdate;
            preferencesPage.OnSignOut += preferencesPage_onSignOut;
            preferencesPage.OnChangeSettings += preferencesPage_onChangeSettings;
            preferencesPage.OnRestart += preferencesPage_onRestart;
            preferencesPage.OnInit+= preferencesPage_onInit;
            preferencesPage.OnAlarm+= preferencesPage_onAlarm;
        }
        protected void OnDisable()
        {
            photo.OnChangePhoto -= photo_OnChangePhoto;

            preferencesPage.OnUpdateApp -= preferencesPage_onUpdate;
            preferencesPage.OnSignOut -= preferencesPage_onSignOut;
            preferencesPage.OnChangeSettings -= preferencesPage_onChangeSettings;
            preferencesPage.OnRestart -= preferencesPage_onRestart;
            preferencesPage.OnInit -= preferencesPage_onInit;
            preferencesPage.OnAlarm -= preferencesPage_onAlarm;
        }

        // Unity Coroutine
    }
}