using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class ParentSettingPopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            return await showPopup();
        }

        // Events
        [HideInInspector] public event Action OnUpdate;
        [HideInInspector] public event Action OnInfoEdit;
        [HideInInspector] public event Action OnSignOut;
        [HideInInspector] public event Action OnUnregister;
        [HideInInspector] public event Action OnRestart;
        [HideInInspector] public event Action OnInit;
        [HideInInspector] public event Action OnChangeSettings;
        [HideInInspector] public event Action OnAlarm;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

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

        // Event Handlers
        private async void unregisterBT_OnClick()
        {
            LOG.Function(this);

            if (await SystemUI.One.ShowPopupUnregister())
            {
                OnUnregister?.Invoke();
            }
        }
        private void autoLoginOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Parent.AutoSignIn = activate;
            OnChangeSettings?.Invoke();
        }
        private void marketingOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Parent.IsAppNotificationMarketing = activate;
            OnChangeSettings?.Invoke();
        }
        private void learningOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Parent.IsAppNotificationLearning = activate;
            OnChangeSettings?.Invoke();
        }
        private void smsOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Parent.IsSmsReceive = activate;
            OnChangeSettings?.Invoke();
        }
        private void sfxOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Parent.IsAppSoundEffect = activate;
            OnChangeSettings?.Invoke();
        }
        private void bgmOnOff_OnActivated(bool activate)
        {
            LOG.Function(this, $"{activate}");

            UserData.One.Parent.IsAppSoundBackground = activate;
            OnChangeSettings?.Invoke();
        }
        private async UniTask initBT_OnClick()
        {
            if (await SystemUI.One.ShowPopupInit())
            {
                OnInit?.Invoke();
            }
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            nickTMP.text = UserData.One.Parent.NickName;
            createdDateTMP.text = UserData.One.Parent.CreatedDateTime.ToString("yyyy.MM.dd");
            childrenCountTMP.text = LocalizationMGR.One.GetText("WORD_126", UserData.One.Parent.ChildDatas.Sum(data => data.ParentID == UserData.One.Parent.LoginID ? 1 : 0));
            if (UserData.One.Parent.VoucherAvailable)
            {
                voucherDaysTMP.text = LocalizationMGR.One.GetText("WORD_98");
                voucherExpireTMP.text = LocalizationMGR.One.GetText("WORD_99", UserData.One.Parent.VoucherEndDate.ToString("yyyy.MM.dd"));
            }
            else
            {
                voucherDaysTMP.text = LocalizationMGR.One.GetText("WORD_100");
                voucherExpireTMP.text = "";
            }
            initLanguageToggles();
        }



        // Unity Inspectors
        [Header("★ Bindings - Left")]
        [SerializeField] private TMP_Text nickTMP = null;
        [SerializeField] private TMP_Text createdDateTMP = null;
        [SerializeField] private TMP_Text childrenCountTMP = null;
        [SerializeField] private TMP_Text voucherDaysTMP = null;
        [SerializeField] private TMP_Text voucherExpireTMP = null;
        [Header("★ Bindings - Version")]
        [SerializeField] private TextMeshProUGUI currentVersionTXT = null;
        [SerializeField] private TextMeshProUGUI latestVersionTXT = null;
        [SerializeField] private Button updateBT = null;
        [Header("★ Bindings - Account")]
        [SerializeField] private OnOffToggle autoLoginOnOff = null;
        [SerializeField] private Button infoEditBT = null;
        [SerializeField] private Button signOutBT = null;
        [Header("★ Bindings - Language")]
        [SerializeField] private Toggle languageKorTGL = null;
        [SerializeField] private Toggle languageEngTGL = null;
        [SerializeField] private Toggle languageVieTGL = null;
        [SerializeField] private Button unregisterBT = null;
        [Header("★ Bindings - Init")]
        [SerializeField] private Button initBT = null;
        [Header("★ Bindings - alarm")]
        [SerializeField] private GameObject alarmGO = null;
        [SerializeField] private Button alarmBT = null;
        [SerializeField] private OnOffToggle marketingOnOff = null;
        [SerializeField] private OnOffToggle learningOnOff = null;
        [Header("★ Bindings - SMS")]
        [SerializeField] private OnOffToggle smsOnOff = null;
        [Header("★ Bindings - Sound")]
        [SerializeField] private OnOffToggle sfxOnOff = null;
        [SerializeField] private OnOffToggle bgmOnOff = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;

        // Unity Messages
        private void Awake()
        {
            // Version
            currentVersionTXT.text = Application.version;
            updateBT.onClick.AddListener(() => OnUpdate?.Invoke());

            // Account
            infoEditBT.onClick.AddListener(() => OnInfoEdit?.Invoke());
            signOutBT.onClick.AddListener(() => OnSignOut?.Invoke());
            unregisterBT.onClick.AddListener(unregisterBT_OnClick);

            // Init
            initBT.onClick.AddListener(() => initBT_OnClick().Forget());

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
        private void Start()
        {
        }
        protected void OnEnable()
        {
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

            // Account
            autoLoginOnOff.ActivateImmediatly(UserData.One.Parent.AutoSignIn);
            autoLoginOnOff.OnActivated += autoLoginOnOff_OnActivated;

            // Alarm
            marketingOnOff.ActivateImmediatly(UserData.One.Parent.IsAppNotificationMarketing);
            learningOnOff.ActivateImmediatly(UserData.One.Parent.IsAppNotificationLearning);
            marketingOnOff.OnActivated += marketingOnOff_OnActivated;
            learningOnOff.OnActivated += learningOnOff_OnActivated;

            // SMS
            smsOnOff.ActivateImmediatly(UserData.One.Parent.IsSmsReceive);
            smsOnOff.OnActivated += smsOnOff_OnActivated;

            // Sound
            sfxOnOff.ActivateImmediatly(UserData.One.Parent.IsAppSoundEffect);
            bgmOnOff.ActivateImmediatly(UserData.One.Parent.IsAppSoundBackground);
            sfxOnOff.OnActivated += sfxOnOff_OnActivated;
            bgmOnOff.OnActivated += bgmOnOff_OnActivated;
        }
        protected void OnDisable()
        {
            // Account
            autoLoginOnOff.OnActivated -= autoLoginOnOff_OnActivated;

            // Alarm
            marketingOnOff.OnActivated -= marketingOnOff_OnActivated;
            learningOnOff.OnActivated -= learningOnOff_OnActivated;

            // SMS
            smsOnOff.OnActivated -= smsOnOff_OnActivated;

            // Sound
            sfxOnOff.OnActivated -= sfxOnOff_OnActivated;
            bgmOnOff.OnActivated -= bgmOnOff_OnActivated;
        }
    }
}