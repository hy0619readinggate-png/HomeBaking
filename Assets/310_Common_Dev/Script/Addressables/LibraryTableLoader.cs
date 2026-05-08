using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static DoDoEng.Common.MovieMainCategory;

namespace DoDoEng.Common
{
    public class LibraryTableLoader : BYDSingleton<LibraryTableLoader>
    {
        // Properties
        public LibraryMovieTableLoaderResult MovieTable { get; private set; }
        public LibraryEBookTableLoaderResult EBookTable { get; private set; }
        public LibraryActivityTableLoaderResult ActivityTable { get; private set; }
        public LibraryGameTableLoaderResult GameTable { get; private set; }

        // Methods
        public async UniTask<LibraryMovieTableLoaderResult> LoadMovieTable()
        {
            using (LOG.Coroutine($"LoadMovieTable()", this))
            {
                if (MovieTable == null)
                {
                    // 엑셀 로드
                    var hLoad = movieTableAR.LoadAssetAsync();
                    await hLoad.Task;

                    var xlsx = hLoad.Result;

                    Addressables.Release(hLoad);

                    // 버전 정보 읽기
                    var workbook = new WorkBook(xlsx.Bytes);
                    var sheetVersion = workbook["version"];
                    var versions = sheetVersion.Convert<TableVersion>();
                    var version = versions.LastOrDefault();
                    if (version == null)
                    {
                        var msg = $"No version info. is exist.";
                        LOG.Warning(msg, this);
                        throw new NoVersionInfoException(msg);
                    }
                    LOG.Addressable($"VERSION : {version}", this);

                    // 메인 카테고리
                    var sheetMain = workbook["movie_category_main"];
                    var category_main = sheetMain.Convert<MovieCategoryMain>();
                    LOG.Addressable($"Main Category : {category_main}", this);

                    // 서브 카테고리
                    var sheetSub = workbook["movie_category_sub"];
                    var category_sub = sheetSub.Convert<MovieCategorySub>();
                    LOG.Addressable($"Sub Category : {category_sub}", this);

                    // 커리큘럼 찾기
                    var sheet = workbook["movie_list"];
                    var lists = sheet.Convert<MovieList>();
                    LOG.Addressable($"LIST : {lists}", this);

                    MovieTable = new LibraryMovieTableLoaderResult
                    {
                        Version = version,
                        CategoryMain = category_main.ToArray(),
                        CategorySub = category_sub.ToArray(),
                        List = lists.ToArray(),
                    };
                }
            }

            return MovieTable;
        }
        public async UniTask<LibraryEBookTableLoaderResult> LoadEBookTable()
        {
            using (LOG.Coroutine($"LoadEBookTable()", this))
            {
                if (EBookTable == null)
                {
                    // 엑셀 로드
                    var hLoad = ebookTableAR.LoadAssetAsync();
                    await hLoad.Task;

                    var xlsx = hLoad.Result;

                    Addressables.Release(hLoad);

                    // 버전 정보 읽기
                    var workbook = new WorkBook(xlsx.Bytes);
                    var sheetVersion = workbook["version"];
                    var versions = sheetVersion.Convert<TableVersion>();
                    var version = versions.LastOrDefault();
                    if (version == null)
                    {
                        var msg = $"No version info. is exist.";
                        LOG.Warning(msg, this);
                        throw new NoVersionInfoException(msg);
                    }
                    LOG.Addressable($"VERSION : {version}", this);

                    // 메인 카테고리
                    var sheetMain = workbook["ebook_category_main"];
                    var category_main = sheetMain.Convert<EBookCategoryMain>();
                    LOG.Addressable($"Main Category : {category_main}", this);

                    // 서브 카테고리
                    var sheetSub = workbook["ebook_category_sub"];
                    var category_sub = sheetSub.Convert<EBookCategorySub>();
                    LOG.Addressable($"Sub Category : {category_sub}", this);

                    // 커리큘럼 찾기
                    var sheet = workbook["ebook_list"];
                    var lists = sheet.Convert<LibraryEBookList>();
                    LOG.Addressable($"LIST : {lists}", this);

                    EBookTable = new LibraryEBookTableLoaderResult
                    {
                        Version = version,
                        CategoryMain = category_main.ToArray(),
                        CategorySub = category_sub.ToArray(),
                        List = lists.ToArray(),
                    };
                }
            }

            return EBookTable;
        }
        public async UniTask<LibraryActivityTableLoaderResult> LoadActivityTable()
        {
            using (LOG.Coroutine($"LoadActivityTable()", this))
            {
                if (ActivityTable == null)
                {
                    // 카테고리 엑셀 로드
                    var hLoad = activityCategoryTableAR.LoadAssetAsync();
                    await hLoad.Task;
                    var xlsx = hLoad.Result;
                    Addressables.Release(hLoad);

                    // 버전 정보 읽기
                    var workbook = new WorkBook(xlsx.Bytes);
                    var sheetVersion = workbook["version"];
                    var versions = sheetVersion.Convert<TableVersion>();
                    var version = versions.LastOrDefault();
                    if (version == null)
                    {
                        var msg = $"No version info. is exist.";
                        LOG.Warning(msg, this);
                        throw new NoVersionInfoException(msg);
                    }
                    LOG.Addressable($"VERSION : {version}", this);

                    // 액티비티 카테고리
                    var sheetCategory = workbook["activity_category"];
                    var category_main = sheetCategory.Convert<ActivityCategory>();
                    LOG.Addressable($"Activity Category : {category_main}", this);

                    List<TableVersion> versionList = new List<TableVersion>();
                    ActivityType[][] typeLists = new ActivityType[4][];
                    LibraryActivityList[][] activityLists = new LibraryActivityList[4][];
                    string[] langTypes = null;
                    for (int i = 0; i < 4; i++)
                    {
                        // 액티비티 엑셀 로드
                        hLoad = activityTablesAR[i].LoadAssetAsync();
                        await hLoad.Task;
                        xlsx = hLoad.Result;
                        Addressables.Release(hLoad);

                        // 버전 정보 읽기
                        workbook = new WorkBook(xlsx.Bytes);
                        sheetVersion = workbook["version"];
                        versions = sheetVersion.Convert<TableVersion>();
                        var activity_version = versions.LastOrDefault();
                        if (activity_version == null)
                        {
                            var msg = $"No version info. is exist.";
                            LOG.Warning(msg, this);
                            throw new NoVersionInfoException(msg);
                        }
                        LOG.Addressable($"ACT VERSION : {activity_version}", this);
                        versionList.Add(activity_version);

                        var sheet = workbook["activity_type"];
                        var types = sheet.Convert<ActivityType>();
                        //types.ForEach(type => type.Level = i + 1);
                        LOG.Addressable($"Type LIST : {types}", this);
                        typeLists[i] = types.ToArray();

                        sheet = workbook["activity_list"];
                        var activities = sheet.Convert<LibraryActivityList>();
                        //activities.ForEach(act => act.Level = i + 1);
                        LOG.Addressable($"Activity LIST : {activities}", this);
                        activityLists[i] = activities.ToArray();

                        // 언어 영역 찾기
                        if (langTypes == null)
                        {
                            var sheetLangType = workbook["참조용_Lang_Type"];
                            langTypes = sheetLangType.Convert<LangType>().Select(langType => langType.Name).ToArray();
                        }
                        typeLists[i].ForEach(list =>
                        {
                            List<string> langTypeList = new List<string>();
                            if (!string.IsNullOrEmpty(list.LangType1)) langTypeList.Add(langTypes[int.Parse(list.LangType1)]);
                            if (!string.IsNullOrEmpty(list.LangType2)) langTypeList.Add(langTypes[int.Parse(list.LangType2)]);
                            if (!string.IsNullOrEmpty(list.LangType3)) langTypeList.Add(langTypes[int.Parse(list.LangType3)]);
                            list.LangTypes = langTypeList.ToArray();
                        });
                    }

                    ActivityTable = new LibraryActivityTableLoaderResult
                    {
                        CategoryVersion = version,
                        ActivityVersions = versionList.ToArray(),
                        CategoryMain = category_main.ToArray(),
                        TypeLists = typeLists,
                        ActivityLists = activityLists,
                    };
                }
            }

            return ActivityTable;
        }
        public async UniTask<LibraryGameTableLoaderResult> LoadGameTable()
        {
            using (LOG.Coroutine($"LoadGameTable()", this))
            {
                if (GameTable == null)
                {
                    List<TableVersion> versionList = new List<TableVersion>();
                    GameType[][] typeLists = new GameType[4][];
                    LibraryReviewGameList[][] reviewLists = new LibraryReviewGameList[4][];
                    LibraryPlaygroundGameList[][] playgroundLists = new LibraryPlaygroundGameList[4][];
                    for (int i = 0; i < 4; i++)
                    {
                        // 액티비티 엑셀 로드
                        var hLoad = gameTablesAR[i].LoadAssetAsync();
                        await hLoad.Task;
                        var xlsx = hLoad.Result;
                        Addressables.Release(hLoad);

                        // 버전 정보 읽기
                        var workbook = new WorkBook(xlsx.Bytes);
                        var sheetVersion = workbook["version"];
                        var versions = sheetVersion.Convert<TableVersion>();
                        var game_version = versions.LastOrDefault();
                        if (game_version == null)
                        {
                            var msg = $"No version info. is exist.";
                            LOG.Warning(msg, this);
                            throw new NoVersionInfoException(msg);
                        }
                        LOG.Addressable($"GAME VERSION : {game_version}", this);
                        versionList.Add(game_version);

                        var sheet = workbook["game_type"];
                        var types = sheet.Convert<GameType>();
                        LOG.Addressable($"Type LIST : {types} | types={types.Count()}", this);
                        typeLists[i] = types.ToArray();

                        sheet = workbook["reviewgame_list"];
                        var reviews = sheet.Convert<LibraryReviewGameList>();
                        LOG.Addressable($"ReviewGame LIST : {reviews}", this);
                        reviewLists[i] = reviews.ToArray();

                        sheet = workbook["playground_list"];
                        var playgrounds = sheet.Convert<LibraryPlaygroundGameList>();
                        LOG.Addressable($"Playground LIST : {playgrounds}", this);
                        playgroundLists[i] = playgrounds.ToArray();

                        // 언어 영역 찾기
                        var sheetLangType = workbook["참조용_Lang_Type"];
                        var langTypes = sheetLangType.Convert<LangType>();
                        typeLists[i].ForEach(list =>
                        {
                            List<string> langTypeList = new List<string>();
                            var langType1 = langTypes.SingleOrDefault(c => c.Index == list.LangType1);
                            if (langType1 != null) langTypeList.Add(langType1.Name);
                            var langType2 = langTypes.SingleOrDefault(c => c.Index == list.LangType2);
                            if (langType2 != null) langTypeList.Add(langType2.Name);
                            var langType3 = langTypes.SingleOrDefault(c => c.Index == list.LangType3);
                            if (langType3 != null) langTypeList.Add(langType3.Name);
                            list.LangTypes = langTypeList.ToArray();
                        });
                    }

                    GameTable = new LibraryGameTableLoaderResult
                    {
                        GameVersions = versionList.ToArray(),
                        TypeLists = typeLists,
                        ReviewLists = reviewLists,
                        PlaygroundLists = playgroundLists,
                    };
                }
            }

            return GameTable;
        }
        public async UniTask<LibraryAIStudioTableLoaderResult> LoadAIStudioTable()
        {
            using (LOG.Coroutine($"LoadAIStudioTable()", this))
            {
                // 엑셀 로드
                var hLoad = aistudioTableAR.LoadAssetAsync();
                await hLoad.Task;

                var xlsx = hLoad.Result;

                Addressables.Release(hLoad);

                // 버전 정보 읽기
                var workbook = new WorkBook(xlsx.Bytes);
                var sheetVersion = workbook["version"];
                var versions = sheetVersion.Convert<TableVersion>();
                var version = versions.LastOrDefault();
                if (version == null)
                {
                    var msg = $"No version info. is exist.";
                    LOG.Warning(msg, this);
                    throw new NoVersionInfoException(msg);
                }
                LOG.Addressable($"VERSION : {version}", this);

                // 메인 카테고리
                var sheetMain = workbook["aistudio_category_main"];
                var category_main = sheetMain.Convert<AIStudioCategoryMain>().ToArray();
                LOG.Addressable($"Main Category : {category_main}", this);

                // 서브 카테고리
                var sheetSub = workbook["aistudio_category_sub"];
                var category_sub = sheetSub.Convert<AIStudioCategorySub>().ToArray();
                LOG.Addressable($"Sub Category : {category_sub}", this);

                // 커리큘럼 찾기
                var sheet = workbook["aistudio_list"];
                var lists = sheet.Convert<AIStudioList>().ToArray();

                return new LibraryAIStudioTableLoaderResult
                {
                    Version = version,
                    CategoryMain = category_main,
                    CategorySub = category_sub,
                    List = lists,
                };
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset> movieTableAR = null;
        [SerializeField] private AssetReferenceT<XlsxAsset> ebookTableAR = null;
        [SerializeField] private AssetReferenceT<XlsxAsset> activityCategoryTableAR = null;
        [SerializeField] private AssetReferenceT<XlsxAsset>[] activityTablesAR = null;
        [SerializeField] private AssetReferenceT<XlsxAsset>[] gameTablesAR = null;
        [SerializeField] private AssetReferenceT<XlsxAsset> aistudioTableAR = null;
    }

    // Table : Movie
    public class LibraryMovieTableLoaderResult
    {
        public TableVersion Version;
        public MovieCategoryMain[] CategoryMain;
        public MovieCategorySub[] CategorySub;
        public MovieList[] List;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class MovieCategoryMain
    {
        public SongAndChantType SongAndChant => (SongAndChantType)SongAndChantValue;

        [Column("A")] public int Index;
        [Column("B")] public string Name;
        [Column("C")] public int SongAndChantValue;
        [Column("D")] public string InfoKor;
        [Column("E")] public string InfoEng;
        [Column("F")] public string InfoVie;
        [Column("G")] public string Icon;

        public override string ToString()
        {
            return $"{Index} | {Name} | {SongAndChantValue} | {InfoKor} | {Icon}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class MovieCategorySub
    {
        [Column("A")] public int MainIndex;
        [Column("B")] public int Index;
        [Column("C")] public string Name;
        [Column("E")] public string Info;
        [Column("H")] public string Icon;

        public override string ToString()
        {
            return $"{MainIndex} | {Index} | {Name} | {Info} | {Icon}";
        }
    }

    // Table : EBook
    public class LibraryEBookTableLoaderResult
    {
        public TableVersion Version;
        public EBookCategoryMain[] CategoryMain;
        public EBookCategorySub[] CategorySub;
        public LibraryEBookList[] List;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class EBookCategoryMain
    {
        [Column("A")] public int Index;
        [Column("B")] public string Name;
        [Column("C")] public string InfoKor;
        [Column("D")] public string InfoEng;
        [Column("E")] public string InfoVie;
        [Column("F")] public string Icon;

        public override string ToString()
        {
            return $"{Index} | {Name} | {InfoKor} | {Icon}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class EBookCategorySub
    {
        [Column("A")] public int MainIndex;
        [Column("B")] public int Index;
        [Column("C")] public string Name;
        [Column("D")] public string Info;
        [Column("E")] public string Icon;

        public override string ToString()
        {
            return $"{MainIndex} | {Index} | {Name} | {Info} | {Icon}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class LibraryEBookList
    {
        [Column("A")] public string Index;
        [Column("B")] public int MainCategory;
        [Column("C")] public int SubCategory;
        [Column("E")] public string Title;
        [Column("F")] public string Info;
        [Column("G")] public string Word1;
        [Column("H")] public string Word2;
        [Column("I")] public string Word3;
        [Column("J")] public string Word4;
        [Column("K")] public string Word5;
        [Column("L")] public string Word6;
        [Column("M")] public int Stage;
        public string[] Words
        {
            get
            {
                List<string> words = new List<string>();
                if (!string.IsNullOrEmpty(Word1)) words.Add(Word1);
                if (!string.IsNullOrEmpty(Word2)) words.Add(Word2);
                if (!string.IsNullOrEmpty(Word3)) words.Add(Word3);
                if (!string.IsNullOrEmpty(Word4)) words.Add(Word4);
                if (!string.IsNullOrEmpty(Word5)) words.Add(Word5);
                if (!string.IsNullOrEmpty(Word6)) words.Add(Word6);
                return words.ToArray();
            }
        }

        public override string ToString()
        {
            return $"{Index} | {MainCategory} | {SubCategory} | {Title} | {Info} | {Words} | {Stage}";
        }
    }

    // Table : Activity
    public class LibraryActivityTableLoaderResult
    {
        public TableVersion CategoryVersion;
        public TableVersion[] ActivityVersions;
        public ActivityCategory[] CategoryMain;
        public ActivityType[][] TypeLists;
        public LibraryActivityList[][] ActivityLists;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityCategory
    {
        [Column("A")] public int Index;
        [Column("B")] public string Name;
        [Column("C")] public string InfoKor;
        [Column("D")] public string InfoEng;
        [Column("E")] public string InfoVie;
        [Column("F")] public string Icon;

        public override string ToString()
        {
            return $"{Index} | {Name} | {InfoVie} | {Icon}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityType
    {
        [Column("A")] public int MainCategory;
        [Column("B")] public int Type;
        [Column("C")] public string TitleKor;
        [Column("D")] public string TitleEng;
        [Column("E")] public string TitleVie;
        [Column("F")] public string InfoKor;
        [Column("G")] public string InfoEng;
        [Column("H")] public string InfoVie;
        [Column("I")] public string Thumbnail;
        [Column("J")] public string LangType1;
        [Column("K")] public string LangType2;
        [Column("L")] public string LangType3;
        public string[] LangTypes;

        public override string ToString()
        {
            return $"{MainCategory} | {Type} | {TitleKor} | {Thumbnail}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class LibraryActivityList
    {
        [Column("A")] public string Index;
        [Column("B")] public int ActivityType;
        [Column("C")] public int ActivityNum;
        [Column("M")] public string ActivityName3;
        [Column("M")] public string ActivityName4;
        [Column("N")] public string ActivityName2;
        [Column("P")] public string ActivityName1;

        public override string ToString()
        {
            return $"{Index} | {ActivityType} | {ActivityNum} | {ActivityName1} | {ActivityName2} | {ActivityName3}";
        }
    }

    // Table : Game
    public class LibraryGameTableLoaderResult
    {
        public TableVersion[] GameVersions;
        public GameType[][] TypeLists;
        public LibraryReviewGameList[][] ReviewLists;
        public LibraryPlaygroundGameList[][] PlaygroundLists;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameType
    {
        [Column("A")] public int Type;
        [Column("B")] public string TitleKor;
        [Column("C")] public string TitleEng;
        [Column("D")] public string TitleVie;
        [Column("E")] public string InfoKor;
        [Column("F")] public string InfoEng;
        [Column("G")] public string InfoVie;
        [Column("I")] public string LangType1;
        [Column("J")] public string LangType2;
        [Column("K")] public string LangType3;
        public string[] LangTypes;

        public override string ToString()
        {
            return $"{Type} | {InfoKor} | {InfoEng} | {InfoVie}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class LibraryReviewGameList
    {
        [Column("A")] public string Index;
        [Column("B")] public int GameType;
        [Column("C")] public int GameNum;
        [Column("J")] public string GameName;

        public override string ToString()
        {
            return $"{Index} | {GameType} | {GameNum} | {GameName}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class LibraryPlaygroundGameList
    {
        [Column("A")] public string Index;
        [Column("B")] public int GameType;
        [Column("C")] public int GameNum;
        [Column("J")] public string GameName;

        public override string ToString()
        {
            return $"{Index} | {GameType} | {GameNum} | {GameName}";
        }
    }

    // Table : AIStudio
    public class LibraryAIStudioTableLoaderResult
    {
        public TableVersion Version;
        public AIStudioCategoryMain[] CategoryMain;
        public AIStudioCategorySub[] CategorySub;
        public AIStudioList[] List;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class AIStudioCategoryMain
    {
        [Column("A")] public int Index;
        [Column("B")] public string Name;
        //[Column("D")] public string Info;

        public override string ToString()
        {
            return $"{Index} | {Name}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class AIStudioCategorySub
    {
        [Column("A")] public int Index;
        [Column("B")] public string Name;
        public override string ToString()
        {
            return $"{Index} | {Name}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class AIStudioList
    {
        [Column("A")] public string Index;
        [Column("B")] public int MainCategory;
        [Column("C")] public int SubCategory;
        [Column("D")] public int OrderDefault;
        [Column("E")] public string Title;
        [Column("G")] public string OrderSeries;
        [Column("I")] public string PlayTime;
        [Column("K")] public string Thema;
        public int popular = 0;

        public override string ToString()
        {
            return $"{Index} | {MainCategory} | {SubCategory} | {Title}";
        }
    }
}