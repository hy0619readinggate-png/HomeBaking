using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SRDebugger;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using DoDoEng.Behaviour;
using System;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Collections.Generic;

namespace DoDoEng.Library.Framework
{
    [RequireComponent(typeof(LibraryRunner))]
    [RequireComponent(typeof(LibraryTester))]
    public class LibraryBase : MonoBehaviour
    {
        // Definitions
        public enum PageType
        {
            Category,
            Favorite,
            MyRecord
        }

        // Properties

        // Methods
        public async UniTask Prepare()
        {
            LOG.Info($"Prepare()", this);

            await onPrepare();
        }
        public void StartLibrary()
        {
            LOG.Info($"StartLibrary()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            DOVirtual.DelayedCall(startDelay, () => onStartLibrary());
        }
        public void FinishLibrary()
        {
            LOG.Info($"FinishLibrary()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishLibrary();
        }
        public bool Back()
        {
            LOG.Info($"Back()", this);

            return onBack();
        }



        // Virtual
        protected virtual async void onInitLibrary()
        {
            UserData.One.LoadLanguage();

            await UniTask.Yield();

            canvasGroup.blocksRaycasts = true;
            if (playAllBTN)
            {
                playAllBTN.onClick.AddListener(() => playAllBTN_onClick());
                playAllBTN.gameObject.SetActive(true);
            }
            titleTMP.text = string.Empty;
            explanationTMP.text = string.Empty;
            if (subCategory) subCategory.Hide();
            //selectedCategory.SetActive(false);
            //selectedCategoryButton.Enabled = false;

            if (favoriteTGL)
                favoriteTGL.onValueChanged.AddListener((value) => favoriteBTN_onValueChanged(value));
            if (myRecordTGL)
                myRecordTGL.onValueChanged.AddListener((value) => myRecordBTN_onValueChanged(value));

            removeAllSlots();
            slotPF.gameObject.SetActive(false);

            foreach (var ch in categoryRT.GetChildren())
            {
                if (ch != categoryPF.GetComponent<RectTransform>())
                    GameObject.Destroy(ch.gameObject);
            }
            categoryPF.gameObject.SetActive(false);
        }
        protected virtual async UniTask onPrepare()
        {
            //LOG.Function(this, $"{UserData.One.LastLibraryPageType}, {UserData.One.LastLibraryMainCategory}, {UserData.One.LastLibrarySubCategory}");
            //await LMS.One.LoadLibraryData();
            await onPrepareCatetory();
            if (UserData.One.LastLibraryMainCategory < categoryButtons.Count)
                categoryButtons[UserData.One.LastLibraryMainCategory].Select();
            //await DataDownloader.One.DownloadData("Thumbnail/DataDefinitionSO.asset");
            await loadPage(UserData.One.LastLibraryPageType, UserData.One.LastLibraryMainCategory, UserData.One.LastLibrarySubCategory);
            mainScroll.verticalScrollbar.value = UserData.One.LastLibraryMainScroll;
            if (subScroll)
                subScroll.horizontalScrollbar.value = UserData.One.LastLibrarySubScroll;
            pageScroll.verticalScrollbar.value = UserData.One.LastLibraryPageScroll;
            UserData.One.LastLibraryMainScroll = 1;
            UserData.One.LastLibrarySubScroll = 0;
            UserData.One.LastLibraryPageScroll = 1;
            prepared = true;
        }
        protected virtual async UniTask onPrepareCatetory()
        {
            await UniTask.Yield();
        }
        protected virtual void onStartLibrary()
        {
        }
        protected virtual void onFinishLibrary() { }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            //var sort = 400;

            //srOptionContainer.AddOption(
            //        OptionDefinition.FromMethod("Clear Data", () =>
            //        {
            //            LMS.One.ClearPlaygroundScores();
            //            this.onStartLibrary();
            //        }, "LMS", ++sort));
            //if (GetType().IsOverride("onDebugNext"))
            //{
            //    srOptionContainer.AddOption(
            //        OptionDefinition.FromMethod("Next(F1)", () =>
            //        {
            //            SystemEventManager.SystemButtonClick(SystemButtonType.Debug_Next);
            //        }, "Activity", ++sort));
            //}
        }
#endif
        protected virtual async UniTask onLoadPage(CancellationToken cancellationToken, PageType type, int idxMainCategory = 0, int idxSubCategory = 0)
        {
            LOG.Function(this, $"{cancellationToken} | {type} | {idxMainCategory} | {idxSubCategory}");

            //selectedCategory.SetActive(type == PageType.Category);

            if (subCategory) subCategory.Hide();

            if (type == PageType.Category)
            {
                //selectedCategoryButton.Init(categoryButtons[idxMainCategory]);
            }
            else if (type == PageType.Favorite)
            {
                titleTMP.text = "My Favorites";
                explanationTMP.text = string.Empty;
            }
            else if (type == PageType.MyRecord)
            {
                explanationTMP.text = string.Empty;
            }

            removeAllSlots();

            await UniTask.Delay(0);
        }
        protected virtual async UniTask onClickSlot(LibrarySlot slot)
        {
            await UniTask.Delay(0);
        }
        protected virtual bool onBack()
        {
            return true;
        }
        protected virtual void onPlayAll() { }
        protected virtual async UniTask<bool> onDeleteSlot(LibrarySlot slot)
        {
            await UniTask.Delay(0);
            return true;
        }



        // Fields : caching

        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        protected CancellationTokenSource cancel = new CancellationTokenSource();
        protected List<LibrarySlot> slots = new List<LibrarySlot>();
        protected List<LibraryCategoryButton> categoryButtons = new List<LibraryCategoryButton>();
        protected PageType currentType;
        protected int currentMainCategory = 0;
        protected int currentSubCategory = 0;
        protected string learningTypeCode = "";
        private bool prepared = false;

        // Functions
        protected void addCategoryButton(int idx, Sprite icon)
        {
            var button = Instantiate(categoryPF, categoryRT);
            button.Init(idx, icon);
            button.OnClick += categoryButton_onClick;

            categoryButtons.Add(button);
        }
        protected LibrarySlot addSlot()
        {
            var slot = Instantiate(slotPF, slotsRT);
            slot.OnClick += slot_onClick;
            slot.OnFavorite += slot_onFavorite;
            slot.OnDelete += slot_onDelete;

            slots.Add(slot);

            return slot;
        }
        protected void removeAllSlots()
        {
            slots.ForEach(slot =>
            {
                slot.OnClick -= slot_onClick;
                slot.OnFavorite -= slot_onFavorite;
                slot.OnDelete -= slot_onDelete;
            });
            slots.Clear();
            foreach (var ch in slotsRT.GetChildren())
            {
                if (ch != slotPF.GetComponent<RectTransform>())
                    GameObject.Destroy(ch.gameObject);
            }
        }
        protected async UniTask downloadAndStart(IndexBase index)
        {
            cancel.Cancel();

            try
            {
                if (index is EBookPlayAllIndex)
                {
                    foreach (var ebook in ((EBookPlayAllIndex)index).EBooks)
                    {
                        var size = await DataDownloader.One.GetDataDownloadSize(ebook);
                        if (size > 0)
                        {
                            var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                            if (result != SimplePopupResult.Yes)
                                return;

                            canvasGroup.blocksRaycasts = false;
                            await DataDownloader.One.DownloadData(ebook, SystemUI.One.DownloadPU);
                        }
                    }
                }
                else if (index is not MovieSingleIndex && index is not MoviePlayAllIndex)
                {
                    var size = await DataDownloader.One.GetDataDownloadSize(index);
                    if (size > 0)
                    {
                        var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                        if (result != SimplePopupResult.Yes)
                            return;

                        canvasGroup.blocksRaycasts = false;
                        await DataDownloader.One.DownloadData(index, SystemUI.One.DownloadPU);
                    }
                }

                canvasGroup.blocksRaycasts = false;

                UserData.One.LastLibraryPageType = currentType;
                UserData.One.LastLibraryMainCategory = currentMainCategory;
                UserData.One.LastLibrarySubCategory = currentSubCategory;
                UserData.One.LastLibraryMainScroll = mainScroll.verticalScrollbar.value;
                if (subScroll)
                    UserData.One.LastLibrarySubScroll = subScroll.horizontalScrollbar.value;
                UserData.One.LastLibraryPageScroll = pageScroll.verticalScrollbar.value;

                RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                RunnerParam.SelectedIDX = index;
                RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                SceneLoader.One.LoadScene(index.SceneName);
                FinishLibrary();
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }
        protected void showSubCategory(bool show, string[] names = null, int idxSelected = 0)
        {
            if (subCategory)
            {
                if (show)
                    subCategory.Show(names, idxSelected);
                else
                    subCategory.Hide();
            }
        }
        protected async UniTask loadPage(PageType type, int idxMainCategory = 0, int idxSubCategory = 0)
        {
            cancel.Cancel();
            cancel = new CancellationTokenSource();

            UserData.One.LastLibraryPageType = PageType.Category;
            UserData.One.LastLibraryMainCategory = 0;
            UserData.One.LastLibrarySubCategory = 0;

            currentType = type;
            currentMainCategory = idxMainCategory;
            currentSubCategory = idxSubCategory;

            await onLoadPage(cancel.Token, type, idxMainCategory, idxSubCategory);
        }

        // Event Handlers
        private void favoriteBTN_onValueChanged(bool value)
        {
            LOG.Function(this, $"| value={value}");

            if (value)
            {
                categoryButtons.ForEach(button => button.Select(false));
                if (prepared)
                    loadPage(PageType.Favorite).Forget();
            }
        }
        private void myRecordBTN_onValueChanged(bool value)
        {
            LOG.Function(this, $"| value={value}");

            if (value)
            {
                categoryButtons.ForEach(button => button.Select(false));
                if (prepared)
                    loadPage(PageType.MyRecord).Forget();
            }
        }
        private void playAllBTN_onClick()
        {
            LOG.Function(this);

            onPlayAll();
        }
        private void categoryButton_onClick(LibraryCategoryButton button)
        {
            LOG.Function(this, $"{button}");

            loadPage(PageType.Category, button.Index).Forget();
        }
        private void subCategory_onClick(int idxSubCategory)
        {
            LOG.Function(this, $"{idxSubCategory}");

            loadPage(PageType.Category, currentMainCategory, idxSubCategory).Forget();
        }
        private void slot_onClick(LibrarySlot slot)
        {
            LOG.Function(this, $"| slot={slot}");

            onClickSlot(slot).Forget();
        }
        protected void slot_onFavorite(LibrarySlot slot)
        {
            LOG.Function(this, $"| slot={slot}");

            if (slot.IsFavorite)
                LMS.One.AddLibraryFavorite(learningTypeCode, int.Parse(slot.ContentIndex)).Forget();
            else
                LMS.One.RemoveLibraryFavorite(learningTypeCode, int.Parse(slot.ContentIndex)).Forget();
        }
        protected void slot_onDelete(LibrarySlot slot)
        {
            LOG.Function(this, $"| slot={slot}");

            onDeleteSlot(slot).Forget();
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] private Button playAllBTN = null;
        [SerializeField] protected TMP_Text titleTMP = null;
        [SerializeField] protected TMP_Text explanationTMP = null;
        [SerializeField] private LibrarySubCategory subCategory = null;
        [SerializeField] private RectTransform slotsRT = null;
        [SerializeField] protected LibrarySlot slotPF = null;
        [SerializeField] private RectTransform categoryRT = null;
        [SerializeField] protected LibraryCategoryButton categoryPF = null;
        //[SerializeField] private GameObject selectedCategory = null;
        //[SerializeField] protected LibraryCategoryButton selectedCategoryButton = null;
        [SerializeField] private Toggle favoriteTGL = null;
        [SerializeField] private Toggle myRecordTGL = null;
        [SerializeField] private ScrollRect mainScroll = null;
        [SerializeField] private ScrollRect subScroll = null;
        [SerializeField] private ScrollRect pageScroll = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float startDelay = 0.5f;

        // Unity Messages
        protected virtual void Awake()
        {
            onInitLibrary();
        }
        protected virtual void Start()
        {
            AudioMGR.One.PlayBGM(bgmCLIP);
        }
        protected virtual void OnEnable()
        {
            if (subCategory) subCategory.OnClick += subCategory_onClick;
        }
        protected virtual void OnDisable()
        {
            if (subCategory) subCategory.OnClick -= subCategory_onClick;
        }
        protected virtual void OnDestroy()
        {
            cancel.Cancel();
        }

        // Unity Coroutine
    }
}