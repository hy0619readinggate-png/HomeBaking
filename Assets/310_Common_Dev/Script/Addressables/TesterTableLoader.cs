using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class TesterTableLoader : BYDSingleton<TesterTableLoader>
    {
        // Methods
        public async UniTask<ActivityTesterTable[]> LoadActivityTable(bool devServer)
        {
            using (LOG.Coroutine($"LoadActivityTable()", this))
            {
                // 엑셀 로드
                var hLoad = devServer
                    ? testerTableDevServerAR.LoadAssetAsync()
                    : testerTableTestServerAR.LoadAssetAsync();
                await hLoad.Task;

                var ex = hLoad.OperationException;
                var xlsx = hLoad.Result;

                Addressables.Release(hLoad);
                if (ex != null)
                    throw ex;

                // 엑셀 데이터 처리
                var workbook = new WorkBook(xlsx.Bytes);
                var sheet = workbook["activity"];
                var tables = sheet.Convert<ActivityTesterTable>().ToArray();
                if (tables.Length == 0)
                {
                    var msg = $"No table is exist.";
                    LOG.Warning(msg, this);
                    throw new NoDataException(msg);
                }

                // 컬럼데이터 수집
                for (var i = 0; i < tables.Length; i++)
                {
                    var table = tables[i];

                    var list = new List<int>();
                    for (var c = 0; c < table.ActivityCount; c++)
                    {
                        var col = getExcelColumnName(5 + c);   // 5 : 'E'
                        var address = $"{col}{i + 2}";
                        var cell = sheet[address];

                        list.Add(cell.Convert<int>());
                    }

                    table.Activities = list.ToArray();
                }

                foreach (var t in tables)
                    LOG.Addressable($"TABLE : {t}", this);

                return tables;
            }
        }
        public async UniTask<GameTesterTable[]> LoadGameTable(bool devServer, GameMode gameMode)
        {
            using (LOG.Coroutine($"LoadGameTable()", this))
            {
                // 엑셀 로드
                var hLoad = devServer
                    ? testerTableDevServerAR.LoadAssetAsync()
                    : testerTableTestServerAR.LoadAssetAsync();
                await hLoad.Task;

                var ex = hLoad.OperationException;
                var xlsx = hLoad.Result;

                Addressables.Release(hLoad);

                if (ex != null)
                    throw ex;

                // 엑셀 데이터 처리
                var workbook = new WorkBook(xlsx.Bytes);
                var sheetName = gameMode == GameMode.Review
                    ? "game_review"
                    : "game_playground";
                var sheet = workbook[sheetName];
                var tables = sheet.Convert<GameTesterTable>().ToArray();
                if (tables.Length == 0)
                {
                    var msg = $"No table is exist.";
                    LOG.Warning(msg, this);
                    throw new NoDataException(msg);
                }

                // 컬럼데이터 수집
                for (var i = 0; i < tables.Length; i++)
                {
                    var table = tables[i];

                    var list = new List<int>();
                    for (var c = 0; c < table.GameCount; c++)
                    {
                        var col = getExcelColumnName(5 + c);   // 5 : 'E'
                        var address = $"{col}{i + 2}";
                        var cell = sheet[address];

                        list.Add(cell.Convert<int>());
                    }

                    table.Games = list.ToArray();
                }

                foreach (var t in tables)
                    LOG.Addressable($"TABLE : {t}", this);

                return tables;
            }
        }
        public async UniTask<EBookTesterTable[]> LoadEBookTable(bool devServer)
        {
            using (LOG.Coroutine($"LoadEBookTable()", this))
            {
                // 엑셀 로드
                var hLoad = devServer
                    ? testerTableDevServerAR.LoadAssetAsync()
                    : testerTableTestServerAR.LoadAssetAsync();
                await hLoad.Task;

                var ex = hLoad.OperationException;
                var xlsx = hLoad.Result;

                Addressables.Release(hLoad);

                if (ex != null)
                    throw ex;

                // 엑셀 데이터 처리
                var workbook = new WorkBook(xlsx.Bytes);
                var sheet = workbook["ebook"];
                var tables = sheet.Convert<EBookTesterTable>().ToArray();
                if (tables.Length == 0)
                {
                    var msg = $"No table is exist.";
                    LOG.Warning(msg, this);
                    throw new NoDataException(msg);
                }

                // 컬럼데이터 수집
                for (var i = 0; i < tables.Length; i++)
                {
                    var table = tables[i];

                    var list = new List<int>();
                    for (var c = 0; c < table.EBookCount; c++)
                    {
                        var col = getExcelColumnName(5 + c);   // 5 : 'E'
                        var address = $"{col}{i + 2}";
                        var cell = sheet[address];

                        list.Add(cell.Convert<int>());
                    }

                    table.EBooks = list.ToArray();
                }

                foreach (var t in tables)
                    LOG.Addressable($"TABLE : {t}", this);

                return tables;
            }
        }



        // Functions
        private static string getExcelColumnName(int columnNumber)
        {
            var columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset> testerTableDevServerAR = null;
        [SerializeField] private AssetReferenceT<XlsxAsset> testerTableTestServerAR = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            ValueConverter.Register<ActivityIDConverter>();
            ValueConverter.Register<GameIDConverter>();
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityTesterTable
    {
        [Column("A")] public int Course;
        [Column("B")] public ActivityID ActivityType;
        [Column("C")] public string ActivityName;
        [Column("D")] public int ActivityCount;

        public int[] Activities;


        public override string ToString()
        {
            return $"{Course} | {ActivityType} | {ActivityName} | {ActivityCount} | {string.Join("", Activities)}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameTesterTable
    {
        [Column("A")] public int Course;
        [Column("B")] public GameID GameType;
        [Column("C")] public string GameName;
        [Column("D")] public int GameCount;

        public int[] Games;


        public override string ToString()
        {
            return $"{Course} | {GameType} | {GameName} | {GameCount} | {string.Join("", Games)}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class EBookTesterTable
    {
        [Column("A")] public string MainCategoryName;
        [Column("B")] public int MainCategory;
        [Column("C")] public int SubCategory;
        [Column("D")] public int EBookCount;

        public int[] EBooks;



        public override string ToString()
        {
            return $"{MainCategoryName} | {MainCategory} | {SubCategory} | {EBookCount} | {string.Join("", EBooks)}";
        }
    }
}