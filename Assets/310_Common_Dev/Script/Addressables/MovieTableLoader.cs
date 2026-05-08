using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class MovieTableLoader : BYDSingleton<MovieTableLoader>
    {
        // Methods
        public async UniTask<MovieTableLoaderResult> LoadTable(MovieSingleIndex movieIDX)
        {
            using (LOG.Coroutine($"LoadTable() | {movieIDX}", this))
            {
                // 엑셀 로드
                var hLoad = tableAR.LoadAssetAsync();
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
                    var msg = $"No version info. is exist for {movieIDX}";
                    LOG.Warning(msg, this);
                    throw new NoVersionInfoException(msg);
                }
                LOG.Addressable($"VERSION : {version}", this);

                // 커리큘럼 찾기
                var sheet = workbook["movie_list"];
                var lists = sheet.Convert<MovieList>();
                var list = lists.SingleOrDefault(c => c.Index == movieIDX.Index);
                if (list == null)
                {
                    var msg = $"No list is exist for {movieIDX}";
                    LOG.Warning(msg, this);
                    throw new NoListException(msg);
                }
                LOG.Addressable($"LIST : {list}", this);

                // 메인 카테고리 찾기
                var sheetMainCategory = workbook["movie_category_main"];
                var mainCategories = sheetMainCategory.Convert<MovieMainCategory>();
                var mainCategory = mainCategories.SingleOrDefault(c => c.Index == list.MainCategory);
                if (mainCategory == null)
                {
                    var msg = $"No mainCategory is exist for {movieIDX}";
                    LOG.Warning(msg, this);
                    //throw new NoListException(msg);
                }
                LOG.Addressable($"MAIN CATEGORY : {mainCategory}", this);

                // 언어 영역 찾기
                List<string> langTypeList = new List<string>();
                var sheetLangType = workbook["참조용_Lang_Type"];
                var langTypes = sheetLangType.Convert<LangType>();
                var langType1 = langTypes.SingleOrDefault(c => c.Index == list.LangType1);
                if (langType1 != null) langTypeList.Add(langType1.Name);
                var langType2 = langTypes.SingleOrDefault(c => c.Index == list.LangType2);
                if (langType2 != null) langTypeList.Add(langType2.Name);
                var langType3 = langTypes.SingleOrDefault(c => c.Index == list.LangType3);
                if (langType3 != null) langTypeList.Add(langType3.Name);

                return new MovieTableLoaderResult
                {
                    Version = version,
                    MainCategory = mainCategory,
                    List = list,
                    LangTypes = langTypeList.ToArray()
                };
            }
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset> tableAR = null;
    }

    // Table
    public class MovieTableLoaderResult
    {
        public TableVersion Version;
        public MovieMainCategory MainCategory;
        public MovieList List;
        public string[] LangTypes;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class MovieMainCategory
    {
        // Definition
        public enum SongAndChantType
        {
            NoRecord,
            RecordWithMR,
            RecordWithoutMR
        }
        public SongAndChantType SongAndChant => (SongAndChantType)SongAndChantValue;

        [Column("A")] public int Index;
        [Column("B")] public string Name;
        [Column("C")] public int SongAndChantValue;

        public override string ToString()
        {
            return $"{Index} | {Name} | {SongAndChant}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class MovieList
    {
        [Column("A")] public string Index;
        [Column("B")] public int MainCategory;
        [Column("C")] public int SubCategory;
        [Column("E")] public string Title;
        [Column("F")] public string PlayTime;
        [Column("G")] public int Stage;
        [Column("I")] public string LangType1;
        [Column("J")] public string LangType2;
        [Column("K")] public string LangType3;

        public override string ToString()
        {
            return $"{Index} | {MainCategory} | {SubCategory} | {Title} | {PlayTime} | {Stage} | {LangType1} | {LangType2} | {LangType3}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class LangType
    {
        [Column("A")] public string Index;
        [Column("B")] public string Name;

        public override string ToString()
        {
            return $"{Index} | {Name}";
        }
    }
}