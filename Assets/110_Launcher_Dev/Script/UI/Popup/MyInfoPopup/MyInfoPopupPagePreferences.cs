using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class MyInfoPopupPagePreferences : TabPage
    {
        // Events
        [HideInInspector] public event Action OnUpdateApp;
        [HideInInspector] public event Action OnSignOut;
        [HideInInspector] public event Action OnRestart;
        [HideInInspector] public event Action OnInit;
        [HideInInspector] public event Action OnChangeSettings;
        [HideInInspector] public event Action OnAlarm;



        // Functions
        private void initLanguageToggles()
        {
            if (LocalizationMGR.One.Locale == LocalizationLocale.ko)
                languageKorTGL.SetIsOnWithoutNotify(true);
            else if (LocalizationMGR.One.Locale == LocalizationLocale.en)
                languageEngTGL.SetIsOnWithoutNotify(true);
            else if (LocalizationMGR.One.Locale == LocalizationLocale.vi)
                languageVieTGL.SetIsOnWithoutNotify(true);
        }
        private async UniTask changeLanguage(LocalizationLocale locale)
        {
            if (LocalizationMGR.One.Locale != locale)
            {
                if (await SystemUI.One.ShowPopupChangeLanguage())
                {
                    UserData.One.SaveLanguage(locale);
                    OnChangeSettings?.Invoke();
                    OnRestart?.Invoke();
                }
                else
                {
                    initLanguageToggles();
                }
            }
        }
        private async UniTask loadData()
        {
            var data = await LMS.One.LoadTicketInfo();
            if (data.Value<bool>("available"))
            {
                var remainingDays = data.Value<int>("remainingDays") + 1;
                voucherTXT.text = LocalizationMGR.One.GetText("WORD_138", remainingDays);
            }
        }

        // Event Handlers
        private void autoLoginOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Child.AutoSignIn = activate;
            UserData.One.SaveLocalData();
        }
        private void marketingOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Child.IsAppNotificationMarketing = activate;
            OnChangeSettings?.Invoke();
        }
        private void learningOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Child.IsAppNotificationLearning = activate;
            OnChangeSettings?.Invoke();
        }
        private void sfxOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Child.IsAppSoundEffect = activate;
            OnChangeSettings?.Invoke();
        }
        private void bgmOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Child.IsAppSoundBackground = activate;
            AudioMGR.One.BgmVolume = activate ? 1 : 0;
            OnChangeSettings?.Invoke();
        }
        private async UniTask initBT_OnClick()
        {
            if (await SystemUI.One.ShowPopupInit())
            {
                OnInit?.Invoke();
            }
        }



        // Unity Inspectors
        [Header("★ Bindings - Version")]
        [SerializeField] private TextMeshProUGUI currentVersionTXT = null;
        [SerializeField] private TextMeshProUGUI latestVersionTXT = null;
        [SerializeField] private Button updateBT = null;
        [Header("★ Bindings - Parent")]
        [SerializeField] private TextMeshProUGUI parentTXT = null;
        [SerializeField] private TextMeshProUGUI voucherTXT = null;
        [SerializeField] private GameObject[] snsIcons = null;
        [Header("★ Bindings - Account")]
        [SerializeField] private OnOffToggle autoLoginOnOff = null;
        [SerializeField] private Button signOutBT = null;
        [Header("★ Bindings - Lanugage")]
        [SerializeField] private Toggle languageKorTGL = null;
        [SerializeField] private Toggle languageEngTGL = null;
        [SerializeField] private Toggle languageVieTGL = null;
        [Header("★ Bindings - Init")]
        [SerializeField] private Button initBT = null;
        [Header("★ Bindings - alarm")]
        [SerializeField] private GameObject alarmGO = null;
        [SerializeField] private Button alarmBT = null;
        [SerializeField] private OnOffToggle marketingOnOff = null;
        [SerializeField] private OnOffToggle learningOnOff = null;
        [Header("★ Bindings - Sound")]
        [SerializeField] private OnOffToggle sfxOnOff = null;
        [SerializeField] private OnOffToggle bgmOnOff = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            // Version
            currentVersionTXT.text = Application.version;
            updateBT.onClick.AddListener(() => OnUpdateApp?.Invoke());

            // Account
            signOutBT.onClick.AddListener(() => OnSignOut?.Invoke());

            // Init
            initBT.onClick.AddListener(() => initBT_OnClick().Forget());

            snsIcons.ForEach(icon => icon.SetActive(false));

            // Language
            languageKorTGL.onValueChanged.AddListener(value =>
            {
                if (value) changeLanguage(LocalizationLocale.ko).Forget();
            });
            languageEngTGL.onValueChanged.AddListener(value =>
            {
                if (value) changeLanguage(LocalizationLocale.en).Forget();
            });
            languageVieTGL.onValueChanged.AddListener(value =>
            {
                if (value) changeLanguage(LocalizationLocale.vi).Forget();
            });

            alarmBT.onClick.AddListener(() => OnAlarm?.Invoke());
//#if UNITY_IOS
//            alarmGO.SetActive(false);
//#endif
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            // Version
            latestVersionTXT.text = ProductVersion.One.Version;
            var currentVersions = Application.version.Split(".");
            var releaseVersions = ProductVersion.One.Version.Split(".");
            bool isNeedUpdate = false;
            for (int i = 0; i < releaseVersions.Length; i++)
            {
                if (currentVersions.Length >= i + 1)
                {
                    if (int.TryParse(currentVersions[i], out int currentVersion) && int.TryParse(releaseVersions[i], out int releaseVersion))
                    {
                        if (currentVersion < releaseVersion)
                        {
                            isNeedUpdate = true;
                            break;
                        }
                    }
                }
                else
                {
                    isNeedUpdate = true;
                    break;
                }
            }
            updateBT.interactable = isNeedUpdate;

            // Parent
            parentTXT.text = UserData.One.Child.ParentID;
            voucherTXT.text = LocalizationMGR.One.GetText("WORD_138", 0);
            snsIcons.ForEach(icon => icon.SetActive(false));
            if (UserData.One.Child.ParentSignupType == "EMAIL")
                snsIcons[0].SetActive(true);
            else if (UserData.One.Child.ParentSignupType == "NAVER")
                snsIcons[1].SetActive(true);
            else if (UserData.One.Child.ParentSignupType == "KAKAO")
                snsIcons[2].SetActive(true);
            else if (UserData.One.Child.ParentSignupType == "GOOGLE")
                snsIcons[3].SetActive(true);
            else if (UserData.One.Child.ParentSignupType == "APPLE")
                snsIcons[4].SetActive(true);
            loadData().Forget();

            initLanguageToggles();

            // Account
            autoLoginOnOff.ActivateImmediatly(UserData.One.Child.AutoSignIn);
            autoLoginOnOff.OnActivated += autoLoginOnOff_OnActivated;

            // Alarm
            marketingOnOff.ActivateImmediatly(UserData.One.Child.IsAppNotificationMarketing);
            learningOnOff.ActivateImmediatly(UserData.One.Child.IsAppNotificationLearning);
            marketingOnOff.OnActivated += marketingOnOff_OnActivated;
            learningOnOff.OnActivated += learningOnOff_OnActivated;

            // Sound
            sfxOnOff.ActivateImmediatly(UserData.One.Child.IsAppSoundEffect);
            bgmOnOff.ActivateImmediatly(UserData.One.Child.IsAppSoundBackground);
            sfxOnOff.OnActivated += sfxOnOff_OnActivated;
            bgmOnOff.OnActivated += bgmOnOff_OnActivated;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            // Account
            autoLoginOnOff.OnActivated -= autoLoginOnOff_OnActivated;

            // Alarm
            marketingOnOff.OnActivated -= marketingOnOff_OnActivated;
            learningOnOff.OnActivated -= learningOnOff_OnActivated;

            // Sound
            sfxOnOff.OnActivated -= sfxOnOff_OnActivated;
            bgmOnOff.OnActivated -= bgmOnOff_OnActivated;
        }
    }
}