using beyondi.Behaviour;
using TMPro;
using UnityEngine;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Common
{
    public class SettingData : BYDSingleton<SettingData>
    {
        // Definitions
        private static string KEY_AUTO_LOGIN = "KEY_AUTO_LOGIN";
        private static string KEY_SFX_ON = "KEY_SFX_ON";
        private static string KEY_BGM_ON = "KEY_BGM_ON";

        // Properties
        public bool IsAutoLoginOn
        {
            get => isAutoLoginOn;
            set
            {
                if (isAutoLoginOn == value)
                    return;

                isAutoLoginOn = value;

                PlayerPrefs.SetInt(KEY_AUTO_LOGIN, isAutoLoginOn ? 0 : 1);
                PlayerPrefs.Save();
            }
        }
        public bool IsSFXOn
        {
            get => isSFXOn;
            set
            {
                if (isSFXOn == value)
                    return;

                isSFXOn = value;

                AudioMGR.One.SfxVolume = isSFXOn ? 1 : 0;
                PlayerPrefs.SetInt(KEY_SFX_ON, isSFXOn ? 0 : 1);
                PlayerPrefs.Save();
            }
        }
        public bool IsBGMOn
        {
            get => isBGMOn;
            set
            {
                if (isBGMOn == value)
                    return;

                isBGMOn = value;

                AudioMGR.One.BgmVolume = isBGMOn ? 1 : 0;
                PlayerPrefs.SetInt(KEY_BGM_ON, isBGMOn ? 0 : 1);
                PlayerPrefs.Save();
            }
        }
        // Methods
        // Events



        // Fields : caching
        // Fields
        private bool isAutoLoginOn = true;
        private bool isSFXOn = true;
        private bool isBGMOn = true;
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI labelTXT = null;
        [Header("★ Config")]
        [SerializeField] private int intValue = 0;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            // 값설정이 되어 있지 않은 경우 0, 0 : tre, 1 : false
            isAutoLoginOn = PlayerPrefs.GetInt(KEY_AUTO_LOGIN) == 0;
            isSFXOn = PlayerPrefs.GetInt(KEY_SFX_ON) == 0;
            isBGMOn = PlayerPrefs.GetInt(KEY_BGM_ON) == 0;

            AudioMGR.One.SfxVolume = isSFXOn ? 1 : 0;
            AudioMGR.One.BgmVolume = isBGMOn ? 1 : 0;
        }
        private void Start()
        {

        }

        // Unity Coroutine
    }
}