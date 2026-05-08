using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.PlayAll
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
        public void Setup(EBookPlayAll ebook, MenuItemData[] datas)
        {
            LOG.Info($"Setup() | {ebook.gameObject.name}", this);

            this.ebook = ebook;

            setupItems(datas);
        }
        public void SetCurrentBook(int id)
        {
            LOG.Function(this);

            subtitleTGL.IsOn = ebook.IsSubtitle;
            autoModeTGL.IsOn = ebook.IsAutoMode;
            speedBTN.Reset();

            titleTXT.text = ebook.Title;

            for (var i = 0; i < menuItems.Length; i++)
                menuItems[i].SetCurrent(id == i + 1, !ebook.IsPaused);

            var targetPos = (float)(id - 1) / (menuItems.Length - 1);
            var currenPos = scroll.horizontalNormalizedPosition;

            if (Mathf.Abs(targetPos - currenPos) > 0.1f)
            {
                if (!IsShown)
                    scroll.horizontalNormalizedPosition = targetPos;
                else
                {
                    disableAutoHide = true;
                    scroll
                        .DOHorizontalNormalizedPos(targetPos, scrollDuration)
                        .OnComplete(() =>
                        {
                            disableAutoHide = false;
                        });
                }
            }


        }
        public void ShowMenu()
        {
            LOG.Info($"ShowMenu()", this);

            if (!IsShown)
            {
                IsShown = true;
                ebook.UpdateNavigationControl();

                this.StopCoroutineSafe(ref crAutoHideMenu);
                crAutoHideMenu = StartCoroutine(coAutoHideMenu());
            }

            if (!IsLocked)
                ebook.MoveBookTo(bookParentTR);


        }
        public void HideMenu()
        {
            LOG.Info($"HideMenu()", this);

            ebook.MoveBookToOrigin();

            if (IsShown)
            {
                IsShown = false;
                ebook.UpdateNavigationControl();

                this.StopCoroutineSafe(ref crAutoHideMenu);
            }
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private EBookPlayAll ebook = null;
        private bool isLocked = false;
        private bool isShown = false;
        private bool disableAutoHide = false;
        private MenuItem[] menuItems = null;

        // Fields
        private Coroutine crAutoHideMenu;

        // Functions
        private void updatePannel()
        {
            normalPannelGO.SetActive(!IsLocked && IsShown);
            lockedPannelGO.SetActive(IsLocked && IsShown);
        }
        private void setupItems(MenuItemData[] datas)
        {
            Util.RemoveAllChildren(itemParentTR);

            var list = new List<MenuItem>();
            foreach (var data in datas)
            {
                var go = Instantiate(itemPB, itemParentTR);
                var menuItem = go.GetComponent<MenuItem>();
                menuItem.Setup(data);
                menuItem.OnPlayPauseChanged += menuItem_OnPlayPauseChanged;
                menuItem.OnSelected += menuItem_OnSelected;
                list.Add(menuItem);
            }
            menuItems = list.ToArray();
        }



        // Event Handlers
        private void subtitleTGL_OnValueChanged(bool isOn)
        {
            LOG.Info($"subtitleTGL_OnValueChanged() | {isOn}", this);

            ebook.IsSubtitle = isOn;
        }
        private void menuItem_OnPlayPauseChanged(bool isOn)
        {
            LOG.Info($"menuItem_OnPlayPauseChanged() | {isOn}", this);

            ebook.IsPaused = !isOn;
        }
        private void menuItem_OnSelected(int id)
        {
            LOG.Info($"menuItem_OnSelected() | {id}", this);

            disableAutoHide = true;

            var pos = (float)(id - 1) / (menuItems.Length - 1);
            scroll
                .DOHorizontalNormalizedPos(pos, scrollDuration)
                .OnComplete(() =>
                {
                    disableAutoHide = false;

                    ebook.ChangeBook(id);
                });
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

            ebook.MoveBookToOrigin();
            ebook.UpdateNavigationControl();

            SystemUI.One.ShowToastMessage(LocalizationMGR.One.GetText("MESSAGE_16"));
        }
        private void unlockBTN_OnClicked()
        {
            LOG.Info($"unlockBTN_OnClicked()", this);

            IsLocked = false;
            IsShown = false;

            SystemUI.One.ShowToastMessage(LocalizationMGR.One.GetText("MESSAGE_17"));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject normalPannelGO = null;
        [SerializeField] private GameObject lockedPannelGO = null;
        [SerializeField] private Button hideBTN = null;
        [SerializeField] private Button lockBTN = null;
        [SerializeField] private Button unlockBTN = null;
        [SerializeField] private ToggleButton subtitleTGL = null;
        [SerializeField] private ToggleButton autoModeTGL = null;
        [SerializeField] private SpeedButton speedBTN = null;
        [SerializeField] private TextMeshProUGUI titleTXT = null;
        [SerializeField] private Transform bookParentTR = null;
        [SerializeField] private Transform itemParentTR = null;
        [SerializeField] private MenuItem itemPB = null;
        [SerializeField] private ScrollRect scroll = null;
        [Header("★ Config")]
        [SerializeField] private float autoHideDuration = 3;
        [SerializeField] private float scrollDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            normalPannelGO.SetActive(false);
            lockedPannelGO.SetActive(false);
            IsShown = false;

            hideBTN.onClick.AddListener(() => HideMenu());
            lockBTN.onClick.AddListener(() => lockBTN_OnClicked());
            unlockBTN.onClick.AddListener(() => unlockBTN_OnClicked());
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            subtitleTGL.OnValueChanged += subtitleTGL_OnValueChanged;
            autoModeTGL.OnValueChanged += autoModeTBL_OnValueChanged;
            speedBTN.OnSpeedChanged += speedBTN_OnSpeedChanged;
        }
        private void OnDisable()
        {
            subtitleTGL.OnValueChanged -= subtitleTGL_OnValueChanged;
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

                if (!disableAutoHide)
                    timeout -= Time.deltaTime;

                if (Input.GetMouseButtonDown(0))
                    timeout = autoHideDuration;
            }

            HideMenu();
        }
    }

    public class MenuItemData
    {
        public int ID;
        public Sprite ThumnailSPR;
    }

}