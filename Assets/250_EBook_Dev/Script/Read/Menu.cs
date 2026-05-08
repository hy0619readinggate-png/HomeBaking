using DoDoEng.Common;
using DoDoEng.EBook.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Read
{
    public class Menu : MonoBehaviour
    {
        // Properties
        public bool IsLocked
        {
            get => isLocked;
            private set
            {
                isLocked = value;
                updatePannel();
            }
        }
        public bool IsShown
        {
            get => isShown;
            private set
            {
                isShown = value;
                updatePannel();
            }
        }

        // Methods
        public void Setup(EBookRead ebook)
        {
            LOG.Info($"Setup() | {ebook.gameObject.name} {ebook.IsNativeMode}", this);

            this.ebook = ebook;
            subtitleTGL.IsOn = ebook.IsSubtitle;
            playPauseTGL.IsOn = !ebook.IsPaused;
            autoModeTGL.IsOn = ebook.IsAutoMode;

            recordBTN.gameObject.SetActive(!UserData.One.IsReportContents && !ebook.IsNativeMode);

            titleTXT.text = ebook.Title;
        }
        public void ShowMenu()
        {
            LOG.Info($"ShowMenu()", this);

            if (!IsShown)
            {
                IsShown = true;

                if (crAutoHideMenu != null)
                    StopCoroutine(crAutoHideMenu);
                crAutoHideMenu = StartCoroutine(coAutoHideMenu());
            }
        }
        public void HideMenu()
        {
            LOG.Info($"HideMenu()", this);

            if (IsShown)
            {
                IsShown = false;

                StopCoroutine(crAutoHideMenu);
            }
        }



        // Fields
        private EBookRead ebook = null;
        private bool isLocked = false;
        private bool isShown = false;

        // Fields
        private Coroutine crAutoHideMenu;

        // Functions
        private void updatePannel()
        {
            normalPannelGO.SetActive(!IsLocked && IsShown);
            lockedPannelGO.SetActive(IsLocked && IsShown);
        }

        // Event Handlers
        private void subtitleTGL_OnValueChanged(bool isOn)
        {
            LOG.Info($"subtitleTGL_OnValueChanged() | {isOn}", this);

            ebook.IsSubtitle = isOn;
        }
        private void playPauseTGL_OnValueChanged(bool isOn)
        {
            LOG.Info($"playPauseTGL_OnValueChanged() | {isOn}", this);

            ebook.IsPaused = !isOn;
        }
        private void autoModeTBL_OnValueChanged(bool isOn)
        {
            LOG.Info($"autoModeTBL_OnValueChanged() | {isOn}", this);

            ebook.IsAutoMode = isOn;
        }
        private void speedBTN_OnSpeedChanged(float speed)
        {
            LOG.Info($"speedBTN_OnSpeedChanged() | {speed}", this);

            ebook.PlaybackSpeed = speed;
        }
        private void lockBTN_OnClicked()
        {
            LOG.Info($"lockBTN_OnClicked()", this);

            IsLocked = true;

            SystemUI.One.ShowToastMessage(LocalizationMGR.One.GetText("MESSAGE_16"));
        }
        private void unlockBTN_OnClicked()
        {
            LOG.Info($"unlockBTN_OnClicked()", this);

            IsLocked = false;
            IsShown = false;

            SystemUI.One.ShowToastMessage(LocalizationMGR.One.GetText("MESSAGE_17"));
        }
        private async void recordBTN_OnClicked()
        {
            LOG.Info($"recordBTN_OnClicked()", this);

            var result = await UIEBookCommon.One.ConfirmRerecordPopup.ShowPopup();
            if (result == SimplePopupResult.Yes)
            {
                ebook.Halt();

                var currentIDX = RunnerParam.SelectedIDX as EBookSingleIndex;
                var nextIDX = new EBookRecordIndex(currentIDX.Index);

                RunnerParam.SelectedIDX = nextIDX;
                SceneLoader.One.LoadScene(nextIDX.SceneName);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject normalPannelGO = null;
        [SerializeField] private GameObject lockedPannelGO = null;
        [SerializeField] private Button hideBTN = null;
        [SerializeField] private Button lockBTN = null;
        [SerializeField] private Button unlockBTN = null;
        [SerializeField] private Button recordBTN = null;
        [SerializeField] private ToggleButton subtitleTGL = null;
        [SerializeField] private ToggleButton playPauseTGL = null;
        [SerializeField] private ToggleButton autoModeTGL = null;
        [SerializeField] private SpeedButton speedBTN = null;
        [SerializeField] private TextMeshProUGUI titleTXT = null;
        [Header("★ Config")]
        [SerializeField] private float autoHideDuration = 3;

        // Unity Messages
        private void Awake()
        {
            normalPannelGO.SetActive(false);
            lockedPannelGO.SetActive(false);
            IsShown = false;

            hideBTN.onClick.AddListener(() => HideMenu());
            lockBTN.onClick.AddListener(() => lockBTN_OnClicked());
            unlockBTN.onClick.AddListener(() => unlockBTN_OnClicked());
            recordBTN.onClick.AddListener(() => recordBTN_OnClicked());
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            subtitleTGL.OnValueChanged += subtitleTGL_OnValueChanged;
            playPauseTGL.OnValueChanged += playPauseTGL_OnValueChanged;
            autoModeTGL.OnValueChanged += autoModeTBL_OnValueChanged;
            speedBTN.OnSpeedChanged += speedBTN_OnSpeedChanged;
        }
        private void OnDisable()
        {
            subtitleTGL.OnValueChanged -= subtitleTGL_OnValueChanged;
            playPauseTGL.OnValueChanged -= playPauseTGL_OnValueChanged;
            autoModeTGL.OnValueChanged -= autoModeTBL_OnValueChanged;
            speedBTN.OnSpeedChanged -= speedBTN_OnSpeedChanged;
        }

        // Unity Coroutine
        IEnumerator coAutoHideMenu()
        {
            var timeout = autoHideDuration;
            while (timeout > 0)
            {
                yield return null;

                timeout -= Time.deltaTime;

                if (Input.GetMouseButtonDown(0))
                    timeout = autoHideDuration;
            }

            HideMenu();
        }
    }
}