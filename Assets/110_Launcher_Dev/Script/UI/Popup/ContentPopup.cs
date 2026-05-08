using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
// using DoDoEng.AIStudio;

namespace DoDoEng.Launcher.UI
{
    public class ContentPopup : MonoBehaviour
    {
        // Definitions
        // Properties

        // Methods
        public void ShowMovie(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail}");

            thumbnailOrigin = thumbnail;

            keywordsGO.SetActive(true);
            for (int i = 0; i < keywords.Length; i++)
                keywords[i].transform.parent.gameObject.SetActive(false);

            if (thumbnail.IsRecorded)
            {
                keywords[0].transform.parent.gameObject.SetActive(true);
                keywords[0].text = "Listening";
                keywords[1].transform.parent.gameObject.SetActive(true);
                keywords[1].text = "Speaking";
                keywords[2].transform.parent.gameObject.SetActive(true);
                keywords[2].text = "Performance";
            }
            else
            {
                keywords[0].transform.parent.gameObject.SetActive(true);
                keywords[0].text = "Listening";
                keywords[1].transform.parent.gameObject.SetActive(true);
                keywords[1].text = "Speaking";
            }
            lateKeywordRebuildLayout().Forget();

            gameObject.SetActive(true);

            titleTMP.text = thumbnail.Title;
            this.thumbnail.Thumbnail = thumbnail.Thumbnail;
            this.thumbnail.ContentIndex = thumbnail.ContentIndex;

            var ts = TimeSpan.FromSeconds(thumbnail.LearningTime);
            timeTMP.text = $"{Math.Floor(ts.TotalMinutes):00}:{ts.Seconds:00}";

            dateTMP.text = thumbnail.CompleteDatetime.ToString("yyyy.MM.dd");

            explainTMP.gameObject.SetActive(false);

            scoreGO.SetActive(false);
            buttonsGO.SetActive(thumbnail.IsRecorded);
            vocabularyBT.gameObject.SetActive(false);
            worksheetyBT.gameObject.SetActive(false);
            myEbookBT.gameObject.SetActive(false);
            myMovieBT.gameObject.SetActive(true);
            reportBT.gameObject.SetActive(false);
        }
        public void ShowEbook(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail}");

            thumbnailOrigin = thumbnail;

            keywordsGO.SetActive(true);
            for (int i = 0; i < keywords.Length; i++)
                keywords[i].transform.parent.gameObject.SetActive(false);

            if (thumbnail.IsRecorded)
            {
                keywords[0].transform.parent.gameObject.SetActive(true);
                keywords[0].text = "Performance";
                keywords[1].transform.parent.gameObject.SetActive(true);
                keywords[1].text = "Listening";
                keywords[2].transform.parent.gameObject.SetActive(true);
                keywords[2].text = "Speaking";
            }
            else
            {
                keywords[0].transform.parent.gameObject.SetActive(true);
                keywords[0].text = "Listening";
                keywords[1].transform.parent.gameObject.SetActive(true);
                keywords[1].text = "Vocabulary";
                keywords[2].transform.parent.gameObject.SetActive(true);
                keywords[2].text = "Reading";
            }
            lateKeywordRebuildLayout().Forget();

            gameObject.SetActive(true);

            titleTMP.text = thumbnail.Title;
            this.thumbnail.Thumbnail = thumbnail.Thumbnail;
            this.thumbnail.ContentIndex = thumbnail.ContentIndex;

            var ts = TimeSpan.FromSeconds(thumbnail.LearningTime);
            timeTMP.text = $"{Math.Floor(ts.TotalMinutes):00}:{ts.Seconds:00}";

            dateTMP.text = thumbnail.CompleteDatetime.ToString("yyyy.MM.dd");

            explainTMP.gameObject.SetActive(false);

            scoreGO.SetActive(false);
            buttonsGO.SetActive(thumbnail.IsRecorded);
            vocabularyBT.gameObject.SetActive(false);
            worksheetyBT.gameObject.SetActive(false);
            myEbookBT.gameObject.SetActive(thumbnail.IsRecorded);
            myMovieBT.gameObject.SetActive(false);
            reportBT.gameObject.SetActive(false);
        }
        public void ShowActivity(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail}");

            thumbnailOrigin = thumbnail;

            gameObject.SetActive(true);

            titleTMP.text = thumbnail.Title;
            this.thumbnail.Thumbnail = thumbnail.Thumbnail;
            this.thumbnail.ContentIndex = thumbnail.ContentIndex;

            var ts = TimeSpan.FromSeconds(thumbnail.LearningTime);
            timeTMP.text = $"{Math.Floor(ts.TotalMinutes):00}:{ts.Seconds:00}";

            dateTMP.text = thumbnail.CompleteDatetime.ToString("yyyy.MM.dd");

            scoreGO.SetActive(false);
            buttonsGO.SetActive(false);

            keywordsGO.SetActive(true);
            int course = int.Parse(thumbnail.ContentIndex.ToString().Substring(1, 1));
            int type = int.Parse(thumbnail.ContentIndex.ToString().Substring(3, 2));
            var typeData = LibraryTableLoader.One.ActivityTable.TypeLists[course - 1].SingleOrDefault(data => data.Type == type);
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i < typeData.LangTypes.Length)
                {
                    keywords[i].transform.parent.gameObject.SetActive(true);
                    keywords[i].text = typeData.LangTypes[i];
                }
                else
                    keywords[i].transform.parent.gameObject.SetActive(false);
            }
            keywordsGO.GetComponent<MonoBehaviour>().RebuildLayout();

            explainTMP.gameObject.SetActive(true);
            explainTMP.text = typeData.InfoKor;

            lateKeywordRebuildLayout().Forget();
        }
        public void ShowPlayground(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail}");

            thumbnailOrigin = thumbnail;

            gameObject.SetActive(true);

            titleTMP.text = thumbnail.Title;
            this.thumbnail.Thumbnail = thumbnail.Thumbnail;
            this.thumbnail.ContentIndex = thumbnail.ContentIndex;

            var ts = TimeSpan.FromSeconds(thumbnail.LearningTime);
            timeTMP.text = $"{Math.Floor(ts.TotalMinutes):00}:{ts.Seconds:00}";

            dateTMP.text = thumbnail.CompleteDatetime.ToString("yyyy.MM.dd");

            scoreGO.SetActive(false);
            buttonsGO.SetActive(false);

            keywordsGO.SetActive(true);
            int course = int.Parse(thumbnail.ContentIndex.ToString().Substring(1, 1));
            int type = int.Parse(thumbnail.ContentIndex.ToString().Substring(3, 2));
            var typeData = LibraryTableLoader.One.GameTable.TypeLists[course - 1].SingleOrDefault(data => data.Type == type);
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i < typeData.LangTypes.Length)
                {
                    keywords[i].transform.parent.gameObject.SetActive(true);
                    keywords[i].text = typeData.LangTypes[i];
                }
                else
                    keywords[i].transform.parent.gameObject.SetActive(false);
            }
            keywordsGO.GetComponent<MonoBehaviour>().RebuildLayout();

            explainTMP.gameObject.SetActive(true);
            explainTMP.text = typeData.InfoKor;

            lateKeywordRebuildLayout().Forget();
        }
        public void ShowAIStudio(LMSContentThumbnail thumbnail)
        {
            LOG.Function(this, $"| thumbnail={thumbnail}");

            thumbnailOrigin = thumbnail;

            keywordsGO.SetActive(true);
            for (int i = 0; i < keywords.Length; i++)
                keywords[i].transform.parent.gameObject.SetActive(false);

            keywords[0].transform.parent.gameObject.SetActive(true);
            keywords[0].text = "Performance";
            keywords[1].transform.parent.gameObject.SetActive(true);
            keywords[1].text = "Listening";
            keywords[2].transform.parent.gameObject.SetActive(true);
            keywords[2].text = "Speaking";
            lateKeywordRebuildLayout().Forget();

            gameObject.SetActive(true);

            titleTMP.text = thumbnail.Title;
            this.thumbnail.Thumbnail = thumbnail.Thumbnail;
            this.thumbnail.ContentIndex = thumbnail.ContentIndex;
            this.thumbnail.CompleteDatetime = thumbnail.CompleteDatetime;
            this.thumbnail.RecordedType = thumbnail.RecordedType;

            var ts = TimeSpan.FromSeconds(thumbnail.LearningTime);
            timeTMP.text = $"{Math.Floor(ts.TotalMinutes):00}:{ts.Seconds:00}";

            dateTMP.text = thumbnail.CompleteDatetime.ToString("yyyy.MM.dd");

            explainTMP.gameObject.SetActive(false);

            scoreGO.SetActive(false);
            buttonsGO.SetActive(true);
            vocabularyBT.gameObject.SetActive(false);
            worksheetyBT.gameObject.SetActive(false);
            myEbookBT.gameObject.SetActive(false);
            myMovieBT.gameObject.SetActive(false);
            reportBT.gameObject.SetActive(true);
        }

        // Events



        // Fields : caching
        // Fields

        // Functions
        public async UniTask lateKeywordRebuildLayout()
        {
            await UniTask.Yield();
            if (keywordsGO.activeSelf)
                keywordsGO.GetComponent<MonoBehaviour>().RebuildLayout();
        }
        protected async UniTask downloadAndStart(IndexBase index)
        {
            try
            {
                if (index is not MovieSingleIndex)
                {
                    var size = await DataDownloader.One.GetDataDownloadSize(index);
                    if (size > 0)
                    {
                        var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                        if (result != SimplePopupResult.Yes)
                            return;

                        //canvasGroup.blocksRaycasts = false;
                        await DataDownloader.One.DownloadData(index, SystemUI.One.DownloadPU);
                    }
                }

                //canvasGroup.blocksRaycasts = false;

                //UserData.One.LastLibraryPageType = currentType;
                //UserData.One.LastLibraryMainCategory = currentMainCategory;
                //UserData.One.LastLibrarySubCategory = currentSubCategory;

                UserData.One.IsReportContents = true;
                RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                RunnerParam.SelectedIDX = index;
                RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                SceneLoader.One.LoadScene(index.SceneName);
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }

        // Event Handlers
        private void vocabularyBT_onClick()
        {
            LOG.Function(this);
        }
        private void worksheetyBT_onClick()
        {
            LOG.Function(this);
        }
        private void myEbookBT_onClick()
        {
            LOG.Function(this);

            if (thumbnailOrigin.HasRecordFiles)
            {
                RunnerParam.LearningIndex = 0;
                downloadAndStart(new EBookReadIndex(this.thumbnail.ContentIndex.ToString(), EBookReadMode.MyVoice)).Forget();
            }
            else
                SystemUI.One.ShowPopupNoRecordFiles().Forget();
        }
        private void myMovieBT_onClick()
        {
            LOG.Function(this);

            if (thumbnailOrigin.HasRecordFiles)
            {
                RunnerParam.LearningIndex = 0;
                RunnerParam.MovieMode = RunnerParam.MovieModeType.RecordPlay;
                downloadAndStart(new MovieSingleIndex(this.thumbnail.ContentIndex.ToString())).Forget();
            }
            else
                SystemUI.One.ShowPopupNoRecordFiles().Forget();
        }
        private void reportBT_onClick()
        {
            LOG.Function(this);

            reportBT.interactable = false;
            // _aistudio_report.btnOpenLMS = reportBT;
            // _aistudio_report.Init(thumbnail.ContentIndex.ToString(), (Dialog.FullStar)(thumbnail.RecordedType - 2), thumbnail.CompleteDatetime, true, null);
        }

        private LMSContentThumbnail thumbnailOrigin;

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text titleTMP = null;
        [SerializeField] private LMSContentThumbnail thumbnail = null;
        //[SerializeField] private GameObject timeGO = null;
        [SerializeField] private TMP_Text timeTMP = null;
        [SerializeField] private GameObject scoreGO = null;
        //[SerializeField] private TMP_Text scoreTMP = null;
        //[SerializeField] private GameObject dateGO = null;
        [SerializeField] private TMP_Text dateTMP = null;
        [SerializeField] private TMP_Text explainTMP = null;
        [SerializeField] private GameObject keywordsGO = null;
        [SerializeField] private TMP_Text[] keywords = null;
        [SerializeField] private GameObject buttonsGO = null;
        [SerializeField] private Button vocabularyBT = null;
        [SerializeField] private Button worksheetyBT = null;
        [SerializeField] private Button myEbookBT = null;
        [SerializeField] private Button myMovieBT = null;
        [SerializeField] private Button reportBT = null;
        [SerializeField] private Button closeBT = null;
        // [SerializeField] AIStudio.Report _aistudio_report;

        // Unity Messages
        private void Awake()
        {
            // _aistudio_report.gameObject.SetActive(false);
            vocabularyBT.onClick.AddListener(() => vocabularyBT_onClick());
            worksheetyBT.onClick.AddListener(() => worksheetyBT_onClick());
            myEbookBT.onClick.AddListener(() => myEbookBT_onClick());
            myMovieBT.onClick.AddListener(() => myMovieBT_onClick());
            reportBT.onClick.AddListener(() => reportBT_onClick());
            closeBT.onClick.AddListener(() => gameObject.SetActive(false));
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
        }
        private void OnDisable()
        {
        }

        // Unity Coroutine
    }
}