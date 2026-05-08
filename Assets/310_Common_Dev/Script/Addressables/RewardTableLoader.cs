using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class RewardTableLoader : BYDSingleton<RewardTableLoader>
    {
        // Properties
        public RewardTableLoaderResult Table { get; private set; }

        // Methods
        public async UniTask<RewardTableLoaderResult> LoadTable()
        {
            using (LOG.Coroutine($"LoadTable()", this))
            {
                if (Table == null)
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
                        var msg = $"No version info. is exist";
                        LOG.Warning(msg, this);
                        throw new NoVersionInfoException(msg);
                    }
                    LOG.Addressable($"VERSION : {version}", this);

                    // 카테고리 찾기
                    var categorySheet = workbook["Reward_Category"];
                    var categoryLists = categorySheet.Convert<CategoryList>();
                    if (categoryLists == null)
                    {
                        var msg = $"No category list is exist";
                        LOG.Warning(msg, this);
                        throw new NoListException(msg);
                    }
                    LOG.Addressable($"Category LIST : {categoryLists}", this);

                    // 리워드 찾기
                    var rewardSheet = workbook["Reward_List"];
                    var rewardLists = rewardSheet.Convert<RewardList>();
                    if (rewardLists == null)
                    {
                        var msg = $"No reward list is exist";
                        LOG.Warning(msg, this);
                        throw new NoListException(msg);
                    }
                    LOG.Addressable($"Reward LIST : {rewardLists}", this);

                    // 코인 모으는 방법 리스트 찾기
                    var guiSheet = workbook["Reward_List_GUI"];
                    var rewardGUILists = guiSheet.Convert<RewardListGUI>();
                    if (rewardGUILists == null)
                    {
                        var msg = $"No reward gui list is exist";
                        LOG.Warning(msg, this);
                        throw new NoListException(msg);
                    }
                    LOG.Addressable($"Reward GUI LIST : {rewardGUILists}", this);

                    Table = new RewardTableLoaderResult
                    {
                        Version = version,
                        CategoryList = categoryLists.ToArray(),
                        RewardList = rewardLists.ToArray(),
                        RewardListGUI = rewardGUILists.ToArray(),
                    };
                }

                return Table;
            }
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset> tableAR = null;
    }

    // Table
    public class RewardTableLoaderResult
    {
        public TableVersion Version;
        public CategoryList[] CategoryList;
        public RewardList[] RewardList;
        public RewardListGUI[] RewardListGUI;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class CategoryList
    {
        [Column("A")] public int Index;
        [Column("B")] public string Name;
        [Column("C")] public string Image;

        public override string ToString()
        {
            return $"{Index} | {Name} | {Image}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class RewardList
    {
        [Column("A")] public int Index;
        [Column("B")] public string Category;
        [Column("D")] public string NameKor;
        [Column("E")] public string NameEng;
        [Column("F")] public string NameVie;
        [Column("G")] public int Coin;

        public override string ToString()
        {
            return $"{Index} | {Category} | {NameKor} | {NameEng} | {NameVie} | {Coin}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class RewardListGUI
    {
        [Column("A")] public int CategoryIndex;
        [Column("C")] public int RewardIndex;

        public override string ToString()
        {
            return $"{CategoryIndex} | {RewardIndex}";
        }
    }
}