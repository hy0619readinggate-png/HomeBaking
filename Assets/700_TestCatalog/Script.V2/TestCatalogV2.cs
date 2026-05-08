using beyondi.Util;
using DoDoEng.Common;
using FlexFramework.Excel;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace DoDoEng.Tester
{
    public class TestCatalogV2 : MonoBehaviour
    {
        // Properties
        public static TestCatalogV2 One { get; private set; } = null;

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
            itemsCG.blocksRaycasts = active;
        }



        // Fields : caching
        private ToggleGroup c1TG_ = null;
        private ToggleGroup c1TG => c1TG_ ??= c1ParentTR.GetOrAddComponent<ToggleGroup>();
        private ToggleGroup c2TG_ = null;
        private ToggleGroup c2TG => c2TG_ ??= c2ParentTR.GetOrAddComponent<ToggleGroup>();

        // Fields
        private ActivityTesterTable[] tableActivity = null;
        private GameTesterTable[] tableReviewGame = null;
        private GameTesterTable[] tablePlayground = null;
        private EBookTesterTable[] tableEBook = null;

        // Functions
        private async void loadTable()
        {
            LOG.Info($"{nameof(loadTable)}()", this);

            try
            {
                itemsCG.blocksRaycasts = false;
                loadingGO.SetActive(true);

                var devServer = AddressableMGR.HostServerType == HostServerType.Development;
                tableActivity = await TesterTableLoader.One.LoadActivityTable(devServer);
                tableReviewGame = await TesterTableLoader.One.LoadGameTable(devServer, GameMode.Review);
                tablePlayground = await TesterTableLoader.One.LoadGameTable(devServer, GameMode.Playground);
                tableEBook = await TesterTableLoader.One.LoadEBookTable(devServer);

                fillCategory1();

                itemsCG.blocksRaycasts = true;
                loadingGO.SetActive(false);
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);
            }
        }

        // Functions
        private void fillCategory1()
        {
            Util.RemoveAllChildren(c1ParentTR);

            var courseA = tableActivity.Select(t => t.Course).Distinct();
            foreach (var c in courseA)
            {
                var item = Instantiate(itemCategoryPB, c1ParentTR);
                item.Title = $"Activity";
                item.Description = $"Course {c}";
                item.Category = new Category1 { Contents = ContentsType.Activity, Course = c };
                item.ToggleGroup = c1TG;
            }

            var courseRG = tableReviewGame.Select(t => t.Course).Distinct();
            foreach (var c in courseRG)
            {
                var item = Instantiate(itemCategoryPB, c1ParentTR);
                item.Title = $"Review Game";
                item.Description = $"Course {c}";
                item.Category = new Category1 { Contents = ContentsType.ReviewGame, Course = c };
                item.ToggleGroup = c1TG;
            }

            var coursePG = tablePlayground.Select(t => t.Course).Distinct();
            foreach (var c in coursePG)
            {
                var item = Instantiate(itemCategoryPB, c1ParentTR);
                item.Title = $"Playground";
                item.Description = $"Course {c}";
                item.Category = new Category1 { Contents = ContentsType.Playground, Course = c };
                item.ToggleGroup = c1TG;
            }

            {
                var item = Instantiate(itemCategoryPB, c1ParentTR);
                item.Title = $"EBook";
                item.Category = new Category1 { Contents = ContentsType.EBook };
                item.ToggleGroup = c1TG;
            }
        }
        private void fillCategory2_byActivity(int course)
        {
            Util.RemoveAllChildren(c2ParentTR);

            var tables = tableActivity.Where(t => t.Course == course);
            foreach (var t in tables)
            {
                var item = Instantiate(itemCategoryPB, c2ParentTR);
                item.Title = $"{t.ActivityName}";
                item.Description = $"{t.ActivityType}";
                item.Category = new Category2_Activity { ActivityID = t.ActivityType };
                item.ToggleGroup = c2TG;
            }

            c2SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillCategory2_byReviewGame(int course)
        {
            Util.RemoveAllChildren(c2ParentTR);

            var tables = tableReviewGame.Where(t => t.Course == course);
            foreach (var t in tables)
            {
                var item = Instantiate(itemCategoryPB, c2ParentTR);
                item.Title = $"{t.GameName}";
                item.Description = $"{t.GameType}";
                item.Category = new Category2_ReviewGame { GameID = t.GameType };
                item.ToggleGroup = c2TG;
            }

            c2SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillCategory2_byPlayground(int course)
        {
            Util.RemoveAllChildren(c2ParentTR);

            var tables = tablePlayground.Where(t => t.Course == course);
            foreach (var t in tables)
            {
                var item = Instantiate(itemCategoryPB, c2ParentTR);
                item.Title = $"{t.GameName}";
                item.Description = $"{t.GameType}";
                item.Category = new Category2_Playground { GameID = t.GameType };
                item.ToggleGroup = c2TG;
            }

            c2SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillCategory2_byEBook()
        {
            Util.RemoveAllChildren(c2ParentTR);

            var groups = tableEBook
                .GroupBy(t => new { t.MainCategory, t.SubCategory, t.MainCategoryName })
                .Select(g => g.Key);

            foreach (var g in groups)
            {
                var names = g.MainCategoryName.Split(">");
                var name1 = names[0];
                var name2 = names.Length > 1 ? names[1] : "";

                var item = Instantiate(itemCategoryPB, c2ParentTR);
                item.Title = $"{name1}";
                item.Description = $"{name2}";
                item.Category = new Category2_EBook { MainCategory = g.MainCategory, SubCategory = g.SubCategory };
                item.ToggleGroup = c2TG;
            }

            c2SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillContents(Category2_Activity c2)
        {
            Util.RemoveAllChildren(c3ParentTR);

            var table = tableActivity.Single(t => t.ActivityType == c2.ActivityID);
            for (var i = 0; i < table.Activities.Length; i++)
            {
                if (table.Activities[i] <= 0)
                    continue;

                var idx = new ActivityIndex(table.ActivityType, i + 1);

                var item = Instantiate(itemContentsPB, c3ParentTR);
                item.Title = $"{idx.ActivityNum:00}";
                item.Description = idx.Index;
                item.Enabled = table.Activities[i] >= 2;
                item.Index = idx;
            }

            c3SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillContents(Category2_ReviewGame c2)
        {
            Util.RemoveAllChildren(c3ParentTR);

            var table = tableReviewGame.Single(t => t.GameType == c2.GameID);
            for (var i = 0; i < table.Games.Length; i++)
            {
                if (table.Games[i] <= 0)
                    continue;

                var idx = new GameIndex(table.GameType, i + 1, GameMode.Review);

                var item = Instantiate(itemContentsPB, c3ParentTR);
                item.Title = $"{idx.GameNum:00}";
                item.Description = idx.Index;
                item.Enabled = table.Games[i] >= 2;
                item.Index = idx;
            }

            c3SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillContents(Category2_Playground c2)
        {
            Util.RemoveAllChildren(c3ParentTR);

            var table = tableReviewGame.Single(t => t.GameType == c2.GameID);
            for (var i = 0; i < table.Games.Length; i++)
            {
                if (table.Games[i] <= 0)
                    continue;

                var idx = new GameIndex(table.GameType, i + 1, GameMode.Playground);

                var item = Instantiate(itemContentsPB, c3ParentTR);
                item.Title = $"{idx.GameNum:00}";
                item.Description = idx.Index;
                item.Enabled = table.Games[i] >= 2;
                item.Index = idx;
            }

            c3SR.normalizedPosition = new Vector2(0, 1);
        }
        private void fillContents(Category2_EBook c2)
        {
            Util.RemoveAllChildren(c3ParentTR);

            var table = tableEBook.Single(t => t.MainCategory == c2.MainCategory && t.SubCategory == c2.SubCategory);
            for (var i = 0; i < table.EBooks.Length; i++)
            {
                if (table.EBooks[i] <= 0)
                    continue;

                var idx1 = new EBookReadIndex(table.MainCategory, table.SubCategory, i + 1, EBookReadMode.Native);
                var item1 = Instantiate(itemContentsPB, c3ParentTR);
                item1.Title = $"{idx1.EBookNum:00}";
                item1.Description = $"Native";
                item1.Enabled = table.EBooks[i] >= 2;
                item1.Index = idx1;

                var idx2 = new EBookRecordIndex(table.MainCategory, table.SubCategory, i + 1);
                var item2 = Instantiate(itemContentsPB, c3ParentTR);
                item2.Title = $"{idx1.EBookNum:00}";
                item2.Description = $"Record";
                item2.Enabled = table.EBooks[i] >= 2;
                item2.Index = idx2;

                var idx3 = new EBookReadIndex(table.MainCategory, table.SubCategory, i + 1, EBookReadMode.MyVoice);
                var item3 = Instantiate(itemContentsPB, c3ParentTR);
                item3.Title = $"{idx3.EBookNum:00}";
                item3.Description = $"My eBook";
                item3.Enabled = table.EBooks[i] >= 2;
                item3.Index = idx3;

                var idx4 = new EBookQuizIndex(table.MainCategory, table.SubCategory, i + 1);
                var item4 = Instantiate(itemContentsPB, c3ParentTR);
                item4.Title = $"{idx3.EBookNum:00}";
                item4.Description = $"Quiz";
                item4.Enabled = table.EBooks[i] >= 2;
                item4.Index = idx4;
            }

            c3SR.normalizedPosition = new Vector2(0, 1);
        }

        // Functions
        private async void downloadAndStart(IndexBase idx)
        {
            LOG.Addressable($"downloadAndStart() | {idx}", this);

            try
            {
                itemsCG.blocksRaycasts = false;

                var size = await DataDownloader.One.GetDataDownloadSize(idx);
                if (size > 0)
                {
                    var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                    if (result != SimplePopupResult.Yes)
                        return;

                    await DataDownloader.One.DownloadData(idx, SystemUI.One.DownloadPU);
                }

                RunnerParam.SelectedIDX = idx;
                RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                SceneLoader.One.LoadScene(idx.SceneName);
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.GetType()} | {ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }

        // Event Handlers
        private void categoryItem_OnSelect(Category category)
        {
            LOG.Info($"{nameof(categoryItem_OnSelect)}() | {category}", this);

            if (category is Category1)
            {
                var c1 = category as Category1;
                switch (c1.Contents)
                {
                    case ContentsType.Activity: fillCategory2_byActivity(c1.Course); break;
                    case ContentsType.ReviewGame: fillCategory2_byReviewGame(c1.Course); break;
                    case ContentsType.Playground: fillCategory2_byPlayground(c1.Course); break;
                    case ContentsType.EBook: fillCategory2_byEBook(); break;
                }
            }
            if (category is Category2)
            {
                switch (category as Category2)
                {
                    case Category2_Activity c2: fillContents(c2); break;
                    case Category2_ReviewGame c2: fillContents(c2); break;
                    case Category2_Playground c2: fillContents(c2); break;
                    case Category2_EBook c2: fillContents(c2); break;
                }
            }
        }
        private void contentsItem_OnSelect(IndexBase index)
        {
            LOG.Info($"{nameof(contentsItem_OnSelect)}() | {index}", this);

            downloadAndStart(index);
        }



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private CanvasGroup itemsCG = null;
        [SerializeField] private GameObject loadingGO = null;
        [SerializeField] private Transform c1ParentTR = null;
        [SerializeField] private Transform c2ParentTR = null;
        [SerializeField] private Transform c3ParentTR = null;
        [SerializeField] private ScrollRect c2SR = null;
        [SerializeField] private ScrollRect c3SR = null;
        [SerializeField] private Item_Category itemCategoryPB = null;
        [SerializeField] private Item_Contents itemContentsPB = null;

        // Unity Messages
        private void Awake()
        {
            if (One == null)
            {
                One = this;
                DontDestroyOnLoad(this);

                itemsCG.blocksRaycasts = false;
            }
            else Destroy(gameObject);

            Util.RemoveAllChildren(c1ParentTR);
            Util.RemoveAllChildren(c2ParentTR);
            Util.RemoveAllChildren(c3ParentTR);
        }
        private void Start()
        {
            loadTable();
        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.Cateogry_Select>(categoryItem_OnSelect);
            EventBus.Subscribe<EventBus.Contents_Select>(contentsItem_OnSelect);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.Cateogry_Select>(categoryItem_OnSelect);
            EventBus.Unsubscribe<EventBus.Contents_Select>(contentsItem_OnSelect);
        }
        private void OnDestroy()
        {
            if (One == this)
                One = null;
        }
    }
}